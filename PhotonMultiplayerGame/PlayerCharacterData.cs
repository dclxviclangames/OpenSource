// PlayerCharacterData.cs
using UnityEngine;

/// <summary>
/// ScriptableObject для хранения данных о каждом игровом персонаже.
/// Это позволяет легко создавать новых персонажей в редакторе без изменения кода.
/// </summary>
[CreateAssetMenu(fileName = "NewPlayerCharacter", menuName = "Game Data/Player Character")]
public class PlayerCharacterData : ScriptableObject
{
    [Tooltip("Имя персонажа, отображаемое в UI.")]
    public string characterName = "Новый Персонаж";

    [Tooltip("Префаб 3D модели этого персонажа. Должен быть в папке Resources/PhotonPrefabs.")]
    public GameObject characterPrefab;

    [Tooltip("Иконка, отображаемая в UI выбора персонажа.")]
    public Sprite characterIcon;

    [Tooltip("Начальное здоровье для этого персонажа.")]
    public int baseHealth = 100;

    // Добавьте другие характеристики, если необходимо (скорость, урон и т.д.)
    // public float moveSpeed = 5f;
    // public int baseDamage = 10;
}