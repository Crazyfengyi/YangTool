Shader "Shader Forge/BloodShader"
{
    Properties 
    {
        _BloodValue ("BloodValue", Range(0, 1)) = 0.3330996
        _Fade ("Fade", Range(0, 1)) = 1
        _Color ("Color", Color) = (1,1,1,1)
        [PerRendererData] _MainTex ("MainTex", 2D) = "white" {}

        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)

        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

        _Stencil ("Stencil ID", Float) = 0
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilOpFail ("Stencil Fail Operation", Float) = 0
        _StencilOpZFail ("Stencil Z-Fail Operation", Float) = 0
    }
    SubShader 
    {
        Tags 
        {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Lighting Off

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            uniform float _BloodValue;
            uniform float _Fade;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv=i.texcoord;
                fixed bloodA=step(_BloodValue,uv.r);
                fixed bloodB=step(uv.r,_BloodValue);
                float alpha=1;
                clip(lerp((bloodA*0.0)+(bloodB*alpha),alpha,bloodA*bloodB) - 0.5);

                fixed4 col = SampleSpriteTexture (uv) * i.color;
                col.rgb*=col.a;
                col.a = _Fade*col.a;
                return col;
            }

        ENDCG
        }
    }
    FallBack "Sprites/Default"
}
