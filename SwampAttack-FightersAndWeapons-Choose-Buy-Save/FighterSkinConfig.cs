using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Для List.IndexOf
using UnityEngine.UI;

// Класс для настройки оружия, уникального для этого скина
[System.Serializable]
public class WeaponEntry
{
    public string ID; // Уникальный ID оружия
    public GameObject Model; // Модель оружия (должен быть дочерним объектом)
    public int Cost; // Стоимость оружия
    [HideInInspector]
    public bool IsUnlocked = false; // Статус разблокировки (загружается менеджером)
}

public class FighterSkinConfig : MonoBehaviour
{
    [Header("Настройка Скина (Бойца)")]
    public string SkinID; // Уникальный ID этого скина/бойца
    public int Cost = 0; // Стоимость скина
    public Button BuySelectButton; // Кнопка для покупки/выбора этого скина
    [HideInInspector]
    public bool IsUnlocked = false; // Статус разблокировки скина

    [Header("Настройка Оружия (Должно быть дочерним)")]
    public List<WeaponEntry> AllWeapons;

    // Canvas или Панель, которая держит кнопки UI для ОРУЖИЯ этого бойца.
    public GameObject WeaponUIPanel;

    // --- ЛОГИКА ОТОБРАЖЕНИЯ (Вызывается менеджером) ---

    // Активирует этот скин и выбранное оружие по индексу
    public void ActivateSkin(int weaponIndex)
    {
        gameObject.SetActive(true);

        // Активируем UI для оружия, чтобы можно было его выбрать/купить
        if (WeaponUIPanel != null) WeaponUIPanel.SetActive(true);

        // Активируем только выбранное оружие (главная логика)
        for (int i = 0; i < AllWeapons.Count; i++)
        {
            AllWeapons[i].Model.SetActive(i == weaponIndex);
        }
    }

    // Деактивирует этот скин и его UI
    public void DeactivateSkin()
    {
        gameObject.SetActive(false);
        if (WeaponUIPanel != null) WeaponUIPanel.SetActive(false);
    }
}
