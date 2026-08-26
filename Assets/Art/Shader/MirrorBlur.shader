Shader "UI/MirrorBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _BlurAmount ("Blur Amount", Range(0, 10)) = 2.0
        _MirrorStrength ("Mirror Strength", Range(0, 1)) = 0.5
        _MirrorDistortion ("Mirror Distortion", Range(0, 1)) = 0.2
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "MirrorBlur"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_TexelSize;
            
            float _BlurAmount;
            float _MirrorStrength;
            float _MirrorDistortion;

            v2f vert (appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                // 镜面扭曲效果
                float2 distortion = float2(
                    sin(IN.texcoord.y * 10.0 + _Time.y) * _MirrorDistortion,
                    cos(IN.texcoord.x * 10.0 + _Time.y) * _MirrorDistortion
                );
                
                // 基础模糊采样
                float4 texColor = tex2D(_MainTex, IN.texcoord + distortion * _MirrorStrength);
                float blurTotal = texColor.a;
                float4 blurColor = texColor * texColor.a;
                
                // 多次采样实现模糊
                for (int i = 1; i < 9; i++)
                {
                    float2 offset = float2(
                        (i % 3 - 1) * _MainTex_TexelSize.x * _BlurAmount,
                        (i / 3 - 1) * _MainTex_TexelSize.y * _BlurAmount
                    );
                    
                    float2 sampleUV = IN.texcoord + offset + distortion * _MirrorStrength;
                    float4 sampleColor = tex2D(_MainTex, sampleUV);
                    
                    blurColor += sampleColor * sampleColor.a;
                    blurTotal += sampleColor.a;
                }
                
                // 计算最终颜色
                blurColor /= max(blurTotal, 0.001);
                
                // 应用颜色和透明度
                float4 finalColor = blurColor * IN.color;
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                return finalColor;
            }
            ENDCG
        }
    }
}
