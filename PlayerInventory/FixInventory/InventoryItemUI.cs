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
    /// ������������� ������ �������� ��� ����� UI-�������.
    /// </summary>
    public void SetItem(ItemData data)
    {
        itemData = data;
        GetComponent<Image>().sprite = data.icon;

        // ������������ UI-������ � ����������� �� ������� ��������.
        Vector2 cellSize = InventoryManager.Instance.inventorySlotPrefab.GetComponent<RectTransform>().sizeDelta;
        rectTransform.sizeDelta = new Vector2(cellSize.x * data.width, cellSize.y * data.height);
    }

    // ������������� ������� �� ������ ��������� �����.
    public void SetPosition(int x, int y)
    {
        // ������ ������������ ������ �� ����������� � ��������� �� � �������� ��������.
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

        // �������� ������� � "����", ����� �� ��� ��� ����� ���������� UI-����������.
        transform.SetParent(InventoryManager.Instance.transform);
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // ������� ������� �� �����, ����� ���������� �����.
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

        // ���� � ����������� Raycast, ���� �� ������
        foreach (var result in raycastResults)
        {
            // ���������, ���� �� ��������� ������� �� ������� ������
            if (result.gameObject.GetComponent<PlayerDropZone>() != null)
            {
                droppedOnPlayer = true;
                break;
            }
            // ���������, ���� �� ��������� ������� �� ����� ������ ���������
            else if (result.gameObject.GetComponent<InventorySlotUI>() != null)
            {
                newSlot = result.gameObject.GetComponent<InventorySlotUI>();
                break;
            }
        }

        if (newSlot != null)
        {
            // �������� ����������� ������� � ����� ������
            if (InventoryManager.Instance.MoveItem(itemData, parentSlot, newSlot))
            {
                // ���� �������, ������ �� ������, ��� ��� MoveItem ������� UI
                Debug.Log($"[InventoryItemUI] ������� '{itemData.itemName}' ��������� �� {newSlot.GetGridPosition().x},{newSlot.GetGridPosition().y}.");
            }
            else
            {
                // ���� �� �������, ���������� ������� �� �������� �����
                ResetPosition();
                Debug.Log($"[InventoryItemUI] �� ������� ����������� ������� '{itemData.itemName}'.");
            }
        }
        else
        {
            // ���� ��������� ����-�� ���, ���������� ������� �� �������� �����
            ResetPosition();
            Debug.Log($"[InventoryItemUI] ������� '{itemData.itemName}' ��������� �� �������� �������.");
        }
        // ����� OnEndDrag, EventSystem �������� ������� OnDrop, ���� ���� ���������� ����������.
        // ���� OnDrop �� ��������� (��������, ������� ��� ������� � ������ �����),
        // �� ���������� ��� �� �������� �����.

        // ��������, ��� ������� �� ��� ������� ������� ����������� (��������, PlayerDropZone).

        /*

        if (transform.parent == InventoryManager.Instance.transform)
        {
            // ���������� ������� � �������� ���� � �������� ��� ����� ���������� � ������.
            transform.SetParent(originalParent);
            transform.position = originalPosition;
            if (InventoryManager.Instance.PlaceItem(itemData, parentSlot.GetGridPosition().x, parentSlot.GetGridPosition().y))
            {
                // ���������� UI-������ ������� � ����.
                SetPosition(parentSlot.GetGridPosition().x, parentSlot.GetGridPosition().y);
            }
            else
            {
                Debug.LogWarning("�� ������� ������� ������� �� �������� �������. ����� �� ������ �����������.");
            }
        }
        */
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true; 
    }

    // ���������� ������� �� �������� �����, ���� OnDrop �� ��������.
    public void ResetPosition()
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
        // ������ ��������� ������� � ������, ��� ��� �� ��� ������� � OnBeginDrag.
        //InventoryManager.Instance.PlaceItem(itemData, parentSlot.GetGridPosition().x, parentSlot.GetGridPosition().y);
    }
}
