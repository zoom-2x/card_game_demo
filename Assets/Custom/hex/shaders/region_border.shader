Shader "CardGame/hex_region_border"
{
    Properties
    {
        _border_color("Border color", Color) = (1.0, 1.0, 1.0, 1.0)
        _color_hdr("HDR", float) = 1.0
    }

    SubShader
    {
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Cull Back 
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets\Custom\shaders\common\math.cginc"

            float4 _border_color;
            float _color_hdr;

            struct MeshData
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv0 : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: TEXCOORD1;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv0;
                o.normal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                float3 hdr = {_color_hdr, _color_hdr, _color_hdr};
                return _border_color * float4(hdr, 1);
            }

            ENDCG
        }
    }
}

