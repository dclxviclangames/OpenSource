// C# - QuestManager.cs
using UnityEngine;
using System; // ��� Action

public class QuestManager : MonoBehaviour
{
    // --- 1. ���������� ������ ---
    public int TargetAmount { get; private set; } // ��������, 10
    public int CurrentProgress { get; private set; } // ������� ��������
    public bool IsQuestActive { get; private set; } = false;

    // --- 2. ������� ������� (Delegates/Action) ---
    // ��� � ���� ���� "��������"
    public static event Action<int> OnQuestCompleted; // �����������, ����� ����� �������� (int - ����� �������)
    public static event Action<int> OnProgressUpdated; // ��� ���������� UI

    // --- 3. ������� ---
    [Header("�������")]
    public int RewardMoney = 100;
    public int RewardExp = 50;

    void Start()
    {
        GenerateNewQuest();
    }

    public void GenerateNewQuest()
    {
        if (IsQuestActive) return;

        // ��������� ���������� ����� (��������, ����� �� 5 �� 15 ������)
        TargetAmount = UnityEngine.Random.Range(5, 16);
        CurrentProgress = 0;
        IsQuestActive = true;
        Debug.Log($"����� �����: ����� {TargetAmount} ������!");
    }

    // --- 4. ����� ��� ���������� ��������� (���������� �� ������ ��������) ---
    public void IncrementProgress()
    {
        if (!IsQuestActive) return;

        CurrentProgress++;

        // ��������� UI, ��� �������� ���������
        OnProgressUpdated?.Invoke(CurrentProgress);

        // --- �������� �������� ---
        if (CurrentProgress >= TargetAmount)
        {
            CompleteQuest();
        }
    }

    private void CompleteQuest()
    {
        IsQuestActive = false;

        Debug.Log("����� ��������!");

        // �������� �������
        GiveReward();

        // ��������� ���� ����������� (��������, UI-������)
        OnQuestCompleted?.Invoke(RewardMoney);
    }

    private void GiveReward()
    {
        // ����� ������ ���������� �����/�����
        Debug.Log($"�������� �������: {RewardMoney} ����� � {RewardExp} �����.");
        // (��������: PlayerStats.Instance.AddMoney(RewardMoney);)
    }
}

/*
 * // C# - Enemy.cs (��������)

private void Die()
{
    // ... ������ ������ ...
    
    // 1. ����������, ��� QuestManager ����������
    QuestManager manager = FindObjectOfType<QuestManager>();

    if (manager != null)
    {
        // 2. ����������� ������� ��������� � ���������
        manager.IncrementProgress(); 
    }
    
    // ... ����������� ������� ...
}

// C# - UIManager.cs (��������)

void OnEnable()
{
    // ������������� �� ������� ���������� ������
    QuestManager.OnQuestCompleted += OnQuestFinished;
}

void OnDisable()
{
    // ������������, ����� �������� ������ � ������ ������
    QuestManager.OnQuestCompleted -= OnQuestFinished;
}

private void OnQuestFinished(int awardedMoney)
{
    // ���� ��� ���������, ����� QuestManager ������� OnQuestCompleted?.Invoke()
    Debug.Log($"UI ������� ������! ����� ��������. �������: {awardedMoney}");
    // ��������� ���� "����� ��������!"
}

// C# - QuestManager.cs (��������)

// --- 1. ������� (������� �� ��������) ---
[Header("������� ��������")]
public int BaseReward = 50; // ������� �����, ������� �� ������ ���������
public int RewardMultiplier = 10; // ������� ����� �� ������ ������� ���� (TargetAmount)

public int TargetAmount { get; private set; } 
public int RewardMoney { get; private set; } // ���� ����� ���������� ������������ �����

// ... (��������� ���������� � OnQuestCompleted) ...

public void GenerateNewQuest()
{
    if (IsQuestActive) return;

    // 1. ���������� ��������� ���������� ����� (TargetAmount)
    TargetAmount = UnityEngine.Random.Range(5, 16); 
    CurrentProgress = 0;

    // 2. !!! ���� ������������ ������� !!!
    // ������� = ������� ����� + (���� * ���������)
    // ��������: ���� ���� = 10, ������� = 50 + (10 * 10) = 150
    RewardMoney = BaseReward + (TargetAmount * RewardMultiplier);
    
    IsQuestActive = true;
    Debug.Log($"����� �����: ����� {TargetAmount} ������. �������: {RewardMoney} �����.");
}

private void GiveReward()
{
    // ������ �� ���������� ��� ������������ RewardMoney
    Debug.Log($"�������� �������: {RewardMoney} �����."); 
    
    // ... ������ ���������� �����/����� ...
    
    // ��������� ����������� ��� ������������ ������
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
