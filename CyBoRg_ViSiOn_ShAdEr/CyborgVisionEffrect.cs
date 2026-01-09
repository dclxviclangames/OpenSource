using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CyborgVisionEffect : MonoBehaviour
{
    public Material visionMaterial;
    [Header("Glitch Settings")]
    [Range(0f, 1f)]
    public float glitchIntensity = 0.0f;
    [Header("Color Settings")]
    [Range(0f, 1f)]
    public float aberrationIntensity = 0.5f;
    public Color visionColor = Color.green;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (visionMaterial != null)
        {
            visionMaterial.SetFloat("_GlitchIntensity", glitchIntensity);
            visionMaterial.SetFloat("_AberrationIntensity", aberrationIntensity);
            visionMaterial.SetColor("_VisionColor", visionColor);

            // Анимация глитча со временем
            visionMaterial.SetFloat("_TimeScale", Time.time * 10f);

            Graphics.Blit(source, destination, visionMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}

