using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public ItemData itemData;
    private Vector3 originalPosition;
    private Transform originalParent;
    private InventorySlotUI parentSlot;

    public ItemData GetItemData() => itemData;
    public InventorySlotUI GetParentSlot() => parentSlot;

    private static List<RaycastResult> raycastResults = new List<RaycastResult>();

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// Устанавливает данные предмета для этого UI-объекта.
    /// </summary>
    public void SetItem(ItemData data)
    {
        itemData = data;
        GetComponent<Image>().sprite = data.icon;

        // Масштабируем UI-объект в зависимости от размера предмета.
        Vector2 cellSize = InventoryManager.Instance.inventorySlotPrefab.GetComponent<RectTransform>().sizeDelta;
        rectTransform.sizeDelta = new Vector2(cellSize.x * data.width, cellSize.y * data.height);
    }

    // Устанавливает позицию на основе координат сетки.
    public void SetPosition(int x, int y)
    {
        // Найдем родительскую ячейку по координатам и установим ее в качестве родителя.
        InventorySlotUI slot = InventoryManager.Instance.GetSlotAt(x, y);
        if (slot != null)
        {
            transform.SetParent(slot.transform);
            transform.localPosition = Vector3.zero;
            parentSlot = slot;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = transform.position;

        // Помещаем предмет в "руку", чтобы он был над всеми остальными UI-элементами.
        transform.SetParent(InventoryManager.Instance.transform);
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // Удаляем предмет из сетки, чтобы освободить место.
        InventoryManager.Instance.RemoveItem(itemData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / InventoryManager.Instance.GetComponent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        raycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        bool droppedOnPlayer = false;
        InventorySlotUI newSlot = null;

        // Ищем в результатах Raycast, куда мы попали
        foreach (var result in raycastResults)
        {
            // Проверяем, если мы отпустили предмет на область игрока
            if (result.gameObject.GetComponent<PlayerDropZone>() != null)
            {
                droppedOnPlayer = true;
                break;
            }
            // Проверяем, если мы отпустили предмет на новую ячейку инвентаря
            else if (result.gameObject.GetComponent<InventorySlotUI>() != null)
            {
                newSlot = result.gameObject.GetComponent<InventorySlotUI>();
                break;
            }
        }

        if (newSlot != null)
        {
            // Пытаемся переместить предмет в новую ячейку
            if (InventoryManager.Instance.MoveItem(itemData, parentSlot, newSlot))
            {
                // Если успешно, ничего не делаем, так как MoveItem обновит UI
                Debug.Log($"[InventoryItemUI] Предмет '{itemData.itemName}' перемещен на {newSlot.GetGridPosition().x},{newSlot.GetGridPosition().y}.");
            }
            else
            {
                // Если не удалось, возвращаем предмет на исходное место
                ResetPosition();
                Debug.Log($"[InventoryItemUI] Не удалось переместить предмет '{itemData.itemName}'.");
            }
        }
        else
        {
            // Если отпустили куда-то еще, возвращаем предмет на исходное место
            ResetPosition();
            Debug.Log($"[InventoryItemUI] Предмет '{itemData.itemName}' возвращен на исходную позицию.");
        }
        // После OnEndDrag, EventSystem передаст событие OnDrop, если есть подходящий обработчик.
        // Если OnDrop не сработает (например, предмет был отпущен в пустое место),
        // мы возвращаем его на исходное место.

        // Убедимся, что предмет не был передан другому обработчику (например, PlayerDropZone).

        /*

        if (transform.parent == InventoryManager.Instance.transform)
        {
            // Возвращаем предмет в исходный слот и пытаемся его снова разместить в логике.
            transform.SetParent(originalParent);
            transform.position = originalPosition;
            if (InventoryManager.Instance.PlaceItem(itemData, parentSlot.GetGridPosition().x, parentSlot.GetGridPosition().y))
            {
                // Перемещаем UI-объект обратно в слот.
                SetPosition(parentSlot.GetGridPosition().x, parentSlot.GetGridPosition().y);
            }
            else
            {
                Debug.LogWarning("Не удалось вернуть предмет на исходную позицию. Этого не должно происходить.");
            }
        }
        */
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true; 
    }

    // Возвращает предмет на исходное место, если OnDrop не сработал.
    public void ResetPosition()
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
        // Заново размещаем предмет в логике, так как мы его удалили в OnBeginDrag.
        //InventoryManager.Instance.PlaceItem(itemData, parentSlot.GetGridPosition().x, parentSlot.GetGridPosition().y);
    }
}
