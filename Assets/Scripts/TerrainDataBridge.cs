using UnityEngine;

public class TerrainDataBridge : MonoBehaviour
{
    public static TerrainDataBridge Instance { get; private set; }

    // ── Set by mainmenu ───────────────────────────────────────────────────────
    public string NoisemapPath;
    public string TerrainType;       // "River", "Mountain", "Plains"
    public bool   IsUserUploaded;    // true = user uploaded, false = generated

    // ── Set by generationmenu ─────────────────────────────────────────────────
    public byte[] StyleImageBytes;
    public string StyleImagePath;

    // ── Set after API call ────────────────────────────────────────────────────
    public byte[] ResultHeightmapBytes;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
