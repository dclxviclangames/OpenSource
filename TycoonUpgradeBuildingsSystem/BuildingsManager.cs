using UnityEngine;
using System.Collections.Generic;
using System.IO; // Для работы с файловой системой
using System.Linq; // Для LINQ
using LitJson; // Для работы с JSON
using System;

// Интерфейс для зданий, если им нужна какая-то общая логика
// Например, StartProduction(), CollectResources()
/*public interface IBuilding
{
    string GetBuildingId();
    int GetCurrentLevel();
    void SetBuildingLevel(int level);
    void SetBuildingActive(bool active);
    void ActivateVisualLevel(int level); // Для переключения визуальной модели
}

// ====================================================================
// Вспомогательные классы для КОНФИГУРАЦИИ зданий (что это за здание, сколько стоит, какие уровни)
// Это данные, которые вы настраиваете в Инспекторе для каждого ТИПА здания.
// ====================================================================

[System.Serializable]
public class ResourceCost // Стоимость ресурса
{
    public string resourceName; // Имя ресурса (например, "MetalScrap", "Food")
    public int amount;          // Количество
}

[System.Serializable]
public class BuildingLevelConfig // Конфигурация для каждого уровня здания
{
    [Tooltip("Уровень, к которому относится эта конфигурация. 0 - для неразблокированного состояния.")]
    public int level;
    [Tooltip("Стоимость для достижения этого уровня (от предыдущего).")]
    public List<ResourceCost> cost;
    [Tooltip("Игровой объект, который отображает этот уровень. (Установите только один, остальные в этом списке будут неактивны)")]
    public GameObject visualModel; // Единственная модель для этого уровня
    [Tooltip("Описание или бонусы, связанные с этим уровнем.")]
    public string description;
    // Можно добавить сюда: множители производства, лимиты и т.д.
}

[System.Serializable]
public class BuildingConfig // Определение ТИПА здания (например, "Ферма", "Казарма")
{
    [Tooltip("Уникальный идентификатор здания (например, 'Farm', 'Barracks').")]
    public string id;

    public int maxLevel;

    [Tooltip("Отображаемое имя здания.")]
    public string displayName;
    [Tooltip("Список конфигураций для каждого уровня этого здания (уровень 0 - неразблокирован, уровень 1 - первый, и т.д.).")]
    public List<BuildingLevelConfig> levels; // Содержит данные для всех уровней
    [Tooltip("Базовый GameObject для этого здания в сцене (корневой объект).")]
    public GameObject baseGameObject; // Корневой объект здания в сцене
}

// ====================================================================
// Вспомогательные классы для СОСТОЯНИЯ зданий (для сохранения/загрузки)
// Это данные, которые описывают текущее состояние каждого КОНКРЕТНОГО здания в игре.
// ====================================================================

[System.Serializable]
public class BuildingSaveData // Данные для сохранения состояния одного здания
{
    public string id;          // ID здания (из BuildingConfig)
    public int currentLevel;   // Текущий уровень
    public bool isUnlocked;    // Разблокировано ли
    // Если здания инстанцируются динамически, здесь нужно сохранить их позицию, rotацию и т.д.
    // public Vector3 position;
    // public Quaternion rotation;
}*/

// ====================================================================
// ОСНОВНОЙ КЛАСС МЕНЕДЖЕРА ЗДАНИЙ
// ====================================================================

public class BuildingManager : MonoBehaviour
{
    [Header("Конфигурации зданий")]
    [Tooltip("Список всех ТИПОВ зданий в игре и их настроек.")]
    public List<BuildingConfig> allBuildingConfigs; // Назначается в инспекторе

    public static BuildingManager Instance { get; private set; }

    [Header("Ссылки")]
    [Tooltip("Ссылка на скрипт ResourceInventory.")]
    public ResourceInventory resourceInventory; // Назначается в инспекторе

    // Словарь для хранения ССЫЛОК на активные здания в сцене и их текущих состояний
    private Dictionary<string, GameObject> activeBuildingGameObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, BuildingSaveData> currentBuildingStates = new Dictionary<string, BuildingSaveData>();

    // Имя папки и файла для сохранения
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

        // Инициализация пути к файлу сохранения
        string saveFolder = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        saveFilePath = Path.Combine(saveFolder, BUILDINGS_SAVE_FILE_NAME);

