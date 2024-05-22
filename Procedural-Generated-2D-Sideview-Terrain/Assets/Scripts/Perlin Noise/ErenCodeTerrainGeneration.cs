using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ErenCodeTerrainGeneration : MonoBehaviour
{
    [SerializeField] private Sprite tile;
    [SerializeField] private int worldSize = 100;
    [SerializeField] private float terrainValue = 0.5f;
    [FormerlySerializedAs("noiseFrequency")] [SerializeField] private float caveNoiseFrequency = 0.05f;
    [SerializeField] private float terrainNoiseFrequency = 0.05f;
    [SerializeField] private int heightAddition = 25;
    [SerializeField] private float heightMultiplier = 4f;
    [SerializeField] private float seed;
    [SerializeField] private Texture2D noiseTexture;

    private void Start()
    {
        seed = Random.Range(-10000, 10000);
        GenerateNoiseTexture();
        GenerateTerrain();
    }

    private void GenerateNoiseTexture()
    {
        noiseTexture = new Texture2D(worldSize, worldSize);

        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * caveNoiseFrequency, (y + seed) * caveNoiseFrequency);
                noiseTexture.SetPixel(x, y, new Color(v, v, v));
            }
        }
        
        noiseTexture.Apply();
    }

    private void GenerateTerrain()
    {
        for (int x = 0; x < worldSize; x++)
        {
            float height = Mathf.PerlinNoise((x + seed) * terrainNoiseFrequency, seed * terrainNoiseFrequency) * heightMultiplier + heightAddition;
            
            for (int y = 0; y < height; y++)
            {
                if (noiseTexture.GetPixel(x, y).r < terrainValue)
                {
                    GameObject newTile = new GameObject(name = "tile");
                    newTile.transform.parent = transform;
                    newTile.AddComponent<SpriteRenderer>();
                    newTile.GetComponent<SpriteRenderer>().sprite = tile;
                    newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);
                }
                
                // GameObject newTile = new GameObject(name = "tile");
                // newTile.transform.parent = transform;
                // newTile.AddComponent<SpriteRenderer>();
                // newTile.GetComponent<SpriteRenderer>().sprite = tile;
                // newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);
            }
        }
    }
}
