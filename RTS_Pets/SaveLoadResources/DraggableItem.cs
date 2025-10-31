using UnityEngine;
using UnityEngine.EventSystems; // !!! НУЖНО ДЛЯ ИНТЕРФЕЙСОВ !!!

// Реализуем интерфейсы для Drag and Drop
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Параметры сохранения")]
    public string UniqueItemID; 
    public string PrefabID;     
    
    [Header("Настройка коллизий")]
    public LayerMask FurnitureLayer; 
    
    private Vector3 _lastValidPosition; 
    private Vector3 _dragOffset;
    private Collider _placementCollider;
    private float _planeY; // Высота, на которой мы перетаскиваем

    void Awake()
    {
        _placementCollider = GetComponent<Collider>();
        if (_placementCollider == null)
        {
            Debug.LogError($"Объект {name} не имеет Collider. Проверка коллизий отключена!");
        }
        // Устанавливаем плоскость для Drag (напр., высота пола)
        _planeY = transform.position.y;
    }

    public void SetLastValidPosition(Vector3 position)
    {
        _lastValidPosition = position;
    }

    // --- НАЧАЛО ПЕРЕТАСКИВАНИЯ (IBeginDragHandler) ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Устанавливаем текущую позицию как последнюю валидную
        _lastValidPosition = transform.position; 
        
        // Рассчитываем смещение
        _dragOffset = transform.position - GetMouseWorldPosition(eventData.position);
        
        Debug.Log("Drag started for: " + UniqueItemID);
    }

    // --- ПРОЦЕСС ПЕРЕТАСКИВАНИЯ (IDragHandler) ---
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = GetMouseWorldPosition(eventData.position) + _dragOffset;
        // Опционально: Визуальная обратная связь при перетаскивании (например, полупрозрачность)
    }

    // --- ОКОНЧАНИЕ ПЕРЕТАСКИВАНИЯ (IEndDragHandler) ---
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 finalPosition = transform.position;

        // 1. ПРОВЕРКА ВАЛИДНОСТИ ПОЗИЦИИ
        if (IsPlacementValid(finalPosition))
        {
            // Позиция валидна: сохраняем ее
            _lastValidPosition = finalPosition;
            UpdateSaveData();
            SaveManager.Instance.SaveGame();
            Debug.Log($"Предмет {UniqueItemID} успешно размещен.");
        }
        else
        {
            // Позиция невалидна: возвращаем предмет на последнюю валидную позицию
            transform.position = _lastValidPosition;
            Debug.LogWarning($"Размещение {UniqueItemID} не удалось. Место занято.");
        }
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---
    
    // Получение позиции в 3D мире из 2D позиции курсора
    private Vector3 GetMouseWorldPosition(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        // Создаем плоскость на той же высоте, что и объект
        Plane dragPlane = new Plane(Vector3.up, -_planeY); 
        float distance;
        
        if (dragPlane.Raycast(ray, out distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            // Возвращаем точку с оригинальной высотой объекта, чтобы не "проваливался"
            return new Vector3(worldPos.x, _planeY, worldPos.z);
        }
        return transform.position;
    }

    private bool IsPlacementValid(Vector3 targetPosition)
    {
        if (_placementCollider == null) return true;

        // Получаем параметры коллизии
        Vector3 center = targetPosition + _placementCollider.bounds.center - transform.position;
        Vector3 halfExtents = _placementCollider.bounds.extents;
        Quaternion rotation = transform.rotation;
        
        // OverlapBox проверяет пересечения
        Collider[] hits = Physics.OverlapBox(center, halfExtents, rotation, FurnitureLayer);

        foreach (Collider hit in hits)
        {
            // Исключаем проверку самого себя
            if (hit.gameObject != gameObject)
            {
                return false; 
            }
        }
        
        return true;
    }

    private void UpdateSaveData()
    {
        Vector3 pos = transform.position;
        
        SaveManager.InteriorData data = new SaveManager.InteriorData
        {
            PrefabID = this.PrefabID,
            PosX = pos.x,
            PosY = pos.y,
            PosZ = pos.z,
            RotY = transform.eulerAngles.y
        };

        // Обновляем или добавляем элемент в словарь SaveManager
        if (SaveManager.Instance.SavedFurniture.ContainsKey(UniqueItemID))
        {
            SaveManager.Instance.SavedFurniture[UniqueItemID] = data;
        }
        else
        {
            SaveManager.Instance.SavedFurniture.Add(UniqueItemID, data);
        }
    }
}
