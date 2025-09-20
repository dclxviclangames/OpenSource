using System.Collections.Generic;
using UnityEngine;
using System.IO; // ��������� ��� ������ � �������� ��������
using LitJson; // ��� ������ � JSON
using UnityEngine.UI;

// ���� ������ ��������� ��������� (� �������������) ��� ������ NPC.
// ���������� ��� � ������ PlayerMob.
public class ResourceInventory : MonoBehaviour
{
    public Dictionary<string, int> resources = new Dictionary<string, int>();

    // ��� ����� ������ Application.persistentDataPath ��� ����� ����������
    private const string SAVE_FOLDER_NAME = "GameSaves";
    // ������ ���� � ����� ���������� ��� ����� ����������� NPC
    private string saveFilePath;

    public Text moneyText;


    void Awake()
    {
        // ���������� ������ ���� � ����� ����������
        string saveFolder = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
        // ��������, ��� ����� ����������
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
            Debug.Log($"Created save folder: {saveFolder}");
        }

        // ���������� ������ ���� � ����� ���������� ��� ����� NPC
        saveFilePath = Path.Combine(saveFolder, $"{gameObject.name}_Inventory.json");

        // �������� ��������� ������� �� ����� ��� ������
        LoadResourcesFromFile();

        // ���� ����� �������� ������� ��� ��� ����� (�.�. ���� �� ������ ��� ����),
        // �������������� �� ���������� �� ���������.
        if (resources.Count == 0)
        {
            Debug.Log($"No saved resources found for {gameObject.name}. Initializing with default values.");
            resources.Add("MetalScrap", 50);
            resources.Add("CopperWire", 0);
            resources.Add("Pickaxe", 0);
            resources.Add("ToolKit", 0);
            // �������� ������ ��������� ������� ����� �� ���������
            SaveResourcesToFile(); // ��������� �������� �� ���������
        }
    }

    // ���� ����� ���������� ��� ������ �� ���������� ��� ����� ������ ��������������/������������
    void OnApplicationQuit()
    {
        // ������� ��������: ��������� ������ ��� ������ �� ����
        SaveResourcesToFile();
    }

    void OnDisable()
    {
        // ����� ����� ���������, ����� ������ ���������� ����������
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

        System.Text.StringBuilder sb = new System.Text.StringBuilder(); // ���������� StringBuilder ��� �������������

        sb.AppendLine("--- Resources ---"); // ���������

        foreach (var entry in resources)
        {
            // entry.Key - ��� string (��� �������)
            // entry.Value - ��� int (���������� �������)
            // ���������� .ToString() ��� �������������� int � string
            sb.AppendLine($"{entry.Key}: {entry.Value.ToString()}");
        }

        moneyText.text = sb.ToString(); // ����������� �������������� ������ UI Text ����������
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
            SaveResourcesToFile(); // ��������� ����� ���������
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
        SaveResourcesToFile(); // ��������� ����� ���������
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
            // ��� ������� �������� ���� ���������, ��� ��� Add/Consume �������� SaveResourcesToFile()
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

    // --- ������ ���������� � �������� � ��������� ���� ---

    public void SaveResourcesToFile()
    {
        try
        {
            // ����������� Dictionary � JSON ������
            string json = JsonMapper.ToJson(resources);
            // ���������� JSON ������ � ����
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
                // ������ JSON ������ �� �����
                string json = File.ReadAllText(saveFilePath);
                // ������������� JSON ������� � Dictionary
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
            // � ������ ������ ��������, ����� ���������������� ������� �� ���������
            resources.Clear(); // �������, ����� ����� Awake ��������������� ������
        }
    }
}
