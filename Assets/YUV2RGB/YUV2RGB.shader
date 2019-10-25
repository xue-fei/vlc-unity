// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/YUV2RGB" {
	Properties
	{
		_YTex("Y channel texture", 2D) = "white" {}
		_UTex("U channel texture", 2D) = "white" {} 
		_VTex("V channel texture", 2D) = "white" {} 
	}
		SubShader
		{
			// Setting the z write off to make sure our video overlay is always rendered at back.
			ZWrite Off
			ZTest Off
			Tags { "Queue" = "Background" }
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
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				v2f vert(appdata v)
				{ 
					v2f o;
					o.vertex =  UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}
				 
				sampler2D _YTex;
				sampler2D _UTex; 
				sampler2D _VTex; 
  
				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 fY = tex2D(_YTex,i.uv); 
					float y = fY.a;
					 
					fixed4 fU = tex2D(_UTex,i.uv); 
					float u = fU.a;

					fixed4 fV = tex2D(_VTex,i.uv);
					float v = fV.a;
					   
					float r = y + 1.402  * (v-128);
					float g = y - 0.34414 * (u-128) - 0.71414 * (v-128);
					float b = y + 1.772  * (u-128); 
					 
					fY.rgba = float4(r, g, b, 1.f);
					return fY;
				}
				ENDCG
			}
		}
}