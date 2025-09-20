using UnityEngine;
using System.Collections.Generic;
using System.IO; // ��� ������ � �������� ��������
using System.Linq; // ��� LINQ
using LitJson; // ��� ������ � JSON

// ��������� ��� ������, ���� �� ����� �����-�� ����� ������
// ��������, StartProduction(), CollectResources()
public interface IBuilding
{
    string GetBuildingId();
    int GetCurrentLevel();
    void SetBuildingLevel(int level);
    void SetBuildingActive(bool active);
    void ActivateVisualLevel(int level); // ��� ������������ ���������� ������
}
