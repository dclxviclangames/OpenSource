using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("��� ����� �����������������")]
    [Tooltip("ID ������, ������� ����� ������������ ���� �������. 0 = �����.")]
    public int RequiredPlayerID = 0;

    // �����, ������� ��������� ���������� � ��������� ��������
    public bool CanInteract(int commanderID)
    {
        // 1. ���� RequiredPlayerID = 0, ����������������� ����� �����.
        // 2. �����, ID ��������� ������ ��������������� ���������� ID.
        return RequiredPlayerID == commanderID;
    }
}
