using AnotherFileBrowser.Windows;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FileBrowserUpdate : MonoBehaviour
{
    public RawImage rawImage;

    public void OpenFileBrowser()
    {
        var bp = new BrowserProperties();
        bp.filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
        bp.filterIndex = 0;

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            StartCoroutine(LoadImage(path));
        });
    }

    IEnumerator LoadImage(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file:///" + path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                rawImage.texture = DownloadHandlerTexture.GetContent(uwr);
            }
        }
    }
}
