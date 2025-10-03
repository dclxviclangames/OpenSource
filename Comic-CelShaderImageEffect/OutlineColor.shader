Shader "Hidden/OutlinePostEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // Кадр экрана
        _OutlineColor ("Outline Color", Color) = (0,0,0,1) // Цвет контура (обычно черный)
        _DepthThreshold ("Depth Threshold", Range(0.0001, 0.1)) = 0.01 // Порог различия глубины
        _NormalThreshold ("Normal Threshold", Range(0.01, 0.9)) = 0.1 // Порог различия нормалей
    }
    SubShader
    {
        // Отключаем глубину и отсечение граней, смешивание, так как это пост-процесс
        ZTest Always Cull Off ZWrite Off Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _ _UNITY_COLORSPACE_GAMMA _UNITY_COLORSPACE_LINEAR

            #include "UnityCG.cginc" // Для _ScreenParams, UNITY_MATRIX_MVP, и т.д.
           // #include "UnityCG.cginc"
           // #include "UnityPBSLighting.cginc"

            // Объявляем текстуры и переменные из Properties
            sampler2D _MainTex; // Текущий кадр экрана
            sampler2D _CameraDepthTexture; // Буфер глубины камеры
            sampler2D _CameraDepthNormalsTexture; // Можно использовать, если доступен отдельно
            fixed4 _OutlineColor;
            float _DepthThreshold;
            float _NormalThreshold;

            // Структура для входных данных вершинного шейдера
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // Структура для выходных данных вершинного шейдера и входных фрагментного
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Вершинный шейдер: стандартный для пост-эффектов
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Фрагментный шейдер
            fixed4 frag (v2f i) : SV_Target
            {
                // Получаем текущий цвет пикселя из _MainTex
                fixed4 col = tex2D(_MainTex, i.uv);

                // Получаем значения глубины и нормалей для текущего пикселя
                // DecodeDepthNormal преобразует закодированный буфер глубины/нормалей в отдельные float глубины и float3 нормали
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                half3 normal = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv)); // Используем _CameraDepthNormalsTexture

                // Определяем смещения для выборки соседних пикселей
                // _ScreenParams.xy - это ширина и высота экрана в пикселях
                float2 offset = 1.0 / _ScreenParams.xy;

                // Выбираем 4 соседних пикселя (верхний, нижний, левый, правый)
                float depthUp = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + float2(0, offset.y));
                float depthDown = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv - float2(0, offset.y));
                float depthLeft = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv - float2(offset.x, 0));
                float depthRight = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + float2(offset.x, 0));

                half3 normalUp = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + float2(0, offset.y)));
                half3 normalDown = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv - float2(0, offset.y)));
                half3 normalLeft = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv - float2(offset.x, 0)));
                half3 normalRight = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + float2(offset.x, 0)));

                // Вычисляем разницу глубины и нормалей с соседями
                float dDepthX = abs(depthLeft - depthRight);
                float dDepthY = abs(depthUp - depthDown);
                float dNormalX = distance(normalLeft, normalRight); // Расстояние между векторами нормалей
                float dNormalY = distance(normalUp, normalDown);

                // Определяем, является ли пиксель частью контура
                // Если разница глубины или нормалей с соседним пикселем превышает порог, то это край
                bool isOutline = (dDepthX > _DepthThreshold || dDepthY > _DepthThreshold) ||
                                 (dNormalX > _NormalThreshold || dNormalY > _NormalThreshold);

                // Если это контур, возвращаем цвет контура, иначе - исходный цвет пикселя
                if (isOutline)
                {
                    return _OutlineColor;
                }
                else
                {
                    return col;
                }
            }
            ENDCG
        }
    }
}
