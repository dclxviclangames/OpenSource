// Menu.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro; // ��� TextMeshPro
using ExitGames.Client.Photon; // ��� Hashtable
using Hashtable = ExitGames.Client.Photon.Hashtable; // Explicitly indicate which Hashtable is used


public class Menu : MonoBehaviourPunCallbacks // ��������������� PlayerSelectionManager � Menu
{
    [Header("���� ����� � ������ (���� ������������)")]
    public TMP_InputField createRoomInputField; // ��� �������� �������
    public TMP_InputField joinRoomInputField; // ��� ������������� � �������
    public TMP_InputField nicknameInputField; // ��� ����� ��������
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button setNicknameButton;

    [Header("UI �������� ������ ����������")]
    public Transform selectionItemContainer; // ��������� ��� ��������� ������
    public GameObject playerSelectionItemUIPrefab; // ������ ������ �������� ������
    public Image characterImage;
    public TMPro.TextMeshProUGUI selectedCharacterNameText; // ����� ��� ����������� ���������� ���������
    private string selectedCharName = "";
    [Header("������ ����������")]
    public List<PlayerCharacterData> allAvailableCharacters = new List<PlayerCharacterData>();

    private PlayerCharacterData currentlySelectedCharacter; // ������� ��������� ��������
    public const string SelectedCharacterKey = "SelectedPlayerCharacterName"; // ���� ��� PlayerPrefs
    public const string PhotonPlayerCharProperty = "CharName"; // ���� ��� Custom Property Photon
    private int roEvNum = 0;

    void Awake()
    {
        // === �����: ������������� ������������� ���� � Awake ===
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log($"PhotonNetwork.AutomaticallySyncScene = {PhotonNetwork.AutomaticallySyncScene}");
     //   ResetPlayerCustomProperties();
        // DontDestroyOnLoad(this.gameObject);
    }

    public void ResetPlayerCustomProperties()
    {
        // Create a new PunHashtable
        Hashtable customPropertiesToReset = new Hashtable();

        // Add each property you want to clear and set its value to null
        customPropertiesToReset.Add("PlayerLives", null);
        customPropertiesToReset.Add("PlayerScore", null);
        customPropertiesToReset.Add("PlayerStatus", null);
        customPropertiesToReset.Add("CharName", null);

        // Update the local player's custom properties
        PhotonNetwork.LocalPlayer.SetCustomProperties(customPropertiesToReset);

        Debug.Log("Player custom properties reset.");
    }

    public void ResetPlayerProperties()
    {
        Hashtable customProps = new Hashtable();
        customProps[PhotonPlayerCharProperty] = null;
        PlayerPrefs.DeleteAll();// ������������� � null
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
        Debug.Log("�������� ������ ��������.");
    }

    void Start()
    {
        // ������������� UI �� ������ ������������� �������
        if (createRoomButton != null) createRoomButton.onClick.AddListener(CreateRoom);
        if (joinRoomButton != null) joinRoomButton.onClick.AddListener(JoinRoom);
        if (setNicknameButton != null) setNicknameButton.onClick.AddListener(OnClick_SetName);
      //  ResetPlayerProperties();

        // ������������� UI ������ ����������
        PopulateSelectionUI();
        LoadSelectedCharacter();

        // ������������� ������� Photon, ���� �� ��� ����
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerNickname", "")))
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerNickname");
            if (nicknameInputField != null) nicknameInputField.text = PhotonNetwork.NickName;
            Debug.Log($"�������� �������: {PhotonNetwork.NickName}");
        }

