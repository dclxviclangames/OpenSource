using System.Collections.Generic;
using UnityEngine;

public class Stacker : MonoBehaviour
{
    public Transform stackPivot; // Пустой объект за спиной игрока
    public List<GameObject> moneyStack = new List<GameObject>();
    public float offsetStep = 0.2f; // Расстояние между купюрами

    public void AddToStack(GameObject obj)
    {
        obj.GetComponent<Collider>().enabled = false; // Отключаем коллизию, чтобы не мешала
        moneyStack.Add(obj);
        
        // Плавное перемещение в локальную позицию стопки
        Vector3 targetPos = new Vector3(0, moneyStack.Count * offsetStep, 0);
        obj.transform.SetParent(stackPivot);
        StartCoroutine(MoveToStack(obj.transform, targetPos));
    }

    public GameObject RemoveFromStack()
    {
        if (moneyStack.Count == 0) return null;

        GameObject lastObj = moneyStack[moneyStack.Count - 1];
        moneyStack.RemoveAt(moneyStack.Count - 1);
        return lastObj;
    }

    private System.Collections.IEnumerator MoveToStack(Transform obj, Vector3 target)
    {
        while (Vector3.Distance(obj.localPosition, target) > 0.01f)
        {
            obj.localPosition = Vector3.Lerp(obj.localPosition, target, Time.deltaTime * 15f);
            obj.localRotation = Quaternion.Lerp(obj.localRotation, Quaternion.identity, Time.deltaTime * 15f);
            yield return null;
        }
        obj.localPosition = target;
    }
}

using UnityEngine;

public class UpgradeZone : MonoBehaviour
{
    public bool isPlayerInside = false;
    public float payInterval = 0.1f; // Скорость отдачи денег
    private float timer;

    [SerializeField] private int requiredMoney = 10;
    [SerializeField] private GameObject upgradeResult; // Что появится после оплаты

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInside = false;
    }

    private void Update()
    {
        if (isPlayerInside && requiredMoney > 0)
        {
            timer += Time.deltaTime;

            if (timer >= payInterval)
            {
                SpendMoney(GameObject.FindGameObjectWithTag("Player").GetComponent<Stacker>());
                timer = 0;
            }
        }
    }

    void SpendMoney(Stacker stacker)
    {
        GameObject money = stacker.RemoveFromStack();
        if (money != null)
        {
            // Эффектно «всасываем» деньги в зону или просто удаляем
            Destroy(money); 
            requiredMoney--;

            if (requiredMoney <= 0)
            {
                upgradeResult.SetActive(true); // Активируем постройку/апгрейд
                Debug.Log("Upgrade Complete!");
            }
        }
    }
}
