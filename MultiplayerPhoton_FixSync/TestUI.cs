using UnityEngine;
using TMPro; // ���������, ��� � ��� ���� ���� using
using System.Collections;
using System.Collections.Generic;

public class TestUI : MonoBehaviour
{
    // ��������� ���������� ��� ���������� � ����������
    public GameObject myUIPrefab;
    public Transform myContainer;

    void Start()
    {
        Debug.Log("[TestUI] ������ �������.");

        // ���������, ��� ��� ���������� ���������
        if (myUIPrefab == null)
        {
            Debug.LogError("[TestUI] myUIPrefab �� ��������!");
            return;
        }

        if (myContainer == null)
        {
            Debug.LogError("[TestUI] myContainer �� ��������!");
            return;
        }

        // ������� ������� UI-�������
        GameObject newUI = Instantiate(myUIPrefab, myContainer);

        if (newUI != null)
        {
            Debug.Log("[TestUI] ������ ������� ������! ���: " + newUI.name);

            // ������� ����� TextMeshProUGUI
            TMPro.TextMeshProUGUI tmpText = newUI.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = "���� �������!";
            }
            else
            {
                Debug.LogError("[TestUI] �� ��������� ������� ��� ���������� TextMeshProUGUI!");
            }
        }
        else
        {
            Debug.LogError("[TestUI] Instantiation FAILED! ������ �� ��� ������.");
        }
    }
}