        // ������ ������ ������� ������ ��� ����������� � Master Server
        UpdateRoomButtonsInteractable();
    }

    void Update()
    {
        // ��������� ��������� ������ ������ � Update
        UpdateRoomButtonsInteractable();
    }

    private void UpdateRoomButtonsInteractable()
    {
        bool isConnectedAndNotInRoom = PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom;
        if (createRoomButton != null) createRoomButton.interactable = isConnectedAndNotInRoom && !string.IsNullOrEmpty(createRoomInputField?.text);
        if (joinRoomButton != null) joinRoomButton.interactable = isConnectedAndNotInRoom && !string.IsNullOrEmpty(joinRoomInputField?.text);
        if (setNicknameButton != null) setNicknameButton.interactable = isConnectedAndNotInRoom && !string.IsNullOrEmpty(nicknameInputField?.text);
    }

    // --- ������ ��� ������ � ��������� ---
    public void OnClick_SetName()
    {
        if (nicknameInputField != null && !string.IsNullOrEmpty(nicknameInputField.text))
        {
            string newNickname = nicknameInputField.text;
            PhotonNetwork.NickName = newNickname;
            PlayerPrefs.SetString("PlayerNickname", newNickname); // ��������� ������� ��������
            PlayerPrefs.Save();
            Debug.Log($"������� ����������: {newNickname}");
        }
        else
        {
            Debug.LogWarning("����������, ������� �������.");
        }
    }

    // --- ������ ��� ������ � ��������� (���� ������������) ---
    public void CreateRoom()
    {
        if (currentlySelectedCharacter == null)
        {
            Debug.LogWarning("����������, �������� ��������� ����� ��������� �������.");
            return;
        }
        if (!PhotonNetwork.IsConnectedAndReady || PhotonNetwork.InRoom)
        {
            Debug.LogWarning("�� ���������� � Photon Master Server ��� ��� � �������. ���������� ������� �������.");
            return;
        }

        SetLocalPlayerCharacterProperty();

        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 8 }; // ���� MaxPlayers
        PhotonNetwork.CreateRoom(createRoomInputField.text, roomOptions);
        Debug.Log($"������� ������� �������: {createRoomInputField.text}");
    }

    public void JoinRoom()
    {
        if (currentlySelectedCharacter == null)
        {
            Debug.LogWarning("����������, �������� ��������� ����� �������������� � �������.");
            return;
        }
        if (!PhotonNetwork.IsConnectedAndReady || PhotonNetwork.InRoom)
        {
            Debug.LogWarning("�� ���������� � Photon Master Server ��� ��� � �������. ���������� ��������������.");
            return;
        }

        SetLocalPlayerCharacterProperty();

        PhotonNetwork.JoinRoom(joinRoomInputField.text);
        Debug.Log($"������� �������������� � �������: {joinRoomInputField.text}");
    }

    // --- Callbacks Photon ---
    public override void OnCreatedRoom()
    {
        Debug.Log($"������� '{PhotonNetwork.CurrentRoom.Name}' ������� �������!");
        // ����� �������� ������� �� ������������� �������������� � ���,
        // � ��������� OnJoinedRoom()
        if (currentlySelectedCharacter != null)
        {
            Hashtable customProps = new Hashtable();
            customProps[PhotonPlayerCharProperty] = selectedCharName;//currentlySelectedCharacter.name;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            Debug.Log($"����������� Photon Custom Property ��� {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");
        }
        else
        {
            Debug.LogWarning("�������� �� ������. ����� ������������� ��� �������� ���������.");
            // ����� ���������� ��������� �������� ��� ������������� �������������.
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"�������������� � �������: {PhotonNetwork.CurrentRoom.Name}");

        // === �������� ���������: ������������� Custom Property ��������� ===
        if (currentlySelectedCharacter != null)
        {
            Hashtable customProps = new Hashtable();
            customProps[PhotonPlayerCharProperty] = selectedCharName;//currentlySelectedCharacter.name;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            Debug.Log($"����������� Photon Custom Property ��� {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");
        }
        else
        {
            Debug.LogWarning("�������� �� ������. ����� ������������� ��� �������� ���������.");
            // ����� ���������� ��������� �������� ��� ������������� �������������.
        }

        // === �����: ������ ������-������ ��������� ����� ===
        if (PhotonNetwork.IsMasterClient && roEvNum == 0)
        {
            if (currentlySelectedCharacter != null)
            {
                Hashtable customProps = new Hashtable();
                customProps[PhotonPlayerCharProperty] = selectedCharName;//currentlySelectedCharacter.name;
                PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
                Debug.Log($"����������� Photon Custom Property ��� {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");
            }
            Debug.Log("� ������-������. �������� ������� ����� 'SampleScene'.");
            PhotonNetwork.LoadLevel("GameScene"); // �������� �� ��� ����� ������� �����
        }
        else
        {
            Debug.Log("� �� ������-������. ���, ���� ������-������ �������� �����.");
        }

        if (PhotonNetwork.IsMasterClient && roEvNum == 1)
        {
            if (currentlySelectedCharacter != null)
            {
                Hashtable customProps = new Hashtable();
                customProps[PhotonPlayerCharProperty] = selectedCharName;//currentlySelectedCharacter.name;
                PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
                Debug.Log($"����������� Photon Custom Property ��� {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");
            }
            Debug.Log("� ������-������. �������� ������� ����� 'SampleScene'.");
            PhotonNetwork.LoadLevel("GameSceneS"); // �������� �� ��� ����� ������� �����
        }
        else
        {
            Debug.Log("� �� ������-������. ���, ���� ������-������ �������� �����.");
        }

        if (PhotonNetwork.IsMasterClient && roEvNum == 2)
        {
            if (currentlySelectedCharacter != null)
            {
                Hashtable customProps = new Hashtable();
                customProps[PhotonPlayerCharProperty] = selectedCharName;//currentlySelectedCharacter.name;
                PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
                Debug.Log($"����������� Photon Custom Property ��� {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");
            }
            Debug.Log("� ������-������. �������� ������� ����� 'SampleScene'.");
            PhotonNetwork.LoadLevel("GameSceneS 1"); // �������� �� ��� ����� ������� �����
        }
        else
        {
            if (currentlySelectedCharacter != null)
            {
                Hashtable customProps = new Hashtable();
                customProps[PhotonPlayerCharProperty] = selectedCharName;//currentlySelectedCharacter.name;
                PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
                Debug.Log($"����������� Photon Custom Property ��� {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");
            }
            Debug.Log("� �� ������-������. ���, ���� ������-������ �������� �����.");
        }

        if (PhotonNetwork.IsMasterClient && roEvNum == 3)
        {
            Debug.Log("� ������-������. �������� ������� ����� 'SampleScene'.");
            PhotonNetwork.LoadLevel("GameSceneS 2"); // �������� �� ��� ����� ������� �����
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"������ �������� �������: {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"������ ������������� � �������: {message}");
    }

    // --- ������ ������ ���������� ---
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
                    characterImage.sprite = characterData.characterIcon;
                    itemUI.Initialize(characterData, this);
                }
            }
        }
    }

    public void SelectCharacter(PlayerCharacterData data)
    {
        currentlySelectedCharacter = data;
        selectedCharName = data.name;
        characterImage.sprite = data.characterIcon;
        PlayerPrefs.SetString(SelectedCharacterKey, data.name); // ��������� ��� ScriptableObject ��������
        PlayerPrefs.Save();
        Debug.Log($"�������� ������ ��������: {data.characterName}. ��� ScriptableObject: {data.name}");

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
                    Debug.Log($"�������� ����� ��������� �������� �� PlayerPrefs: {data.characterName}");
                    break;
                }
            }
        }
        UpdateSelectedCharacterUI();
    }

    public void DclxviclanRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8, // Example: Set max players
            IsVisible = true,
            IsOpen = true
        };

        if (currentlySelectedCharacter != null)
        {
            Hashtable customProps = new Hashtable();
            customProps[PhotonPlayerCharProperty] = selectedCharName;//currentlySelectedCharacter.name;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            Debug.Log($"����������� Photon Custom Property ��� {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");
        }

        roEvNum = 0;
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public void CreateOrJoinSpecificRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8, // Example: Set max players
            IsVisible = true,
            IsOpen = true
        };
        //  roEvNum = 1;
        if (currentlySelectedCharacter != null)
        {
            Hashtable customProps = new Hashtable();
            customProps[PhotonPlayerCharProperty] = selectedCharName;//currentlySelectedCharacter.name;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            Debug.Log($"����������� Photon Custom Property ��� {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");
        }

        roEvNum = 1;
        
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public void CreateOrDestroyRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8, // Example: Set max players
            IsVisible = true,
            IsOpen = true
        };
        //  roEvNum = 1;

        if (currentlySelectedCharacter != null)
        {
            Hashtable customProps = new Hashtable();
            customProps[PhotonPlayerCharProperty] = selectedCharName;//currentlySelectedCharacter.name;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            Debug.Log($"����������� Photon Custom Property ��� {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");
        }
        roEvNum = 3;

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public void CreateOrJoinSSpecificRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8, // Example: Set max players
            IsVisible = true,
            IsOpen = true
        };
        //  roEvNum = 1;
        if (currentlySelectedCharacter != null)
        {
            Hashtable customProps = new Hashtable();
            customProps[PhotonPlayerCharProperty] = selectedCharName;//currentlySelectedCharacter.name;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            Debug.Log($"����������� Photon Custom Property ��� {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");
        }

        roEvNum = 2;

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
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
            customProps[PhotonPlayerCharProperty] = selectedCharName;// customProps[PhotonPlayerCharProperty] = currentlySelectedCharacter.name;
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
            selectedCharacterNameText.text = currentlySelectedCharacter != null ? $"������: {currentlySelectedCharacter.characterName}" : "�������� ���������";
        }
    }
}
