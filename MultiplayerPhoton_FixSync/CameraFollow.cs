// CameraFollow.cs
using UnityEngine;

/// <summary>
/// ������ ��� ������, ������� ������� �� ������� �� �������� ����.
/// ��������� ������� ������ ����� ������ ������.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Tooltip("������, �� ������� ����� ��������� ������ (������ �����).")]
    public Transform target;

    [Header("������� ������")]
    [Tooltip("���������� ������ �� ������.")]
    public float distance = 7.0f;
    [Tooltip("������ ������ ��� �������.")]
    public float height = 3.0f;
    [Tooltip("�������� ������ �� ������ �� �����������.")]
    public float horizontalOffset = 0f;

    [Header("�������� � �����������")]
    [Tooltip("��������, � ������� ������ ����� ��������� � ������� �������.")]
    public float followSpeed = 5.0f;
    [Tooltip("��������, � ������� ������ ����� ��������� ������ ������.")]
    public float rotationSpeed = 3.0f;

    [Header("��������� ��������")]
    [Tooltip("����������� ���� ������� ������ (����������� �����).")]
    public float minVerticalAngle = -30.0f;
    [Tooltip("������������ ���� ������� ������ (����������� ����).")]
    public float maxVerticalAngle = 60.0f;

    // ���������� ���������� ��� ������������ ����� ��������
    private float currentX = 0.0f;
    private float currentY = 0.0f;

    void LateUpdate() // LateUpdate ����� ��� ������, ����� �������� "��������"
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: ������� ������ (target) �� ��������. ��������� ������ ��� ��������� target.");
            return;
        }

        // �������� ���� ��� �������� ������ �����
        currentX += Input.GetAxis("Mouse X") * rotationSpeed;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;

        // ������������ ������������ �������� ������
        currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);

        // ��������� �������� ������
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Quaternion rotationS = Quaternion.Euler(0, 0, 0);


        // ��������� ������� ������� ������
        Vector3 targetPosition = target.position;
        // ��������� �������� ������ ������������ ������
        Vector3 desiredPosition = targetPosition + rotation * new Vector3(horizontalOffset, height, -distance);
        Vector3 desiredPositionS = targetPosition + rotationS * new Vector3(horizontalOffset, height, -distance);

        // ������� ����������� ������ � ������� �������
        transform.position = Vector3.Lerp(transform.position, desiredPositionS, followSpeed * Time.deltaTime); //Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // ������ ������ ������� �� ������ (� ������ ������)
        transform.LookAt(targetPosition + Vector3.up * height * 0.5f); // ������� ���� ������ ������
    }
} 


