using UnityEngine;
using System; // Для Action
using System.Linq; // Для LINQ (FirstOrDefault)

// Используем интерфейс, который вы предоставили
// public interface IBuilding
// {
//     string GetBuildingId();
//     int GetCurrentLevel();
//     void SetBuildingLevel(int level);
//     void SetBuildingActive(bool active);
//     void ActivateVisualLevel(int level);
// }

// Enum для различных типов зданий (из ResourceCost.cs, но здесь для ясности)


// Этот скрипт прикрепляется к GameObject, представляющему конкретное здание (например, Ферму).
// Он реализует интерфейс IBuilding и взаимодействует с BuildingManager.
public class HomeBuilding : MonoBehaviour, IBuilding
{
    [Header("Building Identification")]
    [Tooltip("Уникальный ID для этого конкретного экземпляра здания. Должен совпадать с 'id' в BuildingConfig.")]
    public string buildingId = "SimpleHome"; // Убедитесь, что это ID из вашего BuildingConfig (например, "Farm")

    // --- Приватные поля для хранения состояния, управляемого BuildingManager ---
    private BuildingSaveData _currentSaveData;
    private BuildingConfig _buildingConfig;

    // --- Реализация свойств интерфейса IBuilding ---
    public string GetBuildingId() => buildingId;
    public int GetCurrentLevel() => _currentSaveData != null ? _currentSaveData.currentLevel : 0;

    /// <summary>
    /// Устанавливает уровень здания. Этот метод должен вызываться BuildingManager'ом.
    /// </summary>
    /// <param name="level">Новый уровень.</param>
    public void SetBuildingLevel(int level)
    {
        if (_currentSaveData == null)
        {
            Debug.LogError($"FarmBuilding: Cannot set level, save data not initialized for {buildingId}.", this);
            return;
        }

        // Обновляем данные в BuildingManager, который является источником истины
        if (BuildingManager.Instance != null)
        {
            BuildingSaveData updatedData = BuildingManager.Instance.GetBuildingState(buildingId);
            if (updatedData != null)
            {
                updatedData.currentLevel = level;
                BuildingManager.Instance.SaveBuildingStates(); // Сохраняем изменения
                _currentSaveData = updatedData; // Обновляем локальную копию
                Debug.Log($"FarmBuilding: Level for {buildingId} set to {level} via SetBuildingLevel.");
                ActivateVisualLevel(level); // Активируем визуальную модель для нового уровня
            }
            else
            {
                Debug.LogError($"FarmBuilding: Could not find save data for {buildingId} in BuildingManager.");
            }
        }
        else
        {
            Debug.LogError("FarmBuilding: BuildingManager.Instance is null. Cannot set building level.", this);
        }
    }

