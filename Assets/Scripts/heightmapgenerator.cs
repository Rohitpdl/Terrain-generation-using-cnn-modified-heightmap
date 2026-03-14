using UnityEngine;
using System.IO;

public class HeightmapGenerator : MonoBehaviour
{
    [Header("Heightmap Settings")]
    public int width = 512;
    public int height = 512;

    [Header("Noise Settings")]
    public int seed = 42;
    public int octaves = 5;
    public float scale = 50f;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    Texture2D heightmapTexture;

    // Exposed so generationmenu can display it
    public Texture2D LastGeneratedTexture { get; private set; }

    // Returns the full path of the saved file
    public string GenerateAndDownload()
    {
        seed = Random.Range(0, 100000);
        heightmapTexture = GenerateHeightmap();
        LastGeneratedTexture = heightmapTexture;
        return SaveHeightmapAsPNG(heightmapTexture);
    }

    Texture2D GenerateHeightmap()
    {
        Texture2D texture = new Texture2D(width, height);
        float[,] noiseMap = GenerateFractalNoise();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float value = noiseMap[x, y];
                Color color = new Color(value, value, value);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    float[,] GenerateFractalNoise()
    {
        float[,] noiseMap = new float[width, height];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x / scale) * frequency + octaveOffsets[i].x;
                    float sampleY = (y / scale) * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(
                    minNoiseHeight,
                    maxNoiseHeight,
                    noiseMap[x, y]
                );
            }
        }

        return noiseMap;
    }

    // Now returns the saved path instead of void
    string SaveHeightmapAsPNG(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();

        string folderPath = Path.Combine(Application.persistentDataPath, "heightmaps");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fileName = "Heightmap_" + seed + ".png";
        string fullPath = Path.Combine(folderPath, fileName);

        File.WriteAllBytes(fullPath, bytes);

        Debug.Log("Heightmap saved to: " + fullPath);
        return fullPath;
    }
}
