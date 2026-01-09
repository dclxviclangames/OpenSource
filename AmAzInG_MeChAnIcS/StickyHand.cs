using UnityEngine;

public class StickyHand : MonoBehaviour
{
    public string grabbableTag = "Grabbable"; // Тег объектов, которые можно схватить
    private GameObject attachedObject = null;

    void Update()
    {
        // Если нажата левая кнопка мыши и в руке что-то есть — отпускаем


        // Отпустили пробел — отцепились
        if (Input.GetKeyUp(KeyCode.Space)) ReleaseObject();
    }

    // Срабатывает при физическом касании
    private void OnCollisionEnter(Collision collision)
    {
        // Если рука пуста и мы коснулись объекта с нужным тегом
        if (attachedObject == null && collision.gameObject.CompareTag(grabbableTag))
        {
            GrabObject(collision.gameObject);
        }
    }

    void GrabObject(GameObject obj)
    {
        attachedObject = obj;

        // Делаем объект дочерним (теперь он движется за рукой)
        obj.transform.SetParent(this.transform);

        // Отключаем физику у объекта, чтобы он не дергался при движении руки
        if (obj.GetComponent<Rigidbody>())
        {
            obj.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    void ReleaseObject()
    {
        // Убираем родителя
        if(attachedObject != null)
            attachedObject.transform.SetParent(null);

        // Возвращаем физику объекту
        if (attachedObject.GetComponent<Rigidbody>())
        {
            attachedObject.GetComponent<Rigidbody>().isKinematic = false;
        }

        attachedObject = null;
    }
}

