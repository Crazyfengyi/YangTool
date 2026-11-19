Shader "UI/LiquidProgress"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _WaveColor ("Wave Color", Color) = (0.40, 0.91, 0.97, 1)
        _WaveStrength ("Wave Strength", Range(0, 1)) = 0.68
        _FlowSpeed ("Flow Speed (px/s)", Float) = 72
        _WaveLength ("Wave Length (px)", Range(16, 512)) = 112

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct Attributes
            {
                float4 position : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                #ifdef UNITY_UI_CLIP_RECT
                float4 mask : TEXCOORD1;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            half4 _Color;
            half4 _TextureSampleAdd;
            half4 _WaveColor;
            half _WaveStrength;
            float _FlowSpeed;
            float _WaveLength;

            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.position = UnityObjectToClipPos(input.position);
                output.color = input.color * _Color;
                output.uv = input.uv;

                #ifdef UNITY_UI_CLIP_RECT
                float2 pixelSize = output.position.w / abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float4 clipRect = clamp(_ClipRect, -2e10, 2e10);
                output.mask = float4(input.position.xy * 2.0 - clipRect.xy - clipRect.zw,
                    0.25 / (0.25 * float2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize)));
                #endif

                return output;
            }

            // 两层波纹使用屏幕像素坐标 使波长独立于填充比例和图集 UV
            half SampleWaves(float2 pixel)
            {
                float cyclesPerPixel = rcp(max(16.0, _WaveLength));
                float travel = _Time.y * _FlowSpeed * cyclesPerPixel;
                float phase = pixel.x * cyclesPerPixel;
                const float tau = 6.2831853;
                float front = sin(tau * frac(phase - travel)) * 4.3;
                float back = sin(tau * frac(phase * 1.37 - travel * 0.83 + 0.31)) * 3.2;

                float frontDistance = abs(frac((pixel.y - front) / 26.0 + 0.5) - 0.5);
                float backDistance = abs(frac((pixel.y - back) / 26.0) - 0.5);
                half frontBand = 1.0 - smoothstep(0.025, 0.25, frontDistance);
                half backBand = 1.0 - smoothstep(0.02, 0.32, backDistance);
                return frontBand * 0.70 + backBand * 0.30;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = (tex2D(_MainTex, input.uv) + _TextureSampleAdd) * input.color;
                half wave = SampleWaves(input.position.xy) * _WaveStrength * _WaveColor.a;
                color.rgb = lerp(color.rgb, _WaveColor.rgb, wave);

                #ifdef UNITY_UI_CLIP_RECT
                float2 mask = saturate((_ClipRect.zw - _ClipRect.xy - abs(input.mask.xy)) * input.mask.zw);
                color.a *= mask.x * mask.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}
