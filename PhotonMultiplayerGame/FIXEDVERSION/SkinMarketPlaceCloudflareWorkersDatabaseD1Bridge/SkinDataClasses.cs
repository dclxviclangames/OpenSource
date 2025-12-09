using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// ГЛАВНЫЙ КОНТЕЙНЕР ДЛЯ ВСЕГО СОХРАНЕНИЯ. 
/// Содержит только данные, связанные со скинами и валютой.
/// </summary>
[Serializable]
public class PlayerSkinSaveSnapshot
{
    // Хранит ID активного скина (тот, который надет в данный момент)
    public string activeSkinId = "default"; 
    
    // Список ID скинов, которыми владеет игрок
    public List<string> ownedSkins = new List<string> { "default" }; 

    // Валюта игрока
    public int coins = 0; 

    public PlayerSkinSaveSnapshot()
    {
        if (ownedSkins == null)
        {
             ownedSkins = new List<string>();
        }
        // Гарантируем, что у игрока всегда есть дефолтный скин
        if (!ownedSkins.Contains("default"))
        {
            ownedSkins.Add("default");
        }
    }
}

// =========================== КОНЕЦ ФАЙЛА SkinDataClasses.cs ===========================

// ========================= НАЧАЛО ФАЙЛА InventoryAPIClient.cs =========================

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Photon.Pun;
using System;
using System.Collections.Generic;

/// <summary>
/// ГЛАВНЫЙ КЛИЕНТ API. Отправляет и получает объект PlayerSkinSaveSnapshot (сохранение скинов и монет).
/// </summary>
public class InventoryAPIClient : MonoBehaviourPunCallbacks
{
    // Адрес вашего Cloudflare Worker (Замените на свой!)
    private const string BASE_API_URL = "https://mychat.djml378z.workers.dev/api/inventory"; 
    // Токен для аутентификации (Замените на свой!)
    private const string API_AUTH_TOKEN = "your_secret_game_token_123"; 

    public static InventoryAPIClient Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Делаем синглтоном, чтобы был доступен из GameManager
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// СОБИРАЕТ и сохраняет данные игрока на бэкенд.
    /// Вызывается из SkinManager после изменения состояния (покупка/ношение).
    /// </summary>
    public void SaveCurrentPlayerData()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.AuthValues?.UserId))
        {
             Debug.LogError("Cannot save: Photon User ID is not set.");
             return;
        }
        
        // 1. Сбор данных Скинов и Валюты
        if (SkinManager.Instance == null)
        {
            Debug.LogError("SkinManager Instance not found. Cannot save data.");
            return;
        }
        
        // Получаем объект данных для сохранения
        PlayerSkinSaveSnapshot dataToSave = SkinManager.Instance.GetCurrentSkinSaveData();
        
        // 2. Сериализация в JSON-строку
        string dataJson = JsonUtility.ToJson(dataToSave);
        StartCoroutine(SendSaveRequest(PhotonNetwork.AuthValues.UserId, dataJson));
    }

    /// <summary>
    /// Загружает данные игрока с бэкенда и применяет их.
    /// Должен вызываться в GameManager после PhotonNetwork.InRoom.
    /// </summary>
    public void LoadAndApplyPlayerData()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.AuthValues?.UserId))
        {
            Debug.LogError("Cannot load: Photon User ID is not set. Wait for connection.");
            return;
        }
        
        LoadPlayerData((loadedData) =>
        {
            if (SkinManager.Instance == null)
            {
                Debug.LogError("SkinManager Instance not found. Cannot apply data.");
                return;
            }

            if (loadedData != null)
            {
                // Применение данных Скинов и Валюты
                SkinManager.Instance.LoadAndApplySkinData(loadedData);
                Debug.Log("[API] Skin and Coin data successfully loaded and applied.");
            }
            else
            {
                Debug.Log("[API] No save file found. Initializing default skin and 100 coins.");
                
                // Инициализация дефолтных данных для нового игрока (выдается 100 монет)
                PlayerSkinSaveSnapshot defaultData = new PlayerSkinSaveSnapshot { coins = 100 };
                SkinManager.Instance.LoadAndApplySkinData(defaultData); 
            }
        });
    }
    
    // --- COROUTINES (Save and Load) ---

    private IEnumerator SendSaveRequest(string playerID, string dataJson)
    {
        string url = $"{BASE_API_URL}/save/{playerID}";
        
        using (UnityWebRequest request = UnityWebRequest.Put(url, dataJson))
        {
            request.method = "POST"; // Принудительно устанавливаем метод POST
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {API_AUTH_TOKEN}");

            // Отправляем данные в теле запроса
            byte[] bodyRaw = Encoding.UTF8.GetBytes(dataJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API Save Error: {request.error}. Response: {request.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"Player skin/coin data {playerID} successfully saved to server.");
            }
        }
    }

    private void LoadPlayerData(Action<PlayerSkinSaveSnapshot> onComplete)
    {
        string playerID = PhotonNetwork.AuthValues.UserId;
        StartCoroutine(SendLoadRequest(playerID, onComplete));
    }

    private IEnumerator SendLoadRequest(string playerID, Action<PlayerSkinSaveSnapshot> onComplete)
    {
        string url = $"{BASE_API_URL}/load/{playerID}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {API_AUTH_TOKEN}");
            
            yield return request.SendWebRequest();

            PlayerSkinSaveSnapshot loadedData = null;

            if (request.result == UnityWebRequest.Result.ProtocolError && request.responseCode == 404)
            {
                // Сохранение не найдено, это ожидаемое поведение
                Debug.Log("[API Load] No save file found (404). Null data will be passed for default init.");
            }
            else if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API Load Error: {request.error}. Response: {request.downloadHandler.text}");
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                
                try
                {
                    // Парсинг чистого JSON-объекта
                    loadedData = JsonUtility.FromJson<PlayerSkinSaveSnapshot>(jsonResponse);
                }
                catch (Exception e)
                {
                    Debug.LogError($"JSON Parsing Error: {e.Message}. JSON: {jsonResponse}");
                }
            }
            
            onComplete?.Invoke(loadedData);
        }
    }
}

