// Prototype full-screen underwater distortion overlay (GDD Section 2/9).
// Attached to a camera-facing quad; fades in/out via UnderwaterEffect.cs
// based on PlayerController.IsSwimming, rather than a full custom
// Renderer Feature pass, to keep this a lightweight prototype.
Shader "GameStart/UnderwaterOverlay"
{
    Properties
    {
        _Color ("Tint", Color) = (0.05, 0.25, 0.45, 0.35)
        _DistortSpeed ("Distort Speed", Float) = 1.5
        _DistortStrength ("Distort Strength", Float) = 0.02
        _Alpha ("Overall Alpha", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+100" "RenderPipeline"="UniversalPipeline" }
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

            float4 _Color;
            float _DistortSpeed;
            float _DistortStrength;
            float _Alpha;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float t = _Time.y * _DistortSpeed;
                float2 distortedUv = IN.uv + float2(sin(IN.uv.y * 12.0 + t) * _DistortStrength,
                                                      cos(IN.uv.x * 10.0 + t) * _DistortStrength);
                float wave = sin(distortedUv.x * 20.0 + t * 1.7) * 0.5 + 0.5;
                float3 color = _Color.rgb + wave * 0.03;
                return float4(color, _Color.a * _Alpha);
            }
            ENDHLSL
        }
    }
}
