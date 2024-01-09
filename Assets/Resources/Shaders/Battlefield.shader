Shader "Custom/Battlefield"
{
    Properties
    {
        _Grass ("Grass", 2D) = "white" {}
        _Mud ("Mud", 2D) = "white" {}
        _Rock ("Rock", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _Grass;
        sampler2D _Mud;
        sampler2D _Rock;

        struct Input
        {
            float2 uv_Grass;
            float2 uv_Mud;
            float2 uv_Rock;
            float4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 grass = tex2D (_Grass, IN.uv_Grass);
            fixed4 mud = tex2D (_Mud, IN.uv_Mud);
            fixed4 rock = tex2D (_Rock, IN.uv_Rock);
            o.Albedo = grass.rgb * IN.color.r + 
                       mud.rgb * IN.color.g +
                       rock.rgb * IN.color.b;

            o.Metallic = 0.2f;
            o.Smoothness = 0.2f;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
