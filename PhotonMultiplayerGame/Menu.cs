// Menu.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro; // Для TextMeshPro
using ExitGames.Client.Photon; // Для Hashtable
using Hashtable = ExitGames.Client.Photon.Hashtable; // Explicitly indicate which Hashtable is used


public class Menu : MonoBehaviourPunCallbacks // Переименовываем PlayerSelectionManager в Menu
{
    [Header("Поля ввода и кнопки (Ваши оригинальные)")]
    public TMP_InputField createRoomInputField; // Для создания комнаты
    public TMP_InputField joinRoomInputField; // Для присоединения к комнате
    public TMP_InputField nicknameInputField; // Для ввода никнейма
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button setNicknameButton;

    [Header("UI Элементы выбора персонажей")]
    public Transform selectionItemContainer; // Контейнер для элементов выбора
    public GameObject playerSelectionItemUIPrefab; // Префаб одного элемента выбора
    public TMPro.TextMeshProUGUI selectedCharacterNameText; // Текст для отображения выбранного персонажа

    [Header("Данные персонажей")]
    public List<PlayerCharacterData> allAvailableCharacters = new List<PlayerCharacterData>();

    private PlayerCharacterData currentlySelectedCharacter; // Текущий выбранный персонаж
    public const string SelectedCharacterKey = "SelectedPlayerCharacterName"; // Ключ для PlayerPrefs
    public const string PhotonPlayerCharProperty = "CharName"; // Ключ для Custom Property Photon

    void Awake()
    {
        // === Важно: Устанавливаем синхронизацию сцен в Awake ===
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log($"PhotonNetwork.AutomaticallySyncScene = {PhotonNetwork.AutomaticallySyncScene}");
    }

    void Start()
    {
        // Инициализация UI из вашего оригинального скрипта
        if (createRoomButton != null) createRoomButton.onClick.AddListener(CreateRoom);
        if (joinRoomButton != null) joinRoomButton.onClick.AddListener(JoinRoom);
        if (setNicknameButton != null) setNicknameButton.onClick.AddListener(OnClick_SetName);

        // Инициализация UI выбора персонажей
        PopulateSelectionUI();
        LoadSelectedCharacter();

        // Устанавливаем никнейм Photon, если он уже есть
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerNickname", "")))
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerNickname");
            if (nicknameInputField != null) nicknameInputField.text = PhotonNetwork.NickName;
            Debug.Log($"Загружен никнейм: {PhotonNetwork.NickName}");
        }

