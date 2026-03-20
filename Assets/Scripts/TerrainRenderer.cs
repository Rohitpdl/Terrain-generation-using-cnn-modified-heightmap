using UnityEngine;

public class TerrainRenderer : MonoBehaviour
{
    [Header("Mesh Settings")]
    public int   resolution  = 256;
    public float terrainSize = 100f;
    public float heightScale = 20f;

    [Header("Material")]
    public Material terrainMaterial;

    [Header("Height Colors (low → high)")]
    public Color waterColor    = new Color(0.10f, 0.25f, 0.55f);  // deep blue
    public Color sandColor     = new Color(0.76f, 0.70f, 0.50f);  // sand
    public Color grassColor    = new Color(0.20f, 0.50f, 0.15f);  // green
    public Color rockColor     = new Color(0.45f, 0.38f, 0.30f);  // brown-grey
    public Color snowColor     = new Color(0.92f, 0.95f, 1.00f);  // white

    // Height thresholds (0-1, fraction of heightScale)
    [Header("Height Thresholds (0-1)")]
    public float waterLevel  = 0.20f;
    public float sandLevel   = 0.28f;
    public float grassLevel  = 0.55f;
    public float rockLevel   = 0.78f;
    // above rockLevel = snow

    void Start()
    {
        if (TerrainDataBridge.Instance == null ||
            TerrainDataBridge.Instance.ResultHeightmapBytes == null)
        {
            Debug.LogError("TerrainRenderer: no result heightmap in bridge!");
            return;
        }

        Texture2D heightmap = BytesToTexture(TerrainDataBridge.Instance.ResultHeightmapBytes);
        BuildMesh(heightmap);
    }

    Texture2D BytesToTexture(byte[] bytes)
    {
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
        tex.LoadImage(bytes);
        return tex;
    }

    void BuildMesh(Texture2D heightmap)
    {
        int   res    = resolution;
        float step   = terrainSize / (res - 1);
        float offset = terrainSize / 2f;

        Vector3[] vertices  = new Vector3[res * res];
        Vector2[] uvs       = new Vector2[res * res];
        Color[]   colors    = new Color[res * res];
        int[]     triangles = new int[(res - 1) * (res - 1) * 6];

        for (int z = 0; z < res; z++)
        {
            for (int x = 0; x < res; x++)
            {
                float u      = (float)x / (res - 1);
                float v      = (float)z / (res - 1);
                float h      = heightmap.GetPixelBilinear(u, v).grayscale;
                float height = h * heightScale;

                int idx       = z * res + x;
                vertices[idx] = new Vector3(x * step - offset, height, z * step - offset);
                uvs[idx]      = new Vector2(u, v);
                colors[idx]   = HeightToColor(h);
            }
        }

        int t = 0;
        for (int z = 0; z < res - 1; z++)
        {
            for (int x = 0; x < res - 1; x++)
            {
                int tl = z * res + x;
                int tr = tl + 1;
                int bl = tl + res;
                int br = bl + 1;

                triangles[t++] = tl; triangles[t++] = bl; triangles[t++] = tr;
                triangles[t++] = tr; triangles[t++] = bl; triangles[t++] = br;
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices    = vertices;
        mesh.uv          = uvs;
        mesh.colors      = colors;
        mesh.triangles   = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter   mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mf.mesh = mesh;

        // Use vertex-color material if none assigned
        if (terrainMaterial != null)
        {
            mr.material = terrainMaterial;
        }
        else
        {
           
            Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
            if (mat.shader.name == "Hidden/InternalErrorShader")
                mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mr.material = mat;
        }

        Debug.Log("Terrain mesh built — vertices: " + vertices.Length);
    }

    Color HeightToColor(float h)
    {
        if (h < waterLevel)
            return Color.Lerp(waterColor, waterColor, Mathf.InverseLerp(0f, waterLevel, h));
        if (h < sandLevel)
            return Color.Lerp(waterColor, sandColor, Mathf.InverseLerp(waterLevel, sandLevel, h));
        if (h < grassLevel)
            return Color.Lerp(sandColor, grassColor, Mathf.InverseLerp(sandLevel, grassLevel, h));
        if (h < rockLevel)
            return Color.Lerp(grassColor, rockColor, Mathf.InverseLerp(grassLevel, rockLevel, h));
        return Color.Lerp(rockColor, snowColor, Mathf.InverseLerp(rockLevel, 1f, h));
    }
}
