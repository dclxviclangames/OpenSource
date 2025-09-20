using UnityEngine;
using System.Collections.Generic;
using System.IO;
using LitJson; // Для JSON сериализации/десериализации
using System; // Для Math.Pow (Mathf.Pow)
using System.Linq; // Для LINQ (FirstOrDefault, Where)

// Класс для сохранения данных о производителе ресурсов
[System.Serializable]
public class ResourceProducerSaveData
{
    public string id; // Уникальный ID для этого экземпляра производителя (например, "FarmProducer_1")
    public int currentLevel; // Текущий уровень производителя
    public float timeSinceLastProduction; // Время, прошедшее с момента последней продукции (для продолжения)
}

// Этот скрипт управляет производством ресурсов и системой улучшения для отдельного здания/объекта.
public class ResourceProducer : MonoBehaviour
{
    [Header("Настройки Производителя")]
    [Tooltip("Уникальный идентификатор этого производителя (например, 'FarmProducer_1', 'MineProducer_A').")]
    public string producerId;
    [Tooltip("Название ресурса, который производит этот объект (например, 'Food', 'Ore').")]
    public string resourceToProduceName;
    [Tooltip("Базовое количество ресурса, производимое за один интервал на уровне 1.")]
    public int baseProductionAmount = 10;
    [Tooltip("Как часто (в секундах) производится ресурс.")]
    public float productionIntervalSeconds = 5f;
    [Tooltip("Текущий уровень производителя. 0 - неактивен/не построен, 1 - первый активный уровень.")]
    public int currentLevel = 0;
    [Tooltip("Максимальный уровень, до которого можно прокачать производителя.")]
    public int maxLevel = 100;

    [Header("Настройки Стоимости Улучшения")]
    [Tooltip("Название ресурса, необходимого для улучшения.")]
    public string initialUpgradeCostResourceName = "Wood";
    [Tooltip("Начальная стоимость улучшения до уровня 1.")]
    public int initialUpgradeCostAmount = 50;
    [Tooltip("Множитель стоимости для уровней 1-94.")]
    [Range(1.01f, 1.2f)] // Легкий прирост: 1% - 20%
    public float mildCostMultiplier = 1.07f; // Например, 7% увеличение стоимости за уровень
    [Tooltip("Множитель стоимости для уровней 95-100 (значительно больше).")]
    [Range(1.2f, 2.0f)] // Резкий прирост: 20% - 100%
    public float steepCostMultiplier = 1.5f; // Например, 50% увеличение стоимости за уровень

    [Header("Масштабирование Производительности")]
    [Tooltip("Множитель увеличения производительности на каждый уровень.")]
    [Range(1.0f, 1.1f)] // Увеличивать на 0% - 10%
    public float productionRateMultiplierPerLevel = 1.02f; // Например, 2% увеличение производительности за уровень

    [Header("Ссылки")]
    [Tooltip("Ссылка на скрипт ResourceInventory (обычно на GameManager или PlayerMob).")]
    public ResourceInventory resourceInventory;
    [Tooltip("Список визуальных моделей для каждого уровня здания. Индекс соответствует уровню (0 - неактивен, 1 - ур.1, и т.д.).")]
    public List<GameObject> visualModelsByLevel; // Например, visualModelsByLevel[0] для ур. 0, visualModelsByLevel[1] для ур. 1

    // --- Приватные переменные ---
    private float productionTimer = 15f;
    private const string SAVE_FOLDER_NAME = "GameSaves"; // Название папки для сохранений
    private string saveFilePath; // Полный путь к файлу сохранения для этого производителя

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

    // Ссылка на IBuilding на этом же GameObject
    private IBuilding _iBuilding;
    //MoveAgent

    void Awake()
    {
        // Если producerId не установлен в инспекторе, используем имя игрового объекта
        if (string.IsNullOrEmpty(producerId))
        {
            producerId = gameObject.name;
            Debug.LogWarning($"ResourceProducer: producerId not set for {gameObject.name}. Using GameObject name as ID.", this);
        }
    
        // Изначально таймер производства равен интервалу
        productionTimer = productionIntervalSeconds;

        // Настройка пути к файлу сохранения
        string saveFolder = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        saveFilePath = Path.Combine(saveFolder, $"{producerId}_Producer.json"); 
    } 

    void Start()
    {
        // Попытаться найти ResourceInventory, если он не был назначен в инспекторе
        if (resourceInventory == null)
        {
            resourceInventory = FindObjectOfType<ResourceInventory>();
            if (resourceInventory == null)
            {
                Debug.LogError("ResourceProducer: ResourceInventory not assigned and not found in scene! Disabling script.", this);
                this.enabled = false; // Отключаем скрипт, если нет инвентаря для работы
                return;
            }
        }

        
        //  LoadState(); // Загружаем сохраненное состояние при старте
        // ApplyVisualsForCurrentLevel(); // Применяем визуализацию после загрузки уровня
    }

