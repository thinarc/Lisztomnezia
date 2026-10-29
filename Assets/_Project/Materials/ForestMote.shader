Shader "Lisztomnezia/Forest Mote" {
Properties { _Tint("Tint", Color)=(1,1,1,1) }
SubShader { Tags { "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
Blend SrcAlpha One
ZWrite Off
Cull Off
Pass { HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A {float4 p:POSITION; float2 uv:TEXCOORD0; half4 c:COLOR;};
struct V {float4 p:SV_POSITION; float2 uv:TEXCOORD0; half4 c:COLOR;};
half4 _Tint;
V vert(A v) {V o; o.p=TransformObjectToHClip(v.p.xyz); o.uv=v.uv; o.c=v.c*_Tint; return o;}
half4 frag(V i):SV_Target {float d=length(i.uv*2-1); float a=pow(saturate(1-d),3); return half4(i.c.rgb,i.c.a*a);}
ENDHLSL }
} }
