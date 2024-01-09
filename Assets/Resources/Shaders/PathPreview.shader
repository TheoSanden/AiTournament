Shader "Hidden/PathPreview"
{
	Properties
	{
		_Amount("Amount", Float) = 0.0
	}

		SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "ForceNoShadowCasting" = "True" }
		LOD 200

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		ZTest LEqual
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float _Amount;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(i.color.rgb, i.color.a * _Amount);
			}
			ENDCG
		}

		ZTest Greater

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float _Amount;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(i.color.rgb, i.color.a * _Amount * 0.5f);
			}
			ENDCG
		}
	}
		FallBack "Diffuse"
}