        InitializeBuildings(); // Инициализация зданий при старте сцены
        LoadBuildingStates(); // Попытка загрузить состояния зданий
    }

    /// <summary>
    /// Инициализирует состояния всех зданий на основе BuildingConfig.
    /// Активирует/деактивирует их базовые GameObjects и визуальные модели.
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

            // Изначально все здания неразблокированы (уровень 0)
            BuildingSaveData initialState = new BuildingSaveData
            {
                id = config.id,
                currentLevel = 0, // Уровень 0 означает "не разблокировано"
                isUnlocked = false
            };
            currentBuildingStates.Add(config.id, initialState);
            activeBuildingGameObjects.Add(config.id, config.baseGameObject);

            // Изначально деактивируем все базовые объекты
            config.baseGameObject.SetActive(false);
            // И все визуальные модели внутри
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
    /// Пытается разблокировать здание (перевести его с уровня 0 на уровень 1).
    /// </summary>
    /// <param name="buildingId">ID здания для разблокировки.</param>
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

        // Стоимость разблокировки - это стоимость уровня 1
        BuildingLevelConfig targetLevelConfig = config.levels.FirstOrDefault(l => l.level == 1);
        if (targetLevelConfig == null)
        {
            Debug.LogError($"Building '{buildingId}' has no Level 1 configuration for unlocking.");
            return false;
        }

        // Проверяем ресурсы и потребляем их
        if (CanAfford(targetLevelConfig.cost))
        {
            ConsumeCost(targetLevelConfig.cost);

            // Обновляем состояние
            currentData.isUnlocked = true;
            currentData.currentLevel = 1;
            currentBuildingStates[buildingId] = currentData; // Обновляем в словаре

            // Активируем базовый объект и нужную визуальную модель
            activeBuildingGameObjects[buildingId].SetActive(true);
            ActivateBuildingVisuals(buildingId, currentData.currentLevel);

            Debug.Log($"Building '{buildingId}' successfully UNLOCKED and upgraded to Level {currentData.currentLevel}.");
            SaveBuildingStates(); // Сохраняем изменение
            return true;
        }
        else
        {
            Debug.Log($"Not enough resources to UNLOCK building '{buildingId}'.");
            return false;
        }
    }


    /// <summary>
    /// Пытается улучшить здание на один уровень.
    /// </summary>
    /// <param name="buildingId">ID здания для улучшения.</param>
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

        // Проверяем ресурсы и потребляем их
        if (CanAfford(targetLevelConfig.cost))
        {
            ConsumeCost(targetLevelConfig.cost);

            // Обновляем состояние
            currentData.currentLevel = nextLevel;
            currentBuildingStates[buildingId] = currentData; // Обновляем в словаре

            // Активируем новую визуальную модель
            ActivateBuildingVisuals(buildingId, currentData.currentLevel);

            Debug.Log($"Building '{buildingId}' successfully upgraded to Level {currentData.currentLevel}.");
            SaveBuildingStates(); // Сохраняем изменение
            return true;
        }
        else
        {
            Debug.Log($"Not enough resources to UPGRADE building '{buildingId}' to Level {nextLevel}.");
            return false;
        }
    }

    /// <summary>
    /// Возвращает текущее состояние здания.
    /// </summary>
    /// <param name="buildingId">ID здания.</param>
    /// <returns>BuildingSaveData или null, если здание не найдено.</returns>
    public BuildingSaveData GetBuildingState(string buildingId)
    {
        if (currentBuildingStates.ContainsKey(buildingId))
        {
            return currentBuildingStates[buildingId];
        }
        return null;
    }

    /// <summary>
    /// Возвращает конфигурацию здания.
    /// </summary>
    public BuildingConfig GetBuildingConfig(string buildingId)
    {
        return allBuildingConfigs.FirstOrDefault(c => c.id == buildingId);
    }

    /// <summary>
    /// Проверяет, достаточно ли ресурсов для оплаты стоимости.
    /// </summary>
    private bool CanAfford(List<ResourceCost> costs)
    {
        if (costs == null) return true; // Если нет стоимости, то всегда можем себе позволить
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
    /// Потребляет ресурсы.
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
    /// Активирует правильную визуальную модель для здания на основе его уровня.
    /// Деактивирует все остальные.
    /// </summary>
    /// <param name="buildingId">ID здания.</param>
    /// <param name="level">Уровень для активации.</param>
    public void ActivateBuildingVisuals(string buildingId, int level)
    {
        BuildingConfig config = GetBuildingConfig(buildingId);
        if (config == null || config.baseGameObject == null) return;

        config.baseGameObject.SetActive(true); // Убедимся, что базовый объект здания активен

        foreach (BuildingLevelConfig levelConfig in config.levels)
        {
            if (levelConfig.visualModel != null)
            {
                levelConfig.visualModel.SetActive(levelConfig.level == level);
            }
        }
    }

    // ====================================================================
    // Методы СОХРАНЕНИЯ и ЗАГРУЗКИ состояния зданий (в JSON файл)
    // ====================================================================

    /// <summary>
    /// Сохраняет текущее состояние всех зданий в JSON файл.
    /// </summary>
    public void SaveBuildingStates()
    {
        try
        {
            // Преобразуем словарь currentBuildingStates в список для JSON-сериализации
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
    /// Загружает состояние всех зданий из JSON файла.
    /// </summary>
    public void LoadBuildingStates()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                List<BuildingSaveData> loadedDataList = JsonMapper.ToObject<List<BuildingSaveData>>(json);

                // Очищаем текущие состояния и загружаем новые
                currentBuildingStates.Clear();
                foreach (BuildingSaveData data in loadedDataList)
                {
                    currentBuildingStates[data.id] = data;
                }
                Debug.Log($"Building states loaded from: {saveFilePath}");

                // Применяем загруженные состояния к игровым объектам
                ApplyLoadedBuildingStates();
            }
            else
            {
                Debug.Log($"Building states save file not found at {saveFilePath}. Initializing with default states.");
                // Если файла нет, здания останутся в начальном состоянии (уровень 0, неразблокированы)
                // Сохраняем их, чтобы создать файл
                SaveBuildingStates();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading building states from file: {e.Message}");
            // В случае ошибки загрузки, можно сбросить все здания в начальное состояние
            InitializeBuildings();
        }
    }

    /// <summary>
    /// Применяет загруженные состояния к игровым объектам в сцене.
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
                // Активируем базовый объект, если здание разблокировано
                config.baseGameObject.SetActive(data.isUnlocked);

                // Активируем правильную визуальную модель для текущего уровня
                ActivateBuildingVisuals(buildingId, data.currentLevel);
            }
        }
    }
}
