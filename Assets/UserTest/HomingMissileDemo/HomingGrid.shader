Shader "YangTools/Demo/HomingGrid"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 ground : TEXCOORD0; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 world = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(world);
                output.ground = world.xz;
                return output;
            }

            float Grid(float2 coordinate, float spacing, float width)
            {
                float2 cell = coordinate / spacing;
                float2 distanceToLine = abs(frac(cell + 0.5) - 0.5);
                float2 pixel = max(fwidth(cell), 0.0001);
                float2 coverage = 1.0 - smoothstep(width, width + pixel, distanceToLine);
                return max(coverage.x, coverage.y);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 ground = input.ground;
                half3 color = lerp(half3(0.026, 0.083, 0.155), half3(0.037, 0.174, 0.286),
                    saturate(1.0 - length(ground / float2(26.0, 30.0))));
                color += Grid(ground, 1.0, 0.005) * half3(0.012, 0.028, 0.039);
                color += Grid(ground, 5.0, 0.004) * half3(0.024, 0.054, 0.073);
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
