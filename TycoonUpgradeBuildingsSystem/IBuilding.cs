using UnityEngine;
using System.Collections.Generic;
using System.IO; // Для работы с файловой системой
using System.Linq; // Для LINQ
using LitJson; // Для работы с JSON

// Интерфейс для зданий, если им нужна какая-то общая логика
// Например, StartProduction(), CollectResources()
public interface IBuilding
{
    string GetBuildingId();
    int GetCurrentLevel();
    void SetBuildingLevel(int level);
    void SetBuildingActive(bool active);
    void ActivateVisualLevel(int level); // Для переключения визуальной модели
}
