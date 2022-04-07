Shader "Hidden/SceneChangeAni"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Value("_Value",float) = 0.5
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Value;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                const float PI = 3.14159;

                float2 uv = abs(i.uv*2 - 1);
                float mask = distance(min(uv-0.5,0),fixed2(0.5,0.5));
                //mask = frac(mask*5);

                if(mask < _Value)
                {
                    return fixed4(0,0,0,1);
                }
           
                return col;
            }
            ENDCG
        }
    }
}
