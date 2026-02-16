using UnityEngine;

public class AudioToShader : MonoBehaviour
{
    public AudioSource audioSource;
    public Material clubMaterial;
    public float sensitivity = 5.0f;
    public float smoothing = 10.0f;

    private float[] samples = new float[256];
    private float currentIntensity;

    void Update()
    {
        // Получаем данные о спектре
        audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

        // Берем среднюю громкость низких частот (бит)
        float sum = 0;
        for (int i = 0; i < 10; i++) sum += samples[i];
        float targetIntensity = (sum / 10) * sensitivity;

        // Плавно сглаживаем переход
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * smoothing);

        // Отправляем значение в шейдер (убедитесь, что имя переменной совпадает)
        clubMaterial.SetFloat("_AudioIntensity", currentIntensity);
    }
}

