using UnityEngine;
using System.Runtime.InteropServices;

public class VkBridge : MonoBehaviour
{
    private const string UNITY_OBJECT_NAME = "VKBridge";
    public string CurrentUserId { get; private set; }
    public bool IsUserAuthorized { get; private set; }

    // DllImports для JS
    [DllImport("__Internal")]
    private static extern void GetVkUserId(string unityObjectName);

    [DllImport("__Internal")]
    private static extern void SetVkStorage(string key, string value);

    [DllImport("__Internal")]
    private static extern void ShowVkNativeAd(string adFormat, string unityObjectName);

    [DllImport("__Internal")]
    private static extern void SetVkLeaderBoardScore(string score, string unityObjectName);

    [DllImport("__Internal")]
    private static extern void ShowVkLeaderBoard(string unityObjectName);

    //ShowLeaderBoard
    public void ShowLeaderBoard()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    ShowVkLeaderBoard("VkBridge");
#else
        Debug.Log("[VK] Таблица лидеров имитирована в редакторе.");
        OnLeaderBoardClosed("Simulated");
#endif
    }

    // Вызывается из JSLIB: Пользователь закрыл окно лидеров
    public void OnLeaderBoardClosed(string status)
    {
        Debug.Log($"[C#] Окно таблицы лидеров закрыто. Статус: {status}");
        // Здесь можно включить UI обратно, если вы его отключали
    }

    // Вызывается из JSLIB: Ошибка
    public void OnLeaderBoardError(string errorMsg)
    {
        Debug.LogError($"[C#] Ошибка при показе таблицы лидеров: {errorMsg}");
    }

    // ... (Ваши предыдущие функции: GetVkUserId, ShowVkOrderBox, ShowVkNativeAd и т.д.) ...



    // SetScoreToWorldLeaderboard
    public void SubmitFinalScore(int finalScore)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    // VK Bridge требует, чтобы score был строкой, хотя в JS мы конвертируем в число
    SetVkLeaderBoardScore(finalScore.ToString(), "VkBridge");
#else
        Debug.Log($"[VK] Отправка счета {finalScore} пропущена в редакторе.");
        OnScoreSubmitted("Simulated");
#endif
    }

    // Вызывается из JSLIB: Счет успешно отправлен
    public void OnScoreSubmitted(string status)
    {
        Debug.Log($"[C#] Счет успешно отправлен в таблицу лидеров. Статус: {status}");
    }

    // Вызывается из JSLIB: Ошибка при отправке
    public void OnScoreSubmissionError(string errorMsg)
    {
        Debug.LogError($"[C#] Ошибка при отправке счета в лидерборд: {errorMsg}");
    }

    // NEW VK ADS SYSTEM

    public void ShowInterstitialAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    ShowVkNativeAd("interstitial", "VkBridge");
#else
        Debug.Log("[VK] Интерстишал реклама имитирована в редакторе.");
        OnAdSuccess("interstitial");
#endif
    }

    // НОВЫЙ МЕТОД: для вызова рекламы с наградой
    public void ShowRewardedAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    ShowVkNativeAd("reward", "VkBridge"); // Передаем 'reward'
#else
        Debug.Log("[VK] Реклама с наградой имитирована в редакторе. Выдаем бонус.");
        OnRewardedSuccess("SimulatedReward"); // Имитируем успех
#endif
    }
    // ...

    // VKBridge.cs
    // ... (после OnAdSuccess и OnAdError) ...

    // НОВЫЙ ОБРАБОТЧИК: Вызывается из JSLIB только после успешного просмотра REWARDED-рекламы
    public void OnRewardedSuccess(string rewardType)
    {
        Debug.Log($"[C#] Успешный просмотр REWARDED-рекламы! Выдаем бонус: {rewardType}");
        Progress.Instance.PlayerInfo.bearS = 1;
        Progress.Instance.Save();

        // --- ВАША ЛОГИКА ВЫДАЧИ БОНУСА ---

        // Пример: Добавить 100 монет или восстановить жизнь
        // GameManager.Instance.AddCoins(100); 
        // PlayerStats.Instance.RestoreHealth();

        // ---------------------------------
    }

    // Вызывается из JSLIB: Реклама успешно показана
    public void OnAdSuccess(string adFormat)
    {
        Debug.Log($"[C#] Реклама ({adFormat}) успешно показана. Продолжаем игру.");
        // Тут можно разблокировать награду, если это была реклама с вознаграждением ('reward')
    }

    // Вызывается из JSLIB: Ошибка
    public void OnAdError(string errorMsg)
    {
        Debug.LogError($"[C#] Ошибка показа рекламы: {errorMsg}");
    }

    // ----------------------------------------------------------------------
    // 1. АВТОРИЗАЦИЯ
    // ----------------------------------------------------------------------
    public void StartAuth()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GetVkUserId("VkBridge");
#else
        Debug.Log("VK Auth: Skipped in Editor.");
        CurrentUserId = "TEST_USER_123";
#endif
    }

    // Вызывается из JSLIB
    public void OnVkIdReceived(string userId)
    {
        CurrentUserId = userId;
        IsUserAuthorized = true; //  Устанавливаем статус в true
        Debug.Log($"[C#] Авторизация VK успешна. ID: {userId}");
        // Здесь запускается игра или разблокируется UI
    }

    // Вызывается из JSLIB
    public void OnVkIdError(string errorMsg)
    {
        Debug.LogError($"[C#] Ошибка авторизации VK: {errorMsg}");
    }

    // ----------------------------------------------------------------------
    // 2. СОХРАНЕНИЕ РЕКОРДА
    // ----------------------------------------------------------------------

    public void SaveHighScore(int score)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (string.IsNullOrEmpty(CurrentUserId))
        {
            Debug.LogError("Ошибка: Пользователь не авторизован для сохранения.");
            return;
        }

        // Ключ для сохранения: "user_score_" + CurrentUserId (пример)
        string key = "user_score_" + CurrentUserId;

        // Сохраняем в хранилище VK
        SetVkStorage(key, score.ToString());
        Debug.Log($"[C#] Рекорд {score} отправлен на сохранение в VK.");
#endif

    }

    public bool IsAuthorized()
    {
        return IsUserAuthorized;
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("AdsCan", 0);
        PlayerPrefs.SetInt("AuthCan", 0);
        // Place any cleanup or saving logic here
        Debug.Log("Application is quitting!");
        // For example, save game data:
        // SaveManager.SaveGame(); 
        // Disconnect from a server:
        // NetworkManager.Disconnect();
    }
}