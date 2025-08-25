// PlayerSelectionItemUI.cs
using UnityEngine;
using UnityEngine.UI; // Для Image и Text
using TMPro; // Для TextMeshPro, если вы его используете

/// <summary>
/// Управляет отображением одного элемента выбора персонажа в UI.
/// </summary>
public class PlayerSelectionItemUI : MonoBehaviour
{
    [Tooltip("Ссылка на данные персонажа, который будет отображаться этим элементом.")]
    public PlayerCharacterData characterData;

    [Tooltip("Ссылка на UI Text/TextMeshPro для имени персонажа.")]
    public TextMeshProUGUI characterNameText; // Или public Text characterNameText;

    [Tooltip("Ссылка на UI Image для иконки персонажа.")]
    public Image characterIconImage;

    [Tooltip("Ссылка на UI Button этого элемента.")]
    public Button selectButton;

    // Ссылка на менеджер выбора персонажей
    public Menu selectionManager;

    /// <summary>
    /// Инициализирует элемент UI с данными персонажа.
    /// </summary>
    /// <param name="data">Данные персонажа.</param>
    /// <param name="manager">Менеджер выбора персонажей.</param>
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

        // Привязываем метод выбора к кнопке
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectCharacter);
        }
    }

    /// <summary>
    /// Вызывается при нажатии на кнопку выбора персонажа.
    /// </summary>
    public void OnSelectCharacter()
    {
        if (selectionManager != null)
        {
            selectionManager.SelectCharacter(characterData);
            Debug.Log($"Выбран персонаж: {characterData.characterName}");
        }
    }

    void OnDestroy()
    {
        // Отписываемся от события кнопки, чтобы избежать утечек памяти
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(OnSelectCharacter);
        }
    }
}
