// C# - UnitSelector.cs

using UnityEngine;

public class UnitSelector : MonoBehaviour
{
    // Статическая переменная для хранения выбранного юнита.
    // Удобно, чтобы все скрипты могли получить к ней доступ.
    public static UnitMovement SelectedUnit { get; private set; }

    void Update()
    {
        // Левая кнопка мыши: Выбор юнита
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }

        // Правая кнопка мыши: Отдача команды (движение)
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
            // Пытаемся получить компонент движения с объекта
            UnitMovement unit = hit.collider.GetComponent<UnitMovement>();

            // Если кликнули по юниту:
            if (unit != null)
            {
                // Снимаем выделение со старого
                if (SelectedUnit != null)
                {
                    SelectedUnit.SetSelected(false);
                }

                // Выделяем новый
                SelectedUnit = unit;
                SelectedUnit.SetSelected(true);
            }
            else
            {
                // Если кликнули в пустоту, снимаем выделение
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
            // 1. Убеждаемся, что мы вообще что-то выбрали
            if (SelectedUnit != null)
            {
                InteractableItem item = hit.collider.GetComponent<InteractableItem>();

                // Получаем ID текущего выбранного юнита
                int currentUnitID = SelectedUnit.GetComponent<PlayerIdentity>().PlayerID;

                // --- ФИЛЬТРАЦИЯ ---
                if (item != null)
                {
                    // 2. Проверяем, разрешено ли выбранному юниту взаимодействовать с этим предметом
                    if (item.CanInteract(currentUnitID))
                    {
                        // РАЗРЕШЕНО: Отдаем команду движения к цели
                        SelectedUnit.MoveTo(hit.point);
                    }
                    else
                    {
                        // ЗАПРЕЩЕНО: Игнорируем клик.
                        Debug.Log("Предмет не предназначен для этого игрока!");
                        // Можно проиграть звук ошибки.
                    }
                }
                else
                {
                    // Кликнули не по предмету, просто двигаем юнит в точку
                    SelectedUnit.MoveTo(hit.point);
                }
            }
        }
    }
}

