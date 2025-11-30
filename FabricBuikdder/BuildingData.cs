using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public int playerMoney;
    public List<BuildingData> buildings = new List<BuildingData>();
    public List<int> resourceAmounts = new List<int>();
}

[Serializable]
public class BuildingData
{
    public string prefabName;
    public int x;
    public int y;
}


using UnityEngine;
using System.IO;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SaveGame()
    {
        GameData data = new GameData();
        // Сохраняем ресурсы
        // data.resourceAmounts.Add(...);
        
        // Сохраняем здания
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");
        foreach (GameObject building in buildings)
        {
            BuildingData buildingData = new BuildingData();
            buildingData.prefabName = building.name.Replace("(Clone)", "");
            GridManager.Instance.GetGridPosition(building.transform.position, out buildingData.x, out buildingData.y);
            data.buildings.Add(buildingData);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
        Debug.Log("Игра сохранена!");
    }

    public void LoadGame()
    {
        string path = Application.persistentDataPath + "/save.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            GameData data = JsonUtility.FromJson<GameData>(json);

            // Загружаем ресурсы
            // ...

            // Загружаем здания
            // Удаляем старые здания
            GameObject[] oldBuildings = GameObject.FindGameObjectsWithTag("Building");
            foreach (GameObject b in oldBuildings)
            {
                Destroy(b);
            }

            foreach (BuildingData bd in data.buildings)
            {
                GameObject prefab = Resources.Load<GameObject>(bd.prefabName);
                if (prefab != null)
                {
                    Vector3 pos = GridManager.Instance.GetWorldPosition(bd.x, bd.y);
                    GameObject newBuilding = Instantiate(prefab, pos, Quaternion.identity);
                    GridManager.Instance.PlaceBuilding(newBuilding, bd.x, bd.y);
                }
            }
            Debug.Log("Игра загружена!");
        }
        else
        {
            Debug.LogError("Файл сохранения не найден!");
        }
    }
}
