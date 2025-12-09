using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using Photon.Pun;
using System;

/// <summary>
/// Client responsible for communicating with the external API (Cloudflare Worker).
/// Uses UnityWebRequest for asynchronous loading and saving of player data.
/// Requires SkinManager and PlayerSkinSaveSnapshot/SkinData classes to function.
/// </summary>
public class InventoryAPIClient : MonoBehaviour
{
    public static InventoryAPIClient Instance;

    [Header("API Configuration")]
    [Tooltip("Base URL of your Cloudflare Worker (e.g., https://my-game-api.workers.dev/api/inventory)")]
    public string apiBaseUrl = "https://your-cloudflare-worker-url.workers.dev/api/inventory"; 
    
    [Tooltip("Secret token for the Authorization header (must match the token in the Cloudflare Worker)")]
    public string apiAuthToken = "your_secret_game_token_123"; 

    private const string LoadEndpoint = "/load/"; 
    private const string SaveEndpoint = "/save/"; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initiates loading of player inventory data from the server.
    /// Called by GameManager after joining a room.
    /// </summary>
    public void LoadPlayerData(string playerId)
    {
        if (SkinManager.Instance == null)
        {
            Debug.LogError("SkinManager Instance not found. Cannot load data.");
            return;
        }

        string fullUrl = apiBaseUrl + LoadEndpoint + playerId;
        StartCoroutine(SendGetRequest(fullUrl, playerId));
    }

    /// <summary>
    /// Saves the current player inventory data to the server.
    /// Called by SkinManager after any purchase or change (skin/coins).
    /// </summary>
    public void SaveCurrentPlayerData()
    {
        if (SkinManager.Instance == null || PhotonNetwork.LocalPlayer == null || !PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Cannot save data: Not in room or managers not ready.");
            return;
        }
        
        string playerId = PhotonNetwork.LocalPlayer.UserId;
        string fullUrl = apiBaseUrl + SaveEndpoint + playerId;
        
        // Get the data object to save
        PlayerSkinSaveSnapshot dataToSave = SkinManager.Instance.GetCurrentSkinSaveData();
        string jsonPayload = JsonUtility.ToJson(dataToSave);
        
        StartCoroutine(SendPostRequest(fullUrl, playerId, jsonPayload));
    }

    // --- HTTP REQUEST LOGIC ---

    private IEnumerator SendGetRequest(string url, string playerId)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("Authorization", "Bearer " + apiAuthToken);
            webRequest.timeout = 10; 

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[API] Data loaded successfully for player {playerId}.");
                
                try
                {
                    // Deserialize and apply data
                    PlayerSkinSaveSnapshot loadedData = JsonUtility.FromJson<PlayerSkinSaveSnapshot>(webRequest.downloadHandler.text);
                    SkinManager.Instance.LoadAndApplySkinData(loadedData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[API] Failed to parse JSON for player {playerId}: {e.Message}");
                    // Fallback to default if parsing fails
                    SkinManager.Instance.LoadAndApplySkinData(null); 
                }
            }
            else if (webRequest.responseCode == 404)
            {
                // Save not found - initialize default data
                Debug.LogWarning($"[API] No existing save found for player {playerId}. Initializing default data.");
                SkinManager.Instance.LoadAndApplySkinData(null); 
            }
            else
            {
                // Critical load error - use default
                Debug.LogError($"[API] Load failed for player {playerId}. Error: {webRequest.error}, Code: {webRequest.responseCode}");
                SkinManager.Instance.LoadAndApplySkinData(null); 
            }
        }
    }

    private IEnumerator SendPostRequest(string url, string playerId, string jsonPayload)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + apiAuthToken);
            webRequest.timeout = 10; 

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[API] Data saved successfully for player {playerId}.");
            }
            else
            {
                Debug.LogError($"[API] Save failed for player {playerId}. Error: {webRequest.error}, Code: {webRequest.responseCode}");
            }
        }
    }
}
