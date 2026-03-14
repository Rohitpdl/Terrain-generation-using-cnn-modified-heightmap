using UnityEngine;

public class TerrainRenderer : MonoBehaviour
{
    [Header("Mesh Settings")]
    public int   resolution  = 256;    // vertex grid: resolution x resolution
    public float terrainSize = 100f;   // world-space width and depth
    public float heightScale = 20f;    // maximum height in world units

    [Header("Material")]
    public Material terrainMaterial;   // assign a URP Lit material in Inspector

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

    // ── Decode bytes → Texture2D ─────────────────────────────────────────────
    Texture2D BytesToTexture(byte[] bytes)
    {
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
        tex.LoadImage(bytes);   // auto-resizes to the actual image dimensions
        return tex;
    }

    // ── Build procedural mesh from heightmap ─────────────────────────────────
    void BuildMesh(Texture2D heightmap)
    {
        int   res    = resolution;
        float step   = terrainSize / (res - 1);
        float offset = terrainSize / 2f;

        Vector3[] vertices  = new Vector3[res * res];
        Vector2[] uvs       = new Vector2[res * res];
        int[]     triangles = new int[(res - 1) * (res - 1) * 6];

        for (int z = 0; z < res; z++)
        {
            for (int x = 0; x < res; x++)
            {
                float u      = (float)x / (res - 1);
                float v      = (float)z / (res - 1);
                float height = heightmap.GetPixelBilinear(u, v).grayscale * heightScale;

                int idx       = z * res + x;
                vertices[idx] = new Vector3(x * step - offset, height, z * step - offset);
                uvs[idx]      = new Vector2(u, v);
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
        mesh.triangles   = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter   mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mf.mesh     = mesh;
        mr.material = terrainMaterial != null
            ? terrainMaterial
            : new Material(Shader.Find("Universal Render Pipeline/Lit"));

        Debug.Log("Terrain mesh built — vertices: " + vertices.Length);
    }
}
