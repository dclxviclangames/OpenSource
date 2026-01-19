using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class DestructableMesh : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private List<int> triangles;

    void Start()
    {
        // Делаем копию меша, чтобы не изменять исходный ассет
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        triangles = new List<int>(mesh.triangles);
    }

    public void DestroyAtPoint(Vector3 hitPoint, float radius)
    {
        // Переводим точку попадания в локальные координаты объекта
        Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);
        float localRadius = radius / transform.lossyScale.x; // Учитываем масштаб

        int triCount = triangles.Count / 3;

        // Проходим по всем треугольникам с конца к началу
        for (int i = triCount - 1; i >= 0; i--)
        {
            // Берем индексы вершин текущего треугольника
            int idx1 = triangles[i * 3 + 0];
            int idx2 = triangles[i * 3 + 1];
            int idx3 = triangles[i * 3 + 2];

            // Проверяем, попадают ли вершины в радиус взрыва
            if (Vector3.Distance(vertices[idx1], localHitPoint) < localRadius ||
                Vector3.Distance(vertices[idx2], localHitPoint) < localRadius ||
                Vector3.Distance(vertices[idx3], localHitPoint) < localRadius)
            {
                // Удаляем 3 индекса, образующих этот треугольник
                triangles.RemoveRange(i * 3, 3);
                ApplyMeshChanges();
            }
        }

        // Обновляем меш
        ApplyMeshChanges();
    }

    void ApplyMeshChanges()
    {
        if (triangles.Count == 0)
        {
            gameObject.SetActive(false); // Или Destroy(gameObject), если объект полностью "съеден"
            return;
        }

        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        MeshCollider col = GetComponent<MeshCollider>();
        if (col != null)
        {
            col.sharedMesh = null;
            col.sharedMesh = mesh;
        }
    }
}