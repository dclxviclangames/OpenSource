Shader "Unlit/CurvedUnlit"
{ 
	Properties
	{
		//_Color ("Main Color", COLOR) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
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
			//#pragma surface surf Lambert vertex:vert
			// make fog work 
			#pragma multi_compile_fog
				
			#include "CurvedCode.cginc"
			//stupid i 
		//	sampler2D _MainTex;
		//	fixed4 _Color;

		//	struct Input {
		//		float2 uv_MainTex;
		//	};

		//	void surf (Input input, inout SurfaceOutputStandard o) {
		//		fixed4 c = tex2D(_MainTex, input.uv_MainTex) * _Color;
		//		o.Albedo = c.rgb;
		//		o.Alpha = c.a;
		//	}
			ENDCG
		}
	}
}
