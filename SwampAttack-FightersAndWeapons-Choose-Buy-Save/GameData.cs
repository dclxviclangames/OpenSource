using System.Collections.Generic;
using UnityEngine;

// [Serializable] ��������� ��� JsonUtility
[System.Serializable]
public class GameData
{
    // ����� ������
    public int Money = 1000;

    // ��������� ������� ��� ��������� � ��������
    // ����: ID ����� (�����), ��������: ������ ���������� ������
    public Dictionary<string, int> SelectedWeaponIndices = new Dictionary<string, int>();

    // ������ �������� ������������� (��������� ��� JsonUtility, ������� �� �������� � Dictionary)
    public List<ItemSaveData> ItemStatus = new List<ItemSaveData>();
}

// ��������� ����� ��� ���������� ������� ������� ��������
[System.Serializable]
public class ItemSaveData
{
    public string ID; // ���������� ID �������� (��������, "Fighter_01" ��� "Fighter_01_Gun_02")
    public bool IsUnlocked; // ������������� �� �������
}
