// C# - AdsManager.cs
using UnityEngine;
using UnityEngine.Advertisements;

// IUnityAdsInitializationListener - для инициализации
// IUnityAdsLoadListener - для загрузки рекламы
// IUnityAdsShowListener - для показа рекламы и обработки награды
public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    // --- ПЕРЕМЕННЫЕ НАСТРОЙКИ ---
    // !!! ЗАМЕНИТЕ В ИНСПЕКТОРЕ НА ВАШИ АКТУАЛЬНЫЕ ID !!!
    [Header("Настройки Unity Ads")]
    [SerializeField] string webglGameId = "ВАШ_WEBGL_GAME_ID"; 
    [SerializeField] string rewardedAdUnitId = "Rewarded_WebGL"; // ID блока Rewarded Video
    
    // Флаг для отладки
    [SerializeField] bool isTestMode = true;

    // --- СОБЫТИЯ ---
    // Событие, которое GameModeManager будет слушать, чтобы дать награду
    public System.Action OnRewardedAdFinished;

    // --- ИНИЦИАЛИЗАЦИЯ ---
    void Start()
    {
        InitializeAds();
    }

    private void InitializeAds()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log($"Инициализация Unity Ads для WebGL с ID: {webglGameId}");
            Advertisement.Initialize(webglGameId, isTestMode, this);
        }
        else
        {
            Debug.LogWarning("Unity Ads: Запущено не на WebGL. Инициализация пропущена.");
        }
    }

    // --- МЕТОД ПОКАЗА РЕКЛАМЫ (вызывается из GameModeManager) ---
    public void ShowRewardedAd()
    {
        Debug.Log($"Попытка показа Rewarded Ad: {rewardedAdUnitId}");
        
        // Сначала пытаемся загрузить рекламу
        Advertisement.Load(rewardedAdUnitId, this);
    }
    
    // --- ОБРАБОТКА ИНИЦИАЛИЗАЦИИ (IUnityAdsInitializationListener) ---
    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads: Инициализация завершена. Можно загружать рекламу.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads: Инициализация провалена: {error} - {message}");
    }
    
    // --- ОБРАБОТКА ЗАГРУЗКИ (IUnityAdsLoadListener) ---
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log($"Unity Ads: Реклама {adUnitId} загружена. Показываем...");
        // Реклама загружена, теперь можно ее показать
        Advertisement.Show(adUnitId, this);
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Unity Ads: Загрузка рекламы {adUnitId} провалена: {error} - {message}");
        // Ошибка: можно предложить игроку попробовать позже
    }
    
    // --- ОБРАБОТКА ПОКАЗА И НАГРАДЫ (IUnityAdsShowListener) ---
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Unity Ads: Показ рекламы {adUnitId} провален: {error} - {message}");
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        Debug.Log("Unity Ads: Показ рекламы начат.");
        // Здесь можно поставить игру на паузу
        Time.timeScale = 0; 
    }

    public void OnUnityAdsShowClick(string adUnitId) {}

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        Time.timeScale = 1; // Снимаем игру с паузы
        
        if (adUnitId.Equals(rewardedAdUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads: Рекламный ролик успешно просмотрен! Выдаем награду.");
            // !!! ВЫДАЧА НАГРАДЫ !!!
            OnRewardedAdFinished?.Invoke(); 
        }
        else
        {
            Debug.LogWarning("Рекламный ролик не завершен, награда не выдается.");
        }
    }
}
