Shader "Hidden/YangTools/HitFeelDemoScreenEffects"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "HitFeelDemoScreenEffects"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float2 _HitFeelCenter;
            float _HitFeelAspect;
            float _HitFeelRadialBlur;
            float _HitFeelGrayscale;
            float _HitFeelColorDistortion;
            float _HitFeelShockwave;
            float _HitFeelShockwaveRadius;
            float _HitFeelShockwaveWidth;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float2 centerOffset = uv - _HitFeelCenter;
                float2 aspectOffset = float2(centerOffset.x * _HitFeelAspect, centerOffset.y);
                float distanceFromHit = length(aspectOffset);
                float localMask = 1.0 - smoothstep(0.04, 0.42, distanceFromHit);
                float2 radialDirection = normalize(aspectOffset + float2(0.00001, 0.00001));

                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                if (_HitFeelRadialBlur > 0.001)
                {
                    float2 blurOffset = radialDirection * _HitFeelRadialBlur * 0.012;
                    half4 blurColor = 0;
                    blurColor += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - blurOffset * 2.0);
                    blurColor += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - blurOffset);
                    blurColor += color;
                    blurColor += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + blurOffset);
                    blurColor += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + blurOffset * 2.0);
                    blurColor *= 0.2;
                    color = lerp(color, blurColor, _HitFeelRadialBlur * localMask);
                }

                if (_HitFeelColorDistortion > 0.001)
                {
                    float2 shift = radialDirection * _HitFeelColorDistortion * 0.004 * localMask;
                    half red = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + shift).r;
                    half blue = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - shift).b;
                    color.rgb = lerp(color.rgb, half3(red, color.g, blue), _HitFeelColorDistortion * localMask);
                    color.rgb = lerp(color.rgb, 1.0h - color.rgb, _HitFeelColorDistortion * localMask * 0.35);
                }

                if (_HitFeelGrayscale > 0.001)
                {
                    half luminance = dot(color.rgb, half3(0.2126, 0.7152, 0.0722));
                    color.rgb = lerp(color.rgb, luminance.xxx, _HitFeelGrayscale * localMask);
                }

                if (_HitFeelShockwave > 0.001)
                {
                    float ringDistance = abs(distanceFromHit - _HitFeelShockwaveRadius);
                    float ring = 1.0 - smoothstep(_HitFeelShockwaveWidth, _HitFeelShockwaveWidth * 2.0, ringDistance);
                    float2 waveOffset = radialDirection * ring * _HitFeelShockwave * 0.025;
                    half4 waveColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + waveOffset);
                    color = lerp(color, waveColor, ring * saturate(_HitFeelShockwave * 20.0));
                }

                return color;
            }
            ENDHLSL
        }
    }
}
