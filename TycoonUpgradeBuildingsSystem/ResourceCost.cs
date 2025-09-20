using UnityEngine;
using System.Collections.Generic;
using System.IO; // ��� ������ � �������� ��������
using System.Linq; // ��� LINQ
using LitJson; // ��� ������ � JSON

// ��������� ��� ������, ���� �� ����� �����-�� ����� ������
// ��������, StartProduction(), CollectResources()
/*public interface IBuilding
{
    string GetBuildingId();
    int GetCurrentLevel();
    void SetBuildingLevel(int level);
    void SetBuildingActive(bool active);
    void ActivateVisualLevel(int level); // ��� ������������ ���������� ������
}*/

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

    [Tooltip("�������� �������, ������� ���������� ���� ������ �� ������ ������ (��������, 'Food', 'Ore').")]
    public string producesResourceName;
    [Tooltip("���������� �������, ������������ �� ���� �������� �� ������ ������.")]
    public int productionAmount; 
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
}

