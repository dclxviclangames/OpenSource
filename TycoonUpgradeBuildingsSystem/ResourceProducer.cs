using UnityEngine;
using System.Collections.Generic;
using System.IO;
using LitJson; // ��� JSON ������������/��������������
using System; // ��� Math.Pow (Mathf.Pow)
using System.Linq; // ��� LINQ (FirstOrDefault, Where)

// ����� ��� ���������� ������ � ������������� ��������
[System.Serializable]
public class ResourceProducerSaveData
{
    public string id; // ���������� ID ��� ����� ���������� ������������� (��������, "FarmProducer_1")
    public int currentLevel; // ������� ������� �������������
    public float timeSinceLastProduction; // �����, ��������� � ������� ��������� ��������� (��� �����������)
}

// ���� ������ ��������� ������������� �������� � �������� ��������� ��� ���������� ������/�������.
public class ResourceProducer : MonoBehaviour
{
    [Header("��������� �������������")]
    [Tooltip("���������� ������������� ����� ������������� (��������, 'FarmProducer_1', 'MineProducer_A').")]
    public string producerId;
    [Tooltip("�������� �������, ������� ���������� ���� ������ (��������, 'Food', 'Ore').")]
    public string resourceToProduceName;
    [Tooltip("������� ���������� �������, ������������ �� ���� �������� �� ������ 1.")]
    public int baseProductionAmount = 10;
    [Tooltip("��� ����� (� ��������) ������������ ������.")]
    public float productionIntervalSeconds = 5f;
    [Tooltip("������� ������� �������������. 0 - ���������/�� ��������, 1 - ������ �������� �������.")]
    public int currentLevel = 0;
    [Tooltip("������������ �������, �� �������� ����� ��������� �������������.")]
    public int maxLevel = 100;

    [Header("��������� ��������� ���������")]
    [Tooltip("�������� �������, ������������ ��� ���������.")]
    public string initialUpgradeCostResourceName = "Wood";
    [Tooltip("��������� ��������� ��������� �� ������ 1.")]
    public int initialUpgradeCostAmount = 50;
    [Tooltip("��������� ��������� ��� ������� 1-94.")]
    [Range(1.01f, 1.2f)] // ������ �������: 1% - 20%
    public float mildCostMultiplier = 1.07f; // ��������, 7% ���������� ��������� �� �������
    [Tooltip("��������� ��������� ��� ������� 95-100 (����������� ������).")]
    [Range(1.2f, 2.0f)] // ������ �������: 20% - 100%
    public float steepCostMultiplier = 1.5f; // ��������, 50% ���������� ��������� �� �������

    [Header("��������������� ������������������")]
    [Tooltip("��������� ���������� ������������������ �� ������ �������.")]
    [Range(1.0f, 1.1f)] // ����������� �� 0% - 10%
    public float productionRateMultiplierPerLevel = 1.02f; // ��������, 2% ���������� ������������������ �� �������

    [Header("������")]
    [Tooltip("������ �� ������ ResourceInventory (������ �� GameManager ��� PlayerMob).")]
    public ResourceInventory resourceInventory;
    [Tooltip("������ ���������� ������� ��� ������� ������ ������. ������ ������������� ������ (0 - ���������, 1 - ��.1, � �.�.).")]
    public List<GameObject> visualModelsByLevel; // ��������, visualModelsByLevel[0] ��� ��. 0, visualModelsByLevel[1] ��� ��. 1

    // --- ��������� ���������� ---
    private float productionTimer = 15f;
    private const string SAVE_FOLDER_NAME = "GameSaves"; // �������� ����� ��� ����������
    private string saveFilePath; // ������ ���� � ����� ���������� ��� ����� �������������

    //MoveAgent
    [Header("Delivery Agent Settings")]
    [Tooltip("Prefab of the DeliveryAgent to be instantiated/pooled.")]
    public DeliveryAgent deliveryAgentPrefab;
    [Tooltip("The point from which the delivery agent will start its journey.")]
    public Transform agentSpawnPoint;
    [Tooltip("The DeliveryPoint script for the destination (e.g., Inventory building location).")]
    public DeliveryPoint deliveryTargetPoint;
    [Tooltip("The DeliveryPoint script for the return point (usually the spawn point).")]
    public DeliveryPoint agentReturnPoint;

    public BuildingManager buildingManager;

    // ������ �� IBuilding �� ���� �� GameObject
    private IBuilding _iBuilding;
    //MoveAgent

