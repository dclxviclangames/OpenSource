using UnityEngine;
using System.Collections.Generic;
using System.Linq; // ��� List.IndexOf
using UnityEngine.UI;

// ����� ��� ��������� ������, ����������� ��� ����� �����
[System.Serializable]
public class WeaponEntry
{
    public string ID; // ���������� ID ������
    public GameObject Model; // ������ ������ (������ ���� �������� ��������)
    public int Cost; // ��������� ������
    [HideInInspector]
    public bool IsUnlocked = false; // ������ ������������� (����������� ����������)
}

public class FighterSkinConfig : MonoBehaviour
{
    [Header("��������� ����� (�����)")]
    public string SkinID; // ���������� ID ����� �����/�����
    public int Cost = 0; // ��������� �����
    public Button BuySelectButton; // ������ ��� �������/������ ����� �����
    [HideInInspector]
    public bool IsUnlocked = false; // ������ ������������� �����

    [Header("��������� ������ (������ ���� ��������)")]
    public List<WeaponEntry> AllWeapons;

    // Canvas ��� ������, ������� ������ ������ UI ��� ������ ����� �����.
    public GameObject WeaponUIPanel;

    // --- ������ ����������� (���������� ����������) ---

    // ���������� ���� ���� � ��������� ������ �� �������
    public void ActivateSkin(int weaponIndex)
    {
        gameObject.SetActive(true);

        // ���������� UI ��� ������, ����� ����� ���� ��� �������/������
        if (WeaponUIPanel != null) WeaponUIPanel.SetActive(true);

        // ���������� ������ ��������� ������ (������� ������)
        for (int i = 0; i < AllWeapons.Count; i++)
        {
            AllWeapons[i].Model.SetActive(i == weaponIndex);
        }
    }

    // ������������ ���� ���� � ��� UI
    public void DeactivateSkin()
    {
        gameObject.SetActive(false);
        if (WeaponUIPanel != null) WeaponUIPanel.SetActive(false);
    }
}