    void Update()
    {
        // Производим ресурсы только если производитель активен (уровень > 0)
        if (currentLevel > 0)
        {
            productionTimer -= Time.deltaTime;
            if (productionTimer <= 0)
            {
                ProduceResource();
                DispatchDelivery();
                productionTimer = productionIntervalSeconds; // Сброс таймера
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
          //  SaveProducerState(); // Сохраняем состояние после производства
        }
        else
        {
            Debug.LogWarning($"ResourceProducer '{producerId}': No production configured for current level {_iBuilding.GetCurrentLevel()} or resource name is empty.", this);
        }
    }

    /// <summary>
    /// Возвращает конфигурацию текущего уровня здания.
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
    /// Возвращает текущее количество производства на основе уровня здания.
    /// </summary>
    public int GetCurrentProductionAmount()
    {
        BuildingLevelConfig config = GetCurrentLevelConfig();
        return config != null ? config.productionAmount : 0;
    }

    /// <summary>
    /// Возвращает текущий интервал производства на основе уровня здания.
    /// </summary>
    

    /// <summary>
    /// Обработчик события улучшения здания. Обновляет таймер.
    /// </summary>
    
    
    */
    // Сохраняем состояние при выходе из приложения
    void OnApplicationQuit()
    {
        SaveState();
    }

    // ====================================================================
    // ЛОГИКА ПРОИЗВОДСТВА
    // ====================================================================

    /// <summary>
    /// Производит ресурсы и добавляет их в инвентарь.
    /// </summary>
    private void ProduceResource()
    {
        int amountProduced = GetCurrentProductionAmount();
        if (resourceInventory != null)
        {
            resourceInventory.AddResources(resourceToProduceName, amountProduced);
            Debug.Log($"{producerId} (Level {currentLevel}) произвел {amountProduced} {resourceToProduceName}.");
        }
    }

    /// <summary>
    /// Рассчитывает текущее количество производства на основе уровня.
    /// </summary>
    /// <returns>Количество ресурса, производимого за один интервал.</returns>
    public int GetCurrentProductionAmount()
    {
        // Производство начинается с уровня 1.
        if (currentLevel <= 0) return 0;

        // Простое экспоненциальное увеличение производительности
        return Mathf.RoundToInt(baseProductionAmount * Mathf.Pow(productionRateMultiplierPerLevel, currentLevel));
    }

    

    // ====================================================================
    // ЛОГИКА УЛУЧШЕНИЯ
    // ====================================================================

    /// <summary>
    /// Пытается улучшить производителя до следующего уровня.
    /// </summary>
    /// <returns>True, если улучшение успешно, false в противном случае.</returns>
    public bool TryUpgrade()
    {
        if (currentLevel >= maxLevel)
        {
            Debug.Log($"{producerId} уже на максимальном уровне ({maxLevel}).");
            return false;
        }

        int targetLevel = currentLevel + 1;
        int requiredAmount = GetUpgradeCostAmount(targetLevel);

        // Проверяем, есть ли ресурсы для улучшения
        if (resourceInventory.HasResources(initialUpgradeCostResourceName, requiredAmount))
        {
            resourceInventory.ConsumeResources(initialUpgradeCostResourceName, requiredAmount); // Потребляем ресурсы
            currentLevel = targetLevel; // Увеличиваем уровень
            Debug.Log($"{producerId} прокачан до Уровня {currentLevel}.");
            ApplyVisualsForCurrentLevel(); // Обновляем визуализацию
            SaveState(); // Сохраняем новое состояние
            return true;
        }
        else
        {
            Debug.Log($"Недостаточно {initialUpgradeCostResourceName} для улучшения {producerId} до Уровня {targetLevel}. Необходимо: {requiredAmount}.");
            return false;
        }
    }

    /// <summary>
    /// Рассчитывает стоимость улучшения до определенного целевого уровня.
    /// Прогрессия делает уровни 95-100 значительно сложнее.
    /// </summary>
    /// <param name="targetLevel">Уровень, который будет достигнут после этого улучшения.</param>
    /// <returns>Количество 'initialUpgradeCostResourceName', необходимое для улучшения.</returns>
    public int GetUpgradeCostAmount(int targetLevel)
    {
        if (targetLevel <= 0 || targetLevel > maxLevel)
        {
            // Уровень 0 - это "не построен". Стоимость до 1-го уровня считается как targetLevel=1.
            // Если запрашивается уровень выше maxLevel, возвращаем 0.
            return 0;
        }

        float cost = initialUpgradeCostAmount;

        if (targetLevel <= 95)
        {
            // Для уровней 1-95: мягкий экспоненциальный рост
            cost = initialUpgradeCostAmount * Mathf.Pow(mildCostMultiplier, targetLevel - 1);
        }
        else // Для уровней 96-100
        {
            // Сначала рассчитываем стоимость до уровня 95, используя мягкий множитель
            float costUpTo95 = initialUpgradeCostAmount * Mathf.Pow(mildCostMultiplier, 94);
            // Затем применяем резкий множитель для уровней, начиная с 96 (т.е. targetLevel - 95 - это смещение от 95)
            cost = costUpTo95 * Mathf.Pow(steepCostMultiplier, targetLevel - 95);
        }

        return Mathf.RoundToInt(cost); // Округляем до ближайшего целого числа
    }

    /// <summary>
    /// Возвращает стоимость следующего улучшения.
    /// </summary>
    public int GetCostForNextLevel()
    {
        return GetUpgradeCostAmount(currentLevel + 1);
    }

    /// <summary>
    /// Возвращает название ресурса, необходимого для улучшения.
    /// </summary>
    public string GetUpgradeResourceName()
    {
        return initialUpgradeCostResourceName;
    }

    /// <summary>
    /// Применяет правильную визуальную модель для текущего уровня производителя.
    /// </summary>
    private void ApplyVisualsForCurrentLevel()
    {
        // Деактивируем все визуальные модели
        foreach (GameObject model in visualModelsByLevel)
        {
            if (model != null)
            {
                model.SetActive(false);
            }
        }

        // Активируем нужную визуальную модель
        if (currentLevel >= 0 && currentLevel < visualModelsByLevel.Count && visualModelsByLevel[currentLevel] != null)
        {
            visualModelsByLevel[currentLevel].SetActive(true);
            // Если уровень 0, корневой объект может быть неактивен. Активируем его при уровне > 0.
            gameObject.SetActive(currentLevel > 0);
        }
        else if (currentLevel == 0 && visualModelsByLevel.Count > 0 && visualModelsByLevel[0] != null)
        {
            // Уровень 0 (неактивен), показываем модель 0, если она есть.
            visualModelsByLevel[0].SetActive(true);
            gameObject.SetActive(true); // Сам объект должен быть активен, чтобы показывать "непостроенное" состояние
        }
        else if (currentLevel > 0 && visualModelsByLevel.Count > 0 && visualModelsByLevel[0] != null)
        {
            // Если у текущего уровня нет модели, но здание активно (currentLevel > 0), 
            // можно показать модель первого уровня или просто оставить активным корневой объект.
            // В данном случае, корневой объект уже активен, если currentLevel > 0.
            // Если нет модели для текущего уровня, то никакая модель не будет активирована.
        }
    }


    // ====================================================================
    // ЛОГИКА СОХРАНЕНИЯ/ЗАГРУЗКИ
    // ====================================================================

    /// <summary>
    /// Сохраняет текущее состояние этого производителя в JSON файл.
    /// </summary>
    public void SaveState()
    {
        try
        {
            ResourceProducerSaveData saveData = new ResourceProducerSaveData
            {
                id = producerId,
                currentLevel = this.currentLevel,
                // Сохраняем оставшееся время до следующей продукции
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
    /// Загружает состояние этого производителя из JSON файла.
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
                // Восстанавливаем таймер производства
                this.productionTimer = productionIntervalSeconds - loadedData.timeSinceLastProduction;
                this.productionTimer = Mathf.Max(0, this.productionTimer); // Убедимся, что таймер не отрицательный

                Debug.Log($"Producer '{producerId}' state loaded. Level: {currentLevel}.");

                // Если загружен уровень 0, сбрасываем таймер, чтобы производство не началось сразу.
                if (currentLevel == 0)
                {
                    productionTimer = productionIntervalSeconds;
                }
            }
            else
            {
                Debug.Log($"Producer '{producerId}' save file not found. Starting at Level {currentLevel} (default).");
                // Если файла нет, создаем его с текущим (по умолчанию) уровнем
                SaveState();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading producer '{producerId}' state: {e.Message}");
            // В случае ошибки загрузки, сбрасываем к уровню по умолчанию (0)
            this.currentLevel = 0;
            this.productionTimer = productionIntervalSeconds;
            // Повторно сохраняем, чтобы создать чистый файл
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
            // Отображение уровня в редакторе
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                                     $"Producer: {producerId}\nLevel: {currentLevel}/{maxLevel}\nProd/s: {GetCurrentProductionAmount() / productionIntervalSeconds:F1}");
        }
    } 
#endif 
}
