Shader "Lisztomnezia/Rabbit Interior Burn"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BurnProgress ("Burn progress", Range(0,1)) = 0
        _OutlinePixels ("Remaining outline width", Range(1,6)) = 2
        _BurnEdgeColor ("Charred edge", Color) = (0.18,0.10,0.08,1)
        _SpriteBounds ("Local center and size", Vector) = (0,0,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            CBUFFER_START(UnityPerMaterial)
                float4 _Color, _BurnEdgeColor, _SpriteBounds;
                float _BurnProgress, _OutlinePixels;
            CBUFFER_END
            struct Attributes { float3 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float2 local:TEXCOORD1; float3 normalWS:TEXCOORD2; float4 color:COLOR; };
            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionCS=TransformObjectToHClip(input.positionOS);
                o.uv=input.uv;
                o.local=(input.positionOS.xy-_SpriteBounds.xy)/max(_SpriteBounds.zw,float2(0.001,0.001));
                o.normalWS=TransformObjectToWorldNormal(input.normalOS);
                o.color=input.color*_Color;
                return o;
            }
            float Alpha(float2 uv) { return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv).a; }
            half4 Frag(Varyings i):SV_Target
            {
                half4 tex=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                float2 pixel=_MainTex_TexelSize.xy*_OutlinePixels;
                float inside=Alpha(i.uv+float2(pixel.x,0));
                inside=min(inside,Alpha(i.uv-float2(pixel.x,0)));
                inside=min(inside,Alpha(i.uv+float2(0,pixel.y)));
                inside=min(inside,Alpha(i.uv-float2(0,pixel.y)));
                inside=min(inside,Alpha(i.uv+pixel));
                inside=min(inside,Alpha(i.uv-pixel));
                inside=min(inside,Alpha(i.uv+float2(pixel.x,-pixel.y)));
                inside=min(inside,Alpha(i.uv+float2(-pixel.x,pixel.y)));
                float outline=1-smoothstep(0.05,0.8,inside);
                float noise=sin(i.local.x*43+sin(i.local.y*29))*sin(i.local.y*51+i.local.x*17)*0.025;
                float distanceFromCenter=length(i.local)+noise;
                float radius=lerp(-0.08,0.85,saturate(_BurnProgress));
                float remaining=smoothstep(radius-0.035,radius+0.035,distanceFromCenter);
                float edge=(1-smoothstep(0.015,0.065,abs(distanceFromCenter-radius)))*step(0.001,_BurnProgress)*(1-outline);
                half3 baseColor=lerp(tex.rgb,_BurnEdgeColor.rgb,edge*0.65);
                // Reuse scene lighting rather than making the rabbit emissive.
                Light light=GetMainLight();
                half3 lighting=SampleSH(normalize(i.normalWS))+light.color*saturate(dot(normalize(i.normalWS),light.direction));
                half alpha=tex.a*i.color.a*max(outline,remaining);
                return half4(baseColor*i.color.rgb*lighting,alpha);
            }
            ENDHLSL
        }
    }
}
