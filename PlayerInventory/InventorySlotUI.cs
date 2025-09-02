using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IDropHandler
{
    private int x;
    private int y;
    
    /// <summary>
    /// Устанавливает координаты ячейки в сетке.
    /// </summary>
    public void SetPosition(int newX, int newY)
    {
        x = newX;
        y = newY;
    }
    
    /// <summary>
    /// Вызывается, когда предмет отпускается над этой ячейкой.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        // Проверяем, что это наш UI-предмет
        InventoryItemUI droppedItem = eventData.pointerDrag.GetComponent<InventoryItemUI>();
        if (droppedItem != null)
        {
            // Пытаемся разместить предмет.
            bool success = InventoryManager.Instance.PlaceItem(droppedItem.GetItemData(), x, y);
            if (success)
            {
                // Если предмет успешно размещен в логике, уничтожаем старый UI-объект.
                Destroy(droppedItem.gameObject);
                Debug.Log($"[InventorySlotUI] Предмет '{droppedItem.GetItemData().itemName}' успешно размещен на {x},{y}.");
            }
            else
            {
                // Если размещение не удалось, возвращаем предмет на место.
                droppedItem.ResetPosition();
                Debug.Log($"[InventorySlotUI] Не удалось разместить предмет '{droppedItem.GetItemData().itemName}' на {x},{y}.");
            }
        }
    }
}
