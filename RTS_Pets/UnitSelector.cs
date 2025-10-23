// C# - UnitSelector.cs

using UnityEngine;

public class UnitSelector : MonoBehaviour
{
    // ����������� ���������� ��� �������� ���������� �����.
    // ������, ����� ��� ������� ����� �������� � ��� ������.
    public static UnitMovement SelectedUnit { get; private set; }

    void Update()
    {
        // ����� ������ ����: ����� �����
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }

        // ������ ������ ����: ������ ������� (��������)
        if (Input.GetMouseButtonDown(1) && SelectedUnit != null)
        {
            HandleMovementCommand();
        }
    }

    private void HandleSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // �������� �������� ��������� �������� � �������
            UnitMovement unit = hit.collider.GetComponent<UnitMovement>();

            // ���� �������� �� �����:
            if (unit != null)
            {
                // ������� ��������� �� �������
                if (SelectedUnit != null)
                {
                    SelectedUnit.SetSelected(false);
                }

                // �������� �����
                SelectedUnit = unit;
                SelectedUnit.SetSelected(true);
            }
            else
            {
                // ���� �������� � �������, ������� ���������
                if (SelectedUnit != null)
                {
                    SelectedUnit.SetSelected(false);
                }
                SelectedUnit = null;
            }
        }
    }

    private void HandleMovementCommand()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 1. ����������, ��� �� ������ ���-�� �������
            if (SelectedUnit != null)
            {
                InteractableItem item = hit.collider.GetComponent<InteractableItem>();

                // �������� ID �������� ���������� �����
                int currentUnitID = SelectedUnit.GetComponent<PlayerIdentity>().PlayerID;

                // --- ���������� ---
                if (item != null)
                {
                    // 2. ���������, ��������� �� ���������� ����� ����������������� � ���� ���������
                    if (item.CanInteract(currentUnitID))
                    {
                        // ���������: ������ ������� �������� � ����
                        SelectedUnit.MoveTo(hit.point);
                    }
                    else
                    {
                        // ���������: ���������� ����.
                        Debug.Log("������� �� ������������ ��� ����� ������!");
                        // ����� ��������� ���� ������.
                    }
                }
                else
                {
                    // �������� �� �� ��������, ������ ������� ���� � �����
                    SelectedUnit.MoveTo(hit.point);
                }
            }
        }
    }
}

