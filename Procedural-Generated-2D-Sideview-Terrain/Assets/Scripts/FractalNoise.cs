using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FractalNoise : MonoBehaviour
{
    [Header("Noise")]
    [SerializeField] [Range(0, 100)]
    private int amplitude;
    [SerializeField] [Range(0, 1)] 
    private float frequency = 0.05f;
    [SerializeField] 
    private int octaves = 3;
    [SerializeField] 
    private float lacunarity = 2.0f;
    [SerializeField] 
    private float persistence = 0.5f;
    
    [Header("World")]
    [SerializeField]
    private int width;
    [SerializeField] 
    private int seed = 0;

    [Header("Render")] 
    [SerializeField] 
    private Tilemap groundTilemap;
    [SerializeField] 
    private BlockData blockData;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            WorldRenderer();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            seed = Random.Range(-9999, 9999);
        }
    }

    private int[] GenerateFractalNoise()
    {
        var grid = new int[width];
        
        for (int i = 0; i < width; i++)
        {
            var elevation = amplitude;
            var tempFrequenzy = frequency;
            var tempAmplitude = amplitude;

            for (int j = 0; j < octaves; j++)
            {
                var x = i * tempFrequenzy;
                var y = seed * tempFrequenzy;

                elevation += Mathf.FloorToInt(Mathf.PerlinNoise(x, y) * tempAmplitude);

                tempFrequenzy *= lacunarity;
                tempAmplitude = Mathf.FloorToInt(tempAmplitude * persistence);

            }

            // Debug.Log(elevation);
            grid[i] = elevation;
        }

        return grid;
    }

    private void WorldRenderer()
    {
        groundTilemap.ClearAllTiles();
        
        var noise = GenerateFractalNoise();

        for (int x = 0; x < noise.Length; x++)
        {
            for (int y = 0; y <= noise[x]; y++)
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), y == noise[x] ? blockData.grassTile : blockData.dirtTile);
            }
        }
    }
}