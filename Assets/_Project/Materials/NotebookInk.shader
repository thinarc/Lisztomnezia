Shader "Lisztomnezia/Notebook Soft Ink"
{
    Properties { _Color ("Tint", Color) = (1,1,1,1) _Droplet("Droplet",Float)=0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            float _Droplet;
            CBUFFER_END
            struct A { float3 positionOS:POSITION; half4 color:COLOR; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; half4 color:COLOR; float2 uv:TEXCOORD0; };
            V Vert(A i) { V o; o.positionCS=TransformObjectToHClip(i.positionOS); o.color=i.color*_Color; o.uv=i.uv; return o; }
            half4 Frag(V i):SV_Target
            {
                half edge=abs(i.uv.y*2-1);
                half soft=exp(-edge*edge*5.5)*(1-smoothstep(0.75,1,edge));
                half grain=0.72+0.28*sin(i.uv.x*71+sin(i.uv.x*23)*2)*sin(i.uv.y*37+i.uv.x*19);
                soft*=grain;
                if(_Droplet>0.5){float2 uv=i.uv*2-1;float d=length(uv);soft=exp(-dot(uv,uv)*4)*(1-smoothstep(0.65,1,d));}
                return half4(i.color.rgb,i.color.a*soft);
            }
            ENDHLSL
        }
    }
}
