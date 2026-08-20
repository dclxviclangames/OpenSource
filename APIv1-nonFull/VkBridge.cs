using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class VkBridge : MonoBehaviour
{
    public static VkBridge Instance { get; private set; }

    [Header("Настройки UI")]
 //   [Tooltip("Тег вашей кнопки Лидеров на первой сцене")]
  //  [SerializeField] private string leaderboardButtonTag = "GameController";
    [Tooltip("Тег вашего слайдера громкости на первой сцене")]
    [SerializeField] private string volumeSliderTag = "Finish";

    public string CurrentUserId { get; private set; } = "UNKNOWN";
    public bool IsInitialized { get; private set; } = false;

    private Button leaderboardButton;
    private Slider volumeSlider;

    // Храним громкость, которую выставил пользователь ползунком
    private float userVolume = 1.0f;
    // Флаг, показывается ли сейчас реклама (чтобы не включать звук раньше времени)
    private bool isAdShowing = false;

    // Импорт JS-методов
    [DllImport("__Internal")] private static extern void InitVkBridge();
    [DllImport("__Internal")] private static extern void GetVkUserId();
    [DllImport("__Internal")] private static extern void SetVkStorage(string key, string value);
    [DllImport("__Internal")] private static extern void GetVkStorage(string key);
    [DllImport("__Internal")] private static extern void ShowVkLeaderBoard(int score);
    [DllImport("__Internal")] private static extern void ShowVkNativeAd(string adFormat);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.name = "VkBridge";

            // Загружаем сохраненную громкость из локального кэша, если она есть
            userVolume = PlayerPrefs.GetFloat("SavedVolume", 1.0f);
            AudioListener.volume = userVolume;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        SetupUI();
        InitializeBridge();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupUI();
    }

    private void SetupUI()
    {
        // 1. Настройка кнопки лидеров
       

        // 2. Настройка слайдера громкости (Динамический поиск при смене сцен)
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveAllListeners();
        }

        GameObject sliderObj = GameObject.FindWithTag(volumeSliderTag);
        if (sliderObj != null)
        {
            volumeSlider = sliderObj.GetComponent<Slider>();
            if (volumeSlider != null)
            {
                volumeSlider.value = userVolume; // Ставим ползунок на место
                volumeSlider.onValueChanged.AddListener(HandleSliderValueChanged);
                Debug.Log("[VkBridge] Слайдер громкости успешно подключен.");
            }
        }
        else
        {
            volumeSlider = null; // На игровых сценах слайдера нет, обнуляем ссылку
        }
    }

    private void InitializeBridge()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        InitVkBridge();
#else
        Debug.Log("[VkBridge] Имитация инициализации в редакторе.");
        OnVkInitialized();
        OnVkIdReceived("TEST_PLAYER_777");