        // Кнопки комнат активны только при подключении к Master Server
        UpdateRoomButtonsInteractable();
    }

    void Update()
    {
        // Обновляем состояние кнопок комнат в Update
        UpdateRoomButtonsInteractable();
    }

    private void UpdateRoomButtonsInteractable()
    {
        bool isConnectedAndNotInRoom = PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom;
        if (createRoomButton != null) createRoomButton.interactable = isConnectedAndNotInRoom && !string.IsNullOrEmpty(createRoomInputField?.text);
        if (joinRoomButton != null) joinRoomButton.interactable = isConnectedAndNotInRoom && !string.IsNullOrEmpty(joinRoomInputField?.text);
        if (setNicknameButton != null) setNicknameButton.interactable = isConnectedAndNotInRoom && !string.IsNullOrEmpty(nicknameInputField?.text);
    }

    // --- Методы для работы с никнеймом ---
    public void OnClick_SetName()
    {
        if (nicknameInputField != null && !string.IsNullOrEmpty(nicknameInputField.text))
        {
            string newNickname = nicknameInputField.text;
            PhotonNetwork.NickName = newNickname;
            PlayerPrefs.SetString("PlayerNickname", newNickname); // Сохраняем никнейм локально
            PlayerPrefs.Save();
            Debug.Log($"Никнейм установлен: {newNickname}");
        }
        else
        {
            Debug.LogWarning("Пожалуйста, введите никнейм.");
        }
    }

    // --- Методы для работы с комнатами (Ваши оригинальные) ---
    public void CreateRoom()
    {
        if (currentlySelectedCharacter == null)
        {
            Debug.LogWarning("Пожалуйста, выберите персонажа перед созданием комнаты.");
            return;
        }
        if (!PhotonNetwork.IsConnectedAndReady || PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Не подключены к Photon Master Server или уже в комнате. Невозможно создать комнату.");
            return;
        }

        SetLocalPlayerCharacterProperty();

        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 8 }; // Ваше MaxPlayers
        PhotonNetwork.CreateRoom(createRoomInputField.text, roomOptions);
        Debug.Log($"Попытка создать комнату: {createRoomInputField.text}");
    }

    public void JoinRoom()
    {
        if (currentlySelectedCharacter == null)
        {
            Debug.LogWarning("Пожалуйста, выберите персонажа перед присоединением к комнате.");
            return;
        }
        if (!PhotonNetwork.IsConnectedAndReady || PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Не подключены к Photon Master Server или уже в комнате. Невозможно присоединиться.");
            return;
        }

        SetLocalPlayerCharacterProperty();

        PhotonNetwork.JoinRoom(joinRoomInputField.text);
        Debug.Log($"Попытка присоединиться к комнате: {joinRoomInputField.text}");
    }

    // --- Callbacks Photon ---
    public override void OnCreatedRoom()
    {
        Debug.Log($"Комната '{PhotonNetwork.CurrentRoom.Name}' создана успешно!");
        // После создания комнаты мы автоматически присоединяемся к ней,
        // и сработает OnJoinedRoom()
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Присоединились к комнате: {PhotonNetwork.CurrentRoom.Name}");

        // === КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: Устанавливаем Custom Property персонажа ===
        if (currentlySelectedCharacter != null)
        {
            Hashtable customProps = new Hashtable();
            customProps[PhotonPlayerCharProperty] = currentlySelectedCharacter.name;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            Debug.Log($"Установлено Photon Custom Property для {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");
        }
        else
        {
            Debug.LogWarning("Персонаж не выбран. Игрок присоединился без свойства персонажа.");
            // Можно установить дефолтный персонаж или предотвратить присоединение.
        }

        // === Важно: Только МАСТЕР-КЛИЕНТ загружает сцену ===
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Я Мастер-клиент. Загружаю игровую сцену 'SampleScene'.");
            PhotonNetwork.LoadLevel("GameScene"); // Замените на имя вашей игровой сцены
        }
        else
        {
            Debug.Log("Я не Мастер-клиент. Жду, пока Мастер-клиент загрузит сцену.");
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Ошибка создания комнаты: {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Ошибка присоединения к комнате: {message}");
    }

    // --- Методы выбора персонажей ---
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

    public void DclxviclanRoom()
    {
        SetLocalPlayerCharacterProperty();

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 7;
        PhotonNetwork.CreateRoom("dclxviclan", roomOptions);
    }

    public void DclxviclanJoin()
    {
        SetLocalPlayerCharacterProperty();

        PhotonNetwork.JoinRoom("dclxviclan");
    }

    private void SetLocalPlayerCharacterProperty()
    {
        if (currentlySelectedCharacter != null && PhotonNetwork.LocalPlayer != null)
        {
            Hashtable customProps = new Hashtable();
            customProps[PhotonPlayerCharProperty] = currentlySelectedCharacter.name;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            Debug.Log($"Menu: Local player Custom Property set: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");
        }
        else
        {
            Debug.LogError("Menu: Cannot set Custom Property: Character not selected or LocalPlayer is null.");
        }
    }

    private void UpdateSelectedCharacterUI()
    {
        if (selectedCharacterNameText != null)
        {
            selectedCharacterNameText.text = currentlySelectedCharacter != null ? $"Выбран: {currentlySelectedCharacter.characterName}" : "Выберите персонажа";
        }
    }
}
