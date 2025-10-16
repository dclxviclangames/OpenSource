// StreamingCommentsSimulator.cs
// StreamingCommentsSimulator.cs
using UnityEngine;
using UnityEngine.UI; // ��� ScrollRect, LayoutRebuilder
using TMPro; // ��� TextMeshProUGUI
using System.Collections; // ��� �������
using System.Text; // ��� StringBuilder
using System.Linq; // ��� Random
using System.Collections.Generic;
using UnityEngine.EventSystems; // ����� ������������ ��� ��������� UI-��������, �� �� ����������� �����

/// <summary>
/// ���������� ����� ������������ ������������ � Scroll View.
/// ������������� ���������� ����������� � ������������ Scroll View ����.
/// </summary>
public class StreamingCommentsSimulaton : MonoBehaviour
{
    [Header("UI ��������")]
   // [Tooltip("������ �� ��������� TextMeshProUGUI ������ Content Scroll View.")]
   // public TextMeshProUGUI chatContentText;

    [Tooltip("������ �� ��� ��������� ScrollRect.")]
    public ScrollRect chatScrollRect;

    [Header("��������� ������������")]
    [Tooltip("������ �����, �� ������� ����� �������������� ��������� �����������.")]
    [TextArea(3, 10)] // ������ ���� ������������� � ����������
    public string[] randomCommentTemplates;

    [Tooltip("����������� �������� ����� ������������� (� ��������).")]
    public float minCommentInterval = 1f;

    [Tooltip("������������ �������� ����� ������������� (� ��������).")]
    public float maxCommentInterval = 3f;

    [Tooltip("������������ ���������� ������������, ������� ����� ��������� � ����.")]
    public int maxChatLines = 50;

    [Tooltip("�������� ��������� Scroll View (������ �������� - ������� ���������).")]
    public float scrollSpeed = 2.5f;

    private float timer = 0; // ������ ��� ������� ������� �� ���������� �����������

    public TMP_Text tMP_Text;

    // ������ ��� ������������ ������� ����� ����
    private readonly List<string> currentChatLines = new List<string>();

    void Start()
    {
       
        if (chatScrollRect == null)
        {
            Debug.LogError("StreamingCommentsSimulator: chatScrollRect �� ��������! ����������, ���������� ScrollRect.");
            enabled = false;
            return;
        }
        if (randomCommentTemplates == null || randomCommentTemplates.Length == 0)
        {
            Debug.LogWarning("StreamingCommentsSimulator: randomCommentTemplates ����. �������� ��������� ��������� ������������.");
            randomCommentTemplates = new string[] { "������ ����!", "����� �������!", "��� ����������?", "���� � ��������!", "� ��� ���������!" };
        }

      //  timer = Random.Range(minCommentInterval, maxCommentInterval);
        //chatContentText.text = "";
    }

    void Update()
    {
        

        if (timer > 0.2)
        {
            if (chatScrollRect.verticalNormalizedPosition > 0)
                chatScrollRect.verticalNormalizedPosition = Mathf.Lerp(chatScrollRect.verticalNormalizedPosition, 0f, Time.deltaTime * scrollSpeed);

            if (chatScrollRect.verticalNormalizedPosition < 0)
                chatScrollRect.verticalNormalizedPosition = Mathf.Lerp(chatScrollRect.verticalNormalizedPosition, 1f, Time.deltaTime * scrollSpeed);


            timer = 0;
            // AddRandomComment();
            // timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
           
        }

        // ������� ���������, ���� �� �� �� ����� ���� (������ ���� ScrollRect �� �������������� �������)
        // ����� ������������ ��� ��� ���������� ����� � ��� �� ������������ �����.
        // �� ������ ������ ��� �����, ���� ��� ����� ������ ��������� ��� ���������� �����������.
        
    }

    /// <summary>
    /// ���������� � ��������� ��������� ����������� � ���.
    /// </summary>
    private void AddRandomComment()
    {
        
        // ��������� �������� ��� ���������, ����� ���� UI ����� �� �����������
        StartCoroutine(ScrollToBottomDelayed());
    }

    /// <summary>
    /// ���������� ��������� �������.
    /// </summary>
    /// <returns>��������� �������.</returns>
   
    /// <summary>
    /// �������� ��� ��������� Scroll View ���� � ��������� ��������� � �������������� ����������� ������.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ScrollToBottomDelayed()
    {
        // ���� ���� ����, ����� TextMeshProUGUI ����� �������� ���� ������
        yield return null;

        // === �������� �����������: �������������� ���������� ������ ===
        // ��� �����������, ��� Content ScrollRect ��������� ����������� ���� ������.
        if (chatScrollRect != null && chatScrollRect.content != null)
        {
            // ������� ��������� ��� Layout Group �� Canvas (���� ����)
            Canvas.ForceUpdateCanvases();
            // ����� ������������� ������������� ����� ��� Content
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatScrollRect.content.GetComponent<RectTransform>());
           // timer = Random.Range(minCommentInterval, maxCommentInterval);

            // ���� ��� ���� ���� ����� �������������� �����������, ����� ��� ������������.
            yield return null;

            // ������ ������������ � ������ ����
            chatScrollRect.verticalNormalizedPosition = 1f;
            Debug.Log("ScrollToBottomDelayed: Scrolled to bottom.");
        }
        else
        {
            Debug.LogWarning("ScrollToBottomDelayed: chatScrollRect or its content is null, cannot scroll.");
        }
    }
}

