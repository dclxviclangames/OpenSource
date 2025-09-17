using UnityEngine;
using UnityEngine.EventSystems;

// Скрипт для UI-элементов слотов инвентаря.
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

    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Вызывается, когда предмет отпускается над этой ячейкой.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        InventoryItemUI droppedItem = eventData.pointerDrag.GetComponent<InventoryItemUI>();
        if (droppedItem != null)
        {
            // Пытаемся переместить предмет, вызывая метод в InventoryManager.
            bool success = InventoryManager.Instance.MoveItem(droppedItem.GetItemData(), droppedItem.GetParentSlot(), this);

            if (!success)
            {
                // Если перемещение не удалось, возвращаем предмет на место.
                droppedItem.ResetPosition();
                Debug.Log($"[InventorySlotUI] Не удалось переместить предмет '{droppedItem.GetItemData().itemName}' на {x},{y}.");
            }
            else
            {
                Debug.Log($"[InventorySlotUI] Предмет '{droppedItem.GetItemData().itemName}' успешно перемещен на {x},{y}.");
                // Обновление UI предмета. Он привяжется к новой ячейке и сбросит позицию.
                droppedItem.transform.SetParent(this.transform);
                droppedItem.GetComponent<RectTransform>().localPosition = Vector3.zero;
            }
        }
        // Проверяем, что это наш UI-предмет.
        /* InventoryItemUI droppedItem = eventData.pointerDrag.GetComponent<InventoryItemUI>();
         if (droppedItem != null)
         {
             // Пытаемся переместить предмет в новую ячейку.
             bool success = InventoryManager.Instance.MoveItem(droppedItem.GetItemData(), droppedItem.GetParentSlot().GetGridPosition(), this.GetGridPosition());
             if (!success)
             {
                 // Если перемещение не удалось, возвращаем предмет на место.
                 droppedItem.ResetPosition();
                 Debug.Log($"[InventorySlotUI] Не удалось переместить предмет '{droppedItem.GetItemData().itemName}' на {x},{y}.");
             }
             else
             {
                 Debug.Log($"[InventorySlotUI] Предмет '{droppedItem.GetItemData().itemName}' успешно размещен на {x},{y}.");
             }
         } */
    }
}
