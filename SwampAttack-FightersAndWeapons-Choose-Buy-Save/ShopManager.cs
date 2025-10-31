using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.UI; // Для Text и Button

public class ShopManager : MonoBehaviour
{
    [Header("Главный Объект-Контейнер")]
    // Родительский объект, который содержит все скины/бойцов в иерархии
    public Transform PlayerContainer;

    [Header("UI Элементы")]
    public Text MoneyText;

    private GameData _data;
    private string _savePath;
    private List<FighterSkinConfig> _allSkins;

    // --- ИНИЦИАЛИЗАЦИЯ ---

    void Awake()
    {
        // Устанавливаем путь сохранения
        _savePath = Path.Combine(Application.persistentDataPath, "gamedata.json");

        // Получаем все FighterSkinConfig со всех дочерних объектов
        // (Даже неактивные, поэтому true)
        _allSkins = PlayerContainer.GetComponentsInChildren<FighterSkinConfig>(true).ToList();

        // Загружаем данные и применяем их
        LoadGameData();
    }

    // --- JSON ЛОГИКА ---

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
            // Разблокируем первый скин и первое оружие по умолчанию
            _data.ItemStatus.Add(new ItemSaveData { ID = _allSkins[0].SkinID, IsUnlocked = true });
            _data.ItemStatus.Add(new ItemSaveData { ID = _allSkins[0].AllWeapons[0].ID, IsUnlocked = true });
            SaveGameData();
        }

        ApplyLoadedStatus();
        UpdateUI();

        // Активируем выбранный скин и его оружие при старте
        // Находим скин, который был выбран последним (здесь упрощенно берем первый, т.к. нет сохранения выбранного)
        SelectFighterSkin(_allSkins.First().SkinID);
    }

    void SaveGameData()
    {
        // 1. Обновляем статус в _data
        _data.ItemStatus.Clear();
        foreach (var skin in _allSkins)
        {
            // Сохраняем статус скина
            _data.ItemStatus.Add(new ItemSaveData { ID = skin.SkinID, IsUnlocked = skin.IsUnlocked });

            // Сохраняем статус оружия
            foreach (var weapon in skin.AllWeapons)
            {
                _data.ItemStatus.Add(new ItemSaveData { ID = weapon.ID, IsUnlocked = weapon.IsUnlocked });
            }
        }

        // 2. Сериализуем и записываем
        string json = JsonUtility.ToJson(_data);
        File.WriteAllText(_savePath, json);
    }

    void ApplyLoadedStatus()
    {
        // Применяем статус разблокировки из JSON к конфигурационным скриптам
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

    // --- ЛОГИКА ПОКУПКИ (Смотри твой псевдокод: Void Buy (int playerCost, int button)) ---

    // Кнопка должна вызывать этот метод, передавая уникальный ID
    public void BuyItem(string itemID)
    {
        var skinToBuy = _allSkins.FirstOrDefault(s => s.SkinID == itemID);
        var weaponToBuy = _allSkins.SelectMany(s => s.AllWeapons).FirstOrDefault(w => w.ID == itemID);

        int cost = 0;
        bool isUnlocked = false;

        if (skinToBuy != null) { cost = skinToBuy.Cost; isUnlocked = skinToBuy.IsUnlocked; }
        else if (weaponToBuy != null) { cost = weaponToBuy.Cost; isUnlocked = weaponToBuy.IsUnlocked; }
        else { Debug.LogError($"Предмет с ID {itemID} не найден."); return; }

        if (isUnlocked) { if (weaponToBuy != null) SelectWeapon(weaponToBuy.ID); if (skinToBuy != null) SelectFighterSkin(skinToBuy.SkinID); return; }

        if (_data.Money >= cost)
        {
            _data.Money -= cost;
            if (skinToBuy != null) skinToBuy.IsUnlocked = true;
            else if (weaponToBuy != null) weaponToBuy.IsUnlocked = true;

            SaveGameData();
            UpdateUI();

            // Если купили скин, сразу его выбираем
            if (skinToBuy != null) SelectFighterSkin(skinToBuy.SkinID);
            // Если купили оружие, сразу его выбираем
            else if (weaponToBuy != null) SelectWeapon(weaponToBuy.ID);
        }
        else
        {
            Debug.LogWarning("Недостаточно денег!");
        }
    }

    // --- ЛОГИКА ВЫБОРА (Смотри твой псевдокод: Void setPlayer(int number)) ---

    // Вызывается кнопкой "Выбрать" для скина/бойца
    public void SelectFighterSkin(string skinID)
    {
        FighterSkinConfig selectedSkin = _allSkins.FirstOrDefault(s => s.SkinID == skinID);

        if (selectedSkin == null || !selectedSkin.IsUnlocked) return;

        // 1. Деактивируем предыдущий и активируем новый
        foreach (var skin in _allSkins)
        {
            skin.DeactivateSkin();
        }

        // 2. Загружаем сохраненный индекс оружия для этого скина
        int selectedWeaponIndex = 0;

        // Пытаемся получить индекс из сохранения
        _data.SelectedWeaponIndices.TryGetValue(skinID, out selectedWeaponIndex);

        // 3. Активируем выбранный скин с его оружием
        selectedSkin.ActivateSkin(selectedWeaponIndex);
    }

    // Вызывается кнопкой "Выбрать" для оружия
    public void SelectWeapon(string weaponID)
    {
        // 1. Находим скин, которому принадлежит это оружие
        FighterSkinConfig ownerSkin = _allSkins.FirstOrDefault(s => s.AllWeapons.Any(w => w.ID == weaponID));
        WeaponEntry selectedWeapon = ownerSkin?.AllWeapons.FirstOrDefault(w => w.ID == weaponID);

        if (ownerSkin == null || selectedWeapon == null || !selectedWeapon.IsUnlocked) return;

        // 2. Получаем индекс выбранного оружия
        int index = ownerSkin.AllWeapons.IndexOf(selectedWeapon);

        // 3. Активируем его на скине
        ownerSkin.ActivateSkin(index);

        // 4. Сохраняем новый индекс оружия для этого скина
        _data.SelectedWeaponIndices[ownerSkin.SkinID] = index;
        SaveGameData();
    }

    // --- ЛОГИКА UI ---

    void UpdateUI()
    {
        // Обновляем текстовое поле с деньгами
        if (MoneyText != null) MoneyText.text = _data.Money.ToString();

        // Обновляем интерактивность кнопок и текст (Здесь нужна отдельная функция, 
        // чтобы найти кнопку по ID и обновить её текст: Купить/Выбрать)
    }
}