    void Awake()
    {
        // ���� producerId �� ���������� � ����������, ���������� ��� �������� �������
        if (string.IsNullOrEmpty(producerId))
        {
            producerId = gameObject.name;
            Debug.LogWarning($"ResourceProducer: producerId not set for {gameObject.name}. Using GameObject name as ID.", this);
        }
    
        // ���������� ������ ������������ ����� ���������
        productionTimer = productionIntervalSeconds;

        // ��������� ���� � ����� ����������
        string saveFolder = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        saveFilePath = Path.Combine(saveFolder, $"{producerId}_Producer.json"); 
    } 

    void Start()
    {
        // ���������� ����� ResourceInventory, ���� �� �� ��� �������� � ����������
        if (resourceInventory == null)
        {
            resourceInventory = FindObjectOfType<ResourceInventory>();
            if (resourceInventory == null)
            {
                Debug.LogError("ResourceProducer: ResourceInventory not assigned and not found in scene! Disabling script.", this);
                this.enabled = false; // ��������� ������, ���� ��� ��������� ��� ������
                return;
            }
        }

        
        //  LoadState(); // ��������� ����������� ��������� ��� ������
        // ApplyVisualsForCurrentLevel(); // ��������� ������������ ����� �������� ������
    }

    void Update()
    {
        // ���������� ������� ������ ���� ������������� ������� (������� > 0)
        if (currentLevel > 0)
        {
            productionTimer -= Time.deltaTime;
            if (productionTimer <= 0)
            {
                ProduceResource();
                DispatchDelivery();
                productionTimer = productionIntervalSeconds; // ����� �������
            }
        }
    }

    private void DispatchDelivery()
    {
        // In a real game, you would use an Object Pool here for performance.
        // For simplicity, we instantiate.
        DeliveryAgent newAgent = Instantiate(deliveryAgentPrefab);

        // --- Subscribe to the agent's events ---
        // Use a lambda expression or a private method to handle the event.
        // It's important to unsubscribe if agents are destroyed/pooled,
        // to prevent memory leaks (though for this simple example, it's less critical).
       // newAgent.OnDeliveryCompleted += HandleDeliveryCompleted;
     //   newAgent.OnReturnCompleted += HandleReturnCompleted;

        newAgent.SetupDelivery(
            agentSpawnPoint.position,
            deliveryTargetPoint, // Pass DeliveryPoint object
            agentReturnPoint // Pass DeliveryPoint object
          //  amountPerDelivery
           // resourceType
        );
      //  Debug.Log($"Resource Producer: Dispatched a delivery agent with {amountPerDelivery} {resourceType}.");
    }

    //New Building Logic
/*    private void ProduceResource()
    {
        BuildingLevelConfig currentLevelConfig = GetCurrentLevelConfig();
        if (currentLevelConfig != null && !string.IsNullOrEmpty(currentLevelConfig.producesResourceName))
        {
            resourceInventory.AddResources(currentLevelConfig.producesResourceName, currentLevelConfig.productionAmount);
            Debug.Log($"{gameObject.name} (Level {_iBuilding.GetCurrentLevel()}): Produced {currentLevelConfig.productionAmount} {currentLevelConfig.producesResourceName}.");
          //  SaveProducerState(); // ��������� ��������� ����� ������������
        }
        else
        {
            Debug.LogWarning($"ResourceProducer '{producerId}': No production configured for current level {_iBuilding.GetCurrentLevel()} or resource name is empty.", this);
        }
    }

    /// <summary>
    /// ���������� ������������ �������� ������ ������.
    /// </summary>
    private BuildingLevelConfig GetCurrentLevelConfig()
    {
        BuildingConfig config = buildingManager.allBuildingConfigs.FirstOrDefault(c => c.id == _iBuilding.GetBuildingId());
        if (config == null)
        {
            Debug.LogError($"ResourceProducer '{producerId}': BuildingConfig not found for ID '{_iBuilding.GetBuildingId()}'.", this);
            return null;
        }
        return config.levels.FirstOrDefault(l => l.level == _iBuilding.GetCurrentLevel());
    }

    /// <summary>
    /// ���������� ������� ���������� ������������ �� ������ ������ ������.
    /// </summary>
    public int GetCurrentProductionAmount()
    {
        BuildingLevelConfig config = GetCurrentLevelConfig();
        return config != null ? config.productionAmount : 0;
    }

    /// <summary>
    /// ���������� ������� �������� ������������ �� ������ ������ ������.
    /// </summary>
    

    /// <summary>
    /// ���������� ������� ��������� ������. ��������� ������.
    /// </summary>
    
    
    */
    // ��������� ��������� ��� ������ �� ����������
    void OnApplicationQuit()
    {
        SaveState();
    }

