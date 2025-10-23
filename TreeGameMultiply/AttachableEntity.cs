using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// C# - AttachableEntity.cs
public class AttachableEntity : MonoBehaviour
{
    // Назначаются в Инспекторе
    public Transform[] AttachmentSlots;

    // Свойство, проверяющее, занят ли слот
    public bool IsSlotFree(Transform slot)
    {
        // Если у точки крепления нет дочерних объектов, она свободна
        return slot.childCount == 0;
    }
}
