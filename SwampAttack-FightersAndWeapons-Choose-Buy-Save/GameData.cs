using System.Collections.Generic;
using UnityEngine;

// [Serializable] необходим для JsonUtility
[System.Serializable]
public class GameData
{
    // Общие данные
    public int Money = 1000;

    // Выбранные индексы для активации в иерархии
    // Ключ: ID бойца (скина), Значение: индекс выбранного оружия
    public Dictionary<string, int> SelectedWeaponIndices = new Dictionary<string, int>();

    // Список статусов разблокировки (требуется для JsonUtility, который не работает с Dictionary)
    public List<ItemSaveData> ItemStatus = new List<ItemSaveData>();
}

// Отдельный класс для сохранения статуса каждого предмета
[System.Serializable]
public class ItemSaveData
{
    public string ID; // Уникальный ID предмета (например, "Fighter_01" или "Fighter_01_Gun_02")
    public bool IsUnlocked; // Разблокирован ли предмет
}
