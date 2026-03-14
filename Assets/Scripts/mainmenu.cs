using UnityEngine;
using UnityEngine.SceneManagement;
public class mainmenu : MonoBehaviour
{
    public HeightmapGenerator generator;

    public void RiverClicked()
    {
        Debug.Log("River button pressed.");
        generator.GenerateAndDownload();
        SceneManager.LoadScene("generationmenu");
    }

    public void MountainClicked()
    {
        Debug.Log("Mountain button pressed");
        generator.GenerateAndDownload();
    }

    public void PlainsClicked()
    {
        Debug.Log("Plains button pressed");
        generator.GenerateAndDownload();
    }
}