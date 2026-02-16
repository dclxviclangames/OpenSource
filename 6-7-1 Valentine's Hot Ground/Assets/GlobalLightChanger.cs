using UnityEngine;
using System.Collections;

public class GlobalLightChanger : MonoBehaviour
{
    [Header("Настройки света")]
    public Light directionalLight; // Перетащите сюда ваш Directional Light
    public Camera mainCamera;
   // public float transitionDuration = 0.5f; // Длительность смены

    // Sky 
    public float changeSpeed = 2f; // Скорость плавного перехода

    public Material skyMaterial;
    private Color targetSkyColor;
    private Color targetGroundColor;
    public Material skyboxMaterial;

    public float transitionDuration = 2.0f; // Длительность перехода в секундах

    private Coroutine currentLerp;
    void Update()
    {
        // Проверка на нажатие левой кнопки мыши
        if (Input.GetMouseButtonDown(0))
        {
            ChangeWorldColors();
            if (currentLerp != null) StopCoroutine(currentLerp);

            // Генерируем случайные цели
            Color targetColor = new Color(Random.value, Random.value, Random.value);
            float targetAtmosphere = Random.Range(0.5f, 2.0f);
            float targetExposure = Random.Range(0.8f, 1.3f);

            currentLerp = StartCoroutine(LerpSkybox(targetColor, targetAtmosphere, targetExposure));
            //  ChangeSky();
        }
       
            
    }

    IEnumerator LerpSkybox(Color endColor, float endAtmosphere, float endExposure)
    {
        float time = 0;

        // Запоминаем текущие значения как стартовые
        Color startColor = skyboxMaterial.GetColor("_SkyTint");
        float startAtmosphere = skyboxMaterial.GetFloat("_AtmosphereThickness");
        float startExposure = skyboxMaterial.GetFloat("_Exposure");

        while (time < transitionDuration)
        {
            float t = time / transitionDuration;
            // Делаем переход нелинейным (мягкое начало и конец)
            t = t * t * (3f - 2f * t);

            // Плавно меняем параметры
            skyboxMaterial.SetColor("_SkyTint", Color.Lerp(startColor, endColor, t));
            skyboxMaterial.SetFloat("_AtmosphereThickness", Mathf.Lerp(startAtmosphere, endAtmosphere, t));
            skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(startExposure, endExposure, t));

            // Пингуем систему освещения
            DynamicGI.UpdateEnvironment();

            time += Time.deltaTime;
            yield return null;
        }

        // Устанавливаем финальные точные значения
        skyboxMaterial.SetColor("_SkyTint", endColor);
        skyboxMaterial.SetFloat("_AtmosphereThickness", endAtmosphere);
        skyboxMaterial.SetFloat("_Exposure", endExposure);
        DynamicGI.UpdateEnvironment();
    }

    public void ChangeFOV(float targetFOV)
    {
        StopAllCoroutines(); // Останавливаем прошлую смену, если она шла
        StartCoroutine(LerpFOV(targetFOV));
    }

    IEnumerator LerpFOV(float target)
    {
        float startFOV = mainCamera.fieldOfView;
        float time = 0;

        while (time < transitionDuration)
        {
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, target, time / transitionDuration);
            time += Time.deltaTime;
            yield return null;
        }
        mainCamera.fieldOfView = target;
    }

    void ChangeWorldColors()
    {
        // 1. Генерируем случайный цвет
        Color newColor = new Color(
            Random.Range(0.01f, 1f),
            Random.Range(0.01f, 1f),
            Random.Range(0.01f, 1f)
        );

        // 2. Меняем цвет солнца (Directional Light)
        if (directionalLight != null)
        {
            directionalLight.color = newColor;
            // Можно также немного менять интенсивность для динамики
            directionalLight.intensity = Random.Range(1f, 1.5f);
        }

        // 3. Меняем Ambient Light (окружающий свет, который красит тени и небо)
        // Чтобы это работало, в Window > Rendering > Lighting > Environment 
        // Source должен стоять Color или Gradient.
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = newColor * 0.5f; // Делаем тени чуть темнее основного цвета

        // 4. (Опционально) Меняем цвет тумана, чтобы он совпадал с небом
        if (RenderSettings.fog)
        {
            RenderSettings.fogColor = newColor;
        }

       // GameBackgrouund();

        Debug.Log("Атмосфера мира изменена!");
    }

    void ChangeSky()
    {
        if (skyboxMaterial == null) return;

        // 1. Генерируем новый цвет
        Color randomColor = new Color(Random.value, Random.value, Random.value);

        // 2. Меняем параметры шейдера (именно эти имена у Procedural Skybox)
        skyboxMaterial.SetColor("_SkyTint", randomColor);
        skyboxMaterial.SetFloat("_AtmosphereThickness", Random.Range(0.5f, 2.0f));
        skyboxMaterial.SetFloat("_Exposure", Random.Range(0.8f, 1.3f));

        // 3. ПРИНУДИТЕЛЬНО переназначаем скайбокс в настройки сцены
        // Это заставляет Unity перерисовать фон
        RenderSettings.skybox = skyboxMaterial;

        // 4. Обновляем окружающий свет (Ambient)
        DynamicGI.UpdateEnvironment();

        Debug.Log("Skybox Updated!");
    }

    public void GameBackgrouund()
    {
        targetSkyColor = new Color(Random.value, Random.value, Random.value);
        targetGroundColor = new Color(Random.value * 0.5f, Random.value * 0.5f, Random.value * 0.5f);
        Color currentSky = Color.Lerp(skyMaterial.GetColor("_SkyTint"), targetSkyColor, Time.deltaTime * changeSpeed);
        Color currentGround = Color.Lerp(skyMaterial.GetColor("_GroundColor"), targetGroundColor, Time.deltaTime * changeSpeed);

        skyMaterial.SetColor("_SkyTint", currentSky);
        skyMaterial.SetColor("_GroundColor", currentGround);

        // ВАЖНО: Обновляем освещение сцены, чтобы Ambient Light подстроился под новый цвет неба
        DynamicGI.UpdateEnvironment();
    }
}

