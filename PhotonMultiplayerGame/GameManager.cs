/* GameManager.cs
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // For LINQ
using Hashtable = ExitGames.Client.Photon.Hashtable; // Explicitly indicate which Hashtable is used
using UnityEngine.SceneManagement;

/// <summary>
/// Manages Photon connection, player spawning,
/// respawning, and overall game session logic.
/// Must be located in the game scene.
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Tooltip("List of all available characters. Must match PlayerSelectionManager.")]
    public List<PlayerCharacterData> allPlayerCharacters = new List<PlayerCharacterData>();

    [Tooltip("List of possible player spawn points.")]
    public List<Transform> spawnPoints;

    [Tooltip("Delay time before respawning a player after death.")]
    public float respawnDelay = 3f;

    // Dictionary for quick access to spawned player objects by their PhotonPlayer
    private Dictionary<Player, GameObject> spawnedPlayers = new Dictionary<Player, GameObject>();

    // Reference to the local player's game object.
    private GameObject localPlayerGameObject;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // Usually, GameManager in the game scene is not needed in DontDestroyOnLoad
    }

    public void Start()
    {

          PhotonNetwork.JoinLobby();
          if (!PhotonNetwork.IsConnectedAndReady)
          {
              Debug.Log("GameManager: Not connected to Photon, connecting (if this is the first scene or testing).");
              PhotonNetwork.ConnectUsingSettings();
              SceneManager.LoadScene("Menu");
          }
          else
          {
              JoinOrCreateRoom();
          }

          
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("GameManager: Connected to Photon Master Server.");
        JoinOrCreateRoom();
    }
   
    public void OnClickConnect()
    {
            
        PhotonNetwork.ConnectUsingSettings();
        
    }

    public void JoinOrCreateRoom()
    {
        string roomName = "MyGameRoom";
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 8 };
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        Debug.Log($"GameManager: Попытка присоединиться/создать комнату: {roomName}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"GameManager: Joined room: {PhotonNetwork.CurrentRoom.Name}");
        SpawnAllPlayersInRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} entered the room.");
        // Wait for player properties to synchronize to spawn the correct character
        StartCoroutine(WaitForPlayerCustomProperties(newPlayer, PlayerSelectionManager.PhotonPlayerCharProperty, () => SpawnPlayerFor(newPlayer)));
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} left the room. Destroying their object.");
        if (spawnedPlayers.ContainsKey(otherPlayer) && spawnedPlayers[otherPlayer] != null)
        {
            // Photon will automatically destroy network objects belonging to the leaving player,
            // but we can clear our local reference.
            spawnedPlayers.Remove(otherPlayer);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(PlayerSelectionManager.PhotonPlayerCharProperty))
        {
            if (spawnedPlayers.ContainsKey(targetPlayer) && spawnedPlayers[targetPlayer] != null)
            {
                Debug.Log($"Player {targetPlayer.NickName} changed character property to {changedProps[PlayerSelectionManager.PhotonPlayerCharProperty]}. Respawning player.");

                PhotonNetwork.Destroy(spawnedPlayers[targetPlayer]); // Destroy old object
                spawnedPlayers.Remove(targetPlayer);
                SpawnPlayerFor(targetPlayer); // Spawn new
            }
            else
            {
                Debug.Log($"Player {targetPlayer.NickName} updated character properties. Spawning them.");
                SpawnPlayerFor(targetPlayer);
            }
        }
    }

    /// <summary>
    /// Spawns all players currently in the room.
    /// Called when first joining the room.
    /// </summary>
    private void SpawnAllPlayersInRoom()
    {
        // Clear existing player objects to avoid duplicates
        foreach (var entry in spawnedPlayers)
        {
            if (entry.Value != null)
            {
                PhotonNetwork.Destroy(entry.Value);
            }
        }
        spawnedPlayers.Clear();
        localPlayerGameObject = null;

        foreach (Player photonPlayer in PhotonNetwork.CurrentRoom.Players.Values)
        {
            SpawnPlayerFor(photonPlayer);
        }
    }

    /// <summary>
    /// Spawns a character for a specific PhotonPlayer.
    /// </summary>
    /// <param name="photonPlayer">The PhotonPlayer for whom to spawn the character.</param>
    private void SpawnPlayerFor(Player photonPlayer)
    {
        if (spawnedPlayers.ContainsKey(photonPlayer) && spawnedPlayers[photonPlayer] != null)
        {
            Debug.LogWarning($"Player {photonPlayer.NickName} is already spawned. Skipping spawn.");
            return;
        }

        string selectedCharName = null;
        if (photonPlayer.CustomProperties.ContainsKey(PlayerSelectionManager.PhotonPlayerCharProperty))
        {
            selectedCharName = photonPlayer.CustomProperties[PlayerSelectionManager.PhotonPlayerCharProperty] as string;
        }

        PlayerCharacterData selectedCharData = allPlayerCharacters.FirstOrDefault(c => c.name == selectedCharName);

        if (selectedCharData == null)
        {
            Debug.LogWarning($"Player {photonPlayer.NickName} has not selected a character or data not found. Using the first available.");
            selectedCharData = allPlayerCharacters.FirstOrDefault();
            if (selectedCharData == null)
            {
                Debug.LogError("No available characters to spawn! Check the allPlayerCharacters list.");
                return;
            }
        }

        Transform randomSpawnPoint = spawnPoints.Count > 0 ? spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)] : null;
        Vector3 spawnPos = (randomSpawnPoint != null) ? randomSpawnPoint.position : Vector3.zero;
        Quaternion spawnRot = (randomSpawnPoint != null) ? randomSpawnPoint.rotation : Quaternion.identity;

        if (selectedCharData.characterPrefab != null)
        {
            string prefabName = selectedCharData.characterPrefab.name;

            GameObject playerGameObject = PhotonNetwork.Instantiate(prefabName, spawnPos, spawnRot);
            spawnedPlayers[photonPlayer] = playerGameObject;

            Debug.Log($"Player spawned: {selectedCharData.characterName} (Prefab: {prefabName}) for {photonPlayer.NickName}");

            PlayerHealth playerHealth = playerGameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.maxHealth = selectedCharData.baseHealth;
                playerHealth.ResetHealthAndActivate();
            }

            if (photonPlayer.IsLocal)
            {
                localPlayerGameObject = playerGameObject;
                SetupLocalPlayerCamera(playerGameObject.transform);
                PlayerMovement playerMovement = playerGameObject.GetComponent<PlayerMovement>();
                if (playerMovement != null) playerMovement.enabled = true;
            }
            else
            {
                PlayerMovement playerMovement = playerGameObject.GetComponent<PlayerMovement>();
                if (playerMovement != null) playerMovement.enabled = false;

                Camera remotePlayerCamera = playerGameObject.GetComponentInChildren<Camera>(true);
                if (remotePlayerCamera != null)
                {
                    remotePlayerCamera.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogError($"Could not spawn player {photonPlayer.NickName}: Prefab for character {selectedCharData.characterName} is not assigned or not found.");
        }
    }

    /// <summary>
    /// Configures the Main Camera to follow the local player.
    /// This function is called only on the LOCAL client.
    /// </summary>
    /// <param name="localPlayerTransform">The Transform of the local player.</param>
    private void SetupLocalPlayerCamera(Transform localPlayerTransform)
    {
        CameraFollow cameraFollow = Camera.main?.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = localPlayerTransform;
            Debug.Log($"Camera '{Camera.main.name}' set to follow local player: {localPlayerTransform.name}");
        }
        else
        {
            Debug.LogWarning("CameraFollow component not found on Main Camera. Make sure it's attached.");
        }
    }

    public void StartRespawn(Player player)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log($"Master Client: Initiating respawn for player {player.NickName} in {respawnDelay} seconds.");
        StartCoroutine(RespawnPlayerCoroutine(player));
    }

    private IEnumerator RespawnPlayerCoroutine(Player playerToRespawn)
    {
        yield return new WaitForSeconds(respawnDelay);

        Debug.Log($"Master Client: Respawning player {playerToRespawn.NickName}.");

        if (spawnedPlayers.ContainsKey(playerToRespawn) && spawnedPlayers[playerToRespawn] != null)
        {
            PhotonNetwork.Destroy(spawnedPlayers[playerToRespawn]);
            spawnedPlayers.Remove(playerToRespawn);
        }
        SpawnPlayerFor(playerToRespawn);
    }

    // --- Helper method to wait for Custom Properties ---
   
    private IEnumerator WaitForPlayerCustomProperties(Player player, string propertyKey, System.Action callback)
    {
        float timeout = 5f; // Max wait time
        float startTime = Time.time;

        while (!player.CustomProperties.ContainsKey(propertyKey) && (Time.time - startTime < timeout))
        {
            yield return null; // Wait for next frame
        }

        if (player.CustomProperties.ContainsKey(propertyKey))
        {
            callback?.Invoke();
        }
        else
        {
            Debug.LogWarning($"Timeout waiting for property {propertyKey} for player {player.NickName}. Spawning with default.");
            callback?.Invoke();
        }
    }

    /// <summary>
    /// Example damage call (for testing).
    /// </summary>
    void Update()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.LocalPlayer != null && Input.GetKeyDown(KeyCode.K))
        {
            PlayerHealth targetPlayerHealth = localPlayerGameObject?.GetComponent<PlayerHealth>();
            if (targetPlayerHealth != null)
            {
                targetPlayerHealth.TakeDamage(10);
            }
            else
            {
                Debug.LogWarning("Local player not found or does not have PlayerHealth for test damage.");
            }
        }
    }
}

WORKED VERSION WITH DUPLICATE SPAWN 

// GameManager.cs
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq; // For LINQ
using Hashtable = ExitGames.Client.Photon.Hashtable; // Explicitly indicate which Hashtable is used
using System.Collections; // For IEnumerator

/// <summary>
/// Manages Photon connection, player spawning,
/// respawning, and overall game session logic.
/// Must be located in the game scene.
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Tooltip("List of all available characters. Must match PlayerSelectionManager.")]
    public List<PlayerCharacterData> allPlayerCharacters = new List<PlayerCharacterData>();

    [Tooltip("List of possible player spawn points.")]
    public List<Transform> spawnPoints;

    [Tooltip("Delay time before respawning a player after death.")]
    public float respawnDelay = 3f;

    // Dictionary for quick access to spawned player objects by their PhotonPlayer
    private Dictionary<Player, GameObject> spawnedPlayers = new Dictionary<Player, GameObject>();

    // Reference to the local player's game object.
    private GameObject localPlayerGameObject;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // GameManager должен спавнить игроков, только если мы УЖЕ в комнате.
        // Если GameScene запускается напрямую (для тестирования), нужно подключиться и присоединиться.
        if (PhotonNetwork.InRoom)
        {
            SpawnAllPlayersInRoom();
        }
        else
        {
            Debug.Log("GameManager: Не в комнате, подключаемся (для прямого тестирования GameScene).");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("GameManager: Подключено к Photon Master Server.");
        // Если GameScene запущена напрямую, присоединяемся к комнате после подключения к Master.
        if (!PhotonNetwork.InRoom)
        {
            JoinOrCreateRoomForDirectTesting();
        }
    }

    private void JoinOrCreateRoomForDirectTesting()
    {
        string roomName = "MyGameRoom"; // Используем дефолтное имя комнаты для тестирования
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 8 };
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        Debug.Log($"GameManager: Попытка присоединиться/создать комнату (для прямого тестирования): {roomName}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"GameManager: Присоединились к комнате: {PhotonNetwork.CurrentRoom.Name}");
        SpawnAllPlayersInRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Игрок {newPlayer.NickName} вошел в комнату.");
        StartCoroutine(WaitForPlayerCustomProperties(newPlayer, Menu.PhotonPlayerCharProperty, () => SpawnPlayerFor(newPlayer)));
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Игрок {otherPlayer.NickName} покинул комнату. Уничтожаем его объект.");
        if (spawnedPlayers.ContainsKey(otherPlayer) && spawnedPlayers[otherPlayer] != null)
        {
            spawnedPlayers.Remove(otherPlayer);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(Menu.PhotonPlayerCharProperty))
        {
            if (spawnedPlayers.ContainsKey(targetPlayer) && spawnedPlayers[targetPlayer] != null)
            {
                Debug.Log($"Игрок {targetPlayer.NickName} изменил свойство персонажа на {changedProps[Menu.PhotonPlayerCharProperty]}. Переспавниваем игрока.");

                PhotonNetwork.Destroy(spawnedPlayers[targetPlayer]);
                spawnedPlayers.Remove(targetPlayer);
                SpawnPlayerFor(targetPlayer);
            }
            else
            {
                Debug.Log($"Игрок {targetPlayer.NickName} обновил свойства персонажа. Спавним его.");
                SpawnPlayerFor(targetPlayer);
            }
        }
    }

    /// <summary>
    /// Spawns all players currently in the room.
    /// Called when first joining the room.
    /// </summary>
    private void SpawnAllPlayersInRoom()
    {
        foreach (var entry in spawnedPlayers)
        {
            if (entry.Value != null)
            {
                PhotonNetwork.Destroy(entry.Value);
            }
        }
        spawnedPlayers.Clear();
        localPlayerGameObject = null;

        foreach (Player photonPlayer in PhotonNetwork.CurrentRoom.Players.Values)
        {
            SpawnPlayerFor(photonPlayer);
        }
    }

    /// <summary>
    /// Spawns a character for a specific PhotonPlayer.
    /// </summary>
    /// <param name="photonPlayer">The PhotonPlayer for whom to spawn the character.</param>
    private void SpawnPlayerFor(Player photonPlayer)
    {
        if (spawnedPlayers.ContainsKey(photonPlayer) && spawnedPlayers[photonPlayer] != null)
        {
            Debug.LogWarning($"Player {photonPlayer.NickName} is already spawned. Skipping spawn.");
            return;
        }

        string selectedCharName = null;
        if (photonPlayer.CustomProperties.ContainsKey(Menu.PhotonPlayerCharProperty))
        {
            selectedCharName = photonPlayer.CustomProperties[Menu.PhotonPlayerCharProperty] as string;
        }

        PlayerCharacterData selectedCharData = allPlayerCharacters.FirstOrDefault(c => c.name == selectedCharName);

        if (selectedCharData == null)
        {
            Debug.LogWarning($"Player {photonPlayer.NickName} has not selected a character or data not found. Using the first available.");
            selectedCharData = allPlayerCharacters.FirstOrDefault();
            if (selectedCharData == null)
            {
                Debug.LogError("No available characters to spawn! Check the allPlayerCharacters list.");
                return;
            }
        }

        Transform randomSpawnPoint = spawnPoints.Count > 0 ? spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)] : null;
        Vector3 spawnPos = (randomSpawnPoint != null) ? randomSpawnPoint.position : Vector3.zero;
        Quaternion spawnRot = (randomSpawnPoint != null) ? randomSpawnPoint.rotation : Quaternion.identity;

        if (selectedCharData.characterPrefab != null)
        {
            string prefabName = selectedCharData.characterPrefab.name;

            GameObject playerGameObject = PhotonNetwork.Instantiate(prefabName, spawnPos, spawnRot);
            spawnedPlayers[photonPlayer] = playerGameObject;

            Debug.Log($"Player spawned: {selectedCharData.characterName} (Prefab: {prefabName}) for {photonPlayer.NickName}");

            PlayerHealth playerHealth = playerGameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.maxHealth = selectedCharData.baseHealth;
                playerHealth.ResetHealthAndActivate();
            }

            if (photonPlayer.IsLocal)
            {
                localPlayerGameObject = playerGameObject;
                SetupLocalPlayerCamera(playerGameObject.transform);
                PlayerMovement playerMovement = playerGameObject.GetComponent<PlayerMovement>();
                if (playerMovement != null) playerMovement.enabled = true;
            }
            else
            {
                PlayerMovement playerMovement = playerGameObject.GetComponent<PlayerMovement>();
                if (playerMovement != null) playerMovement.enabled = false;

                Camera remotePlayerCamera = playerGameObject.GetComponentInChildren<Camera>(true);
                if (remotePlayerCamera != null)
                {
                    remotePlayerCamera.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogError($"Could not spawn player {photonPlayer.NickName}: Prefab for character {selectedCharData.characterName} is not assigned or not found.");
        }
    }

    /// <summary>
    /// Configures the Main Camera to follow the local player.
    /// This function is called only on the LOCAL client.
    /// </summary>
    /// <param name="localPlayerTransform">The Transform of the local player.</param>
    private void SetupLocalPlayerCamera(Transform localPlayerTransform)
    {
        CameraFollow cameraFollow = Camera.main?.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = localPlayerTransform;
            Debug.Log($"Camera '{Camera.main.name}' set to follow local player: {localPlayerTransform.name}");
        }
        else
        {
            Debug.LogWarning("CameraFollow component not found on Main Camera. Make sure it's attached.");
        }
    }

    public void StartRespawn(Player player)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log($"Master Client: Initiating respawn for player {player.NickName} in {respawnDelay} seconds.");
        StartCoroutine(RespawnPlayerCoroutine(player));
    }

    private IEnumerator RespawnPlayerCoroutine(Player playerToRespawn)
    {
        yield return new WaitForSeconds(respawnDelay);

        Debug.Log($"Master Client: Respawning player {playerToRespawn.NickName}.");

        if (spawnedPlayers.ContainsKey(playerToRespawn) && spawnedPlayers[playerToRespawn] != null)
        {
            PhotonNetwork.Destroy(spawnedPlayers[playerToRespawn]);
            spawnedPlayers.Remove(playerToRespawn);
        }
        SpawnPlayerFor(playerToRespawn);
    }

    // --- Helper method to wait for Custom Properties ---
    private IEnumerator WaitForPlayerCustomProperties(Player player, string propertyKey, System.Action callback)
    {
        float timeout = 5f; // Max wait time
        float startTime = Time.time;

        while (!player.CustomProperties.ContainsKey(propertyKey) && (Time.time - startTime < timeout))
        {
            yield return null; // Wait for next frame
        }

        if (player.CustomProperties.ContainsKey(propertyKey))
        {
            callback?.Invoke();
        }
        else
        {
            Debug.LogWarning($"Timeout waiting for property {propertyKey} for player {player.NickName}. Spawning with default.");
            callback?.Invoke();
        }
    }

    /// <summary>
    /// Example damage call (for testing).
    /// </summary>
    void Update()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.LocalPlayer != null && Input.GetKeyDown(KeyCode.K))
        {
            PlayerHealth targetPlayerHealth = localPlayerGameObject?.GetComponent<PlayerHealth>();
            if (targetPlayerHealth != null)
            {
                targetPlayerHealth.TakeDamage(10);
            }
            else
            {
                Debug.LogWarning("Local player not found or does not have PlayerHealth for test damage.");
            }
        }
    }
}

*/

