using UnityEngine;

public class SimpleFilter : MonoBehaviour
{
    [SerializeField] private Shader _shader;

    protected Material _mat;

    private bool _useFilter = true;
  //  private RenderTexture rt;

    private void Awake()
    {
       // rt = new RenderTexture(*width *, *height *, 0, RenderTextureFormat.ARGB32);
        _mat = new Material(_shader);
        Init();
    }

    protected virtual void Init()
    {

    }

    private void Update()
    {
       

        OnUpdate();
    }

    protected virtual void OnUpdate()
    {

    }

    private void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (_useFilter)
        {
            UseFilter(src, dst);
        }
        else
        {
            Graphics.Blit(src, dst);
        }
    }

    protected virtual void UseFilter(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst, _mat);
    }
}
