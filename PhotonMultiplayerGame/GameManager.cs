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
*/

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