// =========================== КОНЕЦ ФАЙЛА InventoryAPIClient.cs ===========================

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon; // Для Hashtable
using System;

/// <summary>
/// Синглтон-менеджер, который управляет:
/// 1. Состоянием инвентаря (монеты, купленные скины, текущий скин).
/// 2. Логикой покупки и ношения скинов.
/// 3. Синхронизацией активного скина через Custom Properties Photon.
/// 4. Взаимодействием с InventoryAPIClient для сохранения и загрузки.
/// ТРЕБУЕТСЯ: SkinDataClasses.cs и InventoryAPIClient.cs
/// </summary>
public class SkinManager : MonoBehaviourPunCallbacks
{
    public static SkinManager Instance;
    
    [Header("Skin Catalog")]
    // Список всех доступных скинов (заполняется в Инспекторе)
    public List<SkinData> allAvailableSkins = new List<SkinData>();
    
    // --- Текущее состояние игрока ---
    private int _coins;
    // Используем HashSet для быстрого поиска купленных скинов
    private HashSet<string> _ownedSkins = new HashSet<string> { "default" }; 
    private string _activeSkinId = "default";

    // Ключ для Custom Property Photon
    public const string PlayerActiveSkinKey = "SKIN_ID"; 
    
    // --- События для UI (Ваш UI должен подписаться на них!) ---
    public event Action<int> OnCoinsUpdated;
    public event Action<string> OnActiveSkinChanged;

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

    // --- МЕТОДЫ ДОСТУПА К ДАННЫМ ---

    public int GetCoins() => _coins;
    public string GetActiveSkinId() => _activeSkinId;
    public bool IsSkinOwned(string skinId) => _ownedSkins.Contains(skinId);
    public SkinData GetSkinData(string skinId) => allAvailableSkins.FirstOrDefault(s => s.skinId == skinId);

    // --- ЛОГИКА ИНВЕНТАРЯ ---

    /// <summary>
    /// Пытается купить скин. Если успешно, уменьшает монеты, добавляет скин и сохраняет.
    /// </summary>
    public bool TryPurchaseSkin(string skinId)
    {
        SkinData skin = GetSkinData(skinId);

        if (skin == null || IsSkinOwned(skinId) || _coins < skin.price)
        {
            if (skin == null) Debug.LogError($"Skin ID {skinId} not found.");
            if (IsSkinOwned(skinId)) Debug.Log($"Skin {skinId} already owned.");
            if (_coins < skin.price) Debug.Log($"Not enough coins to buy {skinId}.");
            return false;
        }

        // --- Успешная покупка ---
        _coins -= skin.price;
        _ownedSkins.Add(skinId);
        
        OnCoinsUpdated?.Invoke(_coins);
        
        // Автоматически экипируем новый скин
        EquipSkin(skinId); 
        
        // Сохраняем новое состояние на сервер
        if (InventoryAPIClient.Instance != null)
        {
            InventoryAPIClient.Instance.SaveCurrentPlayerData();
        }
        
        Debug.Log($"Purchased and equipped skin {skinId}. Remaining coins: {_coins}");
        return true;
    }

