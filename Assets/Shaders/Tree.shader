Shader "Tony/Tree" {
	Properties {
		_Color("Color", Color) = (0.5, 0.5, 0.5, 1)
		_MainTex("Texture", 2D) = "white" {}
		_Tile("Tile", 2D) = "gray" {}

	}

	SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#pragma surface surf Lambert alpha
		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
			float4 color: Color;
		};
		float4 _Color;
		sampler2D _MainTex;
		sampler2D _Tile;
		void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
			o.Albedo *= _Color.rgb;
			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			screenUV = IN.uv_MainTex;
			screenUV *= float2(16, 12);
			o.Albedo *= tex2D(_Tile, screenUV).rgb * 2;
			o.Alpha = tex2D(_MainTex, IN.uv_MainTex).rgb;
			o.Alpha *= _Color.a;
			o.Alpha *= IN.color.a;
		}
		ENDCG
	}
	Fallback "Sprites/Diffuse"
}