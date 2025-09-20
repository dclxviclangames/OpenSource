using System.Collections.Generic;
using UnityEngine;
using System.IO; // Добавляем для работы с файловой системой
using LitJson; // Для работы с JSON
using UnityEngine.UI;

// Этот скрипт управляет ресурсами (и инструментами) для одного NPC.
// Прикрепите его к вашему PlayerMob.
public class ResourceInventory : MonoBehaviour
{
    public Dictionary<string, int> resources = new Dictionary<string, int>();

    // Имя папки внутри Application.persistentDataPath для наших сохранений
    private const string SAVE_FOLDER_NAME = "GameSaves";
    // Полный путь к файлу сохранения для этого конкретного NPC
    private string saveFilePath;

    public Text moneyText;


    void Awake()
    {
        // Определяем полный путь к папке сохранения
        string saveFolder = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
        // Убедимся, что папка существует
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
            Debug.Log($"Created save folder: {saveFolder}");
        }

        // Определяем полный путь к файлу сохранения для этого NPC
        saveFilePath = Path.Combine(saveFolder, $"{gameObject.name}_Inventory.json");

        // Пытаемся загрузить ресурсы из файла при старте
        LoadResourcesFromFile();

        // Если после загрузки ресурсы все еще пусты (т.е. файл не найден или пуст),
        // инициализируем их значениями по умолчанию.
        if (resources.Count == 0)
        {
            Debug.Log($"No saved resources found for {gameObject.name}. Initializing with default values.");
            resources.Add("MetalScrap", 50);
            resources.Add("CopperWire", 0);
            resources.Add("Pickaxe", 0);
            resources.Add("ToolKit", 0);
            // Добавьте другие стартовые ресурсы здесь по умолчанию
            SaveResourcesToFile(); // Сохраняем значения по умолчанию
        }
    }

    // Этот метод вызывается при выходе из приложения или когда объект деактивируется/уничтожается
    void OnApplicationQuit()
    {
        // Хорошая практика: сохранять данные при выходе из игры
        SaveResourcesToFile();
    }

    void OnDisable()
    {
        // Также можно сохранять, когда объект становится неактивным
        // SaveResourcesToFile();
    }

     void Update()
     {
        UpdateResourcesUI();
     } 

    public void UpdateResourcesUI()
    {
        if (moneyText == null)
        {
            Debug.LogWarning("ResourceInventory: resourcesTextDisplay is not assigned in the Inspector. Cannot update UI.");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder(); // Используем StringBuilder для эффективности

        sb.AppendLine("--- Resources ---"); // Заголовок

        foreach (var entry in resources)
        {
            // entry.Key - это string (имя ресурса)
            // entry.Value - это int (количество ресурса)
            // Используем .ToString() для преобразования int в string
            sb.AppendLine($"{entry.Key}: {entry.Value.ToString()}");
        }

        moneyText.text = sb.ToString(); // Присваиваем сформированную строку UI Text компоненту
        Debug.Log("Resources UI updated.");
    }

    public bool HasResources(string resourceName, int amount)
    {
        return resources.ContainsKey(resourceName) && resources[resourceName] >= amount;
    }

    public void ConsumeResources(string resourceName, int amount)
    {
        if (HasResources(resourceName, amount))
        {
            resources[resourceName] -= amount;
            Debug.Log($"{gameObject.name} consumed {amount} {resourceName}. Remaining: {resources[resourceName]}");
            SaveResourcesToFile(); // Сохраняем после изменения
        }
        else
        {
           // Debug.LogWarning($"{gameObject.name} tried to consume {amount} {resourceName} but only had {resources.ContainsKey(resourceName) ? resources[resourceName] : 0}.");
        }
    }

    public void AddResources(string resourceName, int amount)
    {
        if (resources.ContainsKey(resourceName))
        {
            resources[resourceName] += amount;
        }
        else
        {
            resources.Add(resourceName, amount);
        }
        Debug.Log($"{gameObject.name} added {amount} {resourceName}. Total: {resources[resourceName]}");
        SaveResourcesToFile(); // Сохраняем после изменения
    }

    public bool TransferResource(string resourceName, int amount, ResourceInventory targetInventory)
    {
        if (targetInventory == null)
        {
            Debug.LogError("TransferResource: Target inventory is null.");
            return false;
        }

        if (HasResources(resourceName, amount))
        {
            this.ConsumeResources(resourceName, amount);
            targetInventory.AddResources(resourceName, amount);
            Debug.Log($"{gameObject.name} successfully transferred {amount} {resourceName} to {targetInventory.gameObject.name}.");
            // Обе стороны сохранят свои инвентари, так как Add/Consume вызывает SaveResourcesToFile()
            return true;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} failed to transfer {amount} {resourceName}. Not enough resources.");
            return false;
        }
    }

    public bool HasTool(string toolName)
    {
        return HasResources(toolName, 1);
    }

    // --- Методы сохранения и загрузки в локальный файл ---

    public void SaveResourcesToFile()
    {
        try
        {
            // Преобразуем Dictionary в JSON строку
            string json = JsonMapper.ToJson(resources);
            // Записываем JSON строку в файл
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Resources saved to: {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving resources for {gameObject.name} to file: {e.Message}");
        }
    }

    public void LoadResourcesFromFile()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                // Читаем JSON строку из файла
                string json = File.ReadAllText(saveFilePath);
                // Десериализуем JSON обратно в Dictionary
                resources = JsonMapper.ToObject<Dictionary<string, int>>(json);
                Debug.Log($"Resources loaded from: {saveFilePath}");
            }
            else
            {
                Debug.Log($"Save file not found for {gameObject.name} at {saveFilePath}. Starting with default values (if resources.Count == 0).");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading resources for {gameObject.name} from file: {e.Message}");
            // В случае ошибки загрузки, можно инициализировать ресурсы по умолчанию
            resources.Clear(); // Очищаем, чтобы затем Awake инициализировал заново
        }
    }
}
