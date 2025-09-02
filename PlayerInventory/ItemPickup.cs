using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Tooltip("Данные предмета, который игрок подберет.")]
    public ItemData itemData;
    
    /// <summary>
    /// Вызывается, когда игрок входит в триггер.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что это игрок.
        if (other.CompareTag("Player"))
        {
            // Пытаемся добавить предмет в инвентарь.
            if (InventoryManager.Instance.AddItem(itemData))
            {
                // Если предмет добавлен успешно, уничтожаем объект в мире.
                Destroy(gameObject);
                Debug.Log($"[ItemPickup] Игрок подобрал предмет '{itemData.itemName}'.");
            }
            else
            {
                Debug.LogWarning("[ItemPickup] Инвентарь полон. Не удалось подобрать предмет.");
            }
        }
    }
}
