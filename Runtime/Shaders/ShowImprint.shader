Shader "Imprint/Show"
{
    Properties
    {
        _Blend ("Filter Intensity", Range(0, 1)) = 1.0
    }

    HLSLINCLUDE
        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_ST;
        TEXTURE2D_SAMPLER2D(_ImprintTex, sampler_ImprintTex);
        float4 _ImprintTex_ST;

        half _Blend;

        half4 Frag (VaryingsDefault input) : SV_Target {
            float imprint = SAMPLE_TEXTURE2D(_ImprintTex, sampler_ImprintTex, input.texcoord).x;
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);
            color.rgb = lerp(color.rgb, color.rgb * imprint.xxx, _Blend.xxx);
            color.rgb *= color.a;
            return color;
        }
    ENDHLSL

    SubShader {
        Cull Off ZWrite Off ZTest Always

        Pass {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment Frag
            ENDHLSL
        }
    }
}
