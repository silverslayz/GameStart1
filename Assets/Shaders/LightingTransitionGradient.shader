// Prototype screen-space lighting-mood transition (GDD Section 2/9).
// Blends between two tint colors via _BlendT (0-1), driven by
// LightingTransition.cs on dungeon entry/exit, as a lightweight
// stand-in for a full custom lighting/post-process pass.
Shader "GameStart/LightingTransitionGradient"
{
    Properties
    {
        _ColorA ("Color A (outdoors)", Color) = (0.15, 0.18, 0.28, 0.0)
        _ColorB ("Color B (dungeon)", Color) = (0.35, 0.14, 0.05, 0.45)
        _BlendT ("Blend (0=A, 1=B)", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+110" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _ColorA;
            float4 _ColorB;
            float _BlendT;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Slight vertical gradient so the tint reads as lighting, not a flat filter
                float verticalFalloff = lerp(1.0, 0.6, IN.uv.y);
                float4 blended = lerp(_ColorA, _ColorB, _BlendT);
                return float4(blended.rgb, blended.a * verticalFalloff);
            }
            ENDHLSL
        }
    }
}
