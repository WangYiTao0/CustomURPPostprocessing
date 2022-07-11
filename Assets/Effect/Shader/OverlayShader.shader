Shader "Unlit/OverlayShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RendererTexture" = "UniversalPipeline" }
    
        Pass
        {
            Name "Overlay"
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            #pragma vertex vertOverlay
            #pragma fragment fragOverlay
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
                
            float _Intensity;
            float4 _OverlayColor;

            struct a2v
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv        : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vertOverlay(a2v i)
            {
                v2f o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs vertexInput = GetVertexPositionInputs(i.positionOS.xyz);
                o.vertex = vertexInput.positionCS;
                o.uv = i.uv;
                return o;
            }
                
            float4 fragOverlay (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return lerp(color, _OverlayColor, _Intensity);
            }
            
            ENDHLSL
        }
    }
}
