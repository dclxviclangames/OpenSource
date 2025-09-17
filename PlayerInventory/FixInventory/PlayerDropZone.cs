using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerDropZone : MonoBehaviour, IDropHandler
{
    public PlayerEquipment playerEquipment;

    public void OnDrop(PointerEventData eventData)
    {
        if(eventData.pointerDrag != null)
        {
            InventoryItemUI droppedItem = eventData.pointerDrag.GetComponent<InventoryItemUI>();
            if (droppedItem != null)
            {
                if (playerEquipment != null)
                {
                    // ��������� �������.
                    playerEquipment.EquipItem(droppedItem.GetItemData());
                    // ���������� UI-�������, ��� ��� �� ������ �� ����� � ���������.
                    Destroy(droppedItem.gameObject);
                    Debug.Log($"[PlayerDropZone] ������� '{droppedItem.GetItemData().itemName}' ��� ����������.");
                }
            }
        }
        
    }
}
