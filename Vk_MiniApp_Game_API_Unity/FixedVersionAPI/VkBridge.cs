using UnityEngine;
using System.Runtime.InteropServices;

public class VkBridge : MonoBehaviour
{
    private const string UNITY_OBJECT_NAME = "VKBridge";
    public string CurrentUserId { get; private set; }
    public bool IsUserAuthorized { get; private set; }

    // DllImports ��� JS
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
        Debug.Log("[VK] ������� ������� ����������� � ���������.");
        OnLeaderBoardClosed("Simulated");
#endif
    }

    // ���������� �� JSLIB: ������������ ������ ���� �������
    public void OnLeaderBoardClosed(string status)
    {
        Debug.Log($"[C#] ���� ������� ������� �������. ������: {status}");
        // ����� ����� �������� UI �������, ���� �� ��� ���������
    }

    // ���������� �� JSLIB: ������
    public void OnLeaderBoardError(string errorMsg)
    {
        Debug.LogError($"[C#] ������ ��� ������ ������� �������: {errorMsg}");
    }

    // ... (���� ���������� �������: GetVkUserId, ShowVkOrderBox, ShowVkNativeAd � �.�.) ...



    // SetScoreToWorldLeaderboard
    public void SubmitFinalScore(int finalScore)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    // VK Bridge �������, ����� score ��� �������, ���� � JS �� ������������ � �����
    SetVkLeaderBoardScore(finalScore.ToString(), "VkBridge");
#else
        Debug.Log($"[VK] �������� ����� {finalScore} ��������� � ���������.");
        OnScoreSubmitted("Simulated");
#endif
    }

    // ���������� �� JSLIB: ���� ������� ���������
    public void OnScoreSubmitted(string status)
    {
        Debug.Log($"[C#] ���� ������� ��������� � ������� �������. ������: {status}");
    }

    // ���������� �� JSLIB: ������ ��� ��������
    public void OnScoreSubmissionError(string errorMsg)
    {
        Debug.LogError($"[C#] ������ ��� �������� ����� � ���������: {errorMsg}");
    }

    // NEW VK ADS SYSTEM

    public void ShowInterstitialAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    ShowVkNativeAd("interstitial", "VkBridge");
#else
        Debug.Log("[VK] ����������� ������� ����������� � ���������.");
        OnAdSuccess("interstitial");
#endif
    }

    // ����� �����: ��� ������ ������� � ��������
    public void ShowRewardedAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    ShowVkNativeAd("reward", "VkBridge"); // �������� 'reward'
#else
        Debug.Log("[VK] ������� � �������� ����������� � ���������. ������ �����.");
        OnRewardedSuccess("SimulatedReward"); // ��������� �����
#endif
    }
    // ...

    // VKBridge.cs
    // ... (����� OnAdSuccess � OnAdError) ...

    // ����� ����������: ���������� �� JSLIB ������ ����� ��������� ��������� REWARDED-�������
    public void OnRewardedSuccess(string rewardType)
    {
        Debug.Log($"[C#] �������� �������� REWARDED-�������! ������ �����: {rewardType}");
        Progress.Instance.PlayerInfo.bearS = 1;
        Progress.Instance.Save();

        // --- ���� ������ ������ ������ ---

        // ������: �������� 100 ����� ��� ������������ �����
        // GameManager.Instance.AddCoins(100); 
        // PlayerStats.Instance.RestoreHealth();

        // ---------------------------------
    }

    // ���������� �� JSLIB: ������� ������� ��������
    public void OnAdSuccess(string adFormat)
    {
        Debug.Log($"[C#] ������� ({adFormat}) ������� ��������. ���������� ����.");
        // ��� ����� �������������� �������, ���� ��� ���� ������� � ��������������� ('reward')
    }

    // ���������� �� JSLIB: ������
    public void OnAdError(string errorMsg)
    {
        Debug.LogError($"[C#] ������ ������ �������: {errorMsg}");
    }

    // ----------------------------------------------------------------------
    // 1. �����������
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

    // ���������� �� JSLIB
    public void OnVkIdReceived(string userId)
    {
        CurrentUserId = userId;
        IsUserAuthorized = true; //  ������������� ������ � true
        Debug.Log($"[C#] ����������� VK �������. ID: {userId}");
        // ����� ����������� ���� ��� �������������� UI
    }

    // ���������� �� JSLIB
    public void OnVkIdError(string errorMsg)
    {
        Debug.LogError($"[C#] ������ ����������� VK: {errorMsg}");
    }

    // ----------------------------------------------------------------------
    // 2. ���������� �������
    // ----------------------------------------------------------------------

    public void SaveHighScore(int score)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (string.IsNullOrEmpty(CurrentUserId))
        {
            Debug.LogError("������: ������������ �� ����������� ��� ����������.");
            return;
        }

        // ���� ��� ����������: "user_score_" + CurrentUserId (������)
        string key = "user_score_" + CurrentUserId;

        // ��������� � ��������� VK
        SetVkStorage(key, score.ToString());
        Debug.Log($"[C#] ������ {score} ��������� �� ���������� � VK.");
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