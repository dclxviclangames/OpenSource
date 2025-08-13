using UnityEngine;
using System.Runtime.InteropServices; // ��������� ��� ������������� DllImport

public class UnityAnimationController : MonoBehaviour
{
    public Animator animator; // ���������� ���� ��������� Animator ������ ��������� � Inspector

    // �������� ��������� � ����� Animator Controller ��� Talk � Listen
    private const string TALK_TRIGGER_NAME = "Talk";
    private const string LISTEN_TRIGGER_NAME = "Listen";
    private const string IDLE_TRIGGER_NAME = "Idle";

    // --- �������, ������� Unity ����� �������� � JavaScript (��������) ---
    // ��� ������ ���� ��������� ��� extern � ������������� �� "__Internal"

    // ����� ������� JS, ����� ����������� ���� �� ������ "�������" � HTML
    [DllImport("__Internal")]
    private static extern void JsTriggerListenClick();

    // ����� ������� JS, ����� ��������� AI � �������� �������
    [DllImport("__Internal")]
    private static extern void JsAskAIWithPrompt(string prompt);

    // ����� ������� JS, ����� ���������� ��������� � �������� (��� �������)
    [DllImport("__Internal")]
    private static extern void ShowBrowserMessage(string message);

    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("Animator component �� ������ �� ���� GameObject. ����������, ��������� ��� ��� ��������.", this);
        }
    }

    /// <summary>
    /// �������� �������� "��������" � Unity. ���������� �� JavaScript.
    /// </summary>
    public void StartTalkAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(TALK_TRIGGER_NAME);
            Debug.Log("�������� '��������' ������.");
        }
    }

    /// <summary>
    /// ������������� �������� "��������" � ���������� � ��������� �����. ���������� �� JavaScript.
    /// </summary>
    public void StopTalkAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(IDLE_TRIGGER_NAME);
            Debug.Log("�������� '��������' �����������.");
        }
    }

    /// <summary>
    /// �������� �������� "�������������" � Unity. ���������� �� JavaScript.
    /// </summary>
    public void StartListenAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(LISTEN_TRIGGER_NAME);
            Debug.Log("�������� '�������������' ������.");
        }
    }

    /// <summary>
    /// ������������� �������� "�������������" � ���������� � ��������� �����. ���������� �� JavaScript.
    /// </summary>
    public void StopListenAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(IDLE_TRIGGER_NAME);
            Debug.Log("�������� '�������������' �����������.");
        }
    }

    // --- ������, ������� ����� ���� ������� �� UNITY, ����� �������������� �� HTML/JS ---

    /// <summary>
    /// �������� ���� �� ������ "�������" � HTML-�������� �� Unity.
    /// </summary>
    public void RequestListenFromUnity()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            JsTriggerListenClick();
            ShowBrowserMessage("Unity �������� ��������� ��������� (Listen)."); // ��� �������
        }
    }

    /// <summary>
    /// ���������� ��������� ������ � AI ����� JavaScript �� Unity.
    /// </summary>
    /// <param name="prompt">����� �������.</param>
    public void RequestAIFeedbackFromUnity(string prompt)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            JsAskAIWithPrompt(prompt);
            ShowBrowserMessage($"Unity �������� AI � �������: '{prompt}'"); // ��� �������
        }
    }

    // --- ������ ��� ��������� ����������� �� JavaScript � Unity ---
    // (��� ������ ����� ���������� ����� unityInstance.SendMessage �� index.html)

    /// <summary>
    /// ���������� �� JavaScript, ����� AI �������� ������������ �����.
    /// </summary>
    public void OnAIResponseStart()
    {
        Debug.Log("Unity: AI ����� ������������ �����.");
        // ����� ����� ������������ �������� "������" ��� "�������"
    }

    /// <summary>
    /// ���������� �� JavaScript, ����� AI ��������� ������������ �����.
    /// </summary>
    public void OnAIResponseEnd()
    {
        Debug.Log("Unity: AI �������� ������������ �����.");
        // ����� ����� �������������� �������� "������"
    }

    /// <summary>
    /// ���������� �� JavaScript, ����� ������� �������� ���������� �����.
    /// </summary>
    public void OnTTSStart()
    {
        Debug.Log("Unity: ������� ����� ���������� ����� AI.");
        StartTalkAnimation(); // ���������� �������� ���������
    }

    /// <summary>
    /// ���������� �� JavaScript, ����� ������� ����������� ���������� �����.
    /// </summary>
    public void OnTTSEnd()
    {
        Debug.Log("Unity: ������� �������� ���������� ����� AI.");
        StopTalkAnimation(); // ������������� �������� ���������
    }
}
