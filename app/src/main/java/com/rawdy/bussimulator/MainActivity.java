package com.rawdy.bussimulator;

import android.app.Activity;
import android.os.Bundle;
import android.view.*;
import android.graphics.*;
import android.content.Context;
import android.content.SharedPreferences;
import android.media.ToneGenerator;
import android.media.AudioManager;
import java.util.*;

public class MainActivity extends Activity {
    private DriveView game;
    @Override public void onCreate(Bundle state) {
        super.onCreate(state);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        getWindow().getDecorView().setSystemUiVisibility(5894);
        game = new DriveView(this);
        setContentView(game);
    }
    @Override protected void onPause() { super.onPause(); game.suspend(); }
    @Override protected void onResume() { super.onResume(); if(game != null) game.resume(); }
    @Override protected void onDestroy() { game.release(); super.onDestroy(); }

    static class DriveView extends View {
        final Paint p = new Paint(3);
        final Random random = new Random(42);
        final Map<Integer,Integer> fingers = new HashMap<>();
        final SharedPreferences prefs;
        ToneGenerator tone;
        final float[][] traffic = new float[8][3];
        float speed, position, distance, health=100, elapsed, invulnerable;
        int delivered, best;
        boolean started, paused, finished, active=true;
        long previous;
        float scale=1, offsetX, offsetY;
        String notice="Keep left. Drive safely.";
        float noticeTime=5;
        DriveView(Context context) {
            super(context);
            prefs=context.getSharedPreferences("career",0);
            best=prefs.getInt("best",0);
            try { tone=new ToneGenerator(AudioManager.STREAM_MUSIC,60); } catch(RuntimeException ignored) {}
            resetTraffic();
            setContentDescription("Bus Simulator Bangladesh driving game");
        }
        void resetTraffic() {
            for(int i=0;i<traffic.length;i++) {
                traffic[i][0]=(i%2==0?-.52f:.52f);
                traffic[i][1]=180+i*160;
                traffic[i][2]=i%3;
            }
        }
        void reset() {
            speed=position=distance=elapsed=invulnerable=0;
            health=100; delivered=0; finished=false; started=true; paused=false;
            notice="Dhaka to countryside • 3 km"; noticeTime=5;
            fingers.clear(); resetTraffic(); previous=0;
        }
        void suspend() { active=false; speed=0; fingers.clear(); if(started&&!finished) paused=true; previous=0; }
        void resume() { active=true; previous=0; invalidate(); }
        void release() { if(tone!=null) tone.release(); }
        boolean held(int button) { return fingers.containsValue(button); }
        void update(float dt) {
            if(!started||paused||finished) return;
            elapsed+=dt; noticeTime-=dt; invulnerable=Math.max(0,invulnerable-dt);
            speed=Math.max(0,Math.min(100,speed+((held(4)?22: -5)-(held(3)?45:0))*dt));
            position+=((held(2)?1:0)-(held(1)?1:0))*dt*(.35f+speed/90);
            position=Math.max(-1.35f,Math.min(1.35f,position));
            if(Math.abs(position)>.86f) { speed=Math.max(0,speed-20*dt); health-=3*dt; }
            float step=speed/3.6f*dt;
            distance+=step;
            for(float[] car:traffic) {
                float old=car[1];
                car[1]-=step+(car[0]>0?12*dt:-4*dt);
                if(old>6&&car[1]<=6&&Math.abs(position-car[0])<.32f&&invulnerable<=0) {
                    health-=22; speed*=.35f; invulnerable=2;
                    notice="Collision! Brake and keep your distance."; noticeTime=3;
                }
                if(car[1]<-30) { car[1]=1000+random.nextFloat()*300; }
            }
            if(distance>=(delivered+1)*1000 && delivered<3) {
                delivered++;
                notice="Checkpoint "+delivered+"/3 reached"; noticeTime=4;
            }
            if(health<=0||distance>=3000) {
                finished=true; speed=0; fingers.clear();
                int score=distance>=3000?Math.max(0,(int)(health*10+1200-elapsed)):0;
                if(score>best) { best=score; prefs.edit().putInt("best",best).apply(); }
                notice=distance>=3000?"ROUTE COMPLETE  •  Score "+score:"BUS DAMAGED";
            }
        }
        void box(Canvas c,float x,float y,float w,float h,int color) {
            p.setColor(color); c.drawRoundRect(x,y,x+w,y+h,12,12,p);
        }
        void text(Canvas c,String s,float x,float y,float size,int color) {
            p.setColor(color); p.setTypeface(Typeface.create("sans-serif-medium",Typeface.NORMAL));
            p.setTextSize(size); c.drawText(s,x,y,p);
        }
        void poly(Canvas c,int color,float... xy) {
            Path path=new Path(); path.moveTo(xy[0],xy[1]);
            for(int i=2;i<xy.length;i+=2) path.lineTo(xy[i],xy[i+1]);
            path.close(); p.setColor(color); c.drawPath(path,p);
        }
        @Override protected void onDraw(Canvas canvas) {
            long now=System.nanoTime();
            float dt=previous==0?0:Math.min(.05f,(now-previous)/1e9f); previous=now;
            if(active) update(dt);
            scale=Math.min(getWidth()/1000f,getHeight()/560f);
            offsetX=(getWidth()-1000*scale)/2; offsetY=(getHeight()-560*scale)/2;
            canvas.drawColor(Color.rgb(9,18,26));
            canvas.save(); canvas.translate(offsetX,offsetY); canvas.scale(scale,scale);
            Canvas c=canvas;
            box(c,0,0,1000,560,Color.rgb(151,199,214));
            p.setColor(Color.rgb(245,224,161)); c.drawCircle(800,110,36,p);
            poly(c,Color.rgb(61,105,72),0,235,100,170,180,225,300,185,450,240,680,190,800,220,920,165,1000,220,1000,330,0,330);
            box(c,0,245,1000,315,Color.rgb(88,132,68));
            float shift=-position*100;
            poly(c,Color.rgb(196,185,143),465+shift,245,535+shift,245,970+shift,560,30+shift,560);
            poly(c,Color.rgb(52,61,65),478+shift,245,522+shift,245,900+shift,560,100+shift,560);
            for(int i=0;i<14;i++) {
                float z=((i*80+80-distance%80)/1200f);
                float y=245+(1-z)*(1-z)*315;
                float width=(y-245)*.017f+1;
                box(c,500+shift-width,y,width*2,Math.max(2,(1-z)*19),Color.rgb(236,218,159));
            }
            for(int i=0;i<8;i++) {
                float z=((i*150+150-distance%150)/1200f);
                float depth=(1-z)*(1-z), y=245+depth*315;
                float x=500+shift-(60+depth*490);
                p.setColor(Color.rgb(91,72,49)); c.drawRect(x-3,y-65*depth,x+3,y,p);
                p.setColor(Color.rgb(28,91,61)); c.drawCircle(x,y-80*depth,30*depth+3,p);
            }
            // Draw distant vehicles first for correct occlusion.
            List<float[]> sorted=new ArrayList<>(Arrays.asList(traffic));
            Collections.sort(sorted, (a,b)->Float.compare(b[1],a[1]));
            for(float[] car:sorted) {
                if(car[1]<=0||car[1]>1200) continue;
                float d=1-car[1]/1200, depth=d*d;
                float y=245+depth*315, w=8+depth*78, h=w*1.15f;
                float x=500+shift+car[0]*(20+depth*300);
                box(c,x-w/2,y-h,w,h,car[2]==0?Color.rgb(186,71,44):Color.rgb(33,116,118));
                box(c,x-w*.38f,y-h*.86f,w*.76f,h*.32f,Color.rgb(25,45,56));
                box(c,x-w*.4f,y-h*.15f,w*.18f,4,Color.rgb(255,204,90));
                box(c,x+w*.22f,y-h*.15f,w*.18f,4,Color.rgb(255,204,90));
            }
            box(c,20,18,440,65,0xee102631);
            text(c,"BANGLADESH / LOCAL SERVICE",36,43,16,0xff70d9b2);
            text(c,"Dhaka → Countryside",36,69,23,Color.WHITE);
            box(c,720,18,260,65,0xee102631);
            text(c,String.format(Locale.US,"%03d km/h   •   BUS %d%%",(int)speed,(int)Math.max(0,health)),737,47,21,Color.WHITE);
            text(c,String.format(Locale.US,"%.2f / 3.00 km   •   %d/3",distance/1000,delivered),737,69,16,0xffa6bcc7);
            if(noticeTime>0&&!finished) { box(c,260,100,480,40,0xdd102631); text(c,notice,276,127,17,Color.WHITE); }
            box(c,0,441,1000,119,0xff101e28);
            button(c,30,470,105,68,"LEFT",held(1));
            button(c,150,470,105,68,"RIGHT",held(2));
            button(c,710,470,110,68,"BRAKE",held(3));
            button(c,840,460,130,78,"DRIVE",held(4));
            button(c,285,479,85,50,"HORN",held(5));
            button(c,390,479,85,50,"PAUSE",false);
            text(c,"BD",540,484,28,0xff70d9b2);
            text(c,"LOCAL BUS",516,512,13,0xffa6bcc7);
            if(!started||paused||finished) {
                box(c,180,155,640,265,0xf511202c);
                text(c,!started?"BUS SIMULATOR":finished?notice:"DRIVE PAUSED",215,205,29,Color.WHITE);
                text(c,!started?"BANGLADESH":finished?"Best score: "+best:"Controls released. Your bus is stopped.",215,244,22,0xff70d9b2);
                text(c,"Keep left • Avoid traffic • Pass all 3 checkpoints",215,282,19,0xffb5c4cd);
                text(c,"Hold DRIVE to accelerate. BRAKE before overtaking.",215,313,18,0xffb5c4cd);
                button(c,340,345,320,55,paused?"RESUME":started?"DRIVE AGAIN":"START ROUTE",false);
            }
            canvas.restore();
            if(active) postInvalidateOnAnimation();
        }
        void button(Canvas c,int x,int y,int w,int h,String label,boolean down) {
            box(c,x,y,w,h,down?0xff287d69:0xff243b49);
            p.setTextSize(18); float width=p.measureText(label);
            text(c,label,x+(w-width)/2,y+h/2f+6,18,Color.WHITE);
        }
        int hit(float x,float y) {
            if(y>=460&&y<=540) {
                if(x>=30&&x<=135)return 1;
                if(x>=150&&x<=255)return 2;
                if(x>=710&&x<=820)return 3;
                if(x>=840&&x<=970)return 4;
                if(x>=285&&x<=370)return 5;
                if(x>=390&&x<=475)return 6;
            }
            return 0;
        }
        @Override public boolean onTouchEvent(MotionEvent e) {
            int action=e.getActionMasked(), index=e.getActionIndex();
            float x=(e.getX(index)-offsetX)/scale, y=(e.getY(index)-offsetY)/scale;
            if(action==MotionEvent.ACTION_CANCEL) { fingers.clear(); return true; }
            if(action==MotionEvent.ACTION_UP||action==MotionEvent.ACTION_POINTER_UP) {
                fingers.remove(e.getPointerId(index)); performClick(); return true;
            }
            if(action==MotionEvent.ACTION_DOWN||action==MotionEvent.ACTION_POINTER_DOWN) {
                if(!started||paused||finished) {
                    if(x>=340&&x<=660&&y>=345&&y<=400) {
                        if(paused) { paused=false; previous=0; } else reset();
                    }
                    return true;
                }
                int button=hit(x,y);
                if(button==6) { paused=true; speed=0; fingers.clear(); }
                else if(button>0) {
                    fingers.put(e.getPointerId(index),button);
                    if(button==5&&tone!=null) tone.startTone(ToneGenerator.TONE_CDMA_ALERT_CALL_GUARD,250);
                }
            } else if(action==MotionEvent.ACTION_MOVE) {
                for(int i=0;i<e.getPointerCount();i++) {
                    int id=e.getPointerId(i);
                    if(fingers.containsKey(id)) {
                        int b=hit((e.getX(i)-offsetX)/scale,(e.getY(i)-offsetY)/scale);
                        if(b>=1&&b<=5) fingers.put(id,b); else fingers.remove(id);
                    }
                }
            }
            return true;
        }
        @Override public boolean performClick() { super.performClick(); return true; }
    }
}
