using UnityEngine;
using System.Collections; // Для использования Coroutine

/// <summary>
/// Прикрепляется к объектам (стенам, крышам), которые должны становиться прозрачными,
/// когда они перекрывают обзор игрока.
/// </summary>
[RequireComponent(typeof(Renderer))] // Требует наличия компонента Renderer для работы с материалом
public class ObstructingWall : MonoBehaviour
{
    private Renderer wallRenderer;
    private Material runtimeMaterial; // Материал, который мы будем изменять во время выполнения
    private Color originalColor;
    private Coroutine fadeCoroutine; // Для управления плавным исчезновением/появлением

    void Awake()
    {
        wallRenderer = GetComponent<Renderer>();
        if (wallRenderer == null)
        {
            Debug.LogWarning("ObstructingWall: Renderer component не найден на этом GameObject. Скрипт будет отключен.", this);
            enabled = false; // Отключаем скрипт, если нет Renderer
            return;
        }

        // Важно: создаем копию материала, чтобы не менять общий материал ассета,
        // который может использоваться другими объектами.
        runtimeMaterial = wallRenderer.material;
        originalColor = runtimeMaterial.color;

        // Убедимся, что материал поддерживает прозрачность.
        // Это изменяет режим рендеринга материала на "Fade" или "Transparent".
        SetMaterialRenderingMode(runtimeMaterial, true);
    }

    /// <summary>
    /// Устанавливает режим рендеринга материала для поддержки прозрачности (Fade/Transparent)
    /// или возвращает его в непрозрачный режим (Opaque).
    /// </summary>
    /// <param name="mat">Материал, который нужно изменить.</param>
    /// <param name="isTransparent">True, чтобы включить прозрачность, False для непрозрачности.</param>
    private void SetMaterialRenderingMode(Material mat, bool isTransparent)
    {
        if (mat == null) return;

        if (isTransparent)
        {
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0); // Отключаем запись в Z-буфер для правильной прозрачности
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {
            mat.SetOverrideTag("RenderType", ""); // Сбрасываем тег
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1); // Включаем запись в Z-буфер
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1; // -1 означает использовать очередь рендеринга, заданную шейдером по умолчанию
        }
    }

    /// <summary>
    /// Запускает процесс плавного изменения прозрачности объекта.
    /// </summary>
    /// <param name="targetAlpha">Конечная прозрачность (0.0 - полностью прозрачный, 1.0 - полностью непрозрачный).</param>
    /// <param name="fadeDuration">Длительность анимации исчезновения/появления в секундах.</param>
    public void SetTransparent(float targetAlpha, float fadeDuration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine); // Останавливаем предыдущую анимацию, если она есть
        }
        fadeCoroutine = StartCoroutine(FadeAlpha(targetAlpha, fadeDuration));
    }

    /// <summary>
    /// Возвращает объект к исходной непрозрачности.
    /// </summary>
    /// <param name="fadeDuration">Длительность анимации появления в секундах.</param>
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
    /// Корутина для плавной анимации изменения прозрачности.
    /// </summary>
    private IEnumerator FadeAlpha(float targetAlpha, float duration)
    {
        Color currentColor = runtimeMaterial.color;
        float startAlpha = currentColor.a;
        float time = 0;

        // Если целевая прозрачность отличается от текущей, запускаем анимацию
        if (Mathf.Abs(startAlpha - targetAlpha) > 0.01f) // Проверяем, есть ли значимая разница
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

        // Устанавливаем конечное значение точно
       /* currentColor.a = targetAlpha;
        runtimeMaterial.color = currentColor; */

        // Если объект стал полностью непрозрачным, можно сбросить режим рендеринга для оптимизации
        if (targetAlpha >= 0.99f) // С небольшим допуском для ошибок float
        {
            SetMaterialRenderingMode(runtimeMaterial, false);
        }
    }

    void OnDestroy()
    {
        // Очистка созданного в Awake материала, чтобы избежать утечек памяти
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}
