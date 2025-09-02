using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private ItemData itemData;
    private Vector3 originalPosition;

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
    
    /// <summary>
    /// Возвращает данные предмета.
    /// </summary>
    public ItemData GetItemData()
    {
        return itemData;
    }

    /// <summary>
    /// Устанавливает позицию предмета на UI.
    /// </summary>
    public void SetPosition(int x, int y)
    {
        Vector2 cellSize = InventoryManager.Instance.inventorySlotPrefab.GetComponent<RectTransform>().sizeDelta;
        rectTransform.anchoredPosition = new Vector2(
            x * cellSize.x,
            -y * cellSize.y
        );
        originalPosition = rectTransform.position;
    }

    /// <summary>
    /// Возвращает предмет на исходную позицию.
    /// </summary>
    public void ResetPosition()
    {
        transform.position = originalPosition;
    }

    /// <summary>
    /// Начинаем перетаскивание.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Удаляем предмет из логики.
        InventoryManager.Instance.RemoveItem(itemData);
        
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
        transform.SetParent(InventoryManager.Instance.inventoryItemsContainer); // Открепляем от ячейки
        originalPosition = transform.position;
    }

    /// <summary>
    /// Перетаскиваем предмет.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / InventoryManager.Instance.GetComponent<Canvas>().scaleFactor;
    }

    /// <summary>
    /// Заканчиваем перетаскивание.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // Если не было успешного OnDrop, возвращаем предмет на исходную позицию.
        if (transform.parent == InventoryManager.Instance.inventoryItemsContainer)
        {
            InventoryManager.Instance.PlaceItem(itemData, (int)Mathf.Floor(originalPosition.x), (int)Mathf.Floor(originalPosition.y));
            Destroy(gameObject); // Уничтожаем UI-объект, т.к. PlaceItem создаст новый
        }
    }
}
