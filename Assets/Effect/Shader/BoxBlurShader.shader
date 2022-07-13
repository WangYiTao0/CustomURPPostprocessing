Shader "Custom/BoxBlurShader" {
	Properties {
		 [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
    }
	SubShader {
		Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
		
		HLSLINCLUDE
        #pragma vertex vert
        #pragma fragment frag
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		struct a2v
        {
            float4 position : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float4 positionClip : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

		TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;

		float _BlurSize;
		//float4 _BlurColor;

		v2f vert(a2v i)
        {
            v2f o;
            o.positionClip = TransformObjectToHClip(i.position.xyz);
            o.uv = TRANSFORM_TEX(i.uv, _MainTex);
            return o;
        }
		ENDHLSL
		
        Pass
        {
            Name "Verticle box blur"

            HLSLPROGRAM
            half4 frag(v2f i) : SV_TARGET
            {
                float2 res = _MainTex_TexelSize.xy;
                half4 sum = 0;

                int samples = 2 * _BlurSize + 1;

                for (float y = 0; y < samples; y++)
                {
                    float2 offset = float2(0, y - _BlurSize);
                    sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset * res);
                }
                return sum / samples;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Horizontal box blur"

            HLSLPROGRAM
            half4 frag(v2f i) : SV_TARGET
            {
                float2 res = _MainTex_TexelSize.xy;
                half4 sum = 0;

                int samples = 2 * _BlurSize + 1;

                for (float x = 0; x < samples; x++)
                {
                    float2 offset = float2(x - _BlurSize, 0);
                    sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset * res);
                }

                return sum  / samples;
            }
            ENDHLSL
        }
	}
}