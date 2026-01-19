using UnityEngine;

public class MouseDestructor : MonoBehaviour
{
    public float destructionRadius = 2.0f; // Радиус "кирки"
    public Camera cam; // Ссылка на основную камеру (перетащите в инспекторе)

    void Update()
    {
        // Проверяем нажатие левой кнопки мыши
        if (Input.GetMouseButtonDown(0))
        {
            // Создаем луч из позиции мыши в мир
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Если луч попадает в какой-либо объект
            if (Physics.Raycast(ray, out hit))
            {
                // Проверяем, является ли объект разрушаемым
                DestructableMesh destructible = hit.transform.GetComponent<DestructableMesh>();

                if (destructible != null)
                {
                    // Вызываем метод разрушения, передавая точку попадания и радиус
                    destructible.DestroyAtPoint(hit.point, destructionRadius);
                }
            }
        }
    }
}
