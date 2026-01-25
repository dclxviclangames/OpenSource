Shader "Custom/BackroomsWater"
{
    Properties {
        _PlayerPos ("Player Pos", Vector) = (0,1.2,0,0)
        _Yaw ("Yaw", Float) = 0
    }
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float3 _PlayerPos;
            float _Yaw;

            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (float4 vertex : POSITION, float2 uv : TEXCOORD0) {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                o.uv = uv * 2.0 - 1.0;
                return o;
            }

            // Бесконечные стены Backrooms
            float map(float3 p) {
                float3 q = p;
                q.xz = (frac(p.xz * (1.0/6.0)) - 0.5) * 6.0;
                float wall = length(max(abs(q.xz) - 0.5, 0.0)) - 0.1;
                float floor = p.y; // Пол на y=0
                float ceiling = 3.0 - p.y; // Потолок на y=3
                return min(wall, min(floor, ceiling));
            }

            float3 getNormal(float3 p) {
                float2 e = float2(0.01, 0);
                return normalize(float3(map(p+e.xyy)-map(p-e.xyy), map(p+e.yxy)-map(p-e.yxy), map(p+e.yyx)-map(p-e.yyx)));
            }

            fixed4 frag (v2f i) : SV_Target {
                float2x2 rot = float2x2(cos(_Yaw), -sin(_Yaw), sin(_Yaw), cos(_Yaw));
                float3 rd = normalize(float3(i.uv, 1.2));
                rd.xz = mul(rd.xz, rot);
                float3 ro = _PlayerPos;

                float t = 0;
                for(int j=0; j<80; j++) {
                    float d = map(ro + rd * t);
                    if(d < 0.001 || t > 40.0) break;
                    t += d;
                }

                float3 p = ro + rd * t;
                float3 n = getNormal(p);
                
                // Цвет Backrooms: желтые стены, белый потолок
                float3 col = float3(0.8, 0.7, 0.3); 
                if(p.y > 2.8) col = float3(0.9, 0.9, 0.8); // Потолок
                
                // Эффект воды на полу
                if(p.y < 0.05) {
                    float waves = sin(p.x * 4.0 + _Time.y) * cos(p.z * 4.0 + _Time.y) * 0.02;
                    col = float3(0.0, 0.3, 0.4) + waves; // Цвет воды
                    // Простое отражение: инвертируем луч и смотрим "вверх"
                    float3 reflRd = reflect(rd, float3(0, 1, 0));
                    col += float3(0.2, 0.2, 0.1) * 0.5; 
                }

                float fog = exp(-0.08 * t); // Густой "офисный" туман
                return fixed4(col * fog, 1.0);
            }
            ENDCG
        }
    }
}
