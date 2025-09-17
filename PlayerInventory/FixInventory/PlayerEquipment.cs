using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson; // Убедитесь, что эта библиотека импортирована в ваш проект!

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

    // Используем словари для быстрого доступа к экипированным предметам и их моделям.
    private Dictionary<EquipSlot, ItemData> equippedItems = new Dictionary<EquipSlot, ItemData>();
    private Dictionary<EquipSlot, GameObject> equippedModels = new Dictionary<EquipSlot, GameObject>();

    private string saveFilePath;

    void Awake()
    {
        // Устанавливаем путь для файла сохранения.
        saveFilePath = Application.persistentDataPath + "/equipment.json";
    }

    /// <summary>
    /// Экипирует предмет.
    /// </summary>
    /// <param name="item">Данные предмета для экипировки.</param>
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
            // Проверяем, что у предмета есть модель для экипировки.
            if (item.itemModelPrefab != null)
            {
                // Создаем модель предмета и прикрепляем ее к нужному слоту.
                GameObject itemModel = Instantiate(item.itemModelPrefab, slotTransform);
                equippedModels[item.equipSlot] = itemModel;
            }

            equippedItems[item.equipSlot] = item;
            Debug.Log($"[PlayerEquipment] Предмет '{item.itemName}' успешно экипирован.");
        }
        else
        {
            Debug.LogError($"[PlayerEquipment] Не найден трансформ для слота {item.equipSlot}.");
        }
    }

    /// <summary>
    /// Снимает предмет.
    /// </summary>
    /// <param name="item">Данные предмета для снятия.</param>
    public void UnequipItem(ItemData item)
    {
        if (equippedItems.ContainsKey(item.equipSlot))
        {
            // Удаляем модель предмета.
            if (equippedModels.ContainsKey(item.equipSlot) && equippedModels[item.equipSlot] != null)
            {
                Destroy(equippedModels[item.equipSlot]);
                equippedModels.Remove(item.equipSlot);
            }

            equippedItems.Remove(item.equipSlot);
            Debug.Log($"[PlayerEquipment] Предмет '{item.itemName}' успешно снят.");
        }
    }

    /// <summary>
    /// Сохраняет экипированные предметы.
    /// </summary>
    public void SaveEquipmentState()
    {
        List<EquipData> dataToSave = new List<EquipData>();
        foreach (var pair in equippedItems)
        {
            dataToSave.Add(new EquipData
            {
                slot = pair.Key,
                itemName = pair.Value.itemName
            });
        }
        string json = JsonMapper.ToJson(dataToSave);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"[PlayerEquipment] Экипировка сохранена в {saveFilePath}");
    }

    /// <summary>
    /// Загружает экипированные предметы.
    /// </summary>
    public void LoadEquipmentState()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            var dataToLoad = JsonMapper.ToObject<List<EquipData>>(json);

            // Сначала снимаем все текущие предметы.
            foreach (var item in equippedItems.Values)
            {
                if (equippedModels.ContainsKey(item.equipSlot))
                {
                    Destroy(equippedModels[item.equipSlot]);
                }
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
                else
                {
                    Debug.LogWarning($"[PlayerEquipment] Не удалось найти предмет с именем: {data.itemName}");
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

