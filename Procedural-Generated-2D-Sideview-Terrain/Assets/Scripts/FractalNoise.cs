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
    
    
    [Header("Stone Noise")]
    [SerializeField] [Range(0, 1)]
    private float stoneAmplitude = 0.4f;
    [SerializeField] [Range(0, 0.1f)] 
    private float stoneFrequency = 0.02f;
    [SerializeField] 
    private int stoneOctaves = 3;
    [SerializeField] 
    private float stoneLacunarity = 2.0f;
    [SerializeField] 
    private float stonePersistence = 0.5f;
    
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


    private string _generatorVersion = "GenerateWorld3";
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _generatorVersion = "WorldRenderer1";
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _generatorVersion = "WorldRenderer2";
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _generatorVersion = "GenerateWorld3";
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            // WorldRenderer2();
            // GenerateWorld3();
            Invoke(_generatorVersion, 0);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            seed = Random.Range(-9999, 9999);
        }
    }

    #region Terrain
     private int[] GenerateFractalNoise1()
    {
        var grid = new int[width];
        
        for (int i = 0; i < width; i++)
        {
            var elevation = 0f;
            var tempFrequency = frequency;
            var tempAmplitude = amplitude;

            for (int j = 0; j < octaves; j++)
            {
                var x = i * tempFrequency;
                var y = seed * tempFrequency;

                elevation += Mathf.PerlinNoise(x, y) * tempAmplitude;

                tempFrequency *= lacunarity;
                tempAmplitude *= persistence;
            }
            
            grid[i] = Mathf.FloorToInt(elevation * heightModifier);
        }
        
        return grid;
    }

    private void WorldRenderer1()
    {
        groundTilemap.ClearAllTiles();
        backgroundTilemap.ClearAllTiles();
        
        var noise = GenerateFractalNoise1();

        for (int x = 0; x < noise.Length; x++)
        {
            for (int y = 0; y <= noise[x]; y++)
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), y == noise[x] ? blockData.grassTile : blockData.dirtTile);
            }
        }
    }
    #endregion

    #region Terrain + Cave
    private int[,] GenerateFractalNoise2()
    {
        var grid = new int[width, heightModifier * 2];
        
        for (int i = 0; i < width; i++)
        {
            var elevation = 0f;
            var tempFrequency = frequency;
            var tempAmplitude = amplitude;

            for (int j = 0; j < octaves; j++)
            {
                var x = i * tempFrequency;
                var y = seed * tempFrequency;

                elevation += Mathf.PerlinNoise(x, y) * tempAmplitude;

                tempFrequency *= lacunarity;
                tempAmplitude *= persistence;
            }
            Debug.Log(elevation);

            elevation = Mathf.FloorToInt(elevation * heightModifier);
            
            for (int j = 0; j < elevation; j++)
            {
                float caveValueF = Mathf.PerlinNoise(Mathf.Abs(i + seed) * caveModifier, Mathf.Abs(j + seed) * caveModifier);
                int caveValue = Mathf.RoundToInt(caveValueF);
                grid[i,j] = (caveValue == 1 ? 2 : 1);
            }
        }
        
        return grid;
    }

    private void WorldRenderer2()
    {
        groundTilemap.ClearAllTiles();
        backgroundTilemap.ClearAllTiles();
        
        var noise = GenerateFractalNoise2();

        for (int x = 0; x < noise.GetLength(0); x++)
        {
            for (int y = 0; y < noise.GetLength(1); y++)
            {
                if (noise[x,y] == 2)
                {
                    backgroundTilemap.SetTile(new Vector3Int(x, y, 0), blockData.stoneTileBackground);
                }

                if (noise[x,y] == 1)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), noise[x, y+1] == 0 ? blockData.grassTile : blockData.dirtTile);
                }
            }
        }
    }
    #endregion

    #region Terrain + Cave + Stone Layer
    private float TCL_GenerateFractalNoise(int pX, float pFrequency, float pAmplitude, int pOctaves, float pLacunarity, float pPersistence)
    {
        var elevation = 0f;
        var tempFrequency = pFrequency;
        var tempAmplitude = pAmplitude;

        for (int j = 0; j < pOctaves; j++)
        {
            var x = pX * tempFrequency;
            var y = seed * tempFrequency;

            elevation += Mathf.PerlinNoise(x, y) * tempAmplitude;

            tempFrequency *= pLacunarity;
            tempAmplitude *= pPersistence;
        }
        
        return elevation;
    }

    private void GenerateWorld3()
    {
        var cave = new int[width, heightModifier / 2];
        int[] dirtLayer = new int[width];
        int[] stoneLayer = new int[width];
        
        for (int i = 0; i < width; i++)
        {
            var layer1 = TCL_GenerateFractalNoise(i, frequency, amplitude, octaves, lacunarity, persistence);
            var layer2 = TCL_GenerateFractalNoise(i, stoneFrequency, stoneAmplitude, stoneOctaves, stoneLacunarity, stonePersistence);
            var caveLayer =
                Mathf.FloorToInt(TransformRange(layer2 * heightModifier / 3 * 2, heightModifier, 0, heightModifier / 2, 0));
            
            for (int j = 0; j < caveLayer; j++)
            {
                float caveValueF = Mathf.PerlinNoise(Mathf.Abs(i + seed) * caveModifier, Mathf.Abs(j + seed) * caveModifier);
                int caveValue = Mathf.RoundToInt(caveValueF);
                Debug.Log("CaveValue: " + caveValue);
                cave[i,j] = (caveValue == 1 ? 2 : 1);
            }
            
            dirtLayer[i] = Mathf.FloorToInt(TransformRange(layer1 * heightModifier, heightModifier, 0, heightModifier/4*3, heightModifier/2));
            stoneLayer[i] = caveLayer;
        }
        
        WorldRenderer3(dirtLayer, stoneLayer, cave);
    }

    private float TransformRange(float noiseValue, int oldMax, int oldMin, int newMax, int newMin)
    {
        return ((noiseValue - oldMin)/(oldMax - oldMin)) * (newMax - newMin) + newMin;
    }

    private void WorldRenderer3(int[] dirtLayer, int[] stoneLayer, int[,] cave)
    {
        groundTilemap.ClearAllTiles();
        backgroundTilemap.ClearAllTiles();

        for (int x = 0; x < cave.GetLength(0); x++)
        {
            for (int y = 0; y < cave.GetLength(1); y++)
            {
                if (cave[x,y] == 2)
                {
                    backgroundTilemap.SetTile(new Vector3Int(x, y, 0), blockData.stoneTileBackground);
                }
        
                if (cave[x,y] == 1)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), blockData.stoneTile);
                }
            }

            for (int i = stoneLayer[x]; i <= dirtLayer[x]; i++)
            {
                groundTilemap.SetTile(new Vector3Int(x, i, 0), i == dirtLayer[x] ? blockData.grassTile : blockData.dirtTile);
            }
        }
    }
    #endregion
    
    private void Print2DArray(int[,] grid)
    {
        StringBuilder sb = new StringBuilder();
        for(int i = 0; i < grid.GetLength(0); i++)
        {
            for(int j = 0; j < grid.GetLength(1); j++)
            {
                sb.Append(grid[i,j]);
                sb.Append(' ');				   
            }
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }
}