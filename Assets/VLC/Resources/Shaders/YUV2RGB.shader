Shader "Custom/YUV2RGB"
{
    Properties
    {
        _YTex("Y Tex", 2D) = "white" {}
        _UTex("U Tex", 2D) = "white" {}
        _VTex("V Tex", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _YTex;
            float4 _YTex_ST;
            sampler2D _UTex;
            sampler2D _VTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _YTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float y = tex2D(_YTex, i.uv).w;
                float u = tex2D(_UTex, i.uv).w;
                float v = tex2D(_VTex, i.uv).w;
                float r = y + 1.4022 * v - 0.7011;
                float g = y - 0.3456 * u - 0.7145 * v + 0.53005;
                float b = y + 1.771 * u - 0.8855;
                fixed4 col = fixed4(r,g,b,1);
                return col;
            }
            ENDCG
        }
    }
}