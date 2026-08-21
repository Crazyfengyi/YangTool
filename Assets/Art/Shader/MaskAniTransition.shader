Shader "Custom/MaskAniTransition"
{
    Properties
    {
        _TransRange("TransRange",Range(0,1)) = 0
        _ScreenWidth("ScreenWidth",Range(0,9999)) = 1920
        _ScreenHeight("ScreenHeight",Range(0,9999)) = 1080
        _EdgeSoftness("Edge Softness", Range(0.1, 3)) = 1
        [PerRendererData][NoScaleOffset]_MainTex ("Sprite Texture", 2D) = "white" {}
        [NoScaleOffset]_ClipTex ("ClipTexture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
           
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };
 
           struct v2f
           {
               float4 vertex : SV_POSITION;
               float2 uv : TEXCOORD0;
               float2 uv2 : TEXCOORD1;
           };

           sampler2D _MainTex;
            
           sampler2D _ClipTex;
           float4 _ClipTex_ST;

           float _TransRange;
           float _ScreenWidth;
           float _ScreenHeight;
           float _EdgeSoftness;
           
           v2f vert (appdata v)
           {
               v2f o;
               o.vertex = UnityObjectToClipPos(v.vertex);
               _ClipTex_ST.x = _TransRange * _TransRange * 100;
               _ClipTex_ST.y = _ClipTex_ST.x / _ScreenWidth * _ScreenHeight;
               _ClipTex_ST.z = -_ClipTex_ST.x / 2 + 0.5;
               _ClipTex_ST.w = -_ClipTex_ST.y / 2 + 0.5;
               o.uv = v.uv * _ClipTex_ST.xy + _ClipTex_ST.zw;
               o.uv2 = v.uv;
               return o;
           }

           fixed4 frag (v2f i) : SV_Target
           {
               fixed4 clipColor = tex2D(_ClipTex,i.uv);
               fixed4 mainColor = tex2D(_MainTex,i.uv2);
               float mask = 1 - clipColor.a;
               // 根据屏幕导数平滑二值掩码边缘，避免透明边缘出现硬锯齿。
               float edgeWidth = max(fwidth(mask) * _EdgeSoftness, 0.001);
               float maskAlpha = smoothstep(0.5 - edgeWidth, 0.5 + edgeWidth, mask);
               mainColor.a *= maskAlpha;
               return mainColor;
           }
           ENDCG
        }    
    }
}
