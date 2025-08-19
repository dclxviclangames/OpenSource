using UnityEngine;
using System.Collections; // ��� ������������� Coroutine

/// <summary>
/// ������������� � �������� (������, ������), ������� ������ ����������� �����������,
/// ����� ��� ����������� ����� ������.
/// </summary>
[RequireComponent(typeof(Renderer))] // ������� ������� ���������� Renderer ��� ������ � ����������
public class ObstructingWall : MonoBehaviour
{
    private Renderer wallRenderer;
    private Material runtimeMaterial; // ��������, ������� �� ����� �������� �� ����� ����������
    private Color originalColor;
    private Coroutine fadeCoroutine; // ��� ���������� ������� �������������/����������

    void Awake()
    {
        wallRenderer = GetComponent<Renderer>();
        if (wallRenderer == null)
        {
            Debug.LogWarning("ObstructingWall: Renderer component �� ������ �� ���� GameObject. ������ ����� ��������.", this);
            enabled = false; // ��������� ������, ���� ��� Renderer
            return;
        }

        // �����: ������� ����� ���������, ����� �� ������ ����� �������� ������,
        // ������� ����� �������������� ������� ���������.
        runtimeMaterial = wallRenderer.material;
        originalColor = runtimeMaterial.color;

        // ��������, ��� �������� ������������ ������������.
        // ��� �������� ����� ���������� ��������� �� "Fade" ��� "Transparent".
        SetMaterialRenderingMode(runtimeMaterial, true);
    }

    /// <summary>
    /// ������������� ����� ���������� ��������� ��� ��������� ������������ (Fade/Transparent)
    /// ��� ���������� ��� � ������������ ����� (Opaque).
    /// </summary>
    /// <param name="mat">��������, ������� ����� ��������.</param>
    /// <param name="isTransparent">True, ����� �������� ������������, False ��� ��������������.</param>
    private void SetMaterialRenderingMode(Material mat, bool isTransparent)
    {
        if (mat == null) return;

        if (isTransparent)
        {
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0); // ��������� ������ � Z-����� ��� ���������� ������������
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {
            mat.SetOverrideTag("RenderType", ""); // ���������� ���
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1); // �������� ������ � Z-�����
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1; // -1 �������� ������������ ������� ����������, �������� �������� �� ���������
        }
    }

    /// <summary>
    /// ��������� ������� �������� ��������� ������������ �������.
    /// </summary>
    /// <param name="targetAlpha">�������� ������������ (0.0 - ��������� ����������, 1.0 - ��������� ������������).</param>
    /// <param name="fadeDuration">������������ �������� ������������/��������� � ��������.</param>
    public void SetTransparent(float targetAlpha, float fadeDuration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine); // ������������� ���������� ��������, ���� ��� ����
        }
        fadeCoroutine = StartCoroutine(FadeAlpha(targetAlpha, fadeDuration));
    }

    /// <summary>
    /// ���������� ������ � �������� ��������������.
    /// </summary>
    /// <param name="fadeDuration">������������ �������� ��������� � ��������.</param>
    public void SetOpaque(float fadeDuration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        // fadeCoroutine = StartCoroutine(FadeAlpha(1f, fadeDuration));
        Color currentColor = runtimeMaterial.color;
        float startAlpha = 1f;
        currentColor.a = startAlpha;
        runtimeMaterial.color = currentColor;


    }

    /// <summary>
    /// �������� ��� ������� �������� ��������� ������������.
    /// </summary>
    private IEnumerator FadeAlpha(float targetAlpha, float duration)
    {
        Color currentColor = runtimeMaterial.color;
        float startAlpha = currentColor.a;
        float time = 0;

        // ���� ������� ������������ ���������� �� �������, ��������� ��������
        if (Mathf.Abs(startAlpha - targetAlpha) > 0.01f) // ���������, ���� �� �������� �������
        {
            while (time < duration)
            {
                time += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                currentColor.a = newAlpha;
                runtimeMaterial.color = currentColor;
               // StopCoroutine(fadeCoroutine);
                yield return null;
            }
            
        }

        // ������������� �������� �������� �����
       /* currentColor.a = targetAlpha;
        runtimeMaterial.color = currentColor; */

        // ���� ������ ���� ��������� ������������, ����� �������� ����� ���������� ��� �����������
        if (targetAlpha >= 0.99f) // � ��������� �������� ��� ������ float
        {
            SetMaterialRenderingMode(runtimeMaterial, false);
        }
    }

    void OnDestroy()
    {
        // ������� ���������� � Awake ���������, ����� �������� ������ ������
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}
