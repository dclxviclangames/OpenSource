using System.Collections.Generic;
using UnityEngine;

public class CarTree : MonoBehaviour
{

    public Transform FindFreeSlot(Transform rootObject)
    {
        Queue<Transform> searchQueue = new Queue<Transform>();
        searchQueue.Enqueue(rootObject); // Начинаем поиск с главного объекта

        while (searchQueue.Count > 0)
        {
            Transform current = searchQueue.Dequeue();
            AttachableEntity entity = current.GetComponent<AttachableEntity>();

            if (entity == null) continue;

            // 1. ПЕРЕБИРАЕМ СЛОТЫ ТЕКУЩЕЙ СУЩНОСТИ
            foreach (Transform slot in entity.AttachmentSlots)
            {
                // --- ПРОВЕРКА ---
                if (slot.childCount == 0)
                {
                    // СВОБОДНЫЙ СЛОТ НАЙДЕН!
                    return slot;
                }
                else
                {
                    // СЛОТ ЗАНЯТ: Добавляем прикрепленную сущность в очередь, 
                    // чтобы проверить её слоты на следующем шаге.
                    searchQueue.Enqueue(slot.GetChild(0));
                }
            }
        }

        // Если цикл завершился, значит, вся структура заполнена
        return null;
    }

    // Пример использования:
    public void AttachNewEntity(GameObject newEntity, Transform root)
    {
        Transform freeSlot = FindFreeSlot(root);

        if (freeSlot != null)
        {
            // Прикрепляем и позиционируем новую сущность
            newEntity.transform.SetParent(freeSlot);
            newEntity.transform.localPosition = Vector3.zero;
            newEntity.transform.localRotation = Quaternion.identity;
            Rigidbody newRb = newEntity.GetComponent<Rigidbody>();
            if (newRb != null)
            {
                newRb.isKinematic = true;
                newRb.freezeRotation = true;
            }
                

        }
        else
        {
            Debug.Log("Вся структура заполнена!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        AttachableEntity entity = collision.gameObject.GetComponent<AttachableEntity>();
        if (entity != null)
            AttachNewEntity(collision.gameObject, transform);
    }
}



