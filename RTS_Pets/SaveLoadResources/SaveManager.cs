using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SaveManager : MonoBehaviour
{
    // --- ПУТЬ СОХРАНЕНИЯ ---
    private string savePath => Path.Combine(Application.persistentDataPath, "gamedata.json");

    // --- СЛОВАРЬ ДАННЫХ В ПАМЯТИ ---
    public Dictionary<string, bool> UnlockedAnimals = new Dictionary<string, bool>();
    public Dictionary<string, InteriorData> SavedFurniture = new Dictionary<string, InteriorData>();

    public static SaveManager Instance { get; private set; }

    // --- ДАННЫЕ ДЛЯ СЕРИАЛИЗАЦИИ (Вспомогательные классы) ---
    [System.Serializable]
    public class InteriorData
    {
        public string PrefabID; 
        public float PosX;
        public float PosY;
        public float PosZ;
        public float RotY;     
    }
    
    [System.Serializable]
    public class FurnitureEntry
    {
        public string UniqueID; 
        public InteriorData Data;
    }

    [System.Serializable]
    public class GameSaveData
    {
        public List<FurnitureEntry> FurnitureEntries = new List<FurnitureEntry>();
        public List<string> AnimalKeys = new List<string>(); // Разблокированные животные (ID)
    }
    
    // --- ИНИЦИАЛИЗАЦИЯ ---
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        LoadGame();
    }

    // --- СОХРАНЕНИЕ ---
    public void SaveGame()
    {
        GameSaveData data = new GameSaveData
        {
            FurnitureEntries = ConvertFurnitureToEntries(),
            AnimalKeys = new List<string>(UnlockedAnimals.Keys)
        };
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log("Game Saved.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error saving game: " + e.Message);
        }
    }

    // --- ЗАГРУЗКА И СПАВН ---
    public void LoadGame()
    {
        // Здесь должна быть логика очистки старых объектов, если нужно
        
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

            // 1. Загрузка данных
            ConvertEntriesToFurniture(data.FurnitureEntries);
            LoadAnimals(data.AnimalKeys);
            
            // 2. Спавн
            SpawnSavedFurniture(); 
            SpawnUnlockedAnimals(); // !!! НОВЫЙ ВЫЗОВ !!!

            Debug.Log("Game Loaded successfully.");
        }
        else
        {
            Debug.LogWarning("Save file not found. Starting new game.");
        }
    }

    // --- ЛОГИКА СПАВНА СОХРАНЕННЫХ ЖИВОТНЫХ ---
    public void SpawnUnlockedAnimals()
    {
        if (PrefabRepository.Instance == null) return;
        
        // Животные спавнятся только если они разблокированы (есть в словаре)
        foreach (var kvp in UnlockedAnimals)
        {
            string animalID = kvp.Key;
            
            // Находим префаб по ID (предполагаем, что животное - это тоже префаб)
            GameObject prefabToSpawn = PrefabRepository.Instance.GetPrefabByID(animalID);

            if (prefabToSpawn != null)
            {
                // Для простоты: спавним животное в фиксированной или рандомной точке, 
                // если мы не сохраняли его позицию отдельно.
                Vector3 spawnPosition = Vector3.zero; // Замените на вашу логику спавна животных
                
                Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
                Debug.Log($"Создано разблокированное животное: {animalID}");
            }
        }
    }

    // --- ЛОГИКА СПАВНА СОХРАНЕННОЙ ФУРНИТУРЫ ---
    public void SpawnSavedFurniture()
    {
        if (PrefabRepository.Instance == null) return;

        foreach (var kvp in SavedFurniture)
        {
            InteriorData data = kvp.Value;
            GameObject prefabToSpawn = PrefabRepository.Instance.GetPrefabByID(data.PrefabID);

            if (prefabToSpawn != null)
            {
                Vector3 position = new Vector3(data.PosX, data.PosY, data.PosZ);
                Quaternion rotation = Quaternion.Euler(0, data.RotY, 0);
                
                GameObject instance = Instantiate(prefabToSpawn, position, rotation);

                DraggableItem draggable = instance.GetComponent<DraggableItem>();
                if (draggable != null)
                {
                    draggable.UniqueItemID = kvp.Key;
                    draggable.PrefabID = data.PrefabID;
                    draggable.SetLastValidPosition(position); 
                }
            }
        }
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ СЕРИАЛИЗАЦИИ ---
    private List<FurnitureEntry> ConvertFurnitureToEntries()
    {
        List<FurnitureEntry> list = new List<FurnitureEntry>();
        foreach (var kvp in SavedFurniture)
        {
            list.Add(new FurnitureEntry { UniqueID = kvp.Key, Data = kvp.Value });
        }
        return list;
    }

    private void ConvertEntriesToFurniture(List<FurnitureEntry> entries)
    {
        SavedFurniture.Clear();
        foreach (var entry in entries)
        {
            SavedFurniture.Add(entry.UniqueID, entry.Data);
        }
    }

    private void LoadAnimals(List<string> animalKeys)
    {
        UnlockedAnimals.Clear();
        foreach (string animalId in animalKeys)
        {
            // Устанавливаем статус "Разблокировано"
            UnlockedAnimals.Add(animalId, true); 
        }
    }
}
