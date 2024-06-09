using UnityEngine;
using UnityEngine.Tilemaps;

public class PerlinNoise : MonoBehaviour
{
    [Header("Noise")] 
    [SerializeField] [Range(0,100)] private int amplitude;
    [SerializeField] [Range(0,1)] private float frequency =  0.05f;
    
    [Header("World")] 
    [SerializeField] private int width;
    [SerializeField] private int seed = 0;

    [Header("Render")] 
    [SerializeField] private Tilemap groundTilemap;

    [SerializeField] private BlockData blockData;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GeneratePerlinNoise();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            seed = Random.Range(-9999, 9999);
        }
    }


    private void GeneratePerlinNoise()
    {
        groundTilemap.ClearAllTiles();
        
        for (int x = 0; x < width; x++)
        {
            var noiseValue = amplitude * Mathf.PerlinNoise(x * frequency, seed * frequency);
            var mapHeight = Mathf.FloorToInt(noiseValue);
            for (int y = 0; y <= mapHeight; y++)
            {
                if (y == mapHeight)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), blockData.grassTile);
                }
                else
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), blockData.dirtTile);
                }
            }
        }
    }
}
