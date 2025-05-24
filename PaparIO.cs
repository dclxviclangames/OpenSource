using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // ��� ������������� .Last()

public class PaperIO : MonoBehaviour
{
    public float moveSpeed = 5f; // �������� �������� ������
    public LineRenderer lineRenderer; // ������ �� ��������� LineRenderer
    public Material lineMaterial; // �������� ��� LineRenderer (��� �����)
    public float lineWidth = 0.1f; // ������ �����
    public Color playerColor = Color.red; // ���� ����� ������/�������
    public Material territoryMaterial; // �������� ��� ����������� ����������� ����������
    public float lineDrawMinDistance = 0.1f; // ����������� ���������� ��� ���������� ����� ����� � �����

    private List<Vector3> currentLinePoints = new List<Vector3>(); // ������ ����� ��� ������� �������� �����
    private bool isDrawing = false; // ����, �����������, ������ �� ����� � ������ ������
    private Vector3 lastMoveDirection; // ��������� ����������� ��������
    private bool wasMovingLastFrame = false; // ��� �� ����� � �������� � ���������� �����

    public Transform playerModel; // ������ �� ������ ������ (��� ��������)
    public float rotationSpeed = 10f; // �������� �������� ������

    public Transform[] spawnPoints; // ������ ����� ��� ����������� �������

    void Start()
    {
        // ���������, �������� �� LineRenderer
        if (lineRenderer == null)
        {
            // ���� LineRenderer �� ��������, ������� ����� GameObject � LineRenderer
            GameObject lineObject = new GameObject("Line_" + gameObject.name);
            lineObject.transform.SetParent(this.transform); // ������ LineRenderer �������� �������� ������
            lineRenderer = lineObject.AddComponent<LineRenderer>();
        }

        // ����������� LineRenderer
        lineRenderer.material = lineMaterial;
        lineRenderer.widthCurve = AnimationCurve.Constant(0, 0, lineWidth);
        lineRenderer.startColor = playerColor; // ���� ����� ������������� ����� ������
        lineRenderer.endColor = playerColor;
        lineRenderer.useWorldSpace = true; // ���������� ������� ����������
        lineRenderer.positionCount = 0; // �������� � 0 �����
    }

    void Update()
    {
        // �������� ���� �� ������
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // ��������� ����������� ��������
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // ������� ������
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // ���� ����� ���������
        if (moveDirection != Vector3.zero)
        {
            // ������������ ������ ������ � ����������� ��������
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            lastMoveDirection = moveDirection; // ��������� �����������
        }

        bool isMoving = moveDirection != Vector3.zero;

        // ������ ������/��������� ��������� ����� ��� ��������
        if (isMoving && !wasMovingLastFrame && !isDrawing) // ����� ����� ��������� � �� ������
        {
            StartDrawing();
            
        }
        else if (!isMoving && wasMovingLastFrame && isDrawing) // ����� ����������� � ������
        {
            EndDrawing();
        }
        else if (isMoving && isDrawing) // ����� ���������� ��������� � ������
        {
            AddPointToLine();
            lineRenderer.SetPositions(currentLinePoints.ToArray());
            lineRenderer.material = lineMaterial;
        }

        wasMovingLastFrame = isMoving;

        // �������� ������������ � ������� ������ �������
        CheckOtherPlayerLines();
    }

    void StartDrawing()
    {
        isDrawing = true;
        currentLinePoints.Clear(); // �������� ����� ������
        lineRenderer.positionCount = 0;
        // ���������� ���������� ����� LineRenderer

        AddPointToLine(); // ��������� ������ �����

    }

    void EndDrawing()
    {
        if (!isDrawing) return; // ��������, ����� �������� ���������� ������
        isDrawing = false;

        // ������� LineRenderer
        lineRenderer.positionCount = 0;

        // ���� ����� ����������, �������� ��������� ����������
        if (currentLinePoints.Count > 2) // ������� ������ ����� ��� ������� 3 �����
        {
            CaptureTerritory(currentLinePoints);
        }
        currentLinePoints.Clear(); // ������� ����� ����� ���������
    }

