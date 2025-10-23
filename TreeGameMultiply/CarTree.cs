using System.Collections.Generic;
using UnityEngine;

public class CarTree : MonoBehaviour
{

    public Transform FindFreeSlot(Transform rootObject)
    {
        Queue<Transform> searchQueue = new Queue<Transform>();
        searchQueue.Enqueue(rootObject); // �������� ����� � �������� �������

        while (searchQueue.Count > 0)
        {
            Transform current = searchQueue.Dequeue();
            AttachableEntity entity = current.GetComponent<AttachableEntity>();

            if (entity == null) continue;

            // 1. ���������� ����� ������� ��������
            foreach (Transform slot in entity.AttachmentSlots)
            {
                // --- �������� ---
                if (slot.childCount == 0)
                {
                    // ��������� ���� ������!
                    return slot;
                }
                else
                {
                    // ���� �����: ��������� ������������� �������� � �������, 
                    // ����� ��������� � ����� �� ��������� ����.
                    searchQueue.Enqueue(slot.GetChild(0));
                }
            }
        }

        // ���� ���� ����������, ������, ��� ��������� ���������
        return null;
    }

    // ������ �������������:
    public void AttachNewEntity(GameObject newEntity, Transform root)
    {
        Transform freeSlot = FindFreeSlot(root);

        if (freeSlot != null)
        {
            // ����������� � ������������� ����� ��������
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
            Debug.Log("��� ��������� ���������!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        AttachableEntity entity = collision.gameObject.GetComponent<AttachableEntity>();
        if (entity != null)
            AttachNewEntity(collision.gameObject, transform);
    }
}



