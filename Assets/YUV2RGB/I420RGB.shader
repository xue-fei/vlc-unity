Shader "Custom/I420RGB"
{
	Properties
	{
		_YTex ("Y", 2D) = "white" {}
        _UTex ("U", 2D) = "white" {}
        _VTex ("V", 2D) = "white" {} 
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			 
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

            sampler2D _YTex;
            sampler2D _UTex;
            sampler2D _VTex; 
			float4 _YTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _YTex);  
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                //反转uv的y分量
                fixed2 uv = fixed2(i.uv.x,1 - i.uv.y);
                fixed4 ycol = tex2D(_YTex, uv);
                fixed4 ucol = tex2D(_UTex, uv);
				fixed4 vcol = tex2D(_VTex, uv); 

                float r = ycol.a + 1.4022 * vcol.a - 0.7011;
				float g = ycol.a - 0.3456 * ucol.a - 0.7145* vcol.a + 0.5305;
				float b = ycol.a + 1.771 * ucol.a - 0.8855; 
				return fixed4(r,g,b,1);
			}
			ENDCG
		}
	}
}