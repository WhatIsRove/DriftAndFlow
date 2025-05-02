using UnityEngine;

[ExecuteInEditMode]
public class ColourGenerator : MonoBehaviour
{
    public Material mat;
    public Gradient gradient;
    public float normalOffsetWeight;

    Texture2D texture;
    const int textureResolution = 50;

    void Init()
    {
        if (texture == null || texture.width != textureResolution)
        {
            texture = new Texture2D(textureResolution, 1, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
        }
    }

    void Update()
    {
        if (mat == null) return;

        Init();
        UpdateTexture();

        MeshGenerator m = FindObjectOfType<MeshGenerator>();
        if (m != null)
        {
            float boundsY = m.boundsSize * m.numChunks.y;
            mat.SetFloat("_boundsY", boundsY);
        }

        mat.SetFloat("_normalOffsetWeight", normalOffsetWeight);
        mat.SetTexture("_ramp", texture);
    }

    void UpdateTexture()
    {
        if (gradient != null)
        {
            Color[] colours = new Color[textureResolution];
            for (int i = 0; i < textureResolution; i++)
            {
                colours[i] = gradient.Evaluate(i / (textureResolution - 1f));
            }

            texture.SetPixels(colours);
            texture.Apply();
        }
    }
}