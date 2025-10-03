Shader "Custom/PosterizationShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PosterizeLevels ("Posterize Levels", Range(1, 64)) = 8 // Number of color levels per channel
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _PosterizeLevels;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Posterize each color channel
                col.r = floor(col.r * _PosterizeLevels) / _PosterizeLevels;
                col.g = floor(col.g * _PosterizeLevels) / _PosterizeLevels;
                col.b = floor(col.b * _PosterizeLevels) / _PosterizeLevels;

                return col;
            }
            ENDCG
        }
    }
}
