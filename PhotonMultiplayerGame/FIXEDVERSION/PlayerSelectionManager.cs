/* PlayerSelectionManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun; // ��� ������ � Photon Custom Properties
using Photon.Realtime; // ��� Hashtable
using ExitGames.Client.Photon; // ��� Hashtable

/// <summary>
/// ��������� UI ���� ������ ����������.
/// ��������� PlayerCharacterData ScriptableObjects, ����������� ������� �������� UI,
/// � ��������� ����� ������ � PlayerPrefs � � Photon Player Custom Properties.
/// </summary>
public class PlayerSelectionManager : MonoBehaviourPunCallbacks // ����������� ��� OnConnectedToMaster
{
    [Header("UI ��������")]
    public Transform selectionItemContainer;
    public GameObject playerSelectionItemUIPrefab;
    public Button startGameButton;
    public TMPro.TextMeshProUGUI selectedCharacterNameText;
    public TMPro.TextMeshProUGUI connectionStatusText; // ��� ����������� ������� ����������� Photon

    [Header("������")]
    public List<PlayerCharacterData> allAvailableCharacters = new List<PlayerCharacterData>();

    private PlayerCharacterData currentlySelectedCharacter;

    // ����� ��� PlayerPrefs � Photon Custom Properties
    public const string SelectedCharacterKey = "SelectedPlayerCharacterName"; // ��� ScriptableObject
    public const string PhotonPlayerCharProperty = "CharName"; // ���� ��� Custom Property Photon

    void Start()
    {
        PopulateSelectionUI();
        LoadSelectedCharacter(); // ��������� ����� ���������� ��������� �� PlayerPrefs

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGame);
            // ������ �������, ������ ���� �������� ������ � �� ���������� � Photon
            startGameButton.interactable = (currentlySelectedCharacter != null && PhotonNetwork.IsConnectedAndReady);
        }
        UpdateConnectionStatus();
    }

    void Update()
    {
        // ��������� ��������� ������ "������ ����"
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
            connectionStatusText.text = $"Photon: {(PhotonNetwork.IsConnectedAndReady ? "���������" : "��������")}";
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

    private void UpdateSelectedCharacterUI()
    {
        if (selectedCharacterNameText != null)
        {
            selectedCharacterNameText.text = currentlySelectedCharacter != null ? $"������: {currentlySelectedCharacter.characterName}" : "�������� ���������";
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
            Debug.LogWarning("����������, �������� ��������� ����� ������� ����.");
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("�� ���������� � Photon, ������������...");
            PhotonNetwork.ConnectUsingSettings(); // ���������� ��������� �� PhotonServerSettings
            // ����� ����������� OnConnectedToMaster() ����� ������.
            // ��� �� ����� ����������� �������������� � ������� � ���������� Custom Property.
        }
        else
        {
            // ���� ��� ����������, ����� ��������� � ��������� �������� � �������� �����
            SetCharacterCustomPropertyAndLoadGame();
        }
    }

    // --- ��������� ������� Photon ---

    public override void OnConnectedToMaster()
    {
        Debug.Log("���������� � Photon Master Server.");
        // ������, ����� ����������, ����� ���������� Custom Property � ������� � ����
        SetCharacterCustomPropertyAndLoadGame();
    }

    private void SetCharacterCustomPropertyAndLoadGame()
    {
        if (currentlySelectedCharacter == null)
        {
            Debug.LogError("�� ������ �������� ��� ��������� Custom Property!");
            return;
        }

        // ������������� Custom Property ��� ���������� ������
        // ����: "CharName", ��������: ��� ScriptableObject ���������
        Hashtable customProps = new Hashtable();
        customProps[PhotonPlayerCharProperty] = currentlySelectedCharacter.name;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);

        Debug.Log($"����������� Photon Custom Property ��� {PhotonNetwork.LocalPlayer.NickName}: {PhotonPlayerCharProperty} = {currentlySelectedCharacter.name}");

        // ����� ��������� ��������, ��������� ������� �����
        // GameManager � ������� ����� ����� ������������ ��� Custom Property ��� ������.
        PhotonNetwork.LoadLevel("GameScene");
    }
}
*/
