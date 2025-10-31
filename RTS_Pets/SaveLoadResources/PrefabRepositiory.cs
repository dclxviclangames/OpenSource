using UnityEngine;
using System.Collections.Generic;

public class PrefabRepository : MonoBehaviour
{
    public static PrefabRepository Instance { get; private set; }

    [System.Serializable]
    public class PrefabEntry
    {
        public string ID; // PrefabID, который используется для сохранения/загрузки
        public GameObject Prefab;
    }

    [Header("Список всех спавнящихся префабов")]
    public List<PrefabEntry> Prefabs = new List<PrefabEntry>();

    private Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Заполняем словарь для быстрого поиска
            foreach (var entry in Prefabs)
            {
                if (!prefabLookup.ContainsKey(entry.ID))
                {
                    prefabLookup.Add(entry.ID, entry.Prefab);
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Метод для получения префаба по его ID
    public GameObject GetPrefabByID(string id)
    {
        if (prefabLookup.TryGetValue(id, out GameObject prefab))
        {
            return prefab;
        }
        Debug.LogError($"Префаб с ID '{id}' не найден в репозитории!");
        return null;
    }
}
