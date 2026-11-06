Shader "Nusantara/BusLivery" {
Properties {
[MainColor] _BaseColor("Body",Color)=(.718,.11,.11,1)
[MainTexture] _BaseMap("Body texture",2D)="white"{}
_DecalTex("Player decal",2D)="white"{}
_DecalEnabled("Decal enabled",Float)=0
_StripeColor("Stripe",Color)=(1,1,1,1)
_StripeEnabled("Stripe enabled",Float)=1
_TrimColor("Trim",Color)=(.788,.635,.153,1)
_Mask("R: metal G: smoothness B: trim",2D)="black"{}
_DirtTex("Dirt",2D)="white"{}
_DirtAmount("Dirt accumulation",Range(0,1))=0
_EmissionMap("Emission",2D)="black"{}
_EmissionColor("Lights",Color)=(0,0,0,0)
}
SubShader {
Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
Pass {
Tags {"LightMode"="UniversalForward"}
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_instancing
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;float4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
struct V {float4 positionCS:SV_POSITION;float3 normalWS:TEXCOORD0;float2 uv:TEXCOORD1;float3 positionWS:TEXCOORD2;float4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO};
TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
TEXTURE2D(_DecalTex);SAMPLER(sampler_DecalTex);
TEXTURE2D(_Mask);SAMPLER(sampler_Mask);
TEXTURE2D(_DirtTex);SAMPLER(sampler_DirtTex);
TEXTURE2D(_EmissionMap);SAMPLER(sampler_EmissionMap);
CBUFFER_START(UnityPerMaterial)
float4 _BaseColor,_StripeColor,_TrimColor,_EmissionColor;
float _DecalEnabled,_StripeEnabled,_DirtAmount;
CBUFFER_END
V vert(A input) {
V output=(V)0;UNITY_SETUP_INSTANCE_ID(input);UNITY_TRANSFER_INSTANCE_ID(input,output);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
output.positionWS=TransformObjectToWorld(input.positionOS.xyz);output.positionCS=TransformWorldToHClip(output.positionWS);
output.normalWS=TransformObjectToWorldNormal(input.normalOS);output.uv=input.uv;output.color=input.color;return output;
}
half4 frag(V input):SV_Target {
UNITY_SETUP_INSTANCE_ID(input);UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
half3 base=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv).rgb*_BaseColor.rgb;
half stripe=step(.38,input.uv.y)*step(input.uv.y,.51)*_StripeEnabled;
base=lerp(base,_StripeColor.rgb,stripe);
half4 mask=SAMPLE_TEXTURE2D(_Mask,sampler_Mask,input.uv);base=lerp(base,_TrimColor.rgb,mask.b);
half4 decal=SAMPLE_TEXTURE2D(_DecalTex,sampler_DecalTex,input.uv);base=lerp(base,decal.rgb,decal.a*_DecalEnabled);
half dirt=SAMPLE_TEXTURE2D(_DirtTex,sampler_DirtTex,input.uv).r*input.color.r*_DirtAmount;base=lerp(base,half3(.22,.17,.10),dirt);
half3 normal=normalize(input.normalWS);Light sun=GetMainLight(TransformWorldToShadowCoord(input.positionWS));
half3 diffuse=base*(SampleSH(normal)+sun.color*saturate(dot(normal,sun.direction))*sun.shadowAttenuation);
half3 view=GetWorldSpaceNormalizeViewDir(input.positionWS);half spec=pow(saturate(dot(normal,normalize(view+sun.direction))),lerp(8,64,mask.g));
half3 result=diffuse+sun.color*spec*lerp(.03,.65,mask.r)*sun.shadowAttenuation;
result+=SAMPLE_TEXTURE2D(_EmissionMap,sampler_EmissionMap,input.uv).rgb*_EmissionColor.rgb;return half4(result,1);
}
ENDHLSL
}
UsePass "Universal Render Pipeline/Lit/ShadowCaster"
}
}
