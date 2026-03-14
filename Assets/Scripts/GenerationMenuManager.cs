using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using AnotherFileBrowser.Windows;
using TMPro;

public class GenerationMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public RawImage noisemapPreview;    // shows the generated noise map
    public RawImage styleImagePreview;  // shows the user-picked style image
    public Button   generateButton;
    public TMP_Text statusText;

    [Header("API Settings")]
    public string apiUrl = "http://localhost:5000/style_transfer";

    void Start()
    {
        generateButton.interactable = false;

        if (TerrainDataBridge.Instance != null &&
            !string.IsNullOrEmpty(TerrainDataBridge.Instance.NoisemapPath))
        {
            StartCoroutine(LoadTextureFromPath(
                TerrainDataBridge.Instance.NoisemapPath,
                noisemapPreview));

            statusText.text = "Terrain type: " + TerrainDataBridge.Instance.TerrainType +
                              "\nBrowse a style image to continue.";
        }
        else
        {
            statusText.text = "Error: no noise map found. Please go back to the main menu.";
        }
    }

    // ── Browse button ────────────────────────────────────────────────────────
    public void OnBrowseStyleImage()
    {
        var bp = new BrowserProperties();
        bp.filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
        bp.filterIndex = 0;

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            if (string.IsNullOrEmpty(path)) return;

            TerrainDataBridge.Instance.StyleImagePath  = path;
            TerrainDataBridge.Instance.StyleImageBytes = File.ReadAllBytes(path);

            StartCoroutine(LoadTextureFromPath(path, styleImagePreview));

            generateButton.interactable = true;
            statusText.text = "Style image loaded. Press Generate.";
        });
    }

    // ── Generate button ──────────────────────────────────────────────────────
    public void OnGenerateClicked()
    {
        StartCoroutine(SendToAPI());
    }

    IEnumerator SendToAPI()
    {
        generateButton.interactable = false;
        statusText.text = "Sending to style-transfer API...";

        byte[] contentBytes = File.ReadAllBytes(TerrainDataBridge.Instance.NoisemapPath);
        byte[] styleBytes   = TerrainDataBridge.Instance.StyleImageBytes;

        string ext  = Path.GetExtension(TerrainDataBridge.Instance.StyleImagePath).ToLower();
        string mime = (ext == ".jpg" || ext == ".jpeg") ? "image/jpeg" : "image/png";

        var form = new WWWForm();
        form.AddBinaryData("content", contentBytes, "noisemap.png", "image/png");
        form.AddBinaryData("style",   styleBytes,   "style" + ext,  mime);

        using (UnityWebRequest uwr = UnityWebRequest.Post(apiUrl, form))
        {
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                statusText.text = "API Error: " + uwr.error;
                generateButton.interactable = true;
                yield break;
            }

            TerrainDataBridge.Instance.ResultHeightmapBytes = uwr.downloadHandler.data;
            statusText.text = "Success! Loading terrain viewer...";

            yield return new WaitForSeconds(0.6f);
            SceneManager.LoadScene("terrainviewer");
        }
    }

    // ── Shared helper ────────────────────────────────────────────────────────
    IEnumerator LoadTextureFromPath(string path, RawImage target)
    {
        string url = "file:///" + path;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
                target.texture = DownloadHandlerTexture.GetContent(uwr);
            else
                Debug.LogError("Texture load failed: " + uwr.error);
        }
    }
}
