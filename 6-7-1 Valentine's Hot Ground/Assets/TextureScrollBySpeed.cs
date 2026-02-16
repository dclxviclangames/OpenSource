using UnityEngine;

public class TextureScrollBySpeed : MonoBehaviour
{
    public float scrollMultiplier = 0.1f; // Чувствительность прокрутки
    private Renderer rend;
    private Rigidbody rb;
    private Vector2 offset;
    float speed = 5f;
    public Vector3 direction;
    public float boostForce = 155;

    void Start()
    {
        rend = GetComponent<Renderer>();
      //  rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Вычисляем пройденный путь на основе текущей скорости и времени
        // Используем magnitude, чтобы текстура бежала всегда в одну сторону, 
        // либо вектор скорости для направления.
      //  float speed = rb.velocity.magnitude;

        offset.y += -speed * scrollMultiplier * Time.deltaTime;

        // Применяем смещение к основной текстуре материала
        rend.material.mainTextureOffset = offset;
    }

    private void OnCollisionStay(Collision collision)
    {
        // Если врезались во что-то (кроме земли) и еще не в аварии
        if (collision.gameObject.CompareTag("Dude"))
        {
            Rigidbody rigidbody = collision.gameObject.GetComponent<Rigidbody>();
            rigidbody.AddForce(direction * boostForce, ForceMode.Impulse);
        }

        if (collision.gameObject.CompareTag("Dudes"))
        {
            Rigidbody rigidbody = collision.gameObject.GetComponent<Rigidbody>();
            rigidbody.AddForce(direction * boostForce, ForceMode.Impulse);
        }
    }
}

