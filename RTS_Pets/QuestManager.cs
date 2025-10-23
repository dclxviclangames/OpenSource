// C# - QuestManager.cs
using UnityEngine;
using System; // Для Action

public class QuestManager : MonoBehaviour
{
    // --- 1. ПЕРЕМЕННЫЕ КВЕСТА ---
    public int TargetAmount { get; private set; } // Например, 10
    public int CurrentProgress { get; private set; } // Текущий прогресс
    public bool IsQuestActive { get; private set; } = false;

    // --- 2. СИСТЕМА СОБЫТИЙ (Delegates/Action) ---
    // Это и есть ваша "Подписка"
    public static event Action<int> OnQuestCompleted; // Срабатывает, когда квест выполнен (int - сумма награды)
    public static event Action<int> OnProgressUpdated; // Для обновления UI

    // --- 3. НАГРАДЫ ---
    [Header("Награды")]
    public int RewardMoney = 100;
    public int RewardExp = 50;

    void Start()
    {
        GenerateNewQuest();
    }

    public void GenerateNewQuest()
    {
        if (IsQuestActive) return;

        // Генерация рандомного числа (например, убить от 5 до 15 врагов)
        TargetAmount = UnityEngine.Random.Range(5, 16);
        CurrentProgress = 0;
        IsQuestActive = true;
        Debug.Log($"Новый квест: Убить {TargetAmount} врагов!");
    }

    // --- 4. МЕТОД ДЛЯ УВЕЛИЧЕНИЯ ПРОГРЕССА (ВЫЗЫВАЕТСЯ ИЗ ДРУГИХ СКРИПТОВ) ---
    public void IncrementProgress()
    {
        if (!IsQuestActive) return;

        CurrentProgress++;

        // Оповещаем UI, что прогресс изменился
        OnProgressUpdated?.Invoke(CurrentProgress);

        // --- ПРОВЕРКА КОНДИЦИИ ---
        if (CurrentProgress >= TargetAmount)
        {
            CompleteQuest();
        }
    }

    private void CompleteQuest()
    {
        IsQuestActive = false;

        Debug.Log("Квест ВЫПОЛНЕН!");

        // Вызываем награду
        GiveReward();

        // Оповещаем всех подписчиков (например, UI-скрипт)
        OnQuestCompleted?.Invoke(RewardMoney);
    }

    private void GiveReward()
    {
        // Здесь логика начисления денег/опыта
        Debug.Log($"Получена награда: {RewardMoney} денег и {RewardExp} опыта.");
        // (Например: PlayerStats.Instance.AddMoney(RewardMoney);)
    }
}

/*
 * // C# - Enemy.cs (фрагмент)

private void Die()
{
    // ... логика смерти ...
    
    // 1. Убеждаемся, что QuestManager существует
    QuestManager manager = FindObjectOfType<QuestManager>();

    if (manager != null)
    {
        // 2. Увеличиваем счетчик прогресса в менеджере
        manager.IncrementProgress(); 
    }
    
    // ... уничтожение объекта ...
}

// C# - UIManager.cs (фрагмент)

void OnEnable()
{
    // Подписываемся на событие завершения квеста
    QuestManager.OnQuestCompleted += OnQuestFinished;
}

void OnDisable()
{
    // ОТПИСЫВАЕМСЯ, чтобы избежать ошибок и утечек памяти
    QuestManager.OnQuestCompleted -= OnQuestFinished;
}

private void OnQuestFinished(int awardedMoney)
{
    // Этот код сработает, когда QuestManager вызовет OnQuestCompleted?.Invoke()
    Debug.Log($"UI получил сигнал! Квест завершен. Награда: {awardedMoney}");
    // Открываем окно "Квест выполнен!"
}

// C# - QuestManager.cs (Фрагмент)

// --- 1. НАГРАДЫ (Сделаем их базовыми) ---
[Header("Базовые значения")]
public int BaseReward = 50; // Базовая сумма, которую вы всегда получаете
public int RewardMultiplier = 10; // Сколько денег за каждую единицу цели (TargetAmount)

public int TargetAmount { get; private set; } 
public int RewardMoney { get; private set; } // Сюда будем записывать рассчитанную сумму

// ... (остальные переменные и OnQuestCompleted) ...

public void GenerateNewQuest()
{
    if (IsQuestActive) return;

    // 1. Генерируем рандомное количество целей (TargetAmount)
    TargetAmount = UnityEngine.Random.Range(5, 16); 
    CurrentProgress = 0;

    // 2. !!! ВАШЕ ДИНАМИЧЕСКОЕ УСЛОВИЕ !!!
    // Награда = Базовая сумма + (Цель * Множитель)
    // Например: Если цель = 10, Награда = 50 + (10 * 10) = 150
    RewardMoney = BaseReward + (TargetAmount * RewardMultiplier);
    
    IsQuestActive = true;
    Debug.Log($"Новый квест: Убить {TargetAmount} врагов. Награда: {RewardMoney} денег.");
}

private void GiveReward()
{
    // Теперь мы используем уже рассчитанную RewardMoney
    Debug.Log($"Получена награда: {RewardMoney} денег."); 
    
    // ... логика начисления денег/опыта ...
    
    // Оповещаем подписчиков уже рассчитанной суммой
    OnQuestCompleted?.Invoke(RewardMoney); 
}
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */
