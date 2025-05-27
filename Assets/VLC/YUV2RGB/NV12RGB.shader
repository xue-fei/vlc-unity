Shader "Custom/NV12RGB" {
	Properties {
		_YTex ("Y(Alpha8)", 2D) = "white" {}
        _UVTex ("UV(RGB24)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM 
		#pragma surface surf Lambert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _YTex;
        sampler2D _UVTex;

		struct Input {
			float2 uv_YTex;
			float2 uv_UVTex;
		}; 

		void surf (Input IN, inout SurfaceOutput o) {
			float3 uvColor = tex2D (_UVTex, IN.uv_YTex).rgb;
			float y = tex2D(_YTex,IN.uv_YTex).a;
			float u = uvColor.g - 0.5;
			float v = uvColor.r - 0.5;

			float r = y + 1.13983 * v;
			float g = y - 0.39465 * u - 0.58060 * v;
			float b = y + 2.03211 * u;

			o.Albedo = fixed3(r,g,b);
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}