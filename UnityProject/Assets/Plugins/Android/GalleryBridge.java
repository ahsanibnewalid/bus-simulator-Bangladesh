package com.rawdy.nusantara;
import android.app.Activity;
import android.app.Fragment;
import android.content.Intent;
import android.os.Bundle;
import com.unity3d.player.UnityPlayer;
import java.io.*;
public class GalleryBridge extends Fragment {
    private static final int REQUEST=7421;
    private String receiver;
    public static void pick(final String receiver) {
        final Activity activity=UnityPlayer.currentActivity;
        activity.runOnUiThread(()-> {
            if(activity.getFragmentManager().findFragmentByTag("NusantaraGallery")!=null)return;
            GalleryBridge bridge=new GalleryBridge();Bundle args=new Bundle();args.putString("receiver",receiver);bridge.setArguments(args);
            activity.getFragmentManager().beginTransaction().add(bridge,"NusantaraGallery").commitAllowingStateLoss();
        });
    }
    @Override public void onCreate(Bundle state) {
        super.onCreate(state); receiver=getArguments().getString("receiver");
        if(state!=null)return;
        Intent intent=new Intent(Intent.ACTION_OPEN_DOCUMENT);intent.setType("image/*");intent.addCategory(Intent.CATEGORY_OPENABLE);
        intent.putExtra(Intent.EXTRA_MIME_TYPES,new String[]{"image/png","image/jpeg"});
        try {startActivityForResult(intent,REQUEST);}catch(Exception e){finish("OnGalleryError",e.getMessage());}
    }
    @Override public void onActivityResult(int request,int result,Intent data) {
        super.onActivityResult(request,result,data);if(request!=REQUEST)return;
        if(result!=Activity.RESULT_OK||data==null||data.getData()==null){finish("OnGalleryError","Image selection cancelled");return;}
        final android.net.Uri uri=data.getData();final Activity activity=getActivity();
        new Thread(()-> {
            File target=new File(activity.getCacheDir(),"selected-livery.img");
            try(InputStream input=activity.getContentResolver().openInputStream(uri);OutputStream output=new FileOutputStream(target)) {
                if(input==null)throw new IOException("Unable to open image");
                byte[] buffer=new byte[8192];int n,total=0;
                while((n=input.read(buffer))!=-1){total+=n;if(total>=4*1024*1024)throw new IOException("Image must be smaller than 4 MB");output.write(buffer,0,n);}
                activity.runOnUiThread(()->finish("OnGalleryPicked",target.getAbsolutePath()));
            }catch(Exception e){target.delete();activity.runOnUiThread(()->finish("OnGalleryError",e.getMessage()));}
        }).start();
    }
    void finish(String method,String value) {
        UnityPlayer.UnitySendMessage(receiver,method,value==null?"Gallery error":value);
        if(isAdded())getFragmentManager().beginTransaction().remove(this).commitAllowingStateLoss();
    }
}