#endif
    }

    // --- РЕГУЛИРОВКА ЗВУКА СЛАЙДЕРОМ ---
    private void HandleSliderValueChanged(float value)
    {
        userVolume = value;

        // Изменяем звук только если сейчас не играет реклама
        if (!isAdShowing)
        {
            AudioListener.volume = userVolume;
        }

        PlayerPrefs.SetFloat("SavedVolume", userVolume);
        PlayerPrefs.Save();
    }

    // --- АВТО-ГЛУШЕНИЕ ЗВУКА ПРИ СВОРАЧИВАНИИ ВКЛАДКИ БРАУЗЕРА ---
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            // Игрок вернулся на вкладку. Включаем звук назад, НО только если реклама не идет
            if (!isAdShowing)
            {
                AudioListener.volume = userVolume;
                Debug.Log("[VkBridge] Вкладка активна: вернули звук на " + userVolume);
            }
        }
        else
        {
            // Игрок свернул браузер или ушел на другую вкладку ВК
            AudioListener.volume = 0f;
            Debug.Log("[VkBridge] Вкладка скрыта: полностью заглушили звук.");
        }
    }

    // --- РАБОТА С КНОПКОЙ ЛИДЕРБОРДА ---
    private void OnLeaderboardClick()
    {
        if (leaderboardButton != null) leaderboardButton.interactable = false;
        int scoreToSend = SimpleMenu.HighScore;

#if UNITY_WEBGL && !UNITY_EDITOR
            ShowVkLeaderBoard(scoreToSend);
#else
        Debug.Log($"[VkBridge] Имитация Лидерборда. Счет: {scoreToSend}");
        OnLeaderBoardClosed("Success");
#endif
    }

    // --- МЕТОДЫ ДЛЯ ПОКАЗА РЕКЛАМЫ (С глушением звука) ---
    public void PlayInterstitialAd()
    {
        isAdShowing = true;
        AudioListener.volume = 0f; // Глушим звук игры на время рекламы

#if UNITY_WEBGL && !UNITY_EDITOR
            ShowVkNativeAd("interstitial");
#else
        Debug.Log("[VkBridge] Имитация обычной рекламы.");
        OnAdSuccess("interstitial");
#endif
    }

    public void PlayRewardedAd()
    {
        isAdShowing = true;
        AudioListener.volume = 0f; // Глушим звук игры на время рекламы

#if UNITY_WEBGL && !UNITY_EDITOR
            ShowVkNativeAd("rewarded");
#else
        Debug.Log("[VkBridge] Имитация ревард рекламы.");
        OnRewardedSuccess("Success");
#endif
    }

    // --- ВАШИ ПРИВЫЧНЫЕ МЕТОДЫ-ПЕРЕХОДНИКИ ---
    public void SaveHighScore(int score) { SaveScoreToCloud(score); }
    public void SubmitFinalScore(int score) { OnLeaderboardClick(); }

    public void SaveScoreToCloud(int score)
    {
        if (!IsInitialized) return;
        string key = "user_score_" + CurrentUserId;
#if UNITY_WEBGL && !UNITY_EDITOR
            SetVkStorage(key, score.ToString());
#endif
    }

    public void LoadScoreFromCloud()
    {
        if (!IsInitialized) return;
        string key = "user_score_" + CurrentUserId;
#if UNITY_WEBGL && !UNITY_EDITOR
            GetVkStorage(key);
#endif
    }

    // =============================================================
    //  КОЛБЭКИ ИЗ JAVASCRIPT
    // =============================================================

    public void OnVkInitialized()
    {
        Debug.Log("[VkBridge C#] Мост ВК готов. Запрашиваем ID пользователя...");
#if UNITY_WEBGL && !UNITY_EDITOR
            GetVkUserId();
#endif
    }

    public void OnVkIdReceived(string userId)
    {
        CurrentUserId = userId;
        IsInitialized = true;
        LoadScoreFromCloud();
    }

    public void OnVkIdError(string reason) { IsInitialized = true; }

    public void OnVkStorageLoaded(string loadedValue)
    {
        if (int.TryParse(loadedValue, out int cloudScore))
        {
            if (cloudScore > SimpleMenu.HighScore)
            {
                SimpleMenu.HighScore = cloudScore;
                SimpleMenu menu = FindObjectOfType<SimpleMenu>();
                if (menu != null) menu.UpdateMenuUI(cloudScore);
            }
        }
    }

    public void OnLeaderBoardClosed(string status)
    {
        if (leaderboardButton != null) leaderboardButton.interactable = true;
    }

    // Реклама закрылась успешно
    public void OnAdSuccess(string format)
    {
        isAdShowing = false;
        AudioListener.volume = userVolume; // Возвращаем звук игрока обратно!
        Debug.Log($"[VkBridge C#] Реклама {format} завершена. Звук возвращен.");
    }

    // Награда получена
    public void OnRewardedSuccess(string message)
    {
        isAdShowing = false;
        AudioListener.volume = userVolume; // Возвращаем звук игрока обратно!
        Debug.Log("[VkBridge C#] Награда выдана. Звук возвращен.");
        // ТВОЙ КОД НАГРАДЫ ТУТ
    }

    // Реклама закрылась с ошибкой или не доступна
    public void OnAdError(string reason)
    {
        isAdShowing = false;
        AudioListener.volume = userVolume; // Возвращаем звук даже при ошибке, чтобы игра не была немой
        Debug.LogWarning($"[VkBridge C#] Ошибка рекламы: {reason}. Звук возвращен.");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}

