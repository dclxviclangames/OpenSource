Shader "Hidden/OutlinePostEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // ���� ������
        _OutlineColor ("Outline Color", Color) = (0,0,0,1) // ���� ������� (������ ������)
        _DepthThreshold ("Depth Threshold", Range(0.0001, 0.1)) = 0.01 // ����� �������� �������
        _NormalThreshold ("Normal Threshold", Range(0.01, 0.9)) = 0.1 // ����� �������� ��������
    }
    SubShader
    {
        // ��������� ������� � ��������� ������, ����������, ��� ��� ��� ����-�������
        ZTest Always Cull Off ZWrite Off Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _ _UNITY_COLORSPACE_GAMMA _UNITY_COLORSPACE_LINEAR

            #include "UnityCG.cginc" // ��� _ScreenParams, UNITY_MATRIX_MVP, � �.�.
           // #include "UnityCG.cginc"
           // #include "UnityPBSLighting.cginc"

            // ��������� �������� � ���������� �� Properties
            sampler2D _MainTex; // ������� ���� ������
            sampler2D _CameraDepthTexture; // ����� ������� ������
            sampler2D _CameraDepthNormalsTexture; // ����� ������������, ���� �������� ��������
            fixed4 _OutlineColor;
            float _DepthThreshold;
            float _NormalThreshold;

            // ��������� ��� ������� ������ ���������� �������
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // ��������� ��� �������� ������ ���������� ������� � ������� ������������
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // ��������� ������: ����������� ��� ����-��������
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // ����������� ������
            fixed4 frag (v2f i) : SV_Target
            {
                // �������� ������� ���� ������� �� _MainTex
                fixed4 col = tex2D(_MainTex, i.uv);

                // �������� �������� ������� � �������� ��� �������� �������
                // DecodeDepthNormal ����������� �������������� ����� �������/�������� � ��������� float ������� � float3 �������
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                half3 normal = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv)); // ���������� _CameraDepthNormalsTexture

                // ���������� �������� ��� ������� �������� ��������
                // _ScreenParams.xy - ��� ������ � ������ ������ � ��������
                float2 offset = 1.0 / _ScreenParams.xy;

                // �������� 4 �������� ������� (�������, ������, �����, ������)
                float depthUp = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + float2(0, offset.y));
                float depthDown = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv - float2(0, offset.y));
                float depthLeft = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv - float2(offset.x, 0));
                float depthRight = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + float2(offset.x, 0));

                half3 normalUp = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + float2(0, offset.y)));
                half3 normalDown = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv - float2(0, offset.y)));
                half3 normalLeft = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv - float2(offset.x, 0)));
                half3 normalRight = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + float2(offset.x, 0)));

                // ��������� ������� ������� � �������� � ��������
                float dDepthX = abs(depthLeft - depthRight);
                float dDepthY = abs(depthUp - depthDown);
                float dNormalX = distance(normalLeft, normalRight); // ���������� ����� ��������� ��������
                float dNormalY = distance(normalUp, normalDown);

                // ����������, �������� �� ������� ������ �������
                // ���� ������� ������� ��� �������� � �������� �������� ��������� �����, �� ��� ����
                bool isOutline = (dDepthX > _DepthThreshold || dDepthY > _DepthThreshold) ||
                                 (dNormalX > _NormalThreshold || dNormalY > _NormalThreshold);

                // ���� ��� ������, ���������� ���� �������, ����� - �������� ���� �������
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
