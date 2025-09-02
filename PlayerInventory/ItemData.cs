using UnityEngine;

// Enum для определения слотов экипировки.
public enum EquipSlot
{
    None,
    Head,
    Body,
    Legs,
    Feet,
    RightHand,
    LeftHand,
    Back
}

// Атрибут, который позволяет создавать этот объект через меню Unity.
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Tooltip("Название предмета.")]
    public string itemName;
    [Tooltip("Иконка предмета для инвентаря.")]
    public Sprite icon;
    [Tooltip("Модель предмета, которая будет экипирована на игрока.")]
    public GameObject itemModelPrefab;
    
    [Header("Inventory Grid")]
    [Tooltip("Ширина предмета в ячейках инвентаря.")]
    public int width = 1;
    [Tooltip("Высота предмета в ячейках инвентаря.")]
    public int height = 1;

    [Header("Equipment")]
    [Tooltip("Слот, который предмет будет занимать при экипировке.")]
    public EquipSlot equipSlot = EquipSlot.None;
}
