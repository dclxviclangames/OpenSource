// PlayerSelectionItemUI.cs
using UnityEngine;
using UnityEngine.UI; // ��� Image � Text
using TMPro; // ��� TextMeshPro, ���� �� ��� �����������

/// <summary>
/// ��������� ������������ ������ �������� ������ ��������� � UI.
/// </summary>
public class PlayerSelectionItemUI : MonoBehaviour
{
    [Tooltip("������ �� ������ ���������, ������� ����� ������������ ���� ���������.")]
    public PlayerCharacterData characterData;

    [Tooltip("������ �� UI Text/TextMeshPro ��� ����� ���������.")]
    public TextMeshProUGUI characterNameText; // ��� public Text characterNameText;

    [Tooltip("������ �� UI Image ��� ������ ���������.")]
    public Image characterIconImage;

    [Tooltip("������ �� UI Button ����� ��������.")]
    public Button selectButton;

    // ������ �� �������� ������ ����������
    public Menu selectionManager;

    /// <summary>
    /// �������������� ������� UI � ������� ���������.
    /// </summary>
    /// <param name="data">������ ���������.</param>
    /// <param name="manager">�������� ������ ����������.</param>
    public void Initialize(PlayerCharacterData data, Menu manager)
    {
        characterData = data;
        selectionManager = manager;

        if (characterNameText != null)
        {
            characterNameText.text = characterData.characterName;
        }
        if (characterIconImage != null && characterData.characterIcon != null)
        {
            characterIconImage.sprite = characterData.characterIcon;
        }

        // ����������� ����� ������ � ������
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectCharacter);
        }
    }

    /// <summary>
    /// ���������� ��� ������� �� ������ ������ ���������.
    /// </summary>
    public void OnSelectCharacter()
    {
        if (selectionManager != null)
        {
            selectionManager.SelectCharacter(characterData);
            Debug.Log($"������ ��������: {characterData.characterName}");
        }
    }

    void OnDestroy()
    {
        // ������������ �� ������� ������, ����� �������� ������ ������
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(OnSelectCharacter);
        }
    }
}
