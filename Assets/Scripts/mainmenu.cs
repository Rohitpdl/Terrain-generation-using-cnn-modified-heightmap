using UnityEngine;
using UnityEngine.SceneManagement;

public class mainmenu : MonoBehaviour
{
    public HeightmapGenerator generator;

    void GenerateAndProceed(string terrainType)
    {
        string savedPath = generator.GenerateAndDownload();

        // Create bridge if it doesn't exist yet
        if (TerrainDataBridge.Instance == null)
        {
            var go = new GameObject("TerrainDataBridge");
            go.AddComponent<TerrainDataBridge>();
        }

        TerrainDataBridge.Instance.NoisemapPath = savedPath;
        TerrainDataBridge.Instance.TerrainType  = terrainType;

        SceneManager.LoadScene("generationmenu");
    }

    public void RiverClicked()    => GenerateAndProceed("River");
    public void MountainClicked() => GenerateAndProceed("Mountain");
    public void PlainsClicked()   => GenerateAndProceed("Plains");
}
