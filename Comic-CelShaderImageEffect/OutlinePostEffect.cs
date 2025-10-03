using UnityEngine;

// [ExecuteInEditMode] ��������� ������ ������ ����� � ���������
[ExecuteInEditMode]
// ������� ��������� Camera ��� ������
[RequireComponent(typeof(Camera))]
public class OutlinePostEffect : MonoBehaviour
{
    [Tooltip("��������, ������������ ������ OutlinePostEffectShader.")]
    public Material effectMaterial;

    [Header("��������� ��������")]
    public Color outlineColor = Color.black; // ���� �������
    [Range(0.0001f, 0.1f)]
    public float depthThreshold = 0.01f; // ����� ��� ����������� �������� �� �������
    [Range(0.01f, 0.9f)]
    public float normalThreshold = 0.1f; // ����� ��� ����������� �������� �� ��������

    private Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();

        // ������ ����� �������� ����� ������� � �������� ��� �������
        cam.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.DepthNormals;

        // ���� �������� �� ��������, ������� ��� �� �������
        if (effectMaterial == null)
        {
            effectMaterial = new Material(Shader.Find("Hidden/OutlinePostEffectShader"));
            if (effectMaterial == null)
            {
                Debug.LogError("OutlinePostEffect: ������ 'Hidden/OutlinePostEffectShader' �� ������. ���������, ��� ������ ������ � ��� �������� ���������.");
                enabled = false;
                return;
            }
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (effectMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // �������� ��������� � ������
        effectMaterial.SetColor("_OutlineColor", outlineColor);
        effectMaterial.SetFloat("_DepthThreshold", depthThreshold);
        effectMaterial.SetFloat("_NormalThreshold", normalThreshold);

        // ��������� ������
        Graphics.Blit(source, destination, effectMaterial);
    }

   
}