    // ====================================================================
    // ������ ������������
    // ====================================================================

    /// <summary>
    /// ���������� ������� � ��������� �� � ���������.
    /// </summary>
    private void ProduceResource()
    {
        int amountProduced = GetCurrentProductionAmount();
        if (resourceInventory != null)
        {
            resourceInventory.AddResources(resourceToProduceName, amountProduced);
            Debug.Log($"{producerId} (Level {currentLevel}) �������� {amountProduced} {resourceToProduceName}.");
        }
    }

    /// <summary>
    /// ������������ ������� ���������� ������������ �� ������ ������.
    /// </summary>
    /// <returns>���������� �������, ������������� �� ���� ��������.</returns>
    public int GetCurrentProductionAmount()
    {
        // ������������ ���������� � ������ 1.
        if (currentLevel <= 0) return 0;

        // ������� ���������������� ���������� ������������������
        return Mathf.RoundToInt(baseProductionAmount * Mathf.Pow(productionRateMultiplierPerLevel, currentLevel));
    }

    

    // ====================================================================
    // ������ ���������
    // ====================================================================

    /// <summary>
    /// �������� �������� ������������� �� ���������� ������.
    /// </summary>
    /// <returns>True, ���� ��������� �������, false � ��������� ������.</returns>
    public bool TryUpgrade()
    {
        if (currentLevel >= maxLevel)
        {
            Debug.Log($"{producerId} ��� �� ������������ ������ ({maxLevel}).");
            return false;
        }

        int targetLevel = currentLevel + 1;
        int requiredAmount = GetUpgradeCostAmount(targetLevel);

        // ���������, ���� �� ������� ��� ���������
        if (resourceInventory.HasResources(initialUpgradeCostResourceName, requiredAmount))
        {
            resourceInventory.ConsumeResources(initialUpgradeCostResourceName, requiredAmount); // ���������� �������
            currentLevel = targetLevel; // ����������� �������
            Debug.Log($"{producerId} �������� �� ������ {currentLevel}.");
            ApplyVisualsForCurrentLevel(); // ��������� ������������
            SaveState(); // ��������� ����� ���������
            return true;
        }
        else
        {
            Debug.Log($"������������ {initialUpgradeCostResourceName} ��� ��������� {producerId} �� ������ {targetLevel}. ����������: {requiredAmount}.");
            return false;
        }
    }

    /// <summary>
    /// ������������ ��������� ��������� �� ������������� �������� ������.
    /// ���������� ������ ������ 95-100 ����������� �������.
    /// </summary>
    /// <param name="targetLevel">�������, ������� ����� ��������� ����� ����� ���������.</param>
    /// <returns>���������� 'initialUpgradeCostResourceName', ����������� ��� ���������.</returns>
    public int GetUpgradeCostAmount(int targetLevel)
    {
        if (targetLevel <= 0 || targetLevel > maxLevel)
        {
            // ������� 0 - ��� "�� ��������". ��������� �� 1-�� ������ ��������� ��� targetLevel=1.
            // ���� ������������� ������� ���� maxLevel, ���������� 0.
            return 0;
        }

        float cost = initialUpgradeCostAmount;

        if (targetLevel <= 95)
        {
            // ��� ������� 1-95: ������ ���������������� ����
            cost = initialUpgradeCostAmount * Mathf.Pow(mildCostMultiplier, targetLevel - 1);
        }
        else // ��� ������� 96-100
        {
            // ������� ������������ ��������� �� ������ 95, ��������� ������ ���������
            float costUpTo95 = initialUpgradeCostAmount * Mathf.Pow(mildCostMultiplier, 94);
            // ����� ��������� ������ ��������� ��� �������, ������� � 96 (�.�. targetLevel - 95 - ��� �������� �� 95)
            cost = costUpTo95 * Mathf.Pow(steepCostMultiplier, targetLevel - 95);
        }

        return Mathf.RoundToInt(cost); // ��������� �� ���������� ������ �����
    }

    /// <summary>
    /// ���������� ��������� ���������� ���������.
    /// </summary>
    public int GetCostForNextLevel()
    {
        return GetUpgradeCostAmount(currentLevel + 1);
    }

