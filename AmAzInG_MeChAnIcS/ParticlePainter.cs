using UnityEngine;
using System.Collections.Generic;

public class ParticlePainter : MonoBehaviour
{
    public ParticleSystem part;
    public int brushRadius = 5;
    public Color paintColor = Color.red;

    private Texture2D paintTexture;
    private int textureSize;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    private Collider myCollider;

    void Start()
    {
        myCollider = GetComponent<Collider>();
        Renderer rend = GetComponent<Renderer>();

        if (rend != null && rend.material.mainTexture != null)
        {
            // 1. Получаем оригинальную текстуру
            Texture2D sourceTex = rend.material.mainTexture as Texture2D;
            textureSize = sourceTex.width;

            // 2. Создаем копию, чтобы не испортить исходный файл
            // Убедитесь, что в настройках импорта исходной текстуры стоит галочка "Read/Write Enabled"
            paintTexture = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGBA32, false);

            // Копируем пиксели из оригинала в нашу рабочую текстуру
            paintTexture.SetPixels(sourceTex.GetPixels());
            paintTexture.Apply();

            // 3. Назначаем новую текстуру в материал
            rend.material.mainTexture = paintTexture;
        }
        else
        {
            Debug.LogError("На объекте нет рендерера или основной текстуры!");
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (part == null) return;

        int numCollisionEvents = part.GetCollisionEvents(this.gameObject, collisionEvents);

        for (int i = 0; i < numCollisionEvents; i++)
        {
            Vector3 pos = collisionEvents[i].intersection;
            Vector3 normal = collisionEvents[i].normal;

            // Пускаем луч чуть-чуть "снаружи" внутрь поверхности
            Ray ray = new Ray(pos + normal * 0.05f, -normal);
            RaycastHit hit;

            if (myCollider.Raycast(ray, out hit, 0.5f))
            {
                DrawOnTexture(hit.textureCoord);
            }
        }
    }

    void DrawOnTexture(Vector2 uv)
    {
        int centerX = (int)(uv.x * textureSize);
        int centerY = (int)(uv.y * textureSize);

        bool paintChanged = false;

        for (int x = -brushRadius; x <= brushRadius; x++)
        {
            for (int y = -brushRadius; y <= brushRadius; y++)
            {
                if (x * x + y * y <= brushRadius * brushRadius)
                {
                    int px = centerX + x;
                    int py = centerY + y;

                    if (px >= 0 && px < textureSize && py >= 0 && py < textureSize)
                    {
                        // Рисуем цветом поверх существующей текстуры
                        paintTexture.SetPixel(px, py, paintColor);
                        paintChanged = true;
                    }
                }
            }
        }

        if (paintChanged)
        {
            paintTexture.Apply();
        }
    }
}


