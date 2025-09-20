using UnityEngine;
using System; // ��� Action
using System.Linq; // ��� LINQ (FirstOrDefault)

// ���������� ���������, ������� �� ������������
// public interface IBuilding
// {
//     string GetBuildingId();
//     int GetCurrentLevel();
//     void SetBuildingLevel(int level);
//     void SetBuildingActive(bool active);
//     void ActivateVisualLevel(int level);
// }

// Enum ��� ��������� ����� ������ (�� ResourceCost.cs, �� ����� ��� �������)


// ���� ������ ������������� � GameObject, ��������������� ���������� ������ (��������, �����).
// �� ��������� ��������� IBuilding � ��������������� � BuildingManager.
public class HomeBuilding : MonoBehaviour, IBuilding
{
    [Header("Building Identification")]
    [Tooltip("���������� ID ��� ����� ����������� ���������� ������. ������ ��������� � 'id' � BuildingConfig.")]
    public string buildingId = "SimpleHome"; // ���������, ��� ��� ID �� ������ BuildingConfig (��������, "Farm")

    // --- ��������� ���� ��� �������� ���������, ������������ BuildingManager ---
    private BuildingSaveData _currentSaveData;
    private BuildingConfig _buildingConfig;

    // --- ���������� ������� ���������� IBuilding ---
    public string GetBuildingId() => buildingId;
    public int GetCurrentLevel() => _currentSaveData != null ? _currentSaveData.currentLevel : 0;

    /// <summary>
    /// ������������� ������� ������. ���� ����� ������ ���������� BuildingManager'��.
    /// </summary>
    /// <param name="level">����� �������.</param>
    public void SetBuildingLevel(int level)
    {
        if (_currentSaveData == null)
        {
            Debug.LogError($"FarmBuilding: Cannot set level, save data not initialized for {buildingId}.", this);
            return;
        }

        // ��������� ������ � BuildingManager, ������� �������� ���������� ������
        if (BuildingManager.Instance != null)
        {
            BuildingSaveData updatedData = BuildingManager.Instance.GetBuildingState(buildingId);
            if (updatedData != null)
            {
                updatedData.currentLevel = level;
                BuildingManager.Instance.SaveBuildingStates(); // ��������� ���������
                _currentSaveData = updatedData; // ��������� ��������� �����
                Debug.Log($"FarmBuilding: Level for {buildingId} set to {level} via SetBuildingLevel.");
                ActivateVisualLevel(level); // ���������� ���������� ������ ��� ������ ������
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
    /// ���������� ��� ������������ ������� GameObject ������.
    /// </summary>
    /// <param name="active">True ��� ���������, false ��� �����������.</param>
    public void SetBuildingActive(bool active)
    {
        gameObject.SetActive(active);
        if (_currentSaveData != null)
        {
            _currentSaveData.isUnlocked = active; // ��������� ��������� �������������
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.SaveBuildingStates();
            }
        }
        Debug.Log($"FarmBuilding: {gameObject.name} set active state to {active}.");
    }

    /// <summary>
    /// ���������� ���������� ���������� ������ ��� ������ �� ������ ��������� ������.
    /// </summary>
    /// <param name="level">�������, ��� �������� ����� ������������ ������.</param>
    public void ActivateVisualLevel(int level)
    {
        if (BuildingManager.Instance != null)
        {
            // ���������� ��������� ���������� ������� BuildingManager'�,
            // ��� ��� �� ������ BuildingConfig � ������ �� visualModel.
            BuildingManager.Instance.ActivateBuildingVisuals(buildingId, level);
            Debug.Log($"FarmBuilding: Requested visual update for level {level} from BuildingManager.");
        }
        else
        {
            Debug.LogError("FarmBuilding: BuildingManager.Instance is null. Cannot activate visual level.", this);
        }
    }



    // --- �������������� ������ ��� �������������� (��������, ����� UI) ---

    /// <summary>
    /// ������� �������������� ������ (� ������ 0 �� 1).
    /// </summary>
    public void TryUnlock()
    {
        if (BuildingManager.Instance != null)
        {
            if (BuildingManager.Instance.UnlockBuilding(buildingId))
            {
                // ��������� ��������� ��������� ����� �������� ��������
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
    /// ������� �������� ������ �� ��������� �������.
    /// </summary>
    public void TryUpgrade()
    {
        if (BuildingManager.Instance != null)
        {
            if (BuildingManager.Instance.UpgradeBuilding(buildingId))
            {
                // ��������� ��������� ��������� ����� �������� ��������
                _currentSaveData = BuildingManager.Instance.GetBuildingState(buildingId);
                Debug.Log($"FarmBuilding: Successfully tried to upgrade {buildingId}. Current level: {_currentSaveData.currentLevel}");
            }
            else
            {
                Debug.Log($"FarmBuilding: Failed to upgrade {buildingId}. Check logs for reasons (resources, max level, etc.).");
            }
        }
    }

    // ������ ���������� � ResourceProducer, ���� FarmBuilding ���������� �������
    // ���� FarmBuilding ������ ���� ��������������, �� ResourceProducer ������ ���� ���������� � ����� �� GameObject.
    // �� ������ �������� ��� ���:
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
                enabled = false; // ��������� ������, ���� ��� ������������
                return;
            }
            if (_currentSaveData == null)
            {
                Debug.LogError($"FarmBuilding: No BuildingSaveData found for ID '{buildingId}'. This should be initialized by BuildingManager.", this);
                enabled = false;
                return;
            }

            // ��������, ��� ������� GameObject � BuildingConfig ��������� � ���� GameObject
            if (_buildingConfig.baseGameObject != gameObject)
            {
                Debug.LogWarning($"FarmBuilding: Base GameObject in BuildingConfig for '{buildingId}' does not match this GameObject. Correcting in config.", this);
                _buildingConfig.baseGameObject = gameObject; // ���������� ������ � �������
            }

            // ��������� ��������� ���������, ����������� BuildingManager'��
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
            // ����� ����� ������� ������: ��������, ���� ������ ��������������,
            // ���������� ��������� ������� ResourceProducer.
            // _resourceProducer.currentLevel = GetCurrentLevel(); // ���� ������ ����������������
        }
    }
}
