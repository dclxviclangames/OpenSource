using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Tooltip("Данные предмета, который игрок подберет.")]
    public ItemData itemData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Добавлена проверка на существование InventoryManager
            if (InventoryManager.Instance != null)
            {
                if (InventoryManager.Instance.AddItem(itemData))
                {
                    Destroy(gameObject);
                    Debug.Log($"[ItemPickup] Игрок подобрал предмет '{itemData.itemName}'.");
                }
                else
                {
                    Debug.LogWarning("[ItemPickup] Инвентарь полон. Не удалось подобрать предмет.");
                }
            }
            else
            {
                Debug.LogWarning("[ItemPickup] InventoryManager не найден. Невозможно добавить предмет.");
            }
        }
    }
}
