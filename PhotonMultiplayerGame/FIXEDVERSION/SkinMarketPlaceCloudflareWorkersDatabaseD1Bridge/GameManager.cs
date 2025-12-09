using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using Hashtable = ExitGames.Client.Photon.Hashtable; 
using UnityEngine.SceneManagement;

/// <summary>
/// Manages Photon connection, player spawning,
/// respawning, and overall game session logic.
/// Must be located in the game scene.
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Tooltip("List of all available characters. Must match Menu.")]
    public List<PlayerCharacterData> allPlayerCharacters = new List<PlayerCharacterData>();

    [Tooltip("List of possible player spawn points.")]
    public List<Transform> spawnPoints;

    [Tooltip("Delay time before respawning a player after death.")]
    public float respawnDelay = 3f;

    // Dictionary for quick access to spawned player objects by their PhotonPlayer
    private Dictionary<Player, GameObject> spawnedPlayers = new Dictionary<Player, GameObject>();

    // Reference to the local player's game object.
    private GameObject localPlayerGameObject;
    
    // Flag indicating if inventory data has been loaded.
    private bool isInventoryDataLoaded = false;

    // --- Ваши оригинальные переменные для синхронизации состояния игры ---
    public int enemyRambleCondition = 0;
    public int crystalCondition = 0;
    public float bossHealth = 0;
    public float bossHealt = 0;
    public int fleeEnemy = 0;
    public float truckHealth = 0;
    // -------------------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    // --- PHOTON CALLBACKS ---

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("GameManager: Joined room. Starting data load.");
        
        // 1. Начинаем загрузку данных инвентаря с сервера
        if (InventoryAPIClient.Instance != null && PhotonNetwork.LocalPlayer != null)
        {
            // Используем Photon UserID как ключ для сохранения
            InventoryAPIClient.Instance.LoadPlayerData(PhotonNetwork.LocalPlayer.UserId);
        }
        else
        {
            Debug.LogError("InventoryAPIClient not found. Spawning player immediately with default skin.");
            // Если клиент API отсутствует, сразу спавним игрока, но это может вызвать race condition.
            SpawnLocalPlayer(Menu.SelectedCharacterKey);
        }
    }

    /// <summary>
    /// Called by SkinManager when inventory data is successfully loaded/initialized.
    /// </summary>
    public void OnInventoryLoadCompleted()
    {
        if (isInventoryDataLoaded) return; // Protection against repeat calls
        
        isInventoryDataLoaded = true;
        Debug.Log("GameManager: Inventory data loaded/initialized. Spawning player.");
        
        // 2. Spawn the player only after inventory is loaded
        SpawnLocalPlayer(Menu.SelectedCharacterKey);
    }


    /// <summary>
    /// Spawns the local player in the scene.
    /// </summary>
    private void SpawnLocalPlayer(string characterPropertyKey)
    {
        if (localPlayerGameObject != null)
        {
            Debug.LogWarning("Local player already spawned.");
            return;
        }

        Player localPlayer = PhotonNetwork.LocalPlayer;
        if (localPlayer.CustomProperties.TryGetValue(characterPropertyKey, out object charNameObject) && charNameObject is string charName)
        {
            PlayerCharacterData characterData = allPlayerCharacters.FirstOrDefault(c => c.characterName == charName);

            if (characterData != null)
            {
                Transform spawnPoint = GetRandomSpawnPoint();
                
                // Spawn the player using the CHARACTER prefab name
                localPlayerGameObject = PhotonNetwork.Instantiate(
                    characterData.characterPrefab.name, 
                    spawnPoint.position, 
                    spawnPoint.rotation
                );

                // 3. Update visuals after spawning, using the active skin from SkinManager
                string activeSkinId = SkinManager.Instance.GetActiveSkinId();
                UpdatePlayerVisuals(localPlayer, activeSkinId);
                
                spawnedPlayers[localPlayer] = localPlayerGameObject;
                Debug.Log($"Spawned local player {localPlayer.NickName} with character {charName} and skin {activeSkinId}.");

            }
            else
            {
                Debug.LogError($"Character data not found for name: {charName}");
            }
        }
        else
        {
            Debug.LogError($"Player {localPlayer.NickName} has no character property set.");
        }
    }
    
    /// <summary>
    /// Public method to handle skin visual changes for any player (local or remote).
    /// Called by SkinManager.OnPlayerPropertiesUpdate.
    /// </summary>
    /// <param name="targetPlayer">The player whose visual needs updating.</param>
    /// <param name="skinId">The ID of the skin to apply.</param>
    public void UpdatePlayerVisuals(Player targetPlayer, string skinId)
    {
        if (!spawnedPlayers.TryGetValue(targetPlayer, out GameObject playerObj))
        {
            Debug.LogWarning($"Cannot update visuals: Player object not found for {targetPlayer.NickName}.");
            return;
        }

        // 1. Find skin data
        SkinData skinData = SkinManager.Instance.GetSkinData(skinId);

        if (skinData == null || skinData.playerVisualPrefab == null)
        {
            Debug.LogError($"Skin data or visual prefab not found for ID: {skinId}");
            return;
        }

        // 2. Find the visual container (assuming it is named "VisualContainer")
        Transform visualContainer = playerObj.transform.Find("VisualContainer");
        if (visualContainer == null)
        {
            Debug.LogError($"VisualContainer not found on player object {playerObj.name}. Cannot apply skin. Please add an empty child object named 'VisualContainer' to your player prefab.");
            return;
        }

        // 3. Clear old model
        foreach (Transform child in visualContainer)
        {
            Destroy(child.gameObject);
        }

        // 4. Instantiate the new skin model
        GameObject newVisual = Instantiate(skinData.playerVisualPrefab, visualContainer);
        newVisual.transform.localPosition = Vector3.zero;
        newVisual.transform.localRotation = Quaternion.identity;
        
        Debug.Log($"Visuals updated for {targetPlayer.NickName} to skin ID: {skinId}");
    }
    

    // --- SPAWN AND RESPAWN ---

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points assigned.");
            return transform; // Fallback to GameManager position
        }
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }

    public void StartRespawn(GameObject deadPlayerObject, float delay)
    {
        StartCoroutine(RespawnCoroutine(deadPlayerObject, delay));
    }

    private IEnumerator RespawnCoroutine(GameObject deadPlayerObject, float delay)
    {
        // 1. Wait for the death sequence
        yield return new WaitForSeconds(delay);

        // 2. Destroy the dead player object
        PhotonNetwork.Destroy(deadPlayerObject);

        // 3. Respawn (This uses the PhotonPlayerCharProperty set previously)
        SpawnLocalPlayer(Menu.SelectedCharacterKey);
    }
    
    // --- PHOTON SYNCHRONIZATION (ORIGINAL CODE) ---

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Master client sends the state
            stream.SendNext(enemyRambleCondition);
            stream.SendNext(crystalCondition);
            stream.SendNext(bossHealth);
            stream.SendNext(bossHealt);
            stream.SendNext(fleeEnemy);
            stream.SendNext(truckHealth);
        }
        else
        {
            // Other clients receive the state
            enemyRambleCondition = (int)stream.ReceiveNext();
            crystalCondition = (int)stream.ReceiveNext();
            bossHealth = (float)stream.ReceiveNext();
            bossHealt = (float)stream.ReceiveNext();
            fleeEnemy = (int)stream.ReceiveNext();
            truckHealth = (float)stream.ReceiveNext();
        }
    }

    void Update()
    {
        // ... (Your original Update logic remains here)
        if (PhotonNetwork.InRoom && PhotonNetwork.LocalPlayer != null && localPlayerGameObject != null && Input.GetKeyDown(KeyCode.K))
        {
            PlayerHealth targetPlayerHealth = localPlayerGameObject.GetComponent<PlayerHealth>();
            if (targetPlayerHealth != null)
            {
                Debug.Log($"[GameManager.Update] K pressed. Applying 10 damage to local player {PhotonNetwork.LocalPlayer.NickName}.");
                
                // --- TEST LOGIC FOR SKINS/CURRENCY ---
                if (SkinManager.Instance != null)
                {
                    // Test: Add coins
                    SkinManager.Instance.AddCoins(50);
                    
                    // Test: Try to equip a skin (replace "skin_red" with an ID you actually add in SkinManager)
                    // SkinManager.Instance.EquipSkin("skin_red");
                }
                // --- END TEST LOGIC ---
            }
            else
            {
                Debug.LogWarning("[GameManager.Update] Local player object found, but does not have PlayerHealth for test damage.");
            }
        }
    }
}
