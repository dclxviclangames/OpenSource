using UnityEngine;
using UnityEngine.EventSystems;

// ������ ��� UI-��������� ������ ���������.
public class InventorySlotUI : MonoBehaviour, IDropHandler
{
    private int x;
    private int y;

    /// <summary>
    /// ������������� ���������� ������ � �����.
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
    /// ����������, ����� ������� ����������� ��� ���� �������.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        InventoryItemUI droppedItem = eventData.pointerDrag.GetComponent<InventoryItemUI>();
        if (droppedItem != null)
        {
            // �������� ����������� �������, ������� ����� � InventoryManager.
            bool success = InventoryManager.Instance.MoveItem(droppedItem.GetItemData(), droppedItem.GetParentSlot(), this);

            if (!success)
            {
                // ���� ����������� �� �������, ���������� ������� �� �����.
                droppedItem.ResetPosition();
                Debug.Log($"[InventorySlotUI] �� ������� ����������� ������� '{droppedItem.GetItemData().itemName}' �� {x},{y}.");
            }
            else
            {
                Debug.Log($"[InventorySlotUI] ������� '{droppedItem.GetItemData().itemName}' ������� ��������� �� {x},{y}.");
                // ���������� UI ��������. �� ���������� � ����� ������ � ������� �������.
                droppedItem.transform.SetParent(this.transform);
                droppedItem.GetComponent<RectTransform>().localPosition = Vector3.zero;
            }
        }
        // ���������, ��� ��� ��� UI-�������.
        /* InventoryItemUI droppedItem = eventData.pointerDrag.GetComponent<InventoryItemUI>();
         if (droppedItem != null)
         {
             // �������� ����������� ������� � ����� ������.
             bool success = InventoryManager.Instance.MoveItem(droppedItem.GetItemData(), droppedItem.GetParentSlot().GetGridPosition(), this.GetGridPosition());
             if (!success)
             {
                 // ���� ����������� �� �������, ���������� ������� �� �����.
                 droppedItem.ResetPosition();
                 Debug.Log($"[InventorySlotUI] �� ������� ����������� ������� '{droppedItem.GetItemData().itemName}' �� {x},{y}.");
             }
             else
             {
                 Debug.Log($"[InventorySlotUI] ������� '{droppedItem.GetItemData().itemName}' ������� �������� �� {x},{y}.");
             }
         } */
    }
}