    void AddPointToLine()
    {
        Vector3 currentPosition = transform.position;
        // ��������� �����, ������ ���� ��� ���������� ������ �� ����������
        if (currentLinePoints.Count == 0 || Vector3.Distance(currentLinePoints.Last(), currentPosition) > lineDrawMinDistance)
        {
            currentLinePoints.Add(currentPosition);
            lineRenderer.positionCount = currentLinePoints.Count;
            lineRenderer.SetPositions(currentLinePoints.ToArray());
           // lineRenderer.material = territoryMaterial;
        }
    }

    void CaptureTerritory(List<Vector3> points)
    {
        // ��������: ��� ���������� ������ ������� ���������� ��� Paper.io.
        // ��� ������� ��� �� ���������� ������� �����.
        // ��� ����������� Paper.io ��������� ����� ������� �������� �������/����������� ����������,
        // ������� ������������ ���������������, ��������� � ������� � ������������� ������������.
        // ����� ������������ ������� ������������ "������", ������� �������� ������ ��� �������� ���������.

        if (points.Count < 3) return; // ������� ������ ����� ��� ������� 3 �����

        // ��������� ������ ����� � �����, ����� �������� ������ ��� ������������
        List<Vector3> closedLoopPoints = new List<Vector3>(points);
        closedLoopPoints.Add(points[0]);

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // ���������� ������������ ��� �������� ��������� (�� �������� ��� ������������������/��������)
        // ��� ������ ������ ����������� Triangle.NET ��� ����������� ����������
        for (int i = 0; i < closedLoopPoints.Count - 2; i++)
        {
            // ���������� ������ ����� ��� "�����" �����
            vertices.Add(closedLoopPoints[0]);
            vertices.Add(closedLoopPoints[i + 1]);
            vertices.Add(closedLoopPoints[i + 2]);

            triangles.Add(vertices.Count - 3);
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        // ������� GameObject ��� ����������� ����������� ����������
        GameObject territoryObject = new GameObject("CapturedTerritory_" + this.name + "_" + Time.time); // ���������� ���
        territoryObject.transform.position = playerModel.transform.position; //Vector3.zero; // ������� ���� ������������ ������� ���������
        territoryObject.transform.Rotate(180f, 0f, 0f);
        territoryObject.AddComponent<MeshFilter>().mesh = mesh;
        MeshRenderer renderer = territoryObject.AddComponent<MeshRenderer>();
        renderer.material = territoryMaterial;
        renderer.material.color = playerColor; // ���������� � ���� ������
    }

    void CheckOtherPlayerLines()
    {
        PaperIO[] allPlayers = FindObjectsOfType<PaperIO>();
        foreach (PaperIO otherPlayer in allPlayers)
        {
            if (otherPlayer == this) continue; // ���������� ����

            // ���������, ������ �� ������ ����� �����
            if (otherPlayer.isDrawing && otherPlayer.currentLinePoints.Count > 1)
            {
                // ���������� �������� ������������: ��������� ���������� �� ����� ����� ������� ������.
                // ��� ������ �������� ������������ � ������� (����������� ��������)
                // ����������� ����� ������� �������� ��� ������������� ����������� �� �����.
                foreach (Vector3 linePoint in otherPlayer.currentLinePoints)
                {
                    // ���� ������� ����� ��������� ���������� ������ � ����� �� ����� ������� ������
                    if (Vector3.Distance(transform.position, linePoint) < 0.5f) // 0.5f - ��� ��������� ������ ������������
                    {
                        Debug.Log(this.name + " ���������� � ������ " + otherPlayer.name + "!");
                        otherPlayer.Respawn(); // ���������� ������, ��� ����� ���� ������
                        return; // ������� ����� ������� ������������, ����� �� ���������� ��������� ���
                    }
                }
            }
        }
    }

    public void Respawn()
    {
        Debug.Log(this.name + " ��� ��������� � ������������!");
        // ������� ������� �����
        currentLinePoints.Clear();
        lineRenderer.positionCount = 0;
        isDrawing = false;

        // ���������� � ��������� �����
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            transform.position = spawnPoints[randomIndex].position;
        }
        else
        {
            Debug.LogWarning("����� ����������� �� ��������� ��� " + this.name + ". ����������� � (0,0,0).");
            transform.position = Vector3.zero;
        }
    }
}