// GameManager.cs
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections;

/// <summary>
/// Manages Photon connection, player spawning,
/// respawning, and overall game session logic.
/// Must be located in the game scene.
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Tooltip("List of all available characters. Must match PlayerSelectionManager.")]
    public List<PlayerCharacterData> allPlayerCharacters = new List<PlayerCharacterData>();

    [Tooltip("List of possible player spawn points.")]
    public List<Transform> spawnPoints;

    [Tooltip("Delay time before respawning a player after death.")]
    public float respawnDelay = 3f;

    private Dictionary<Player, GameObject> spawnedPlayers = new Dictionary<Player, GameObject>();
    private GameObject localPlayerGameObject; // Ссылка на игровой объект локального игрока

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[GameManager.Awake] Found duplicate GameManager on '{gameObject.name}'. Destroying this one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log($"[GameManager.Awake] GameManager instance created on '{gameObject.name}'.");
    }

    void Start()
    {
        Debug.Log("[GameManager.Start] Start method called.");
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("[GameManager.Start] Already in a Photon room. Initiating player spawning.");
            SpawnAllPlayersInRoom();
        }
        else
        {
            Debug.Log("[GameManager.Start] Not in a room. Connecting to Photon Master (likely for direct GameScene testing).");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[GameManager.OnConnectedToMaster] Connected to Photon Master Server.");
        if (!PhotonNetwork.InRoom)
        {
            Debug.Log("[GameManager.OnConnectedToMaster] Not in a room. Joining/Creating room for direct testing.");
            JoinOrCreateRoomForDirectTesting();
        }
    }

    private void JoinOrCreateRoomForDirectTesting()
    {
        string roomName = "MyGameRoom";
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 8 };
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        Debug.Log($"[GameManager.JoinOrCreateRoomForDirectTesting] Attempting to join/create room: {roomName}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[GameManager.OnJoinedRoom] Successfully joined room: {PhotonNetwork.CurrentRoom.Name}. Total players in room: {PhotonNetwork.CurrentRoom.PlayerCount}");
        SpawnAllPlayersInRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[GameManager.OnPlayerEnteredRoom] Player {newPlayer.NickName} entered the room. Waiting for properties to spawn.");
        StartCoroutine(WaitForPlayerCustomProperties(newPlayer, Menu.PhotonPlayerCharProperty, () => SpawnPlayerFor(newPlayer)));
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[GameManager.OnPlayerLeftRoom] Player {otherPlayer.NickName} left the room. Removing their object from tracking.");
        if (spawnedPlayers.ContainsKey(otherPlayer) && spawnedPlayers[otherPlayer] != null)
        {
            spawnedPlayers.Remove(otherPlayer);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log($"[GameManager.OnPlayerPropertiesUpdate] Player properties updated for {targetPlayer.NickName}. Changed props: {string.Join(", ", changedProps.Keys.Cast<object>())}");
        if (changedProps.ContainsKey(Menu.PhotonPlayerCharProperty))
        {
            if (spawnedPlayers.ContainsKey(targetPlayer) && spawnedPlayers[targetPlayer] != null)
            {
                string oldCharName = null;
                if (targetPlayer.CustomProperties.ContainsKey(Menu.PhotonPlayerCharProperty))
                {
                    oldCharName = targetPlayer.CustomProperties[Menu.PhotonPlayerCharProperty] as string;
                }
                string newCharName = changedProps[Menu.PhotonPlayerCharProperty] as string;

                if (oldCharName != newCharName)
                {
                    Debug.Log($"[GameManager.OnPlayerPropertiesUpdate] Character model for {targetPlayer.NickName} has changed ({oldCharName} -> {newCharName}). Destroying old and respawning.");
                    PhotonNetwork.Destroy(spawnedPlayers[targetPlayer]);
                    spawnedPlayers.Remove(targetPlayer);
                    SpawnPlayerFor(targetPlayer); // Re-spawn with the new character
                }
                else
                {
                    Debug.Log($"[GameManager.OnPlayerPropertiesUpdate] Character property updated for {targetPlayer.NickName}, but model did not change. No re-spawn needed.");
                }
            }
            else
            {
                Debug.Log($"[GameManager.OnPlayerPropertiesUpdate] Character property updated for {targetPlayer.NickName}. Spawning them as they were not tracked yet.");
                SpawnPlayerFor(targetPlayer);
            }
        }
    }

    private void SpawnAllPlayersInRoom()
    {
        Debug.Log($"[GameManager.SpawnAllPlayersInRoom] Called. Currently {PhotonNetwork.CurrentRoom.Players.Count} players in room.");

        spawnedPlayers.Clear(); // Очищаем отслеживание перед обработкой всех игроков
        localPlayerGameObject = null; // Сбрасываем ссылку на локальный объект

        foreach (Player photonPlayer in PhotonNetwork.CurrentRoom.Players.Values)
        {
            Debug.Log($"[GameManager.SpawnAllPlayersInRoom] Processing player: {photonPlayer.NickName}.");
            SpawnPlayerFor(photonPlayer);
        }
        Debug.Log($"[GameManager.SpawnAllPlayersInRoom] Finished. spawnedPlayers count: {spawnedPlayers.Count}");
    }

    /// <summary>
    /// Spawns a character for a specific PhotonPlayer or tracks an existing one.
    /// This method is now much more robust against duplicate instantiations.
    /// </summary>
    /// <param name="photonPlayer">The PhotonPlayer for whom to spawn the character.</param>
    private void SpawnPlayerFor(Player photonPlayer)
    {
        Debug.Log($"[SpawnPlayerFor] --- START for player: {photonPlayer.NickName} (IsLocal: {photonPlayer.IsLocal}) ---");

        // 1. ПРОВЕРКА: Уже отслеживаем этот PhotonPlayer на этом клиенте?
        if (spawnedPlayers.ContainsKey(photonPlayer) && spawnedPlayers[photonPlayer] != null)
        {
            Debug.Log($"[SpawnPlayerFor] Player {photonPlayer.NickName} (IsLocal: {photonPlayer.IsLocal}) already tracked locally ({spawnedPlayers[photonPlayer].name}). Reconfiguring components if necessary.");
            ConfigurePlayerComponents(photonPlayer, spawnedPlayers[photonPlayer]);
            Debug.Log($"[SpawnPlayerFor] --- END for player: {photonPlayer.NickName} (Already Tracked) ---");
            return;
        }

        // 2. ПРОВЕРКА: Ищем существующий *сетевой* объект в сцене, у которого ЕСТЬ PlayerMovement (включая дочерние)
        PhotonView existingPlayerView = null;
        if (photonPlayer.IsLocal)
        {
            existingPlayerView = FindObjectsOfType<PhotonView>()
                .FirstOrDefault(pv => pv.IsMine
                                    && pv.gameObject.activeInHierarchy
                                    && pv.GetComponentInChildren<PlayerMovement>(true) != null); // <--- ИЗМЕНЕНИЕ: GetComponentInChildren

            if (existingPlayerView != null)
            {
                Debug.Log($"[SpawnPlayerFor] LOCAL player {photonPlayer.NickName} found active 'IsMine' PhotonView ({existingPlayerView.gameObject.name}) with PlayerMovement. Tracking and configuring it.");
                spawnedPlayers[photonPlayer] = existingPlayerView.gameObject;
                ConfigurePlayerComponents(photonPlayer, existingPlayerView.gameObject);
                Debug.Log($"[SpawnPlayerFor] --- END for LOCAL player: {photonPlayer.NickName} (Used Existing Local Object) ---");
                return;
            }
        }
        else // Remote player
        {
            existingPlayerView = FindObjectsOfType<PhotonView>()
                .FirstOrDefault(pv => pv.Owner == photonPlayer
                                    && pv.gameObject.activeInHierarchy
                                    && pv.GetComponentInChildren<PlayerMovement>(true) != null); // <--- ИЗМЕНЕНИЕ: GetComponentInChildren

            if (existingPlayerView != null)
            {
                Debug.Log($"[SpawnPlayerFor] REMOTE player {photonPlayer.NickName} found active networked object ({existingPlayerView.gameObject.name}) with PlayerMovement. Tracking and configuring it.");
                spawnedPlayers[photonPlayer] = existingPlayerView.gameObject;
                ConfigurePlayerComponents(photonPlayer, existingPlayerView.gameObject);
                Debug.Log($"[SpawnPlayerFor] --- END for REMOTE player: {photonPlayer.NickName} (Used Existing Remote Object) ---");
                return;
            }
        }

        // 3. Если мы дошли сюда, объект НЕ отслеживается и НЕ найден в сцене.
        // ТОЛЬКО ЛОКАЛЬНЫЙ ИГРОК МОЖЕТ ИНСТАНЦИИРОВАТЬ СВОЙ ОБЪЕКТ.
        if (photonPlayer.IsLocal)
        {
            Debug.Log($"[SpawnPlayerFor] LOCAL player {photonPlayer.NickName} has no tracked or existing 'IsMine' object with PlayerMovement. Proceeding to INSTANTIATE.");

            GameObject playerGameObject = TryInstantiatePlayerPrefab(photonPlayer);
            if (playerGameObject != null)
            {
                spawnedPlayers[photonPlayer] = playerGameObject; // Отслеживаем только что инстанциированный объект
                ConfigurePlayerComponents(photonPlayer, playerGameObject);
            }
            Debug.Log($"[SpawnPlayerFor] --- END for LOCAL player: {photonPlayer.NickName} (Instantiated) ---");
            return;
        }

        // 4. Если это УДАЛЕННЫЙ ИГРОК, и его объект НЕ НАЙДЕН в сцене (и не отслеживается),
        // это означает, что ЕГО ВЛАДЕЛЕЦ ЕЩЕ НЕ ИНСТАНЦИИРОВАЛ ЕГО, ИЛИ МЫ ЕЩЕ НЕ ПОЛУЧИЛИ СИНХРОНИЗАЦИЮ.
        // Мы НЕ ДОЛЖНЫ ИНСТАНЦИИРОВАТЬ ЕГО ЗДЕСЬ. Мы ждем, пока его владелец это сделает.
        Debug.LogWarning($"[SpawnPlayerFor] REMOTE player {photonPlayer.NickName} has no tracked or existing networked object with PlayerMovement found. Assuming owner will instantiate it. Skipping instantiation on THIS client.");
        Debug.Log($"[SpawnPlayerFor] --- END for REMOTE player: {photonPlayer.NickName} (Skipped Instantiation) ---");
    }

    /// <summary>
    /// Вспомогательный метод для выполнения PhotonNetwork.Instantiate и первичной настройки.
    /// Возвращает GameObject только что инстанциированного объекта.
    /// </summary>
    private GameObject TryInstantiatePlayerPrefab(Player photonPlayer)
    {
        string selectedCharName = null;
        if (photonPlayer.CustomProperties.ContainsKey(Menu.PhotonPlayerCharProperty))
        {
            selectedCharName = photonPlayer.CustomProperties[Menu.PhotonPlayerCharProperty] as string;
            Debug.Log($"[TryInstantiatePlayerPrefab] Retrieved selected character name for {photonPlayer.NickName}: {selectedCharName}");
        }
        else
        {
            Debug.LogWarning($"[TryInstantiatePlayerPrefab] Custom property '{Menu.PhotonPlayerCharProperty}' NOT found for player {photonPlayer.NickName}. Falling back to default.");
        }

        PlayerCharacterData selectedCharData = allPlayerCharacters.FirstOrDefault(c => c.name == selectedCharName);

        if (selectedCharData == null)
        {
            Debug.LogWarning($"[TryInstantiatePlayerPrefab] Character data for '{selectedCharName}' not found in 'allPlayerCharacters' list for player {photonPlayer.NickName}. Using first available.");
            selectedCharData = allPlayerCharacters.FirstOrDefault();
            if (selectedCharData == null)
            {
                Debug.LogError("[TryInstantiatePlayerPrefab] ERROR: No available characters in 'allPlayerCharacters' list to instantiate!");
                return null;
            }
            Debug.Log($"[TryInstantiatePlayerPrefab] Using default character: {selectedCharData.characterName}");
        }
        else
        {
            Debug.Log($"[TryInstantiatePlayerPrefab] Found character data: {selectedCharData.characterName} (Prefab: {selectedCharData.characterPrefab?.name ?? "NULL"})");
        }

        Transform randomSpawnPoint = null;
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            randomSpawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
            Debug.Log($"[TryInstantiatePlayerPrefab] Selected spawn point: {randomSpawnPoint.name}");
        }
        else
        {
            Debug.LogWarning("[TryInstantiatePlayerPrefab] No spawn points assigned in 'spawnPoints' list. Spawning at Vector3.zero.");
        }
        Vector3 spawnPos = (randomSpawnPoint != null) ? randomSpawnPoint.position : Vector3.zero;
        Quaternion spawnRot = (randomSpawnPoint != null) ? randomSpawnPoint.rotation : Quaternion.identity;
        Debug.Log($"[TryInstantiatePlayerPrefab] Calculated spawn position: {spawnPos}, rotation: {spawnRot}");

        if (selectedCharData.characterPrefab != null)
        {
            string prefabName = selectedCharData.characterPrefab.name;
            Debug.Log($"[TryInstantiatePlayerPrefab] Attempting to PhotonNetwork.Instantiate prefab named: '{prefabName}' for {photonPlayer.NickName}.");

            GameObject playerGameObject = null;
            try
            {
                playerGameObject = PhotonNetwork.Instantiate(prefabName, spawnPos, spawnRot);
                Debug.Log($"[TryInstantiatePlayerPrefab] Successfully INSTANTIATED '{prefabName}' for {photonPlayer.NickName}. GameObject: {playerGameObject.name}, ViewID: {playerGameObject.GetComponent<PhotonView>()?.ViewID}");

                MeshRenderer mr = playerGameObject.GetComponentInChildren<MeshRenderer>(true);
                if (mr != null)
                {
                    Debug.Log($"[TryInstantiatePlayerPrefab] Instantiated player '{playerGameObject.name}' has MeshRenderer. Is enabled: {mr.enabled}, Has mesh: {mr.GetComponent<MeshFilter>()?.sharedMesh != null}. Materials count: {mr.sharedMaterials.Length}.");
                }
                else
                {
                    Debug.LogWarning($"[TryInstantiatePlayerPrefab] Instantiated player '{playerGameObject.name}' DOES NOT have a MeshRenderer (or any in children). Model might be invisible.");
                }
                return playerGameObject;

            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TryInstantiatePlayerPrefab] ERROR: Failed to instantiate prefab '{prefabName}'! Error: {e.Message}. Make sure it's in 'Resources/PhotonPrefabs' and has a PhotonView. Details: {selectedCharData.characterPrefab}", selectedCharData.characterPrefab);
                return null;
            }
        }
        else
        {
            Debug.LogError($"[TryInstantiatePlayerPrefab] ERROR: Prefab for character {selectedCharData.characterName} is NOT assigned in PlayerCharacterData ScriptableObject!");
            return null;
        }
    }

    /// <summary>
    /// Вспомогательный метод для настройки компонентов игрока (Health, Movement, Camera).
    /// </summary>
    private void ConfigurePlayerComponents(Player photonPlayer, GameObject playerGameObject)
    {
        Debug.Log($"[ConfigurePlayerComponents] Configuring components for {photonPlayer.NickName} (IsLocal: {photonPlayer.IsLocal}). Object: {playerGameObject?.name ?? "NULL"}");

        // Ensure the GameObject itself is active
        if (playerGameObject != null && !playerGameObject.activeSelf)
        {
            playerGameObject.SetActive(true);
            Debug.Log($"[ConfigurePlayerComponents] Activated GameObject '{playerGameObject.name}'.");
        }

        PlayerHealth playerHealth = playerGameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            string selectedCharName = null;
            if (photonPlayer.CustomProperties.ContainsKey(Menu.PhotonPlayerCharProperty))
            {
                selectedCharName = photonPlayer.CustomProperties[Menu.PhotonPlayerCharProperty] as string;
            }
            PlayerCharacterData selectedCharData = allPlayerCharacters.FirstOrDefault(c => c.name == selectedCharName);
            if (selectedCharData != null)
            {
                playerHealth.maxHealth = selectedCharData.baseHealth;
            }
            else
            {
                Debug.LogWarning($"[ConfigurePlayerComponents] Character data for {selectedCharName} not found for health initialization.");
            }
            playerHealth.ResetHealthAndActivate();
            Debug.Log($"[ConfigurePlayerComponents] PlayerHealth initialized for {photonPlayer.NickName}. Current Health: {playerHealth.GetCurrentHealth()}");
        }
        else
        {
            Debug.LogWarning($"[ConfigurePlayerComponents] PlayerHealth component NOT found on player prefab '{playerGameObject?.name ?? "NULL"}' for {photonPlayer.NickName}.");
        }

        if (photonPlayer.IsLocal)
        {
            localPlayerGameObject = playerGameObject;
            Debug.Log($"[ConfigurePlayerComponents] Configuring LOCAL player {photonPlayer.NickName}. Enabling movement and setting up camera.");
            SetupLocalPlayerCamera(playerGameObject.transform);
            PlayerMovement playerMovement = playerGameObject.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
                Debug.Log($"[ConfigurePlayerComponents] PlayerMovement enabled for local player {photonPlayer.NickName}.");
            }
            else
            {
                Debug.LogWarning($"[ConfigurePlayerComponents] PlayerMovement component NOT found on local player prefab '{playerGameObject?.name ?? "NULL"}' for {photonPlayer.NickName}.");
            }
        }
        else // Remote player
        {
            Debug.Log($"[ConfigurePlayerComponents] Configuring REMOTE player {photonPlayer.NickName}. Disabling movement and camera.");
            PlayerMovement playerMovement = playerGameObject.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
                Debug.Log($"[ConfigurePlayerComponents] PlayerMovement disabled for remote player {photonPlayer.NickName}.");
            }
            Camera remotePlayerCamera = playerGameObject.GetComponentInChildren<Camera>(true);
            if (remotePlayerCamera != null)
            {
                remotePlayerCamera.gameObject.SetActive(false);
                Debug.Log($"[ConfigurePlayerComponents] Remote player camera disabled for {photonPlayer.NickName}.");
            }
        }
    }

    private void SetupLocalPlayerCamera(Transform localPlayerTransform)
    {
        Debug.Log($"[SetupLocalPlayerCamera] Setting up camera for {localPlayerTransform.name}.");
        CameraFollow cameraFollow = Camera.main?.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = localPlayerTransform;
            Debug.Log($"[SetupLocalPlayerCamera] Camera '{Camera.main.name}' target set to {localPlayerTransform.name}.");
        }
        else
        {
            Debug.LogWarning("[SetupLocalPlayerCamera] CameraFollow component NOT found on Main Camera. Make sure it's attached and Main Camera has the 'MainCamera' tag.");
            Camera playerCamera = localPlayerTransform.GetComponentInChildren<Camera>(true);
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(true);
                Debug.Log($"[SetupLocalPlayerCamera] Found and activated camera attached to player prefab: {playerCamera.name}.");
            }
            else
            {
                Debug.LogError("[SetupLocalPlayerCamera] No CameraFollow on Main Camera and no camera found in player prefab. Local player will have no view!");
            }
        }
    }

    public void StartRespawn(Player player)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        Debug.Log($"[GameManager.StartRespawn] Master Client initiating respawn for {player.NickName} in {respawnDelay} seconds.");
        StartCoroutine(RespawnPlayerCoroutine(player));
    }

    private IEnumerator RespawnPlayerCoroutine(Player playerToRespawn)
    {
        Debug.Log($"[GameManager.RespawnPlayerCoroutine] Coroutine started for {playerToRespawn.NickName}. Delaying {respawnDelay}s.");
        yield return new WaitForSeconds(respawnDelay);

        Debug.Log($"[GameManager.RespawnPlayerCoroutine] Master Client respawning player {playerToRespawn.NickName}.");

        if (spawnedPlayers.ContainsKey(playerToRespawn) && spawnedPlayers[playerToRespawn] != null)
        {
            PhotonNetwork.Destroy(spawnedPlayers[playerToRespawn]);
            spawnedPlayers.Remove(playerToRespawn);
            Debug.Log($"[GameManager.RespawnPlayerCoroutine] Destroyed old player object for {playerToRespawn.NickName}.");
        }
        SpawnPlayerFor(playerToRespawn);
    }

    private IEnumerator WaitForPlayerCustomProperties(Player player, string propertyKey, System.Action callback)
    {
        float timeout = 5f;
        float startTime = Time.time;
        Debug.Log($"[GameManager.WaitForPlayerCustomProperties] Waiting for property '{propertyKey}' for player {player.NickName}.");

        while (!player.CustomProperties.ContainsKey(propertyKey) && (Time.time - startTime < timeout))
        {
            yield return null;
        }

        if (player.CustomProperties.ContainsKey(propertyKey))
        {
            Debug.Log($"[GameManager.WaitForPlayerCustomProperties] Property '{propertyKey}' found for {player.NickName}. Executing callback.");
            callback?.Invoke();
        }
        else
        {
            Debug.LogWarning($"[GameManager.WaitForPlayerCustomProperties] Timeout waiting for property '{propertyKey}' for player {player.NickName}. Executing callback with default.");
            callback?.Invoke();
        }
    }

    void Update()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.LocalPlayer != null && localPlayerGameObject != null && Input.GetKeyDown(KeyCode.K))
        {
            PlayerHealth targetPlayerHealth = localPlayerGameObject.GetComponent<PlayerHealth>();
            if (targetPlayerHealth != null)
            {
                Debug.Log($"[GameManager.Update] K pressed. Applying 10 damage to local player {PhotonNetwork.LocalPlayer.NickName}.");
                targetPlayerHealth.TakeDamage(10);
            }
            else
            {
                Debug.LogWarning("[GameManager.Update] Local player object found, but does not have PlayerHealth for test damage.");
            }
        }
    }
}

