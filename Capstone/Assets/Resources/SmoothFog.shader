Shader "DigGame/SmoothFog"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (0.02, 0.02, 0.04, 0.98)
        _Center ("View Center", Vector) = (0, 0, 0, 0)
        _InnerRadius ("Clear Radius", Float) = 2.5
        _OuterRadius ("Fade Radius", Float) = 3.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "SmoothFog"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _FogColor;
                float4 _Center;
                float _InnerRadius;
                float _OuterRadius;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 worldPosition = TransformObjectToWorld(input.positionOS);
                output.positionHCS = TransformWorldToHClip(worldPosition);
                output.positionWS = worldPosition.xy;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float distanceFromPlayer = distance(input.positionWS, _Center.xy);
                half fogAmount = smoothstep(_InnerRadius, _OuterRadius, distanceFromPlayer);
                return half4(_FogColor.rgb, _FogColor.a * fogAmount);
            }
            ENDHLSL
        }
    }
}
