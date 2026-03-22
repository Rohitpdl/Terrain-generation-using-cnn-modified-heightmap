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
    // ── Already-wired slots (DO NOT rename) ───────────────────────────────────
    [Header("UI References")]
    public RawImage noisemapPreview;
    public RawImage styleImagePreview;
    public Button   generateButton;
    public TMP_Text statusText;

    [Header("Extra UI")]
    public Button   backButton;
    public Button   changeStyleButton;
    public Button   resetStyleButton;
    public TMP_Text terrainTypeLabel;
    public TMP_Text styleSourceLabel;

    [Header("Loading UI")]
    public GameObject loadingPanel;  // assign a panel/spinner GameObject — shown during API call

    [Header("API Settings")]
    public string apiUrl = "http://localhost:5000/style_transfer";

    bool      _styleReady        = false;
    Coroutine _dotAnimCoroutine  = null;

    // ── Start ─────────────────────────────────────────────────────────────────
    void Start()
    {
        generateButton.interactable = false;

        if (loadingPanel != null) loadingPanel.SetActive(false);

        if (TerrainDataBridge.Instance == null ||
            string.IsNullOrEmpty(TerrainDataBridge.Instance.NoisemapPath))
        {
            statusText.text = "No heightmap found. Go back and try again.";
            return;
        }

        string tType = TerrainDataBridge.Instance.TerrainType;
        if (terrainTypeLabel != null)
            terrainTypeLabel.text = string.IsNullOrEmpty(tType) ? "Custom" : tType + " Terrain";

        // Show content image
        StartCoroutine(LoadTexture(TerrainDataBridge.Instance.NoisemapPath, noisemapPreview));

        // Case A: came via Upload button — style explicitly set by user
        if (TerrainDataBridge.Instance.StyleImageBytes != null &&
            TerrainDataBridge.Instance.StyleImageBytes.Length > 0 &&
            TerrainDataBridge.Instance.IsUserUploaded == true)
        {
            StartCoroutine(LoadTexture(TerrainDataBridge.Instance.StyleImagePath, styleImagePreview));
            _styleReady = true;
            generateButton.interactable = true;
            if (styleSourceLabel != null) styleSourceLabel.text = "Uploaded style";
            statusText.text = "Style image ready.\nPress Generate, or change the style first.";
        }
        // Case B: terrain type chosen — load correct default from StreamingAssets
        else
        {
            // Clear any stale bytes from a previous run
            TerrainDataBridge.Instance.StyleImageBytes = null;
            TerrainDataBridge.Instance.StyleImagePath  = "";
            StartCoroutine(TryLoadDefaultStyleFromDisk(tType));
        }
    }

    // ── Dot animation coroutine ───────────────────────────────────────────────
    IEnumerator AnimateDots()
    {
        string[] frames = {
            "Generating .",
            "Generating . .",
            "Generating . . ."
        };
        int i = 0;
        while (true)
        {
            statusText.text = frames[i % frames.Length];
            i++;
            yield return new WaitForSeconds(0.5f);
        }
    }

    void StopLoading()
    {
        if (_dotAnimCoroutine != null)
        {
            StopCoroutine(_dotAnimCoroutine);
            _dotAnimCoroutine = null;
        }
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    // ── Default style loader ──────────────────────────────────────────────────
    IEnumerator TryLoadDefaultStyleFromDisk(string terrainType)
    {
        string fileName = terrainType + ".png";
        string filePath = Path.Combine(Application.streamingAssetsPath,
                                       "DefaultStyles", fileName);
        string url = new System.Uri(filePath).AbsoluteUri;

        statusText.text = terrainType + " Terrain\nLoading default style...";

        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success ||
                uwr.downloadHandler.data.Length == 0)
            {
                _styleReady = false;
                generateButton.interactable = false;
                if (styleSourceLabel != null) styleSourceLabel.text = "No style selected";
                statusText.text = terrainType + " Terrain\n" +
                                  "No default style found. Place " + fileName +
                                  " in Assets/StreamingAssets/DefaultStyles/\n" +
                                  "or browse a style image below.";
                Debug.LogWarning("[GMM] Default style not found at: " + filePath);
                yield break;
            }

            byte[] bytes = uwr.downloadHandler.data;
            TerrainDataBridge.Instance.StyleImageBytes = bytes;
            TerrainDataBridge.Instance.StyleImagePath  = filePath;

            _styleReady = true;
            generateButton.interactable = true;
            if (styleSourceLabel != null) styleSourceLabel.text = "Default style (auto)";
            statusText.text = terrainType + " Terrain\n" +
                              "Default style loaded. Press Generate, or change the style.";
        }

        // Load texture for preview only
        string texUrl = new System.Uri(filePath).AbsoluteUri;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(texUrl))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
                styleImagePreview.texture = DownloadHandlerTexture.GetContent(uwr);
        }
    }

    // ── Browse / Change Style button ──────────────────────────────────────────
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

            StartCoroutine(LoadTexture(path, styleImagePreview));

            _styleReady = true;
            generateButton.interactable = true;
            if (styleSourceLabel != null) styleSourceLabel.text = "Custom style";
            statusText.text = "Custom style loaded. Press Generate when ready.";
        });
    }

    // ── Reset to Default button ───────────────────────────────────────────────
    public void OnResetToDefaultStyle()
    {
        if (TerrainDataBridge.Instance == null) return;
        TerrainDataBridge.Instance.StyleImageBytes = null;
        TerrainDataBridge.Instance.StyleImagePath  = "";
        StartCoroutine(TryLoadDefaultStyleFromDisk(TerrainDataBridge.Instance.TerrainType));
    }

    // ── Back button ───────────────────────────────────────────────────────────
    public void OnBackClicked()
    {
        StopLoading();
        if (TerrainDataBridge.Instance != null)
        {
            TerrainDataBridge.Instance.StyleImageBytes = null;
            TerrainDataBridge.Instance.StyleImagePath  = "";
        }
        SceneManager.LoadScene("mainmenu");
    }

    // ── Generate button ───────────────────────────────────────────────────────
    public void OnGenerateClicked()
    {
        if (!_styleReady)
        {
            statusText.text = "Please select a style image first.";
            return;
        }
        StartCoroutine(SendToAPI());
    }

    IEnumerator SendToAPI()
    {
        SetButtonsInteractable(false);

        // Start animated dots + show loading panel
        _dotAnimCoroutine = StartCoroutine(AnimateDots());
        if (loadingPanel != null) loadingPanel.SetActive(true);

        byte[] contentBytes = File.ReadAllBytes(TerrainDataBridge.Instance.NoisemapPath);
        byte[] styleBytes   = TerrainDataBridge.Instance.StyleImageBytes;

        string ext  = Path.GetExtension(TerrainDataBridge.Instance.StyleImagePath).ToLower();
        if (string.IsNullOrEmpty(ext)) ext = ".png";
        string mime = (ext == ".jpg" || ext == ".jpeg") ? "image/jpeg" : "image/png";

        var form = new WWWForm();
        form.AddField("steps", "200");   // change to 1000 for final quality
        form.AddBinaryData("content", contentBytes, "noisemap.png", "image/png");
        form.AddBinaryData("style",   styleBytes,   "style" + ext,  mime);

        using (UnityWebRequest uwr = UnityWebRequest.Post(apiUrl, form))
        {
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();

            // ── Error ─────────────────────────────────────────────────────────
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                StopLoading();
                statusText.text = "API Error: " + uwr.error +
                                  "\nMake sure your Python server is running.";
                SetButtonsInteractable(true);
                yield break;
            }

            // ── Success ───────────────────────────────────────────────────────
            StopLoading();
            TerrainDataBridge.Instance.ResultHeightmapBytes = uwr.downloadHandler.data;
            statusText.text = "Done! Loading terrain viewer...";
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene("terrainviewer");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    void SetButtonsInteractable(bool state)
    {
        generateButton.interactable = state;
        if (backButton        != null) backButton.interactable        = state;
        if (changeStyleButton != null) changeStyleButton.interactable = state;
        if (resetStyleButton  != null) resetStyleButton.interactable  = state;
    }

    IEnumerator LoadTexture(string path, RawImage target)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file:///" + path))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
                target.texture = DownloadHandlerTexture.GetContent(uwr);
            else
                Debug.LogError("Texture load failed: " + uwr.error);
        }
    }
}