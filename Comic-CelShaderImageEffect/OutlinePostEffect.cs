using UnityEngine;

// [ExecuteInEditMode] позволяет видеть эффект прямо в редакторе
[ExecuteInEditMode]
// Требует компонент Camera для работы
[RequireComponent(typeof(Camera))]
public class OutlinePostEffect : MonoBehaviour
{
    [Tooltip("Материал, использующий шейдер OutlinePostEffectShader.")]
    public Material effectMaterial;

    [Header("Настройки контуров")]
    public Color outlineColor = Color.black; // Цвет контура
    [Range(0.0001f, 0.1f)]
    public float depthThreshold = 0.01f; // Порог для обнаружения контуров по глубине
    [Range(0.01f, 0.9f)]
    public float normalThreshold = 0.1f; // Порог для обнаружения контуров по нормалям

    private Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();

        // Камере нужно отдавать буфер глубины и нормалей для шейдера
        cam.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.DepthNormals;

        // Если материал не назначен, создаем его из шейдера
        if (effectMaterial == null)
        {
            effectMaterial = new Material(Shader.Find("Hidden/OutlinePostEffectShader"));
            if (effectMaterial == null)
            {
                Debug.LogError("OutlinePostEffect: Шейдер 'Hidden/OutlinePostEffectShader' не найден. Убедитесь, что шейдер создан и его название совпадает.");
                enabled = false;
                return;
            }
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (effectMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Передаем параметры в шейдер
        effectMaterial.SetColor("_OutlineColor", outlineColor);
        effectMaterial.SetFloat("_DepthThreshold", depthThreshold);
        effectMaterial.SetFloat("_NormalThreshold", normalThreshold);

        // Применяем шейдер
        Graphics.Blit(source, destination, effectMaterial);
    }

   
}
