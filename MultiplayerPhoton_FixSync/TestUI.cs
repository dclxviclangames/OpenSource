using UnityEngine;
using TMPro; // Убедитесь, что у вас есть этот using
using System.Collections;
using System.Collections.Generic;

public class TestUI : MonoBehaviour
{
    // Публичные переменные для назначения в Инспекторе
    public GameObject myUIPrefab;
    public Transform myContainer;

    void Start()
    {
        Debug.Log("[TestUI] Скрипт запущен.");

        // Проверяем, что обе переменные назначены
        if (myUIPrefab == null)
        {
            Debug.LogError("[TestUI] myUIPrefab не назначен!");
            return;
        }

        if (myContainer == null)
        {
            Debug.LogError("[TestUI] myContainer не назначен!");
            return;
        }

        // Пробуем создать UI-элемент
        GameObject newUI = Instantiate(myUIPrefab, myContainer);

        if (newUI != null)
        {
            Debug.Log("[TestUI] Объект успешно создан! Имя: " + newUI.name);

            // Пробуем найти TextMeshProUGUI
            TMPro.TextMeshProUGUI tmpText = newUI.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = "ТЕСТ УСПЕШЕН!";
            }
            else
            {
                Debug.LogError("[TestUI] На созданном объекте нет компонента TextMeshProUGUI!");
            }
        }
        else
        {
            Debug.LogError("[TestUI] Instantiation FAILED! Объект не был создан.");
        }
    }
}
