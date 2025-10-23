using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// C# - AttachableEntity.cs
public class AttachableEntity : MonoBehaviour
{
    // ����������� � ����������
    public Transform[] AttachmentSlots;

    // ��������, �����������, ����� �� ����
    public bool IsSlotFree(Transform slot)
    {
        // ���� � ����� ��������� ��� �������� ��������, ��� ��������
        return slot.childCount == 0;
    }
}
