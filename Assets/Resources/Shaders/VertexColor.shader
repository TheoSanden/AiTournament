Shader "Common/VertexColor" 
{
	Properties 
	{
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		half _Glossiness;
		half _Metallic;

		struct Input
		{
			float4 vertexColor : COLOR;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			o.Albedo = IN.vertexColor;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