    /// <summary>
    /// Экипирует (надевает) купленный скин.
    /// </summary>
    public void EquipSkin(string skinId)
    {
        if (!IsSkinOwned(skinId))
        {
            Debug.LogWarning($"Cannot equip skin {skinId}: not owned.");
            return;
        }
        
        if (_activeSkinId == skinId) return; // Скин уже активен

        _activeSkinId = skinId;
        OnActiveSkinChanged?.Invoke(skinId);
        
        // Синхронизация скина через Photon Custom Properties
        SetLocalPlayerSkinProperty(skinId);
        
        // Сохраняем новое состояние на сервер
        if (InventoryAPIClient.Instance != null)
        {
            InventoryAPIClient.Instance.SaveCurrentPlayerData();
        }
        
        Debug.Log($"Equipped skin: {skinId}");
    }

    /// <summary>
    /// Добавляет монеты (вызывается при выполнении заданий или убийствах) и сохраняет.
    /// </summary>
    public void AddCoins(int amount)
    {
        _coins += amount;
        OnCoinsUpdated?.Invoke(_coins);
        Debug.Log($"Coins added: {amount}. Total coins: {_coins}");

        // Сохраняем новое состояние
        if (PhotonNetwork.InRoom && InventoryAPIClient.Instance != null)
        {
             InventoryAPIClient.Instance.SaveCurrentPlayerData();
        }
    }

    // --- СИНХРОНИЗАЦИЯ С PHOTON ---

    /// <summary>
    /// Устанавливает Custom Property для локального игрока.
    /// </summary>
    private void SetLocalPlayerSkinProperty(string skinId)
    {
        if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.InRoom)
        {
            Hashtable customProps = new Hashtable();
            customProps[PlayerActiveSkinKey] = skinId;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            Debug.Log($"Photon Skin Property set: {PlayerActiveSkinKey} = {skinId}");
        }
    }

    /// <summary>
    /// Переопределение: вызывается, когда меняются свойства игроков.
    /// GameManager должен прослушивать это, чтобы обновить внешний вид игрока.
    /// </summary>
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        
        if (changedProps.ContainsKey(PlayerActiveSkinKey))
        {
            string newSkinId = (string)changedProps[PlayerActiveSkinKey];
            Debug.Log($"Received skin property update for {targetPlayer.NickName}: {newSkinId}");
            
            if (GameManager.Instance != null)
            {
                // GameManager должен обновить 3D-модель игрока
                GameManager.Instance.UpdatePlayerVisuals(targetPlayer, newSkinId);
            }
        }
    }
    
    // --- СЕРИАЛИЗАЦИЯ ДЛЯ API ---

    /// <summary>
    /// Собирает текущие данные в объект PlayerSkinSaveSnapshot для отправки на сервер.
    /// </summary>
    public PlayerSkinSaveSnapshot GetCurrentSkinSaveData()
    {
        return new PlayerSkinSaveSnapshot
        {
            activeSkinId = _activeSkinId,
            ownedSkins = _ownedSkins.ToList(), // Преобразуем HashSet обратно в List
            coins = _coins
        };
    }

    /// <summary>
    /// Применяет загруженные данные, вызывается InventoryAPIClient.
    /// </summary>
    public void LoadAndApplySkinData(PlayerSkinSaveSnapshot loadedData)
    {
        if (loadedData != null)
        {
            _coins = loadedData.coins;
            _ownedSkins = new HashSet<string>(loadedData.ownedSkins);
            _activeSkinId = loadedData.activeSkinId;
            
            // Если скин, который был активным, почему-то не куплен, ставим дефолтный
            if (!_ownedSkins.Contains(_activeSkinId))
            {
                _activeSkinId = "default";
            }
        }
        else
        {
            // Инициализация дефолтных данных для нового игрока
            _coins = 100; // Стартовый капитал
            _ownedSkins = new HashSet<string> { "default" };
            _activeSkinId = "default";
            
            // И сразу сохраняем дефолтные данные на сервер
            if (InventoryAPIClient.Instance != null)
            {
                 InventoryAPIClient.Instance.SaveCurrentPlayerData();
            }
        }

        // Обновляем UI (если он подписан) и синхронизируем с Photon
        OnCoinsUpdated?.Invoke(_coins);
        OnActiveSkinChanged?.Invoke(_activeSkinId);
        
        // Устанавливаем свойство активного скина для текущей сессии
        SetLocalPlayerSkinProperty(_activeSkinId); 
        
        Debug.Log($"SkinManager initialized. Coins: {_coins}, Active Skin: {_activeSkinId}");
    }
}

