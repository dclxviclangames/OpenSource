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
                    // Экипируем предмет.
                    playerEquipment.EquipItem(droppedItem.GetItemData());
                    // Уничтожаем UI-элемент, так как он больше не нужен в инвентаре.
                    Destroy(droppedItem.gameObject);
                    Debug.Log($"[PlayerDropZone] Предмет '{droppedItem.GetItemData().itemName}' был экипирован.");
                }
            }
        }
        
    }
}
