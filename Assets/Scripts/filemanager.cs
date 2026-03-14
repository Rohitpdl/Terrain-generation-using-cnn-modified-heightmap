using UnityEngine;
using System.Collections;
using AnotherFileBrowser.Windows;
using UnityEngine.UI;
using UnityEngine.Networking;

public class filemanager : MonoBehaviour
{
    public RawImage rawImage;

    public void OpenFileExplorer()
    {
        var bp = new BrowserProperties();
        bp.filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
        bp.filterIndex = 0;

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            Debug.Log("Selected path: " + path);
            StartCoroutine(LoadImage(path));
        });
    }

    IEnumerator LoadImage(string path)
    {
        string fileUrl = "file:///" + path;

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(fileUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Image load failed: " + uwr.error);
            }
            else
            {
                Texture2D loadedTexture = DownloadHandlerTexture.GetContent(uwr);
                rawImage.texture = loadedTexture;
                Debug.Log("Image loaded successfully");
            }
        }
    }
}