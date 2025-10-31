using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.UI; // ��� Text � Button

public class ShopManager : MonoBehaviour
{
    [Header("������� ������-���������")]
    // ������������ ������, ������� �������� ��� �����/������ � ��������
    public Transform PlayerContainer;

    [Header("UI ��������")]
    public Text MoneyText;

    private GameData _data;
    private string _savePath;
    private List<FighterSkinConfig> _allSkins;

    // --- ������������� ---

    void Awake()
    {
        // ������������� ���� ����������
        _savePath = Path.Combine(Application.persistentDataPath, "gamedata.json");

        // �������� ��� FighterSkinConfig �� ���� �������� ��������
        // (���� ����������, ������� true)
        _allSkins = PlayerContainer.GetComponentsInChildren<FighterSkinConfig>(true).ToList();

        // ��������� ������ � ��������� ��
        LoadGameData();
    }

    // --- JSON ������ ---

    void LoadGameData()
    {
        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);
            _data = JsonUtility.FromJson<GameData>(json);
        }
        else
        {
            _data = new GameData();
            // ������������ ������ ���� � ������ ������ �� ���������
            _data.ItemStatus.Add(new ItemSaveData { ID = _allSkins[0].SkinID, IsUnlocked = true });
            _data.ItemStatus.Add(new ItemSaveData { ID = _allSkins[0].AllWeapons[0].ID, IsUnlocked = true });
            SaveGameData();
        }

        ApplyLoadedStatus();
        UpdateUI();

        // ���������� ��������� ���� � ��� ������ ��� ������
        // ������� ����, ������� ��� ������ ��������� (����� ��������� ����� ������, �.�. ��� ���������� ����������)
        SelectFighterSkin(_allSkins.First().SkinID);
    }

    void SaveGameData()
    {
        // 1. ��������� ������ � _data
        _data.ItemStatus.Clear();
        foreach (var skin in _allSkins)
        {
            // ��������� ������ �����
            _data.ItemStatus.Add(new ItemSaveData { ID = skin.SkinID, IsUnlocked = skin.IsUnlocked });

            // ��������� ������ ������
            foreach (var weapon in skin.AllWeapons)
            {
                _data.ItemStatus.Add(new ItemSaveData { ID = weapon.ID, IsUnlocked = weapon.IsUnlocked });
            }
        }

        // 2. ����������� � ����������
        string json = JsonUtility.ToJson(_data);
        File.WriteAllText(_savePath, json);
    }

    void ApplyLoadedStatus()
    {
        // ��������� ������ ������������� �� JSON � ���������������� ��������
        foreach (var skin in _allSkins)
        {
            var savedSkinStatus = _data.ItemStatus.Find(item => item.ID == skin.SkinID);
            if (savedSkinStatus != null) skin.IsUnlocked = savedSkinStatus.IsUnlocked;

            foreach (var weapon in skin.AllWeapons)
            {
                var savedWeaponStatus = _data.ItemStatus.Find(item => item.ID == weapon.ID);
                if (savedWeaponStatus != null) weapon.IsUnlocked = savedWeaponStatus.IsUnlocked;
            }
        }
    }

    // --- ������ ������� (������ ���� ���������: Void Buy (int playerCost, int button)) ---

    // ������ ������ �������� ���� �����, ��������� ���������� ID
    public void BuyItem(string itemID)
    {
        var skinToBuy = _allSkins.FirstOrDefault(s => s.SkinID == itemID);
        var weaponToBuy = _allSkins.SelectMany(s => s.AllWeapons).FirstOrDefault(w => w.ID == itemID);

        int cost = 0;
        bool isUnlocked = false;

        if (skinToBuy != null) { cost = skinToBuy.Cost; isUnlocked = skinToBuy.IsUnlocked; }
        else if (weaponToBuy != null) { cost = weaponToBuy.Cost; isUnlocked = weaponToBuy.IsUnlocked; }
        else { Debug.LogError($"������� � ID {itemID} �� ������."); return; }

        if (isUnlocked) { if (weaponToBuy != null) SelectWeapon(weaponToBuy.ID); if (skinToBuy != null) SelectFighterSkin(skinToBuy.SkinID); return; }

        if (_data.Money >= cost)
        {
            _data.Money -= cost;
            if (skinToBuy != null) skinToBuy.IsUnlocked = true;
            else if (weaponToBuy != null) weaponToBuy.IsUnlocked = true;

            SaveGameData();
            UpdateUI();

            // ���� ������ ����, ����� ��� ��������
            if (skinToBuy != null) SelectFighterSkin(skinToBuy.SkinID);
            // ���� ������ ������, ����� ��� ��������
            else if (weaponToBuy != null) SelectWeapon(weaponToBuy.ID);
        }
        else
        {
            Debug.LogWarning("������������ �����!");
        }
    }

    // --- ������ ������ (������ ���� ���������: Void setPlayer(int number)) ---

    // ���������� ������� "�������" ��� �����/�����
    public void SelectFighterSkin(string skinID)
    {
        FighterSkinConfig selectedSkin = _allSkins.FirstOrDefault(s => s.SkinID == skinID);

        if (selectedSkin == null || !selectedSkin.IsUnlocked) return;

        // 1. ������������ ���������� � ���������� �����
        foreach (var skin in _allSkins)
        {
            skin.DeactivateSkin();
        }

        // 2. ��������� ����������� ������ ������ ��� ����� �����
        int selectedWeaponIndex = 0;

        // �������� �������� ������ �� ����������
        _data.SelectedWeaponIndices.TryGetValue(skinID, out selectedWeaponIndex);

        // 3. ���������� ��������� ���� � ��� �������
        selectedSkin.ActivateSkin(selectedWeaponIndex);
    }

    // ���������� ������� "�������" ��� ������
    public void SelectWeapon(string weaponID)
    {
        // 1. ������� ����, �������� ����������� ��� ������
        FighterSkinConfig ownerSkin = _allSkins.FirstOrDefault(s => s.AllWeapons.Any(w => w.ID == weaponID));
        WeaponEntry selectedWeapon = ownerSkin?.AllWeapons.FirstOrDefault(w => w.ID == weaponID);

        if (ownerSkin == null || selectedWeapon == null || !selectedWeapon.IsUnlocked) return;

        // 2. �������� ������ ���������� ������
        int index = ownerSkin.AllWeapons.IndexOf(selectedWeapon);

        // 3. ���������� ��� �� �����
        ownerSkin.ActivateSkin(index);

        // 4. ��������� ����� ������ ������ ��� ����� �����
        _data.SelectedWeaponIndices[ownerSkin.SkinID] = index;
        SaveGameData();
    }

    // --- ������ UI ---

    void UpdateUI()
    {
        // ��������� ��������� ���� � ��������
        if (MoneyText != null) MoneyText.text = _data.Money.ToString();

        // ��������� ��������������� ������ � ����� (����� ����� ��������� �������, 
        // ����� ����� ������ �� ID � �������� � �����: ������/�������)
    }
}
