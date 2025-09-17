using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

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

    [Header("Dependencies")]
    [Tooltip("Объект, на который будут экипироваться предметы.")]
    public PlayerEquipment playerEquipment;

    [Tooltip("Drag all your ItemData ScriptableObjects here.")]
    public List<ItemData> allItemDatas;

    private InventoryGrid grid;
    private InventorySlotUI[,] inventorySlots;
    private Dictionary<string, ItemData> itemDataLookup = new Dictionary<string, ItemData>();

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

        // Создаем словарь для быстрого поиска ItemData по имени.
        foreach (var item in allItemDatas)
        {
            if (!itemDataLookup.ContainsKey(item.itemName))
            {
                itemDataLookup.Add(item.itemName, item);
            }
        }
    }

    void Start()
    {
        InitializeInventoryGrid();
        LoadInventory();
    }

    /// <summary>
    /// Инициализирует инвентарную сетку.
    /// </summary>
    private void InitializeInventoryGrid()
    {
        grid = new InventoryGrid(inventoryWidth, inventoryHeight);
        inventorySlots = new InventorySlotUI[inventoryWidth, inventoryHeight];

        // Уничтожаем старые слоты, если они есть.
        foreach (Transform child in inventorySlotsContainer)
        {
            Destroy(child.gameObject);
        }

        // Создаем новые слоты.
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
    }

    public InventorySlotUI GetSlotAt(int x, int y)
    {
        if (x >= 0 && x < inventoryWidth && y >= 0 && y < inventoryHeight)
        {
            return inventorySlots[x, y];
        }
        return null;
    }

    /// <summary>
    /// Добавляет предмет в инвентарь (например, при подборе).
    /// </summary>
    public bool AddItem(ItemData itemData)
    {
        // Ищем свободное место.
        for (int y = 0; y < inventoryHeight; y++)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                if (grid.CanPlaceItem(itemData, x, y))
                {
                    return PlaceItem(itemData, x, y);
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Пытается разместить предмет в инвентаре.
    /// </summary>
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
    /// Перемещает предмет с одной позиции в другую.
    /// </summary>
    public bool MoveItem(ItemData item, InventorySlotUI fromSlot, InventorySlotUI toSlot)
    {
        Vector2Int fromPos = fromSlot.GetGridPosition();
        Vector2Int toPos = toSlot.GetGridPosition();

        if (grid.MoveItem(item, fromPos.x, fromPos.y, toPos.x, toPos.y))
        {
            // Находим и обновляем UI-элемент предмета.
            InventoryItemUI itemUI = inventorySlots[fromPos.x, fromPos.y].GetComponentInChildren<InventoryItemUI>();
            if (itemUI != null)
            {
                // Привязываем UI-объект к новой родительской ячейке
                itemUI.transform.SetParent(toSlot.transform);
                itemUI.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            SaveInventory();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Удаляет предмет из инвентаря.
    /// </summary>
    public void RemoveItem(ItemData itemData)
    {
        grid.RemoveItem(itemData);
    }

    public void SaveInventory()
    {
        List<InventoryData> dataToSave = grid.GetItemDataForSave();
        string json = JsonMapper.ToJson(dataToSave);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("[InventoryManager] Inventory saved.");
    }

    public void LoadInventory()
    {
        // Очищаем старые предметы перед загрузкой.
        grid.ClearGrid();
        foreach (Transform child in inventoryItemsContainer)
        {
            Destroy(child.gameObject);
        }

        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            var dataToLoad = JsonMapper.ToObject<List<InventoryData>>(json);

            foreach (var data in dataToLoad)
            {
                if (itemDataLookup.TryGetValue(data.itemName, out ItemData item))
                {
                    PlaceItem(item, data.x, data.y);
                }
            }
            Debug.Log("[InventoryManager] Inventory loaded.");
        }
        else
        {
            Debug.LogWarning("[InventoryManager] Inventory save file not found. Creating a new one.");
        }
    }
}


public class InventoryData
{
    public string itemName;
    public int x;
    public int y;
}

