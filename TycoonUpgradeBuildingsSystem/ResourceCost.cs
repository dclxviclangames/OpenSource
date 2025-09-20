using UnityEngine;
using System.Collections.Generic;
using System.IO; // Для работы с файловой системой
using System.Linq; // Для LINQ
using LitJson; // Для работы с JSON

// Интерфейс для зданий, если им нужна какая-то общая логика
// Например, StartProduction(), CollectResources()
/*public interface IBuilding
{
    string GetBuildingId();
    int GetCurrentLevel();
    void SetBuildingLevel(int level);
    void SetBuildingActive(bool active);
    void ActivateVisualLevel(int level); // Для переключения визуальной модели
}*/

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

    [Tooltip("Название ресурса, который производит этот объект на данном уровне (например, 'Food', 'Ore').")]
    public string producesResourceName;
    [Tooltip("Количество ресурса, производимое за один интервал на данном уровне.")]
    public int productionAmount; 
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
}