// ВНИМАНИЕ: ЗАМЕНИТЕ ЭТОТ ТОКЕН НА ВАШ СЕКРЕТНЫЙ КЛЮЧ
const API_AUTH_TOKEN = "your_secret_game_token_123"; 

export default {
  // Env содержит привязку D1 Database под именем 'DB'
  async fetch(request, env) {
    const url = new URL(request.url);
    const pathSegments = url.pathname.split('/').filter(segment => segment.length > 0);
    
    // --- АУТЕНТИФИКАЦИЯ ---
    if (request.headers.get("Authorization") !== `Bearer ${API_AUTH_TOKEN}`) {
        return new Response("Unauthorized", { status: 401 });
    }

    // Проверка пути /api/inventory/{action}/{playerID}
    if (pathSegments[0] !== 'api' || pathSegments[1] !== 'inventory') {
        return new Response("Not Found", { status: 404 });
    }

    const action = pathSegments[2]; 
    const playerID = pathSegments[3]; 

    if (!playerID) {
        return new Response("Player ID is required.", { status: 400 });
    }

    // --- ЛОГИКА СОХРАНЕНИЯ (POST /save/{playerID}) ---
    if (action === 'save' && request.method === 'POST') {
        try {
            const data = await request.json(); 

            // Валидация полей: coins, activeSkinId, ownedSkins
            if (data.coins === undefined || !data.activeSkinId || !Array.isArray(data.ownedSkins)) {
                 return new Response("Missing required skin/coin fields.", { status: 400 });
            }

            const dataJson = JSON.stringify(data);

            // INSERT OR REPLACE для сохранения или обновления
            const { success } = await env.DB.prepare(
                "INSERT OR REPLACE INTO player_saves (player_id, data_json) VALUES (?1, ?2)"
            )
            .bind(playerID, dataJson)
            .run();

            if (success) {
                return new Response("Save successful", { status: 200 });
            } else {
                return new Response("Save failed", { status: 500 });
            }
        } catch (e) {
            return new Response(`Error processing save request: ${e.message}`, { status: 500 });
        }
    }

    // --- ЛОГИКА ЗАГРУЗКИ (GET /load/{playerID}) ---
    if (action === 'load' && request.method === 'GET') {
        try {
            const { results } = await env.DB.prepare(
                "SELECT data_json FROM player_saves WHERE player_id = ?1"
            )
            .bind(playerID)
            .all();

            if (results.length > 0) {
                // Возвращаем сохраненный JSON
                return new Response(results[0].data_json, {
                    status: 200,
                    headers: { 'Content-Type': 'application/json' }
                });
            } else {
                // 404 для нового игрока (нет сохранения)
                return new Response("No save data found.", { status: 404 });
            }
        } catch (e) {
            return new Response(`Error processing load request: ${e.message}`, { status: 500 });
        }
    }

    return new Response("Invalid API Endpoint or Method", { status: 400 });
  }
};

using UnityEngine;
using System.Collections.Generic;
using System;

// =================================================================================
// 1. PlayerSkinSaveSnapshot: КЛАСС ДАННЫХ ДЛЯ СОХРАНЕНИЯ/ЗАГРУЗКИ С СЕРВЕРА
// (Используется для JSON-сериализации)
// =================================================================================

/// <summary>
/// Структура данных, которая точно соответствует JSON, 
/// который мы сохраняем и загружаем с Cloudflare Worker.
/// </summary>
[Serializable]
public class PlayerSkinSaveSnapshot
{
    // ID активного скина
    public string activeSkinId = "default"; 
    
    // Список строковых ID купленных скинов
    public List<string> ownedSkins = new List<string> { "default" }; 
    
    // Валюта игрока
    public int coins = 0;

    public PlayerSkinSaveSnapshot()
    {
        // Гарантируем наличие дефолтного скина
        if (ownedSkins == null) ownedSkins = new List<string>();
        if (!ownedSkins.Contains("default")) ownedSkins.Add("default");
    }
}

// =================================================================================
// 2. SkinData: КЛАСС МЕТАДАННЫХ СКИНОВ ДЛЯ КАТАЛОГА
// (Заполняется в Инспекторе SkinManager)
// =================================================================================

/// <summary>
/// Класс для описания каждого доступного скина в игре.
/// </summary>
[Serializable]
public class SkinData
{
    // Уникальный ID скина (должен быть уникальным)
    public string skinId = "default";
    
    // Цена скина
    public int price = 0; 
    
    public string displayName = "Standard";
    
    [Tooltip("Префаб/объект, который будет применен к игроку")]
    public GameObject playerVisualPrefab;
}
