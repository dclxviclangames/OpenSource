using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [Header("Inventory Grid Settings")]
    [Tooltip("Ширина инвентаря в ячейках.")]
    public int inventoryWidth = 50;
    [Tooltip("Высота инвентаря в ячейках.")]
    public int inventoryHeight = 50;
    [Tooltip("Префаб UI-ячейки инвентаря.")]
    public GameObject inventorySlotPrefab;
    [Tooltip("Контейнер (родительский объект) для всех UI-ячеек.")]
    public Transform inventorySlotsContainer;
    
    [Header("UI References")]
    [Tooltip("Контейнер (родительский объект) для UI-предметов.")]
    public Transform inventoryItemsContainer;
    [Tooltip("Префаб UI-предмета.")]
    public GameObject inventoryItemPrefab;
    [Tooltip("Объект, на который будут экипироваться предметы.")]
    public PlayerEquipment playerEquipment;

    private InventoryGrid grid;
    private InventorySlotUI[,] inventorySlots;

    private string saveFilePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        saveFilePath = Application.persistentDataPath + "/inventory.json";
    }

    void Start()
    {
        InitializeInventoryGrid();
        LoadInventory();
    }

    /// <summary>
    /// Инициализация логики и визуального представления инвентаря.
    /// </summary>
    void InitializeInventoryGrid()
    {
        grid = new InventoryGrid(inventoryWidth, inventoryHeight);
        inventorySlots = new InventorySlotUI[inventoryWidth, inventoryHeight];

        // Создаем все ячейки инвентаря
        for (int y = 0; y < inventoryHeight; y++)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                GameObject slotGO = Instantiate(inventorySlotPrefab, inventorySlotsContainer);
                InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
                slotUI.SetPosition(x, y);
                inventorySlots[x, y] = slotUI;
            }
        }
        Debug.Log($"[InventoryManager] Инвентарь инициализирован. Сетка: {inventoryWidth}x{inventoryHeight}.");
    }

    /// <summary>
    /// Находит свободное место и добавляет предмет в инвентарь.
    /// </summary>
    /// <param name="itemData">Данные предмета, который нужно добавить.</param>
    public bool AddItem(ItemData itemData)
    {
        // Ищем первое свободное место в сетке.
        for (int y = 0; y < inventoryHeight; y++)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                // Если ячейка свободна и предмет помещается...
                if (grid.CanPlaceItem(itemData, x, y))
                {
                    // Размещаем предмет и создаем его UI.
                    return PlaceItem(itemData, x, y);
                }
            }
        }
        
        Debug.LogWarning("[InventoryManager] Не удалось найти свободное место для предмета.");
        return false;
    }
    
    /// <summary>
    /// Сохраняет текущее состояние инвентаря в JSON-файл.
    /// </summary>
    public void SaveInventory()
    {
        string json = JsonMapper.ToJson(grid.GetItemDataForSave());
        System.IO.File.WriteAllText(saveFilePath, json);
        Debug.Log($"[InventoryManager] Инвентарь сохранен в {saveFilePath}");

        playerEquipment.SaveEquipmentState();
    }

    /// <summary>
    /// Загружает состояние инвентаря из JSON-файла.
    /// </summary>
    public void LoadInventory()
    {
        if (System.IO.File.Exists(saveFilePath))
        {
            string json = System.IO.File.ReadAllText(saveFilePath);
            var itemDataList = JsonMapper.ToObject<List<InventoryData>>(json);
            
            // Очищаем существующие предметы в UI
            foreach (Transform child in inventoryItemsContainer)
            {
                Destroy(child.gameObject);
            }
            grid.ClearGrid();

            foreach (var itemData in itemDataList)
            {
                ItemData item = Resources.Load<ItemData>($"Items/{itemData.name}");
                if (item != null)
                {
                    PlaceItem(item, itemData.x, itemData.y);
                }
            }
            Debug.Log($"[InventoryManager] Инвентарь загружен из {saveFilePath}");
        }
        else
        {
            Debug.LogWarning("[InventoryManager] Файл сохранения инвентаря не найден. Создаем новый.");
        }

        playerEquipment.LoadEquipmentState();
    }

    /// <summary>
    /// Пытается разместить предмет в инвентаре.
    /// </summary>
    /// <param name="itemData">Данные предмета.</param>
    /// <param name="x">Координата X.</param>
    /// <param name="y">Координата Y.</param>
    /// <returns>True, если предмет был размещен успешно.</returns>
    public bool PlaceItem(ItemData itemData, int x, int y)
    {
        if (grid.PlaceItem(itemData, x, y))
        {
            GameObject itemGO = Instantiate(inventoryItemPrefab, inventoryItemsContainer);
            InventoryItemUI itemUI = itemGO.GetComponent<InventoryItemUI>();
            itemUI.SetItem(itemData);
            itemUI.SetPosition(x, y);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Удаляет предмет из инвентаря.
    /// </summary>
    /// <param name="itemData">Данные предмета для удаления.</param>
    public void RemoveItem(ItemData itemData)
    {
        grid.RemoveItem(itemData);
    }
}

// Класс для сохранения данных о предмете в JSON.
[System.Serializable]
public class InventoryData
{
    public string name;
    public int x;
    public int y;
}
