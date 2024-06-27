using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FractalNoise : MonoBehaviour
{
    [SerializeField] private FractalNoiseScriptableObject noiseData;

    [Header("Render")] 
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap backgroundTilemap;
    [SerializeField] private BlockData blockData;


    private string _generatorVersion = "WorldRenderer1";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            _generatorVersion = "WorldRenderer1";
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            _generatorVersion = "WorldRenderer2";
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            _generatorVersion = "GenerateWorld3";
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            noiseData.seed = Random.Range(-9999, 9999);
        }
        
        Invoke(_generatorVersion, 0);
    }

    #region Terrain

    private int[] GenerateFractalNoise1()
    {
        var grid = new int[noiseData.width];

        for (int i = 0; i < noiseData.width; i++)
        {
            var elevation = 0f;
            var tempFrequency = noiseData.frequency;
            var tempAmplitude = noiseData.amplitude;

            for (int j = 0; j < noiseData.octaves; j++)
            {
                var x = i * tempFrequency;
                var y = noiseData.seed * tempFrequency;

                elevation += Mathf.PerlinNoise(x, y) * tempAmplitude;

                tempFrequency *= noiseData.lacunarity;
                tempAmplitude *= noiseData.persistence;
            }

            grid[i] = Mathf.FloorToInt(elevation * noiseData.heightModifier);
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
                groundTilemap.SetTile(new Vector3Int(x, y, 0),
                    y == noise[x] ? blockData.grassTile : blockData.dirtTile);
            }
        }
    }

    #endregion

    #region Terrain + Cave

    private int[,] GenerateFractalNoise2()
    {
        var grid = new int[noiseData.width, noiseData.heightModifier * 2];

        for (int i = 0; i < noiseData.width; i++)
        {
            var elevation = 0f;
            var tempFrequency = noiseData.frequency;
            var tempAmplitude = noiseData.amplitude;

            for (int j = 0; j < noiseData.octaves; j++)
            {
                var x = i * tempFrequency;
                var y = noiseData.seed * tempFrequency;

                elevation += Mathf.PerlinNoise(x, y) * tempAmplitude;

                tempFrequency *= noiseData.lacunarity;
                tempAmplitude *= noiseData.persistence;
            }

            Debug.Log(elevation);

            elevation = Mathf.FloorToInt(elevation * noiseData.heightModifier);

            for (int j = 0; j < elevation; j++)
            {
                float caveValueF = Mathf.PerlinNoise(Mathf.Abs(i + noiseData.seed) * noiseData.caveModifier,
                    Mathf.Abs(j + noiseData.seed) * noiseData.caveModifier);
                int caveValue = Mathf.RoundToInt(caveValueF);
                grid[i, j] = (caveValue == 1 ? 2 : 1);
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
                if (noise[x, y] == 2)
                {
                    backgroundTilemap.SetTile(new Vector3Int(x, y, 0), blockData.stoneTileBackground);
                }

                if (noise[x, y] == 1)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0),
                        noise[x, y + 1] == 0 ? blockData.grassTile : blockData.dirtTile);
                }
            }
        }
    }

    #endregion

    #region Terrain + Cave + Stone Layer

    private float TCL_GenerateFractalNoise(int pX, float pFrequency, float pAmplitude, int pOctaves, float pLacunarity,
        float pPersistence)
    {
        var elevation = 0f;
        var tempFrequency = pFrequency;
        var tempAmplitude = pAmplitude;

        for (int j = 0; j < pOctaves; j++)
        {
            var x = pX * tempFrequency;
            var y = noiseData.seed * tempFrequency;

            elevation += Mathf.PerlinNoise(x, y) * tempAmplitude;

            tempFrequency *= pLacunarity;
            tempAmplitude *= pPersistence;
        }

        return elevation;
    }

    private void GenerateWorld3()
    {
        var cave = new int[noiseData.width, noiseData.heightModifier / 2];
        int[] dirtLayer = new int[noiseData.width];
        int[] stoneLayer = new int[noiseData.width];

        for (int i = 0; i < noiseData.width; i++)
        {
            var layer1 = TCL_GenerateFractalNoise(i, noiseData.frequency, noiseData.amplitude, noiseData.octaves,
                noiseData.lacunarity, noiseData.persistence);
            var layer2 = TCL_GenerateFractalNoise(i, noiseData.stoneFrequency, noiseData.stoneAmplitude,
                noiseData.stoneOctaves, noiseData.stoneLacunarity, noiseData.stonePersistence);
            var caveLayer =
                Mathf.FloorToInt(TransformRange(layer2 * noiseData.heightModifier / 3 * 2, noiseData.heightModifier, 0,
                    noiseData.heightModifier / 2, 0));

            for (int j = 0; j < caveLayer; j++)
            {
                float caveValueF = Mathf.PerlinNoise(Mathf.Abs(i + noiseData.seed) * noiseData.caveModifier,
                    Mathf.Abs(j + noiseData.seed) * noiseData.caveModifier);
                int caveValue = Mathf.RoundToInt(caveValueF);
                Debug.Log("CaveValue: " + caveValue);
                cave[i, j] = (caveValue == 1 ? 2 : 1);
            }

            dirtLayer[i] = Mathf.FloorToInt(TransformRange(layer1 * noiseData.heightModifier, noiseData.heightModifier,
                0, noiseData.heightModifier / 4 * 3, noiseData.heightModifier / 2));
            stoneLayer[i] = caveLayer;
        }

        WorldRenderer3(dirtLayer, stoneLayer, cave);
    }

    private float TransformRange(float noiseValue, int oldMax, int oldMin, int newMax, int newMin)
    {
        return ((noiseValue - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
    }

    private void WorldRenderer3(int[] dirtLayer, int[] stoneLayer, int[,] cave)
    {
        groundTilemap.ClearAllTiles();
        backgroundTilemap.ClearAllTiles();

        for (int x = 0; x < cave.GetLength(0); x++)
        {
            for (int y = 0; y < cave.GetLength(1); y++)
            {
                if (cave[x, y] == 2)
                {
                    backgroundTilemap.SetTile(new Vector3Int(x, y, 0), blockData.stoneTileBackground);
                }

                if (cave[x, y] == 1)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), blockData.stoneTile);
                }
            }

            for (int i = stoneLayer[x]; i <= dirtLayer[x]; i++)
            {
                groundTilemap.SetTile(new Vector3Int(x, i, 0),
                    i == dirtLayer[x] ? blockData.grassTile : blockData.dirtTile);
            }
        }
    }

    #endregion

    private void Print2DArray(int[,] grid)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                sb.Append(grid[i, j]);
                sb.Append(' ');
            }

            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }
}