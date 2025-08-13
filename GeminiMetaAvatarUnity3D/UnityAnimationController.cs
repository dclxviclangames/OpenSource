using UnityEngine;
using System.Runtime.InteropServices; // Необходим для использования DllImport

public class UnityAnimationController : MonoBehaviour
{
    public Animator animator; // Перетащите сюда компонент Animator вашего персонажа в Inspector

    // Названия триггеров в вашем Animator Controller для Talk и Listen
    private const string TALK_TRIGGER_NAME = "Talk";
    private const string LISTEN_TRIGGER_NAME = "Listen";
    private const string IDLE_TRIGGER_NAME = "Idle";

    // --- Функции, которые Unity будет вызывать в JavaScript (браузере) ---
    // Они должны быть объявлены как extern и импортированы из "__Internal"

    // Вызов функции JS, чтобы имитировать клик по кнопке "Слушать" в HTML
    [DllImport("__Internal")]
    private static extern void JsTriggerListenClick();

    // Вызов функции JS, чтобы запросить AI с заданным текстом
    [DllImport("__Internal")]
    private static extern void JsAskAIWithPrompt(string prompt);

    // Вызов функции JS, чтобы отобразить сообщение в браузере (для отладки)
    [DllImport("__Internal")]
    private static extern void ShowBrowserMessage(string message);

    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("Animator component не найден на этом GameObject. Пожалуйста, назначьте его или добавьте.", this);
        }
    }

    /// <summary>
    /// Вызывает анимацию "Разговор" в Unity. Вызывается из JavaScript.
    /// </summary>
    public void StartTalkAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(TALK_TRIGGER_NAME);
            Debug.Log("Анимация 'Разговор' начата.");
        }
    }

    /// <summary>
    /// Останавливает анимацию "Разговор" и возвращает к состоянию покоя. Вызывается из JavaScript.
    /// </summary>
    public void StopTalkAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(IDLE_TRIGGER_NAME);
            Debug.Log("Анимация 'Разговор' остановлена.");
        }
    }

    /// <summary>
    /// Вызывает анимацию "Прослушивание" в Unity. Вызывается из JavaScript.
    /// </summary>
    public void StartListenAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(LISTEN_TRIGGER_NAME);
            Debug.Log("Анимация 'Прослушивание' начата.");
        }
    }

    /// <summary>
    /// Останавливает анимацию "Прослушивание" и возвращает к состоянию покоя. Вызывается из JavaScript.
    /// </summary>
    public void StopListenAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(IDLE_TRIGGER_NAME);
            Debug.Log("Анимация 'Прослушивание' остановлена.");
        }
    }

    // --- Методы, которые могут быть вызваны ИЗ UNITY, чтобы воздействовать на HTML/JS ---

    /// <summary>
    /// Вызывает клик по кнопке "Слушать" в HTML-странице из Unity.
    /// </summary>
    public void RequestListenFromUnity()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            JsTriggerListenClick();
            ShowBrowserMessage("Unity запросил активацию микрофона (Listen)."); // Для отладки
        }
    }

    /// <summary>
    /// Отправляет текстовый запрос в AI через JavaScript из Unity.
    /// </summary>
    /// <param name="prompt">Текст запроса.</param>
    public void RequestAIFeedbackFromUnity(string prompt)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            JsAskAIWithPrompt(prompt);
            ShowBrowserMessage($"Unity запросил AI с текстом: '{prompt}'"); // Для отладки
        }
    }

    // --- Методы для получения уведомлений ИЗ JavaScript В Unity ---
    // (Эти методы будут вызываться через unityInstance.SendMessage из index.html)

    /// <summary>
    /// Вызывается из JavaScript, когда AI начинает генерировать ответ.
    /// </summary>
    public void OnAIResponseStart()
    {
        Debug.Log("Unity: AI начал генерировать ответ.");
        // Здесь можно активировать анимацию "думает" или "ожидает"
    }

    /// <summary>
    /// Вызывается из JavaScript, когда AI полностью сгенерировал ответ.
    /// </summary>
    public void OnAIResponseEnd()
    {
        Debug.Log("Unity: AI закончил генерировать ответ.");
        // Здесь можно деактивировать анимацию "думает"
    }

    /// <summary>
    /// Вызывается из JavaScript, когда браузер начинает озвучивать текст.
    /// </summary>
    public void OnTTSStart()
    {
        Debug.Log("Unity: Браузер начал озвучивать ответ AI.");
        StartTalkAnimation(); // Активируем анимацию разговора
    }

    /// <summary>
    /// Вызывается из JavaScript, когда браузер заканчивает озвучивать текст.
    /// </summary>
    public void OnTTSEnd()
    {
        Debug.Log("Unity: Браузер закончил озвучивать ответ AI.");
        StopTalkAnimation(); // Останавливаем анимацию разговора
    }
}
