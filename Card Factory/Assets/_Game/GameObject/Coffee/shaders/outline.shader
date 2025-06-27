Shader "Custom/Outline" {
	Properties {
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTestMask ("ZTest Mask", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTestFill ("ZTest Fill", Float) = 0
		_OutlineColor ("Outline Color", Vector) = (1,1,1,1)
		_OutlineWidth ("Outline Width", Range(0, 10)) = 2
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
}