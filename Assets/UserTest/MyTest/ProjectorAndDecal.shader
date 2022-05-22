Shader "LSQ/EffectAchievement/ProjectorAndDecal_2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "IgnoreProjector"="true" 
            "DisableBatching"="true"
        }

		Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
		Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {   
                fixed2 screenPos = i.screenPos.xy / i.screenPos.w;
                //��ͼ�ռ�����ֵ
                float depth = tex2D(_CameraDepthTexture, screenPos).r;
                //�������ֵ�ؽ��������꣺ֱ��������ˮ������������
                //�ü��ռ�
                fixed4 clipPos = fixed4(screenPos.x * 2 - 1, screenPos.y * 2 - 1, -depth * 2 + 1, 1) * LinearEyeDepth(depth);
                //��ԭ������ռ�
				float4 viewPos = mul(unity_CameraInvProjection, clipPos);
				//��ԭ������ռ� unity_MatrixInvV = UNITY_MATRIX_I_V unity_CameraToWorld 
				float4 worldPos = mul(unity_MatrixInvV, viewPos);
                //תΪ����ڱ�����ľֲ�����(�任���󶼱�������)
                float3 objectPos = mul(unity_WorldToObject, worldPos).xyz;
                //�����屾������-0.5~0.5
                clip(0.5 - abs(objectPos));
                //�����������ĵ�Ϊ0����UVΪ0.5
                objectPos += 0.5;

                fixed4 col = tex2D(_MainTex, objectPos.xy);

                return col;
            }
            ENDCG
        }
    }
}