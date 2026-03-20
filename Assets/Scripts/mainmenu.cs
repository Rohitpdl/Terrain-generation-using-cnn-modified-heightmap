using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using AnotherFileBrowser.Windows;
using TMPro;

public class mainmenu : MonoBehaviour
{
    [Header("Required")]
    public HeightmapGenerator generator;

    [Header("Upload section UI")]
    public RawImage uploadedHeightmapPreview;  // RawImage in the upload panel
    public TMP_Text uploadStatusText;           // optional label under upload panel

    // Path of the user-uploaded image (empty = not uploaded yet)
    string _uploadedStylePath  = "";
    // string _uploadedStyleBytes_path = "";

    public void OnUploadHeightmapClicked()
    {
        var bp = new BrowserProperties();
        bp.filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
        bp.filterIndex = 0;

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            if (string.IsNullOrEmpty(path)) return;
            _uploadedStylePath = path;
            StartCoroutine(UploadAndProceed(path));
        });
    }

    IEnumerator UploadAndProceed(string stylePath)
    {
        // Show preview of the chosen image
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file:///" + stylePath))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success && uploadedHeightmapPreview != null)
                uploadedHeightmapPreview.texture = DownloadHandlerTexture.GetContent(uwr);
        }

        if (uploadStatusText != null)
            uploadStatusText.text = "Image loaded. Generating noise map...";

        EnsureBridge();

        // Auto-generate a noise heightmap as the content image
        string noisePath = generator.GenerateAndDownload();

        // Store everything in bridge
        TerrainDataBridge.Instance.NoisemapPath    = noisePath;
        TerrainDataBridge.Instance.IsUserUploaded  = false;       // content is generated noise
        TerrainDataBridge.Instance.TerrainType     = "Custom";    // no specific terrain type
        TerrainDataBridge.Instance.StyleImageBytes = File.ReadAllBytes(stylePath);
        TerrainDataBridge.Instance.StyleImagePath  = stylePath;

        Debug.Log("[mainmenu] Upload path: noise=" + noisePath + "  style=" + stylePath);

        // Go directly to generation menu — style is already set, just needs Generate
        SceneManager.LoadScene("generationmenu");
    }

    
    public void RiverClicked()    => Proceed("River");
    public void MountainClicked() => Proceed("Mountain");
    public void PlainsClicked()   => Proceed("Plains");

    void Proceed(string terrainType)
    {
        EnsureBridge();

        string noisePath = generator.GenerateAndDownload();

        TerrainDataBridge.Instance.NoisemapPath    = noisePath;
        TerrainDataBridge.Instance.IsUserUploaded  = false;
        TerrainDataBridge.Instance.TerrainType     = terrainType;
        // Clear any leftover style from a previous run
        TerrainDataBridge.Instance.StyleImageBytes = null;
        TerrainDataBridge.Instance.StyleImagePath  = "";

        Debug.Log("[mainmenu] Terrain=" + terrainType + "  noise=" + noisePath);
        SceneManager.LoadScene("generationmenu");
    }

    void EnsureBridge()
    {
        if (TerrainDataBridge.Instance == null)
        {
            var go = new GameObject("TerrainDataBridge");
            go.AddComponent<TerrainDataBridge>();
        }
    }
}
