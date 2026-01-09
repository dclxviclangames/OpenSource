Shader "Custom/CyborgVision"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlitchIntensity ("Glitch Intensity", Range(0.0, 1.0)) = 0.0
        _AberrationIntensity ("Aberration Intensity", Range(0.0, 1.0)) = 0.5
        _VisionColor ("Vision Tint Color", Color) = (1.0, 1.0, 1.0, 1.0) // Сделали белым по умолчанию
        _VisionColorIntensity ("Vision Tint Intensity", Range(0.0, 1.0)) = 0.0 // Новый параметр для контроля тонирования
        _TimeScale ("Time Scale (Internal)", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc" 

            sampler2D _MainTex;
            float _GlitchIntensity;
            float _AberrationIntensity;
            float4 _VisionColor;
            float _VisionColorIntensity; // Новая переменная
            float _TimeScale;

            float4 frag(v2f_img input) : SV_Target
            {
                float2 uv = input.uv;

                // --- 1. Эффект Глитча (смещение UV по Y) ---
                float glitchNoise = sin(uv.y * 50.0 + _TimeScale) * 0.1;
                glitchNoise = floor(glitchNoise * (10.0 * _GlitchIntensity)) / 10.0;
                uv.x += glitchNoise * 0.05 * _GlitchIntensity;

                // --- 2. Хроматическая аберрация ---
                float2 uvR = uv + float2(_AberrationIntensity * 0.01, 0.0);
                float2 uvG = uv; 
                float2 uvB = uv - float2(_AberrationIntensity * 0.01, 0.0);

                float r = tex2D(_MainTex, uvR).r;
                float g = tex2D(_MainTex, uvG).g;
                float b = tex2D(_MainTex, uvB).b;
                
                // Собираем цвет из каналов, уже содержащих аберрацию и глитч
                float4 originalColorWithEffects = float4(r, g, b, 1.0);

                // --- 3. Тонирование (теперь опциональное) ---
                // Создаем яркость для опционального тонирования
                float brightness = (r + g + b) / 3.0;
                float4 tintedColor = float4(brightness * _VisionColor.rgb, 1.0);

                // Смешиваем оригинальный цвет с эффектами и тонированный цвет
                // Используем _VisionColorIntensity для контроля силы тонирования
                float4 finalColor = lerp(originalColorWithEffects, tintedColor, _VisionColorIntensity);

                return finalColor;
            }
            ENDCG
        }
    }
}


