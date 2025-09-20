using UnityEngine;
using System.Collections.Generic;
using System.IO; // ��� ������ � �������� ��������
using System.Linq; // ��� LINQ
using LitJson; // ��� ������ � JSON
using System;

// ��������� ��� ������, ���� �� ����� �����-�� ����� ������
// ��������, StartProduction(), CollectResources()
/*public interface IBuilding
{
    string GetBuildingId();
    int GetCurrentLevel();
    void SetBuildingLevel(int level);
    void SetBuildingActive(bool active);
    void ActivateVisualLevel(int level); // ��� ������������ ���������� ������
}

// ====================================================================
// ��������������� ������ ��� ������������ ������ (��� ��� �� ������, ������� �����, ����� ������)
// ��� ������, ������� �� ������������ � ���������� ��� ������� ���� ������.
// ====================================================================

[System.Serializable]
public class ResourceCost // ��������� �������
{
    public string resourceName; // ��� ������� (��������, "MetalScrap", "Food")
    public int amount;          // ����������
}

[System.Serializable]
public class BuildingLevelConfig // ������������ ��� ������� ������ ������
{
    [Tooltip("�������, � �������� ��������� ��� ������������. 0 - ��� ������������������� ���������.")]
    public int level;
    [Tooltip("��������� ��� ���������� ����� ������ (�� �����������).")]
    public List<ResourceCost> cost;
    [Tooltip("������� ������, ������� ���������� ���� �������. (���������� ������ ����, ��������� � ���� ������ ����� ���������)")]
    public GameObject visualModel; // ������������ ������ ��� ����� ������
    [Tooltip("�������� ��� ������, ��������� � ���� �������.")]
    public string description;
    // ����� �������� ����: ��������� ������������, ������ � �.�.
}

[System.Serializable]
public class BuildingConfig // ����������� ���� ������ (��������, "�����", "�������")
{
    [Tooltip("���������� ������������� ������ (��������, 'Farm', 'Barracks').")]
    public string id;

    public int maxLevel;

    [Tooltip("������������ ��� ������.")]
    public string displayName;
    [Tooltip("������ ������������ ��� ������� ������ ����� ������ (������� 0 - ���������������, ������� 1 - ������, � �.�.).")]
    public List<BuildingLevelConfig> levels; // �������� ������ ��� ���� �������
    [Tooltip("������� GameObject ��� ����� ������ � ����� (�������� ������).")]
    public GameObject baseGameObject; // �������� ������ ������ � �����
}

// ====================================================================
// ��������������� ������ ��� ��������� ������ (��� ����������/��������)
// ��� ������, ������� ��������� ������� ��������� ������� ����������� ������ � ����.
// ====================================================================

[System.Serializable]
public class BuildingSaveData // ������ ��� ���������� ��������� ������ ������
{
    public string id;          // ID ������ (�� BuildingConfig)
    public int currentLevel;   // ������� �������
    public bool isUnlocked;    // �������������� ��
    // ���� ������ �������������� �����������, ����� ����� ��������� �� �������, rot���� � �.�.
    // public Vector3 position;
    // public Quaternion rotation;
}*/

// ====================================================================
// �������� ����� ��������� ������
// ====================================================================

public class BuildingManager : MonoBehaviour
{
    [Header("������������ ������")]
    [Tooltip("������ ���� ����� ������ � ���� � �� ��������.")]
    public List<BuildingConfig> allBuildingConfigs; // ����������� � ����������

    public static BuildingManager Instance { get; private set; }

    [Header("������")]
    [Tooltip("������ �� ������ ResourceInventory.")]
    public ResourceInventory resourceInventory; // ����������� � ����������

    // ������� ��� �������� ������ �� �������� ������ � ����� � �� ������� ���������
    private Dictionary<string, GameObject> activeBuildingGameObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, BuildingSaveData> currentBuildingStates = new Dictionary<string, BuildingSaveData>();

    // ��� ����� � ����� ��� ����������
    private const string SAVE_FOLDER_NAME = "GameSaves";
    private const string BUILDINGS_SAVE_FILE_NAME = "buildings.json";
    private string saveFilePath;

    public event Action<string, int> OnBuildingUnlocked;
    public event Action<string, int> OnBuildingUpgradedEvent;

    void Awake()
    {
        if (resourceInventory == null)
        {
            Debug.LogError("BuildingManager: ResourceInventory not assigned! Please assign it in the Inspector.", this);
            return;
        }

        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ������������� ���� � ����� ����������
        string saveFolder = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        saveFilePath = Path.Combine(saveFolder, BUILDINGS_SAVE_FILE_NAME);

        InitializeBuildings(); // ������������� ������ ��� ������ �����
        LoadBuildingStates(); // ������� ��������� ��������� ������
    }

