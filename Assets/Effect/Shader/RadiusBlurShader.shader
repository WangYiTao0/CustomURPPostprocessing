Shader "Custom/RadiusBlurShader" 
{
	Properties 
	{
		[HideInInspector]_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags {"RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
		
		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#pragma vertex vert
		#pragma fragment frag
		
		struct a2v
		{
			float4 position : Position;
			float2 uv : TEXCOORD;
		};
		struct v2f
		{
			float4 positionClip : SV_Position;
			float2 uv : TEXCOORD0;
		};
		
		TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;
		
		float2 _BlurCenter;
		int _SampleCount;
		float _BlurRange;
		float _BlurPower;
		//float4 _BlurColor;
		ENDHLSL
		
		Pass
		{
			Name "Radius Blur"
			
			HLSLPROGRAM
			v2f vert(a2v i)
	        {
	            v2f o;
	            o.positionClip = TransformObjectToHClip(i.position.xyz);
	            o.uv = TRANSFORM_TEX(i.uv, _MainTex);
	            return o;
	        }

			half4 frag(v2f i) : SV_TARGET
			{
				float2 dir =  i.uv - _BlurCenter;
				float2 dir_normal = normalize(dir) * _BlurRange;
				float4 col;
				for (int j = 1; j<= _SampleCount * 0.5; j++)
				{
					col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - dir_normal * 0.01 * j);
				}
				for (int k = 1; k<= _SampleCount * 0.5; k++)
				{
					col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + dir_normal * 0.01 * k);
				}
				col /= _SampleCount;
				float d = length(dir);
				col = lerp(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv),col
					,saturate(_BlurPower * d * d));
				return col;
			}
			ENDHLSL
		}
	}
}