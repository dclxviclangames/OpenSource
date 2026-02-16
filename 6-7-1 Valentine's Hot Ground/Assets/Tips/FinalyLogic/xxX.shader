Shader "Custom/ViralSciFiClubShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (0.1, 0.1, 0.1, 1)
        _RainbowSpeed ("Rainbow Speed", Float) = 0.5
        _GridScale ("Grid Scale", Float) = 20.0
        _AudioIntensity ("Audio Intensity", Float) = 0.0
        _PulsePower ("Pulse Power", Float) = 0.2
        [HDR] _EmissionColor ("Emission Strength", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
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
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _RainbowSpeed;
            float _GridScale;
            float _AudioIntensity;
            float _PulsePower;
            float4 _EmissionColor;

            // Функция для генерации радуги
            float3 spectral_rainbow(float t)
            {
                float3 c = float3(0.5, 0.5, 0.5) + float3(0.5, 0.5, 0.5) * cos(6.28318 * (float3(1,1,1) * t + float3(0.0, 0.33, 0.67)));
                return c;
            }

            v2f vert (appdata v)
            {
                v2f o;
                // 1. ВЕРТЕКСНОЕ СМЕЩЕНИЕ (Audio Pulse)
                // Смещаем вершины по нормалям в зависимости от громкости музыки
                float displacement = _AudioIntensity * _PulsePower;
                v.vertex.xyz += v.normal * displacement;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 2. РАДУЖНЫЙ ЭФФЕКТ (Fresnel + Time)
                 fixed4 texColor = tex2D(_MainTex, i.uv);

    // 2. РАДУЖНЫЙ ЭФФЕКТ (Fresnel + Time)
    float fresnel = 1.0 - saturate(dot(float3(0,1,0), i.viewDir)); 
    float rainbowTime = _Time.y * _RainbowSpeed + fresnel;
    float3 rainbow = spectral_rainbow(rainbowTime);

    // 3. SCI-FI СЕТКА
    float2 gridUV = i.uv * _GridScale;
    float grid = float(step(0.95, frac(gridUV.x)) + step(0.95, frac(gridUV.y)));
    grid = saturate(grid);

    // 4. КОМБИНАЦИЯ
    // Умножаем текстуру на базовый цвет
    float3 finalBase = texColor.rgb * _Color.rgb;
    
    // Свечение: радуга + сетка + влияние звука
    // Мы добавляем это ПОВЕРХ текстуры
    float3 finalEmission = rainbow * (grid + 0.2) * (_AudioIntensity * 5.0 + 0.5) * _EmissionColor.rgb;
    
    // Итоговый цвет: текстура + светящиеся эффекты
    fixed4 col = fixed4(finalBase + finalEmission, texColor.a);
    return col;
            }
            ENDCG
        }
    }
}

