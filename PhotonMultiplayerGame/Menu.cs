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
    public TMPro.TextMeshProUGUI selectedCharacterNameText; // ����� ��� ����������� ���������� ���������

    [Header("������ ����������")]
    public List<PlayerCharacterData> allAvailableCharacters = new List<PlayerCharacterData>();

    private PlayerCharacterData currentlySelectedCharacter; // ������� ��������� ��������
    public const string SelectedCharacterKey = "SelectedPlayerCharacterName"; // ���� ��� PlayerPrefs
    public const string PhotonPlayerCharProperty = "CharName"; // ���� ��� Custom Property Photon

    void Awake()
    {
        // === �����: ������������� ������������� ���� � Awake ===
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log($"PhotonNetwork.AutomaticallySyncScene = {PhotonNetwork.AutomaticallySyncScene}");
    }

    void Start()
    {
        // ������������� UI �� ������ ������������� �������
        if (createRoomButton != null) createRoomButton.onClick.AddListener(CreateRoom);
        if (joinRoomButton != null) joinRoomButton.onClick.AddListener(JoinRoom);
        if (setNicknameButton != null) setNicknameButton.onClick.AddListener(OnClick_SetName);

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
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"�������������� � �������: {PhotonNetwork.CurrentRoom.Name}");

        // === �������� ���������: ������������� Custom Property ��������� ===
        if (currentlySelectedCharacter != null)
        {
            Hashtable customProps = new Hashtable();
            customProps[PhotonPlayerCharProperty] = currentlySelectedCharacter.name;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            Debug.Log($"����������� Photon Custom Property ��� {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");
        }
        else
        {
            Debug.LogWarning("�������� �� ������. ����� ������������� ��� �������� ���������.");
            // ����� ���������� ��������� �������� ��� ������������� �������������.
        }

        // === �����: ������ ������-������ ��������� ����� ===
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("� ������-������. �������� ������� ����� 'SampleScene'.");
            PhotonNetwork.LoadLevel("GameScene"); // �������� �� ��� ����� ������� �����
        }
        else
        {
            Debug.Log("� �� ������-������. ���, ���� ������-������ �������� �����.");
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
                    itemUI.Initialize(characterData, this);
                }
            }
        }
    }

    public void SelectCharacter(PlayerCharacterData data)
    {
        currentlySelectedCharacter = data;
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
            selectedCharacterNameText.text = currentlySelectedCharacter != null ? $"������: {currentlySelectedCharacter.characterName}" : "�������� ���������";
        }
    }
}