    /// <summary>
    /// Активирует или деактивирует базовый GameObject здания.
    /// </summary>
    /// <param name="active">True для активации, false для деактивации.</param>
    public void SetBuildingActive(bool active)
    {
        gameObject.SetActive(active);
        if (_currentSaveData != null)
        {
            _currentSaveData.isUnlocked = active; // Обновляем состояние разблокировки
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.SaveBuildingStates();
            }
        }
        Debug.Log($"FarmBuilding: {gameObject.name} set active state to {active}.");
    }

    /// <summary>
    /// Активирует правильную визуальную модель для здания на основе заданного уровня.
    /// </summary>
    /// <param name="level">Уровень, для которого нужно активировать модель.</param>
    public void ActivateVisualLevel(int level)
    {
        if (BuildingManager.Instance != null)
        {
            // Делегируем активацию визуальных моделей BuildingManager'у,
            // так как он хранит BuildingConfig и ссылки на visualModel.
            BuildingManager.Instance.ActivateBuildingVisuals(buildingId, level);
            Debug.Log($"FarmBuilding: Requested visual update for level {level} from BuildingManager.");
        }
        else
        {
            Debug.LogError("FarmBuilding: BuildingManager.Instance is null. Cannot activate visual level.", this);
        }
    }



    // --- Дополнительные методы для взаимодействия (например, через UI) ---

    /// <summary>
    /// Попытка разблокировать здание (с уровня 0 на 1).
    /// </summary>
    public void TryUnlock()
    {
        if (BuildingManager.Instance != null)
        {
            if (BuildingManager.Instance.UnlockBuilding(buildingId))
            {
                // Обновляем локальное состояние после успешной операции
                _currentSaveData = BuildingManager.Instance.GetBuildingState(buildingId);
                Debug.Log($"FarmBuilding: Successfully tried to unlock {buildingId}. Current level: {_currentSaveData.currentLevel}");
            }
            else
            {
                Debug.Log($"FarmBuilding: Failed to unlock {buildingId}. Check logs for reasons (resources, max level, etc.).");
            }
        }
    }

    /// <summary>
    /// Попытка улучшить здание на следующий уровень.
    /// </summary>
    public void TryUpgrade()
    {
        if (BuildingManager.Instance != null)
        {
            if (BuildingManager.Instance.UpgradeBuilding(buildingId))
            {
                // Обновляем локальное состояние после успешной операции
                _currentSaveData = BuildingManager.Instance.GetBuildingState(buildingId);
                Debug.Log($"FarmBuilding: Successfully tried to upgrade {buildingId}. Current level: {_currentSaveData.currentLevel}");
            }
            else
            {
                Debug.Log($"FarmBuilding: Failed to upgrade {buildingId}. Check logs for reasons (resources, max level, etc.).");
            }
        }
    }

    // Пример интеграции с ResourceProducer, если FarmBuilding производит ресурсы
    // Если FarmBuilding должен быть производителем, то ResourceProducer должен быть прикреплен к этому же GameObject.
    // Вы можете получить его так:
    private ResourceProducer _resourceProducer;
    void Start()
    {
        if (BuildingManager.Instance != null)
        {
            _buildingConfig = BuildingManager.Instance.allBuildingConfigs.FirstOrDefault(c => c.id == buildingId);
            _currentSaveData = BuildingManager.Instance.GetBuildingState(buildingId);

            if (_buildingConfig == null)
            {
                Debug.LogError($"FarmBuilding: No BuildingConfig found for ID '{buildingId}'. Please add it to BuildingManager's 'All Building Configs'.", this);
                enabled = false; // Отключаем скрипт, если нет конфигурации
                return;
            }
            if (_currentSaveData == null)
            {
                Debug.LogError($"FarmBuilding: No BuildingSaveData found for ID '{buildingId}'. This should be initialized by BuildingManager.", this);
                enabled = false;
                return;
            }

            // Убедимся, что базовый GameObject в BuildingConfig совпадает с этим GameObject
            if (_buildingConfig.baseGameObject != gameObject)
            {
                Debug.LogWarning($"FarmBuilding: Base GameObject in BuildingConfig for '{buildingId}' does not match this GameObject. Correcting in config.", this);
                _buildingConfig.baseGameObject = gameObject; // Исправляем ссылку в конфиге
            }

            // Применяем начальное состояние, загруженное BuildingManager'ом
            SetBuildingActive(_currentSaveData.isUnlocked);
            ActivateVisualLevel(_currentSaveData.currentLevel);
        }
        else
        {
            Debug.LogError("FarmBuilding: BuildingManager.Instance is null. Ensure BuildingManager is in the scene and initialized.", this);
            enabled = false;
        }
        _resourceProducer = GetComponent<ResourceProducer>();
        if (_resourceProducer != null)
        {
            Debug.Log($"FarmBuilding: Found ResourceProducer component on this object.");
            // Здесь можно связать логику: например, если здание разблокировано,
            // установить начальный уровень ResourceProducer.
            // _resourceProducer.currentLevel = GetCurrentLevel(); // Если уровни синхронизированы
        }
    }
}
