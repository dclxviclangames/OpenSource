using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/Quest")]
public class Quest : ScriptableObject
{
    public string questName;
    [TextArea(3, 10)]
    public string questDescription;

    public List<BuildObjective> buildObjectives = new List<BuildObjective>();
    public List<CollectResourceObjective> collectObjectives = new List<CollectResourceObjective>();
    public List<UnitObjective> unitObjectives = new List<UnitObjective>();

    // Список возможных "следующих" квестов
    public List<QuestChoice> choices = new List<QuestChoice>();
}

using System.Collections.Generic;

[System.Serializable]
public class QuestData
{
    public string questName;
    public bool isStarted;
    public bool isCompleted;

    public List<BuildObjectiveData> buildObjectives = new List<BuildObjectiveData>();
    public List<CollectObjectiveData> collectObjectives = new List<CollectObjectiveData>();
    public List<UnitObjectiveData> unitObjectives = new List<UnitObjectiveData>();

    public QuestData(Quest quest)
    {
        this.questName = quest.questName;
        this.isStarted = true;
        this.isCompleted = false;
        
        foreach (var obj in quest.buildObjectives)
        {
            buildObjectives.Add(new BuildObjectiveData { objectiveId = obj.buildingName, isCompleted = false, buildingName = obj.buildingName, currentCount = 0, requiredCount = obj.requiredCount });
        }
        foreach (var obj in quest.collectObjectives)
        {
            collectObjectives.Add(new CollectObjectiveData { objectiveId = obj.resourceType, isCompleted = false, resourceType = obj.resourceType, currentCount = 0, requiredCount = obj.requiredCount });
        }
        foreach (var obj in quest.unitObjectives)
        {
            unitObjectives.Add(new UnitObjectiveData { objectiveId = obj.unitName, isCompleted = false, unitName = obj.unitName, currentCount = 0, requiredCount = obj.requiredCount });
        }
    }
}


using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public Quest currentQuest;
    public Dictionary<string, QuestData> questProgress = new Dictionary<string, QuestData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartQuest(Quest quest)
    {
        if (!questProgress.ContainsKey(quest.questName))
        {
            questProgress.Add(quest.questName, new QuestData(quest));
            currentQuest = quest;
            Debug.Log($"Квест '{quest.questName}' начат.");
        }
    }
    
    public bool CheckQuestCompletion(Quest quest)
    {
        if (questProgress.ContainsKey(quest.questName))
        {
            QuestData data = questProgress[quest.questName];

            foreach (var obj in data.buildObjectives)
            {
                if (!obj.isCompleted) return false;
            }
            foreach (var obj in data.collectObjectives)
            {
                if (!obj.isCompleted) return false;
            }
            foreach (var obj in data.unitObjectives)
            {
                if (!obj.isCompleted) return false;
            }
            return true;
        }
        return false;
    }

    public void NotifyBuildingBuilt(string buildingName)
    {
        if (currentQuest != null && questProgress.ContainsKey(currentQuest.questName))
        {
            QuestData data = questProgress[currentQuest.questName];
            BuildObjectiveData objective = data.buildObjectives.Find(o => o.buildingName == buildingName);
            if (objective != null && !objective.isCompleted)
            {
                objective.currentCount++;
                if (objective.currentCount >= objective.requiredCount)
                {
                    objective.isCompleted = true;
                }
                CheckForCompletion(currentQuest);
            }
        }
    }

    private void CheckForCompletion(Quest quest)
    {
        if (CheckQuestCompletion(quest))
        {
            questProgress[quest.questName].isCompleted = true;
            Debug.Log($"Квест '{quest.questName}' завершен!");
            // Здесь можно добавить логику для выдачи награды или запуска следующего квеста
        }
    }
}

using System;
using System.Collections.Generic;

[Serializable]
public class BuildObjective
{
    public string buildingName;
    public int requiredCount;
}

[Serializable]
public class CollectResourceObjective
{
    public string resourceType;
    public int requiredCount;
}

[Serializable]
public class UnitObjective
{
    public string unitName;
    public int requiredCount;
}

[Serializable]
public class QuestChoice
{
    public string choiceText;
    public Quest nextQuest;
}

using System;

[Serializable]
public class BuildObjectiveData
{
    public string objectiveId;
    public string buildingName;
    public int currentCount;
    public int requiredCount;
    public bool isCompleted;
}

[Serializable]
public class CollectObjectiveData
{
    public string objectiveId;
    public string resourceType;
    public int currentCount;
    public int requiredCount;
    public bool isCompleted;
}

[Serializable]
public class UnitObjectiveData
{
    public string objectiveId;
    public string unitName;
    public int currentCount;
    public int requiredCount;
    public bool isCompleted;
}



using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class SaveData
{
    public Dictionary<string, QuestData> questProgress;
    // Здесь могут быть и другие данные для сохранения
}

public class QuestManager : MonoBehaviour
{
    // ... (остальной код)

    public void SaveQuestProgress(string filePath)
    {
        SaveData dataToSave = new SaveData
        {
            questProgress = this.questProgress
        };
        string json = JsonUtility.ToJson(dataToSave);
        File.WriteAllText(filePath, json);
        Debug.Log("Прогресс квестов сохранен в JSON.");
    }

    public void LoadQuestProgress(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            SaveData loadedData = JsonUtility.FromJson<SaveData>(json);
            this.questProgress = loadedData.questProgress;
            Debug.Log("Прогресс квестов загружен из JSON.");
        }
    }
}
