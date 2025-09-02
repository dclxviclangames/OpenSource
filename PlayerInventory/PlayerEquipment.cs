using System.Collections.Generic;
using UnityEngine;
using LitJson; // Не забудьте импортировать библиотеку LitJson!

public class PlayerEquipment : MonoBehaviour
{
    [System.Serializable]
    public class EquipSlotTransform
    {
        public EquipSlot slotType;
        public Transform slotTransform;
    }

    [Tooltip("Список трансформов для слотов экипировки.")]
    public List<EquipSlotTransform> equipSlots;
    
    private Dictionary<EquipSlot, ItemData> equippedItems = new Dictionary<EquipSlot, ItemData>();
    private Dictionary<EquipSlot, GameObject> equippedModels = new Dictionary<EquipSlot, GameObject>();
    
    private string saveFilePath;

    void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/equipment.json";
    }

    /// <summary>
    /// Экипирует предмет.
    /// </summary>
    public void EquipItem(ItemData item)
    {
        if (item.equipSlot == EquipSlot.None)
        {
            Debug.LogWarning($"[PlayerEquipment] Предмет '{item.itemName}' не имеет слота экипировки. Невозможно экипировать.");
            return;
        }

        // Если в слоте уже есть предмет, сначала снимаем его.
        if (equippedItems.ContainsKey(item.equipSlot))
        {
            UnequipItem(equippedItems[item.equipSlot]);
        }
        
        // Ищем нужный трансформ для экипировки.
        Transform slotTransform = equipSlots.Find(s => s.slotType == item.equipSlot)?.slotTransform;
        if (slotTransform != null)
        {
            // Создаем модель предмета и прикрепляем к трансформу.
            if (item.itemModelPrefab != null)
            {
                GameObject model = Instantiate(item.itemModelPrefab, slotTransform);
                equippedModels[item.equipSlot] = model;
            }
            equippedItems[item.equipSlot] = item;
            Debug.Log($"[PlayerEquipment] Предмет '{item.itemName}' экипирован в слот '{item.equipSlot}'.");
        }
        else
        {
            Debug.LogError($"[PlayerEquipment] Трансформ для слота '{item.equipSlot}' не найден!");
        }
    }

    /// <summary>
    /// Снимает предмет.
    /// </summary>
    public void UnequipItem(ItemData item)
    {
        if (equippedItems.ContainsKey(item.equipSlot))
        {
            if (equippedModels.ContainsKey(item.equipSlot))
            {
                Destroy(equippedModels[item.equipSlot]);
                equippedModels.Remove(item.equipSlot);
            }
            equippedItems.Remove(item.equipSlot);
            Debug.Log($"[PlayerEquipment] Предмет '{item.itemName}' снят.");
        }
    }

    /// <summary>
    /// Сохраняет экипированные предметы.
    /// </summary>
    public void SaveEquipmentState()
    {
        var dataToSave = new List<EquipData>();
        foreach (var entry in equippedItems)
        {
            dataToSave.Add(new EquipData { slot = entry.Key, itemName = entry.Value.itemName });
        }
        string json = JsonMapper.ToJson(dataToSave);
        System.IO.File.WriteAllText(saveFilePath, json);
        Debug.Log($"[PlayerEquipment] Экипировка сохранена в {saveFilePath}");
    }

    /// <summary>
    /// Загружает экипированные предметы.
    /// </summary>
    public void LoadEquipmentState()
    {
        if (System.IO.File.Exists(saveFilePath))
        {
            string json = System.IO.File.ReadAllText(saveFilePath);
            var dataToLoad = JsonMapper.ToObject<List<EquipData>>(json);
            
            // Сначала снимаем все текущие предметы.
            foreach (var item in equippedItems.Values)
            {
                Destroy(equippedModels[item.equipSlot]);
            }
            equippedItems.Clear();
            equippedModels.Clear();

            // Экипируем сохраненные предметы.
            foreach (var data in dataToLoad)
            {
                ItemData item = Resources.Load<ItemData>($"Items/{data.itemName}");
                if (item != null)
                {
                    EquipItem(item);
                }
            }
            Debug.Log($"[PlayerEquipment] Экипировка загружена из {saveFilePath}");
        }
        else
        {
            Debug.LogWarning("[PlayerEquipment] Файл сохранения экипировки не найден.");
        }
    }
}

// Класс для сохранения данных об экипировке.
[System.Serializable]
public class EquipData
{
    public EquipSlot slot;
    public string itemName;
}
