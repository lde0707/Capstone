Shader "DigGame/SkyCycle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _TopColor ("Top Color", Color) = (0.08, 0.34, 0.76, 1)
        _BottomColor ("Bottom Color", Color) = (0.48, 0.76, 0.96, 1)
        _SunColor ("Sun Color", Color) = (1, 0.95, 0.58, 1)
        _SunPosition ("Sun Position", Vector) = (0.5, 0.8, 0, 0)
        _Aspect ("Screen Aspect", Float) = 1.7778
        _NightAmount ("Night Amount", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        Blend One Zero

        Pass
        {
            Name "SkyCycle"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _BottomColor;
                half4 _SunColor;
                float4 _SunPosition;
                float _Aspect;
                float _NightAmount;
            CBUFFER_END

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 worldPosition = TransformObjectToWorld(input.positionOS);
                output.positionHCS = TransformWorldToHClip(worldPosition);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = saturate(input.uv);
                float gradientT = smoothstep(0.02, 0.98, uv.y);
                half3 sky = lerp(_BottomColor.rgb, _TopColor.rgb, gradientT);

                // 화면비를 보정해 해가 가로로 찌그러지지 않도록 한다.
                float2 sunDelta = uv - _SunPosition.xy;
                sunDelta.x *= max(0.01, _Aspect);
                float sunDistance = length(sunDelta);
                float sunCore = 1.0 - smoothstep(0.045, 0.060, sunDistance);
                float sunGlow = 1.0 - smoothstep(0.055, 0.22, sunDistance);
                float sunAlpha = saturate(_SunColor.a);
                sky += _SunColor.rgb * sunGlow * 0.24 * sunAlpha;
                sky = lerp(sky, _SunColor.rgb, sunCore * sunAlpha);

                // 밤이 깊어질수록 상단부터 작은 별이 자연스럽게 나타난다.
                float2 starGrid = uv * float2(96.0, 54.0);
                float2 starCell = floor(starGrid);
                float2 starLocal = frac(starGrid) - 0.5;
                float starSeed = Hash21(starCell);
                float starShape = 1.0 - smoothstep(0.025, 0.11, length(starLocal));
                float starExists = step(0.972, starSeed);
                float horizonFade = smoothstep(0.30, 0.62, uv.y);
                float starLight = starShape * starExists * horizonFade * saturate(_NightAmount);
                sky += half3(0.72, 0.82, 1.0) * starLight;

                return half4(saturate(sky), 1.0);
            }
            ENDHLSL
        }
    }
}
