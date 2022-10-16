Shader "MyShader/Effect/FX_smoke"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset]_MaskTex("MaskTex",2D)="white"{}
        _Color("Color",Color)=(1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        Cull Off Lighting Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_particles

            #include "UnityCG.cginc"

            sampler2D _MainTex,_MaskTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed _Clip;
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color:COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 maskuv:TEXCOORD1;
                float4 vertex : SV_POSITION;
                float4 color:COLOR;
            };


            // float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv=TRANSFORM_TEX(v.uv,_MainTex);
                o.maskuv=v.uv;
                o.color=v.color*_Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 mask=tex2D(_MaskTex,i.maskuv);
                col.rgb*=i.color.rgb*2;
                fixed alpha=i.color.a*mask.a*_Color.a;
                // col.a=alpha;
                clip(i.color.a-mask.a);
                return col;
            }
            ENDCG
        }
    }
}
