using UnityEngine;

public class BackroomsRaymarch : MonoBehaviour
{
    public Material raymarchMaterial;
    public float speed = 3.0f;
    private Vector3 pos = new Vector3(0, 1.2f, 0); // Высота глаз
    private float yaw = 0;

    void Update()
    {
        yaw += Input.GetAxis("Horizontal") * 90f * Time.deltaTime;
        float move = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3(Mathf.Sin(yaw * Mathf.Deg2Rad), 0, Mathf.Cos(yaw * Mathf.Deg2Rad));
        Vector3 nextPos = pos + dir * move * speed * Time.deltaTime;

        // Коллизия: проверяем стены лабиринта (математика должна совпадать с шейдером)
        if (GetMazeDist(nextPos) > 0.4f) pos = nextPos;

        // Эффект покачивания при ходьбе
        float bobbing = move != 0 ? Mathf.Sin(Time.time * 10f) * 0.05f : 0;

        raymarchMaterial.SetVector("_PlayerPos", pos + Vector3.up * bobbing);
        raymarchMaterial.SetFloat("_Yaw", yaw * Mathf.Deg2Rad);
    }

    float GetMazeDist(Vector3 p)
    {
        Vector2 grid = new Vector2(Mathf.Repeat(p.x, 6.0f) - 3.0f, Mathf.Repeat(p.z, 6.0f) - 3.0f);
        return Mathf.Max(Mathf.Abs(grid.x) - 0.5f, Mathf.Abs(grid.y) - 0.5f);
    }
}