    /// <summary>
    /// ���������� �������� �������, ������������ ��� ���������.
    /// </summary>
    public string GetUpgradeResourceName()
    {
        return initialUpgradeCostResourceName;
    }

    /// <summary>
    /// ��������� ���������� ���������� ������ ��� �������� ������ �������������.
    /// </summary>
    private void ApplyVisualsForCurrentLevel()
    {
        // ������������ ��� ���������� ������
        foreach (GameObject model in visualModelsByLevel)
        {
            if (model != null)
            {
                model.SetActive(false);
            }
        }

        // ���������� ������ ���������� ������
        if (currentLevel >= 0 && currentLevel < visualModelsByLevel.Count && visualModelsByLevel[currentLevel] != null)
        {
            visualModelsByLevel[currentLevel].SetActive(true);
            // ���� ������� 0, �������� ������ ����� ���� ���������. ���������� ��� ��� ������ > 0.
            gameObject.SetActive(currentLevel > 0);
        }
        else if (currentLevel == 0 && visualModelsByLevel.Count > 0 && visualModelsByLevel[0] != null)
        {
            // ������� 0 (���������), ���������� ������ 0, ���� ��� ����.
            visualModelsByLevel[0].SetActive(true);
            gameObject.SetActive(true); // ��� ������ ������ ���� �������, ����� ���������� "�������������" ���������
        }
        else if (currentLevel > 0 && visualModelsByLevel.Count > 0 && visualModelsByLevel[0] != null)
        {
            // ���� � �������� ������ ��� ������, �� ������ ������� (currentLevel > 0), 
            // ����� �������� ������ ������� ������ ��� ������ �������� �������� �������� ������.
            // � ������ ������, �������� ������ ��� �������, ���� currentLevel > 0.
            // ���� ��� ������ ��� �������� ������, �� ������� ������ �� ����� ������������.
        }
    }


    // ====================================================================
    // ������ ����������/��������
    // ====================================================================

    /// <summary>
    /// ��������� ������� ��������� ����� ������������� � JSON ����.
    /// </summary>
    public void SaveState()
    {
        try
        {
            ResourceProducerSaveData saveData = new ResourceProducerSaveData
            {
                id = producerId,
                currentLevel = this.currentLevel,
                // ��������� ���������� ����� �� ��������� ���������
                timeSinceLastProduction = productionIntervalSeconds - productionTimer
            };
            string json = JsonMapper.ToJson(saveData);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Producer '{producerId}' state saved. Level: {currentLevel}.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving producer '{producerId}' state: {e.Message}");
        }
    }

    /// <summary>
    /// ��������� ��������� ����� ������������� �� JSON �����.
    /// </summary>
    public void LoadState()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                ResourceProducerSaveData loadedData = JsonMapper.ToObject<ResourceProducerSaveData>(json);

                this.currentLevel = loadedData.currentLevel;
                // ��������������� ������ ������������
                this.productionTimer = productionIntervalSeconds - loadedData.timeSinceLastProduction;
                this.productionTimer = Mathf.Max(0, this.productionTimer); // ��������, ��� ������ �� �������������

                Debug.Log($"Producer '{producerId}' state loaded. Level: {currentLevel}.");

                // ���� �������� ������� 0, ���������� ������, ����� ������������ �� �������� �����.
                if (currentLevel == 0)
                {
                    productionTimer = productionIntervalSeconds;
                }
            }
            else
            {
                Debug.Log($"Producer '{producerId}' save file not found. Starting at Level {currentLevel} (default).");
                // ���� ����� ���, ������� ��� � ������� (�� ���������) �������
                SaveState();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading producer '{producerId}' state: {e.Message}");
            // � ������ ������ ��������, ���������� � ������ �� ��������� (0)
            this.currentLevel = 0;
            this.productionTimer = productionIntervalSeconds;
            // �������� ���������, ����� ������� ������ ����
            SaveState();
        }
    }

    // Optional: Visualise settings in Editor (Requires 'using UnityEditor;' for Handles.Label)
    // If you don't use UnityEditor, remove this method or wrap it in #if UNITY_EDITOR
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
        if (!string.IsNullOrEmpty(producerId))
        {
            // ����������� ������ � ���������
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                                     $"Producer: {producerId}\nLevel: {currentLevel}/{maxLevel}\nProd/s: {GetCurrentProductionAmount() / productionIntervalSeconds:F1}");
        }
    } 
#endif 
}
