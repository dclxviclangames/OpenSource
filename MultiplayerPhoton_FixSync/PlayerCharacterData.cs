// PlayerCharacterData.cs
using UnityEngine;

/// <summary>
/// ScriptableObject ��� �������� ������ � ������ ������� ���������.
/// ��� ��������� ����� ��������� ����� ���������� � ��������� ��� ��������� ����.
/// </summary>
[CreateAssetMenu(fileName = "NewPlayerCharacter", menuName = "Game Data/Player Character")]
public class PlayerCharacterData : ScriptableObject
{
    [Tooltip("��� ���������, ������������ � UI.")]
    public string characterName = "����� ��������";

    [Tooltip("������ 3D ������ ����� ���������. ������ ���� � ����� Resources/PhotonPrefabs.")]
    public GameObject characterPrefab;

    [Tooltip("������, ������������ � UI ������ ���������.")]
    public Sprite characterIcon;

    [Tooltip("��������� �������� ��� ����� ���������.")]
    public int baseHealth = 100;

    // �������� ������ ��������������, ���� ���������� (��������, ���� � �.�.)
    // public float moveSpeed = 5f;
    // public int baseDamage = 10;
}