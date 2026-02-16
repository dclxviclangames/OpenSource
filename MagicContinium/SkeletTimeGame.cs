public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
    public float timeModifier = 1f; // На это число умножаем всё движение
    public bool isSlowMo = false;

    void Awake() => Instance = this;

    void Update()
    {
        float targetModifier = isSlowMo ? 0.2f : 1f;
        // Плавный переход для красоты
        timeModifier = Mathf.MoveTowards(timeModifier, targetModifier, Time.unscaledDeltaTime * 2f);
    }
}

// В скрипте моба или пули:
void Update()
{
    transform.position += direction * speed * TimeManager.Instance.timeModifier * Time.deltaTime;
}

//Second Variant
Time.timeScale = 0.2f;
Time.fixedDeltaTime = 0.02f * Time.timeScale; // Подстраиваем шаг физики под новое время
