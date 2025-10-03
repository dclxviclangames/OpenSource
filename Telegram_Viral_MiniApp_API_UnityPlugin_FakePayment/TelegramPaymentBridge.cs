using UnityEngine;
using System.Runtime.InteropServices;

public class TelegramPaymentBridge : MonoBehaviour
{
    // !!! ВАЖНО: Имя объекта в сцене ДОЛЖНО быть "TelegramBridge" !!!
    private const string UNITY_OBJECT_NAME = "TelegramPaymentBridge";
    public GameObject requestPanel;

    // Интерфейс функции в JSLIB-плагине
    [DllImport("__Internal")]
    private static extern void RequestTelegramInvoice(string unityObjectName, string itemIdentifier, string price);

    // Вызывается вашей кнопкой "Купить бойца"
    public void PurchaseFighter(string fighterID)
    {
        int starsPrice = 1; // Цена в ЗВЕЗДАХ

#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log($"[C#] Инициирование фейковой покупки: {fighterID}");
            
            // Вызываем функцию в JavaScript-плагине
            RequestTelegramInvoice(UNITY_OBJECT_NAME, fighterID, starsPrice.ToString());
#else
        Debug.Log("[C#] Запуск симуляции оплаты в редакторе пропущен.");
#endif
    }

    // Вызывается из JavaScript при симуляции успешного платежа
    public void OnPaymentSuccess(string itemIdentifier)
    {
        Debug.Log($" [C#] ФЕЙКОВЫЙ УСПЕХ! Товар разблокирован: {itemIdentifier}");

        // --- ЛОГИКА РАЗБЛОКИРОВКИ ---
        if (itemIdentifier == "Fighter_Star_1")
        {
            requestPanel.SetActive(false);
        }
    }

    // Вызывается из JavaScript при ошибке
    public void OnPaymentFailure(string message)
    {
        Debug.LogError($" [C#] Ошибка оплаты: {message}");
    }
}
