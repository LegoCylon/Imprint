Shader "Imprint/Make" {
    HLSLINCLUDE
        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
        float4x4 unity_MatrixV;

        struct Attributes {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
        };

        struct Varyings {
            float4 positionCS : SV_POSITION;
            float4 positionSS_depth_fresnel : TEXCOORD0; // xy = positionSS, z = depth, w = fresnel
        };

        #if defined(UNITY_REVERSED_Z)
            #define COMPARE_DEPTH(a, b) step(b, a)
        #else
            #define COMPARE_DEPTH(a, b) step(a, b)
        #endif

        Varyings Vert (Attributes input) {
            Varyings output = (Varyings)0;

            float4 positionWS = mul(unity_ObjectToWorld, input.positionOS);
            output.positionCS = mul(unity_MatrixVP, positionWS);

            // Calculate positionSS
            output.positionSS_depth_fresnel.xy = (output.positionCS.xy / output.positionCS.w) * 0.5 + 0.5;
#if UNITY_UV_STARTS_AT_TOP
            output.positionSS_depth_fresnel.xy *= float2(1.0, -1.0);
            output.positionSS_depth_fresnel.xy += float2(0.0, 1.0);
#endif

            // Calculate depth
            output.positionSS_depth_fresnel.z = -mul(unity_MatrixV, positionWS).z;

            // Calculate fresnel
            // In this case the fresnel is inverted so that edges are darkened.
            // Taking the absolute value of the dotted normals also ensures that if the camera is inside the geometry it
            // will appear bright instead of dark.
            float3 eyeNormalWS = normalize(positionWS.xyz - _WorldSpaceCameraPos.xyz);
            half3 vertNormalWS = normalize(mul(unity_ObjectToWorld, input.normalOS));
            output.positionSS_depth_fresnel.w = 1. - pow(1. - abs(dot(eyeNormalWS, vertNormalWS)), 0.75);

            return output;
        }

        half4 Frag (Varyings input) : SV_Target {
            half packedDepth = SAMPLE_DEPTH_TEXTURE(
                _CameraDepthTexture,
                sampler_CameraDepthTexture,
                input.positionSS_depth_fresnel.xy);
            float sceneDepth = LinearEyeDepth(packedDepth);

            return COMPARE_DEPTH(sceneDepth, input.positionSS_depth_fresnel.z) * input.positionSS_depth_fresnel.w;
        }
    ENDHLSL

    Category {
        Tags { "IgnoreProjectors" = "True" }

        LOD 100
        Blend One One
        BlendOp Max
        ZWrite Off
        Cull Off

        SubShader {
            Tags { "RenderType" = "Opaque" }

            Pass {
                HLSLPROGRAM
                    #pragma vertex Vert
                    #pragma fragment Frag
                ENDHLSL
            }
        }
    }
}
