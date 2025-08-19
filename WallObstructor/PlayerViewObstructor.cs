using UnityEngine;
using System.Collections.Generic; // ��� ������������� HashSet

/// <summary>
/// ����������� �������, ������������� ����� �� ������ � ������,
/// � ������ �� ����������� � ������� ������� ObstructingWall.
/// </summary>
public class PlayerViewObstructor : MonoBehaviour
{
    [Tooltip("������ �� Transform ������, ��� ���� ������ �������������.")]
    public Transform playerTransform;

    [Tooltip("������ �� Transform ������, ������� ������� �� ������.")]
    public Transform cameraTransform;

    [Tooltip("����(�), �� ������� ��������� �������, ������� ����� ���� ������� ����������� (��������, 'Walls', 'Roofs').")]
    public LayerMask obstructingLayers;

    [Tooltip("��������� ����������� ������ ����� ������� (0.0 - ��������� ����������, 1.0 - ��������� ������������).")]
    [Range(0.0f, 1.0f)]
    public float transparentAlpha = 0.3f; // ��������, 30% ��������������

    [Tooltip("������������ �������� ������������/��������� � ��������.")]
    public float fadeDuration = 0.2f;

    // ���������� HashSet ��� ������������ ������������ ����, ������� � ������ ������ ���������
    private HashSet<ObstructingWall> currentlyTransparentWalls = new HashSet<ObstructingWall>();
    private HashSet<ObstructingWall> previouslyTransparentWalls = new HashSet<ObstructingWall>();

    void Update()
    {
        if (playerTransform == null || cameraTransform == null)
        {
            Debug.LogWarning("Player Transform ��� Camera Transform �� ��������� � PlayerViewObstructor. ����������, ��������� �� � ����������.", this);
            return;
        }

        // ������� ������� ������ ��� ������ �����
        currentlyTransparentWalls.Clear();
        //previouslyTransparentWalls.Clear();
        // ����������� �� ������ � ������
        Vector3 direction = playerTransform.position - cameraTransform.position;
        float distance = direction.magnitude;

        // ��������� RaycastAll, ����� �������� ��� ������� ����� ������� � �������
        // (RaycastAll �����, ����� ������ ��� �������, � �� ������ ������)
        RaycastHit[] hits = Physics.RaycastAll(cameraTransform.position, direction.normalized, distance, obstructingLayers);

        // ���������� ��� �������, ������� ���� ������ �����
        foreach (RaycastHit hit in hits)
        {
            // ���������, ���� �� �� ������� ������� ��� ������ ObstructingWall
            ObstructingWall wall = hit.collider.GetComponent<ObstructingWall>();
            if (wall != null)
            {
                
                // ���� ��� �����, ������� �� ����� ������� ����������, ��������� �������� ������������
                wall.SetTransparent(transparentAlpha, fadeDuration);
                currentlyTransparentWalls.Add(wall); // ��������� � ������ ������� ���������� ����
            }
        }
        
        
        // ��������� �����, ������� ���� ����������� � ���������� �����,
        // �� ������ �� ��������� � ������ ������� (�.�., ��� ������ �� ����������� �����)
        foreach (ObstructingWall wall in previouslyTransparentWalls)
        {
            // ����������, ��� ����� ��� ��� ���������� � �� ��������� � ������ ������� ���������� ����
            if (wall != null && !currentlyTransparentWalls.Contains(wall))
            {
                

                wall.SetOpaque(fadeDuration); // ���������� ����� � �������������
                
            }
        } 

        // �������� ������� ���������� ����� � ������ "����������" ��� ���������� �����
        // �����: ������� ����� HashSet, ����� �������� ������� � ���������� �� ����� ��������
        previouslyTransparentWalls = new HashSet<ObstructingWall>(currentlyTransparentWalls); 
    }

    /// <summary>
    /// ���������� ������ ����� ����� ������� � ������� ��� ������� � ������ Scene.
    /// </summary>
    void OnDrawGizmos()
    {
        if (playerTransform != null && cameraTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(cameraTransform.position, playerTransform.position);
            Gizmos.DrawSphere(playerTransform.position, 0.1f); // Player position
            Gizmos.DrawSphere(cameraTransform.position, 0.1f); // Camera position
        }
    }
}
