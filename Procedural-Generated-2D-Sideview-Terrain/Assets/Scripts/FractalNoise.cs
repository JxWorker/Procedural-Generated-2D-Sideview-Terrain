using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FractalNoise : MonoBehaviour
{
    [Header("Noise")]
    [SerializeField] [Range(0, 1)]
    private float amplitude = 0.4f;
    [SerializeField] [Range(0, 0.1f)] 
    private float frequency = 0.02f;
    [SerializeField] 
    private int octaves = 3;
    [SerializeField] 
    private float lacunarity = 2.0f;
    [SerializeField] 
    private float persistence = 0.5f;
    
    [Header("World")]
    [SerializeField]
    private int width = 100;
    [SerializeField] 
    private int seed = 1;
    [SerializeField] [Range(1, 100)]
    private int heightModifier = 100;
    [SerializeField] [Range(0, 1)] 
    private float caveModifier;

    [Header("Render")] 
    [SerializeField] 
    private Tilemap groundTilemap;
    [SerializeField] 
    private Tilemap backgroundTilemap;
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

    private int[,] GenerateFractalNoise()
    {
        var grid = new int[width];
        var grid2 = new int[width, heightModifier * 2];
        
        for (int i = 0; i < width; i++)
        {
            var elevation = 0f;
            var tempFrequenzy = frequency;
            var tempAmplitude = amplitude;

            for (int j = 0; j < octaves; j++)
            {
                var x = i * tempFrequenzy;
                var y = seed * tempFrequenzy;

                elevation += Mathf.PerlinNoise(x, y) * tempAmplitude;

                tempFrequenzy *= lacunarity;
                tempAmplitude *= persistence;
            }

            elevation = Mathf.FloorToInt(elevation * heightModifier);
            
            for (int j = 0; j < elevation; j++)
            {
                float caveValueF = Mathf.PerlinNoise(Mathf.Abs(i + seed) * caveModifier, Mathf.Abs(j + seed) * caveModifier);
                // Debug.Log("Float: " + caveValueF);
                int caveValue = Mathf.RoundToInt(caveValueF);
                // Debug.Log("Int: " + caveValue);
                // Debug.Log("If: " + (caveValue == 1 ? 2 : 1));
                grid2[i,j] = (caveValue == 1 ? 2 : 1);
            }
            
            // Debug.Log(elevation);
            // grid[i] = Mathf.FloorToInt(elevation * heightModifier);
        }

        // Print2DArray(grid2);
        
        return grid2;
    }

    private void WorldRenderer()
    {
        groundTilemap.ClearAllTiles();
        backgroundTilemap.ClearAllTiles();
        
        var noise = GenerateFractalNoise();

        for (int x = 0; x < noise.GetLength(0); x++)
        {
            // for (int y = 0; y <= noise[x]; y++)
            // {
            //     groundTilemap.SetTile(new Vector3Int(x, y, 0), y == noise[x] ? blockData.grassTile : blockData.dirtTile);
            // }
            
            for (int y = 0; y < noise.GetLength(1); y++)
            {
                if (noise[x,y] == 2)
                {
                    backgroundTilemap.SetTile(new Vector3Int(x, y, 0), blockData.stoneTileBackground);
                }

                if (noise[x,y] == 1)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), y == noise.GetLength(1) ? blockData.grassTile : blockData.dirtTile);
                }
            }
        }
    }
    
    private void Print2DArray(int[,] grid)
    {
        StringBuilder sb = new StringBuilder();
        for(int i=0; i < grid.GetLength(0); i++)
        {
            for(int j=0; j< grid.GetLength(1); j++)
            {
                sb.Append(grid[i,j]);
                sb.Append(' ');				   
            }
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }
}