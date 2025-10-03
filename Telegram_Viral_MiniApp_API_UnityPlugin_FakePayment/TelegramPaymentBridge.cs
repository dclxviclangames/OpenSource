using UnityEngine;
using System.Runtime.InteropServices;

public class TelegramPaymentBridge : MonoBehaviour
{
    // !!! �����: ��� ������� � ����� ������ ���� "TelegramBridge" !!!
    private const string UNITY_OBJECT_NAME = "TelegramPaymentBridge";
    public GameObject requestPanel;

    // ��������� ������� � JSLIB-�������
    [DllImport("__Internal")]
    private static extern void RequestTelegramInvoice(string unityObjectName, string itemIdentifier, string price);

    // ���������� ����� ������� "������ �����"
    public void PurchaseFighter(string fighterID)
    {
        int starsPrice = 1; // ���� � �������

#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log($"[C#] ������������� �������� �������: {fighterID}");
            
            // �������� ������� � JavaScript-�������
            RequestTelegramInvoice(UNITY_OBJECT_NAME, fighterID, starsPrice.ToString());
#else
        Debug.Log("[C#] ������ ��������� ������ � ��������� ��������.");
#endif
    }

    // ���������� �� JavaScript ��� ��������� ��������� �������
    public void OnPaymentSuccess(string itemIdentifier)
    {
        Debug.Log($" [C#] �������� �����! ����� �������������: {itemIdentifier}");

        // --- ������ ������������� ---
        if (itemIdentifier == "Fighter_Star_1")
        {
            requestPanel.SetActive(false);
        }
    }

    // ���������� �� JavaScript ��� ������
    public void OnPaymentFailure(string message)
    {
        Debug.LogError($" [C#] ������ ������: {message}");
    }
}