    /// <summary>
    /// �������������� ��������� ���� ������ �� ������ BuildingConfig.
    /// ����������/������������ �� ������� GameObjects � ���������� ������.
    /// </summary>
    private void InitializeBuildings()
    {
        currentBuildingStates.Clear();
        activeBuildingGameObjects.Clear();

        foreach (BuildingConfig config in allBuildingConfigs)
        {
            if (config.baseGameObject == null)
            {
                Debug.LogWarning($"BuildingManager: Base GameObject for building ID '{config.id}' is not assigned. Skipping.", this);
                continue;
            }

            // ���������� ��� ������ ���������������� (������� 0)
            BuildingSaveData initialState = new BuildingSaveData
            {
                id = config.id,
                currentLevel = 0, // ������� 0 �������� "�� ��������������"
                isUnlocked = false
            };
            currentBuildingStates.Add(config.id, initialState);
            activeBuildingGameObjects.Add(config.id, config.baseGameObject);

            // ���������� ������������ ��� ������� �������
            config.baseGameObject.SetActive(false);
            // � ��� ���������� ������ ������
            foreach (var levelConfig in config.levels)
            {
                if (levelConfig.visualModel != null)
                {
                    levelConfig.visualModel.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// �������� �������������� ������ (��������� ��� � ������ 0 �� ������� 1).
    /// </summary>
    /// <param name="buildingId">ID ������ ��� �������������.</param>
    public bool UnlockBuilding(string buildingId)
    {
        if (!currentBuildingStates.ContainsKey(buildingId))
        {
            Debug.LogError($"Building with ID '{buildingId}' not found in BuildingManager configuration.");
            return false;
        }

        BuildingSaveData currentData = currentBuildingStates[buildingId];
        BuildingConfig config = GetBuildingConfig(buildingId);

        if (currentData.isUnlocked)
        {
            Debug.LogWarning($"Building '{buildingId}' is already unlocked.");
            return false;
        }
        if (config == null || config.levels == null || config.levels.Count == 0)
        {
            Debug.LogError($"Building '{buildingId}' config or levels data is missing.");
            return false;
        }

        // ��������� ������������� - ��� ��������� ������ 1
        BuildingLevelConfig targetLevelConfig = config.levels.FirstOrDefault(l => l.level == 1);
        if (targetLevelConfig == null)
        {
            Debug.LogError($"Building '{buildingId}' has no Level 1 configuration for unlocking.");
            return false;
        }

        // ��������� ������� � ���������� ��
        if (CanAfford(targetLevelConfig.cost))
        {
            ConsumeCost(targetLevelConfig.cost);

            // ��������� ���������
            currentData.isUnlocked = true;
            currentData.currentLevel = 1;
            currentBuildingStates[buildingId] = currentData; // ��������� � �������

            // ���������� ������� ������ � ������ ���������� ������
            activeBuildingGameObjects[buildingId].SetActive(true);
            ActivateBuildingVisuals(buildingId, currentData.currentLevel);

            Debug.Log($"Building '{buildingId}' successfully UNLOCKED and upgraded to Level {currentData.currentLevel}.");
            SaveBuildingStates(); // ��������� ���������
            return true;
        }
        else
        {
            Debug.Log($"Not enough resources to UNLOCK building '{buildingId}'.");
            return false;
        }
    }


    /// <summary>
    /// �������� �������� ������ �� ���� �������.
    /// </summary>
    /// <param name="buildingId">ID ������ ��� ���������.</param>
    public bool UpgradeBuilding(string buildingId)
    {
        if (!currentBuildingStates.ContainsKey(buildingId))
        {
            Debug.LogError($"Building with ID '{buildingId}' not found in BuildingManager configuration.");
            return false;
        }

        BuildingSaveData currentData = currentBuildingStates[buildingId];
        BuildingConfig config = GetBuildingConfig(buildingId);

        if (!currentData.isUnlocked)
        {
            Debug.LogWarning($"Building '{buildingId}' is not unlocked yet. Please unlock it first.");
            return false;
        }
        if (config == null || config.levels == null || config.levels.Count == 0)
        {
            Debug.LogError($"Building '{buildingId}' config or levels data is missing.");
            return false;
        }

        int nextLevel = currentData.currentLevel + 1;
        if (nextLevel > config.maxLevel)
        {
            Debug.Log($"Building '{buildingId}' is already at max level ({config.maxLevel}).");
            return false;
        }

        BuildingLevelConfig targetLevelConfig = config.levels.FirstOrDefault(l => l.level == nextLevel);
        if (targetLevelConfig == null)
        {
            Debug.LogError($"Building '{buildingId}' has no configuration for level {nextLevel}.");
            return false;
        }

        // ��������� ������� � ���������� ��
        if (CanAfford(targetLevelConfig.cost))
        {
            ConsumeCost(targetLevelConfig.cost);

            // ��������� ���������
            currentData.currentLevel = nextLevel;
            currentBuildingStates[buildingId] = currentData; // ��������� � �������

            // ���������� ����� ���������� ������
            ActivateBuildingVisuals(buildingId, currentData.currentLevel);

            Debug.Log($"Building '{buildingId}' successfully upgraded to Level {currentData.currentLevel}.");
            SaveBuildingStates(); // ��������� ���������
            return true;
        }
        else
        {
            Debug.Log($"Not enough resources to UPGRADE building '{buildingId}' to Level {nextLevel}.");
            return false;
        }
    }

    /// <summary>
    /// ���������� ������� ��������� ������.
    /// </summary>
    /// <param name="buildingId">ID ������.</param>
    /// <returns>BuildingSaveData ��� null, ���� ������ �� �������.</returns>
    public BuildingSaveData GetBuildingState(string buildingId)
    {
        if (currentBuildingStates.ContainsKey(buildingId))
        {
            return currentBuildingStates[buildingId];
        }
        return null;
    }

    /// <summary>
    /// ���������� ������������ ������.
    /// </summary>
    public BuildingConfig GetBuildingConfig(string buildingId)
    {
        return allBuildingConfigs.FirstOrDefault(c => c.id == buildingId);
    }

    /// <summary>
    /// ���������, ���������� �� �������� ��� ������ ���������.
    /// </summary>
    private bool CanAfford(List<ResourceCost> costs)
    {
        if (costs == null) return true; // ���� ��� ���������, �� ������ ����� ���� ���������
        foreach (ResourceCost cost in costs)
        {
            if (!resourceInventory.HasResources(cost.resourceName, cost.amount))
            {
                Debug.Log($"Missing {cost.amount} of {cost.resourceName}");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// ���������� �������.
    /// </summary>
    private void ConsumeCost(List<ResourceCost> costs)
    {
        if (costs == null) return;
        foreach (ResourceCost cost in costs)
        {
            resourceInventory.ConsumeResources(cost.resourceName, cost.amount);
        }
    }

    /// <summary>
    /// ���������� ���������� ���������� ������ ��� ������ �� ������ ��� ������.
    /// ������������ ��� ���������.
    /// </summary>
    /// <param name="buildingId">ID ������.</param>
    /// <param name="level">������� ��� ���������.</param>
    public void ActivateBuildingVisuals(string buildingId, int level)
    {
        BuildingConfig config = GetBuildingConfig(buildingId);
        if (config == null || config.baseGameObject == null) return;

        config.baseGameObject.SetActive(true); // ��������, ��� ������� ������ ������ �������

        foreach (BuildingLevelConfig levelConfig in config.levels)
        {
            if (levelConfig.visualModel != null)
            {
                levelConfig.visualModel.SetActive(levelConfig.level == level);
            }
        }
    }

    // ====================================================================
    // ������ ���������� � �������� ��������� ������ (� JSON ����)
    // ====================================================================

    /// <summary>
    /// ��������� ������� ��������� ���� ������ � JSON ����.
    /// </summary>
    public void SaveBuildingStates()
    {
        try
        {
            // ����������� ������� currentBuildingStates � ������ ��� JSON-������������
            List<BuildingSaveData> saveDataList = currentBuildingStates.Values.ToList();
            string json = JsonMapper.ToJson(saveDataList);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Building states saved to: {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving building states to file: {e.Message}");
        }
    }

    /// <summary>
    /// ��������� ��������� ���� ������ �� JSON �����.
    /// </summary>
    public void LoadBuildingStates()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                List<BuildingSaveData> loadedDataList = JsonMapper.ToObject<List<BuildingSaveData>>(json);

                // ������� ������� ��������� � ��������� �����
                currentBuildingStates.Clear();
                foreach (BuildingSaveData data in loadedDataList)
                {
                    currentBuildingStates[data.id] = data;
                }
                Debug.Log($"Building states loaded from: {saveFilePath}");

                // ��������� ����������� ��������� � ������� ��������
                ApplyLoadedBuildingStates();
            }
            else
            {
                Debug.Log($"Building states save file not found at {saveFilePath}. Initializing with default states.");
                // ���� ����� ���, ������ ��������� � ��������� ��������� (������� 0, ����������������)
                // ��������� ��, ����� ������� ����
                SaveBuildingStates();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading building states from file: {e.Message}");
            // � ������ ������ ��������, ����� �������� ��� ������ � ��������� ���������
            InitializeBuildings();
        }
    }

    /// <summary>
    /// ��������� ����������� ��������� � ������� �������� � �����.
    /// </summary>
    private void ApplyLoadedBuildingStates()
    {
        foreach (var entry in currentBuildingStates)
        {
            string buildingId = entry.Key;
            BuildingSaveData data = entry.Value;
            BuildingConfig config = GetBuildingConfig(buildingId);

            if (config != null && config.baseGameObject != null)
            {
                // ���������� ������� ������, ���� ������ ��������������
                config.baseGameObject.SetActive(data.isUnlocked);

                // ���������� ���������� ���������� ������ ��� �������� ������
                ActivateBuildingVisuals(buildingId, data.currentLevel);
            }
        }
    }
}
