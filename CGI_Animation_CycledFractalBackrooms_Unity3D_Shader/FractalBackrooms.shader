Shader "Custom/ClubLabyrinth_Object"
{
    Properties
    {
        _Speed ("Speed", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Speed;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float3x3 getRotYMat(float a) {
                float c = cos(a);
                float s = sin(a);
                return float3x3(c, 0., s, 0., 1., 0., -s, 0., c);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = _Time.y * _Speed * 0.2; 
                float c, d, m;
                float2 s = _ScreenParams.xy;

                // Используем UV-координаты для эмуляции лучей
                float3 p = float3((2. * i.uv.xy - 1.0), 1.); 
                float3 r = p;
                float3 q = r;

                p = mul(p, getRotYMat(-t)); 
                q.zx += 10. + float2(sin(t), cos(t)) * 3.; 

                // Итерации фрактала
                for (float iter = 1.; iter > 0.; iter -= 0.01) {
                    c = d = 0.; 
                    m = 1.;
                    for (int j = 0; j < 3 ; j++) {
                        r = max(r *= r *= r *= r *= r = fmod(q * m + 1., 2.) - 1., r.yzx);
                        d = max(d, (0.29 - length(r) * 0.6) / m) * 0.8;
                        m *= 1.1;
                    }
                    q += p * d;
                    c = iter;
                    if(d < 1e-5) break;
                }

                float k = dot(r, r + 0.15); 
                float3 col = float3(1., k, k / c) - 0.8; 

                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}
