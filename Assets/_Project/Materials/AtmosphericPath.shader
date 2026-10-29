Shader "Lisztomnezia/Atmospheric Path"
{
 Properties { _Soil("Soil",Color)=(0.23,0.20,0.15,1) _Moss("Moss",Color)=(0.18,0.23,0.14,1) }
 SubShader {
 Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
 Cull Off
 ZWrite Off
 Blend SrcAlpha OneMinusSrcAlpha

 Pass {
 Tags {"LightMode"="SRPDefaultUnlit"}
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #pragma multi_compile_fog
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _Soil, _Moss;
 CBUFFER_END
 struct A {float3 positionOS:POSITION;float3 normalOS:NORMAL;};
 struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float fog:TEXCOORD2;};
 V Vert(A i){ V o; o.world=TransformObjectToWorld(i.positionOS);o.positionCS=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(i.normalOS);o.fog=ComputeFogFactor(o.positionCS.z);return o; }
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 half4 Frag(V i):SV_Target {
 float2 p=i.world.xz;
 float broad=noise(p*0.48),detail=noise(p*5.7),grit=noise(p*36);
 float edge=smoothstep(2.2,6.7,abs(p.x)+(broad-0.5)*2.2);
 half3 soil=_Soil.rgb*(0.7+broad*0.6+detail*0.22);
 float journey=smoothstep(15,185,p.y);
 half3 moss=lerp(_Moss.rgb,half3(0.22,0.37,0.24),journey);
 half3 base=lerp(soil,moss*(0.65+detail*0.6),edge);
 base=lerp(base,base*half3(1.25,1.35,1.12),journey);
 float worn=exp(-pow((abs(p.x)-1.1)*2.1,2));
 base*=1-worn*0.17;
 float fleck=smoothstep(0.85,0.96,grit)*(0.4+0.6*detail);
 base+=fleck*half3(0.035,0.032,0.025);
 float damp=smoothstep(0.52,0.7,noise(p*0.85))* (1-edge);
 base*=1-damp*lerp(0.52,0.18,journey);
 Light light=GetMainLight(TransformWorldToShadowCoord(i.world));
 half3 n=normalize(i.normal+float3((noise(p+0.02)-noise(p-0.02))*0.7,0,(detail-0.5)*0.08));
 half3 illumination=SampleSH(n)+light.color*saturate(dot(n,light.direction))*light.shadowAttenuation;
 half3 view=normalize(GetWorldSpaceViewDir(i.world));
 half spec=pow(saturate(dot(n,normalize(view+light.direction))),48)*damp*0.09*light.shadowAttenuation;
 half3 result=base*max(illumination,half3(0.65,0.65,0.65))+spec*light.color;
 return half4(MixFog(result,i.fog),1);
 }
 ENDHLSL
 }
 }
}

