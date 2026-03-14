using UnityEngine;

public class TerrainDataBridge : MonoBehaviour
{
    public static TerrainDataBridge Instance { get; private set; }

    // Set by mainmenu when noise map is saved
    public string NoisemapPath;
    public string TerrainType;    // "River", "Mountain", "Plains"

    // Set by generationmenu when user picks a style image
    public byte[] StyleImageBytes;
    public string StyleImagePath;

    // Set by generationmenu after API call succeeds
    public byte[] ResultHeightmapBytes;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
