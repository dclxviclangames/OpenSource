/* PlayerSelectionManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun; // Для работы с Photon Custom Properties
using Photon.Realtime; // Для Hashtable
using ExitGames.Client.Photon; // Для Hashtable

/// <summary>
/// Управляет UI меню выбора персонажей.
/// Загружает PlayerCharacterData ScriptableObjects, динамически создает элементы UI,
/// и сохраняет выбор игрока в PlayerPrefs И В Photon Player Custom Properties.
/// </summary>
public class PlayerSelectionManager : MonoBehaviourPunCallbacks // Наследуемся для OnConnectedToMaster
{
    [Header("UI Элементы")]
    public Transform selectionItemContainer;
    public GameObject playerSelectionItemUIPrefab;
    public Button startGameButton;
    public TMPro.TextMeshProUGUI selectedCharacterNameText;
    public TMPro.TextMeshProUGUI connectionStatusText; // Для отображения статуса подключения Photon

    [Header("Данные")]
    public List<PlayerCharacterData> allAvailableCharacters = new List<PlayerCharacterData>();

    private PlayerCharacterData currentlySelectedCharacter;

    // Ключи для PlayerPrefs и Photon Custom Properties
    public const string SelectedCharacterKey = "SelectedPlayerCharacterName"; // Имя ScriptableObject
    public const string PhotonPlayerCharProperty = "CharName"; // Ключ для Custom Property Photon

    void Start()
    {
        PopulateSelectionUI();
        LoadSelectedCharacter(); // Загружаем ранее выбранного персонажа из PlayerPrefs

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGame);
            // Кнопка активна, только если персонаж выбран И мы подключены к Photon
            startGameButton.interactable = (currentlySelectedCharacter != null && PhotonNetwork.IsConnectedAndReady);
        }
        UpdateConnectionStatus();
    }

    void Update()
    {
        // Обновляем состояние кнопки "Начать игру"
        if (startGameButton != null)
        {
            startGameButton.interactable = (currentlySelectedCharacter != null && PhotonNetwork.IsConnectedAndReady);
        }
        UpdateConnectionStatus();
    }

    private void UpdateConnectionStatus()
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = $"Photon: {(PhotonNetwork.IsConnectedAndReady ? "Подключен" : "Отключен")}";
        }
    }

    private void PopulateSelectionUI()
    {
        foreach (Transform child in selectionItemContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (PlayerCharacterData characterData in allAvailableCharacters)
        {
            if (playerSelectionItemUIPrefab != null)
            {
                GameObject itemGO = Instantiate(playerSelectionItemUIPrefab, selectionItemContainer);
                PlayerSelectionItemUI itemUI = itemGO.GetComponent<PlayerSelectionItemUI>();
                if (itemUI != null)
                {
                    itemUI.Initialize(characterData, this);
                }
            }
        }
    }

    public void SelectCharacter(PlayerCharacterData data)
    {
        currentlySelectedCharacter = data;
        PlayerPrefs.SetString(SelectedCharacterKey, data.name); // Сохраняем имя ScriptableObject локально
        PlayerPrefs.Save();
        Debug.Log($"Локально выбран персонаж: {data.characterName}. Имя ScriptableObject: {data.name}");

        UpdateSelectedCharacterUI();
    }

    private void LoadSelectedCharacter()
    {
        if (PlayerPrefs.HasKey(SelectedCharacterKey))
        {
            string savedCharacterName = PlayerPrefs.GetString(SelectedCharacterKey);
            foreach (PlayerCharacterData data in allAvailableCharacters)
            {
                if (data.name == savedCharacterName)
                {
                    currentlySelectedCharacter = data;
                    Debug.Log($"Загружен ранее выбранный персонаж из PlayerPrefs: {data.characterName}");
                    break;
                }
            }
        }
        UpdateSelectedCharacterUI();
    }

    private void UpdateSelectedCharacterUI()
    {
        if (selectedCharacterNameText != null)
        {
            selectedCharacterNameText.text = currentlySelectedCharacter != null ? $"Выбран: {currentlySelectedCharacter.characterName}" : "Выберите персонажа";
        }
    }

    public PlayerCharacterData GetSelectedCharacter()
    {
        return currentlySelectedCharacter;
    }

    private void OnStartGame()
    {
        if (currentlySelectedCharacter == null)
        {
            Debug.LogWarning("Пожалуйста, выберите персонажа перед началом игры.");
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Не подключены к Photon, подключаемся...");
            PhotonNetwork.ConnectUsingSettings(); // Используем настройки из PhotonServerSettings
            // После подключения OnConnectedToMaster() будет вызван.
            // Там мы можем попробовать присоединиться к комнате и установить Custom Property.
        }
        else
        {
            // Если уже подключены, сразу переходим к установке свойства и загрузке сцены
            SetCharacterCustomPropertyAndLoadGame();
        }
    }

    // --- Обработка событий Photon ---

    public override void OnConnectedToMaster()
    {
        Debug.Log("Подключено к Photon Master Server.");
        // Теперь, когда подключены, можем установить Custom Property и перейти в игру
        SetCharacterCustomPropertyAndLoadGame();
    }

    private void SetCharacterCustomPropertyAndLoadGame()
    {
        if (currentlySelectedCharacter == null)
        {
            Debug.LogError("Не выбран персонаж для установки Custom Property!");
            return;
        }

        // Устанавливаем Custom Property для локального игрока
        // Ключ: "CharName", Значение: Имя ScriptableObject персонажа
        Hashtable customProps = new Hashtable();
        customProps[PhotonPlayerCharProperty] = currentlySelectedCharacter.name;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);

        Debug.Log($"Установлено Photon Custom Property для {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");

        // После установки свойства, загружаем игровую сцену
        // GameManager в игровой сцене будет использовать это Custom Property для спавна.
        PhotonNetwork.LoadLevel("GameScene");
    }
}
*/
