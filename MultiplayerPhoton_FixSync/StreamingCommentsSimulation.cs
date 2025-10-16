// StreamingCommentsSimulator.cs
// StreamingCommentsSimulator.cs
using UnityEngine;
using UnityEngine.UI; // Для ScrollRect, LayoutRebuilder
using TMPro; // Для TextMeshProUGUI
using System.Collections; // Для корутин
using System.Text; // Для StringBuilder
using System.Linq; // Для Random
using System.Collections.Generic;
using UnityEngine.EventSystems; // Может понадобиться для некоторых UI-операций, но не обязательно здесь

/// <summary>
/// Симулирует поток стриминговых комментариев в Scroll View.
/// Автоматически генерирует комментарии и прокручивает Scroll View вниз.
/// </summary>
public class StreamingCommentsSimulaton : MonoBehaviour
{
    [Header("UI Элементы")]
   // [Tooltip("Ссылка на компонент TextMeshProUGUI внутри Content Scroll View.")]
   // public TextMeshProUGUI chatContentText;

    [Tooltip("Ссылка на сам компонент ScrollRect.")]
    public ScrollRect chatScrollRect;

    [Header("Настройки комментариев")]
    [Tooltip("Массив строк, из которых будут генерироваться случайные комментарии.")]
    [TextArea(3, 10)] // Делает поле многострочным в инспекторе
    public string[] randomCommentTemplates;

    [Tooltip("Минимальный интервал между комментариями (в секундах).")]
    public float minCommentInterval = 1f;

    [Tooltip("Максимальный интервал между комментариями (в секундах).")]
    public float maxCommentInterval = 3f;

    [Tooltip("Максимальное количество комментариев, которое будет храниться в чате.")]
    public int maxChatLines = 50;

    [Tooltip("Скорость прокрутки Scroll View (больше значение - быстрее прокрутка).")]
    public float scrollSpeed = 2.5f;

    private float timer = 0; // Таймер для отсчета времени до следующего комментария

    public TMP_Text tMP_Text;

    // Список для отслеживания текущих строк чата
    private readonly List<string> currentChatLines = new List<string>();

    void Start()
    {
       
        if (chatScrollRect == null)
        {
            Debug.LogError("StreamingCommentsSimulator: chatScrollRect не назначен! Пожалуйста, перетащите ScrollRect.");
            enabled = false;
            return;
        }
        if (randomCommentTemplates == null || randomCommentTemplates.Length == 0)
        {
            Debug.LogWarning("StreamingCommentsSimulator: randomCommentTemplates пуст. Добавьте несколько шаблонных комментариев.");
            randomCommentTemplates = new string[] { "Привет всем!", "Круто играешь!", "Что происходит?", "Лайк и подписка!", "Я тут новенький!" };
        }

      //  timer = Random.Range(minCommentInterval, maxCommentInterval);
        //chatContentText.text = "";
    }

    void Update()
    {
        

        if (timer > 0.2)
        {
            if (chatScrollRect.verticalNormalizedPosition > 0)
                chatScrollRect.verticalNormalizedPosition = Mathf.Lerp(chatScrollRect.verticalNormalizedPosition, 0f, Time.deltaTime * scrollSpeed);

            if (chatScrollRect.verticalNormalizedPosition < 0)
                chatScrollRect.verticalNormalizedPosition = Mathf.Lerp(chatScrollRect.verticalNormalizedPosition, 1f, Time.deltaTime * scrollSpeed);


            timer = 0;
            // AddRandomComment();
            // timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
           
        }

        // Плавная прокрутка, если мы не на самом низу (только если ScrollRect не прокручивается вручную)
        // Чтобы пользователь мог сам прокрутить вверх и это не сбрасывалось сразу.
        // Вы можете убрать эту часть, если вам нужна только прокрутка при добавлении комментария.
        
    }

    /// <summary>
    /// Генерирует и добавляет случайный комментарий в чат.
    /// </summary>
    private void AddRandomComment()
    {
        
        // Запускаем корутину для прокрутки, чтобы дать UI время на перерисовку
        StartCoroutine(ScrollToBottomDelayed());
    }

    /// <summary>
    /// Генерирует случайный никнейм.
    /// </summary>
    /// <returns>Случайный никнейм.</returns>
   
    /// <summary>
    /// Корутина для прокрутки Scroll View вниз с небольшой задержкой и принудительным обновлением макета.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ScrollToBottomDelayed()
    {
        // Ждем один кадр, чтобы TextMeshProUGUI успел обновить свой размер
        yield return null;

        // === КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ: Принудительное обновление макета ===
        // Это гарантирует, что Content ScrollRect полностью пересчитает свой размер.
        if (chatScrollRect != null && chatScrollRect.content != null)
        {
            // Сначала обновляем все Layout Group на Canvas (если есть)
            Canvas.ForceUpdateCanvases();
            // Затем принудительно перестраиваем макет для Content
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatScrollRect.content.GetComponent<RectTransform>());
           // timer = Random.Range(minCommentInterval, maxCommentInterval);

            // Ждем еще один кадр после принудительной перестройки, чтобы все устаканилось.
            yield return null;

            // Теперь прокручиваем к самому низу
            chatScrollRect.verticalNormalizedPosition = 1f;
            Debug.Log("ScrollToBottomDelayed: Scrolled to bottom.");
        }
        else
        {
            Debug.LogWarning("ScrollToBottomDelayed: chatScrollRect or its content is null, cannot scroll.");
        }
    }
}

