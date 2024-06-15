using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class CellularAutomata : MonoBehaviour
{
    [Header("Cellular Automata")]
    [SerializeField] 
    private int caveIterations;
    [SerializeField]
    private int caveIterationCount;
    [FormerlySerializedAs("neighbourBreak")] [SerializeField] [Range(1, 8)] 
    private int neighbourBreakCave;
    [SerializeField]
    private int terrainIterations;
    [SerializeField]
    private int terrainIterationCount;
    [SerializeField] [Range(1, 8)] 
    private int neighbourBreakTerrain;

    private readonly int[] _xDirection = { -1, -1, -1, 0, 0, 1, 1, 1 };
    private readonly int[] _yDirection = { -1, 0, 1, -1, 1, -1, 0, 1 };
    
    private bool[,] _caveNoiseGrid;
    private bool[,] _caveTempGrid;
    
    private bool[,] _terrainNoiseGrid;
    private bool[,] _terrainTempGrid;
    
    [Header("Noise Grid Generator")]
    [SerializeField] [Range(1, 100)]
    private int caveDensity;
    [SerializeField] 
    private int caveHeight;
    [SerializeField] 
    private int caveWidth;
    [SerializeField] [Range(1, 100)] 
    private int terrainDensity;
    [SerializeField] 
    private int terrainHeight;
    [SerializeField] 
    private int terrainWidth;
    
    [Header("Renderer")] 
    [SerializeField] 
    private Tilemap groundTilemap;
    [SerializeField] 
    private Tilemap backgroundTilemap;
    [SerializeField] 
    private BlockData blockData;
    [SerializeField] 
    private int yOffset = 10;
    
    private List<bool[,]> _caveGridIterations = new List<bool[,]>();
    private List<bool[,]> _terrainGridIterations = new List<bool[,]>();
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            ResetCellularAutomata();
            GenerateNoiseGridForCave();
            GenerateNoiseGridForTerrain();
            RenderTileWorld();
        }
        
        #region Cave
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (caveIterations - caveIterationCount == 1)
            {
                caveIterationCount++;
                caveIterations++;
                GenerateCave();
            }
            else if (caveIterations == 0 && caveIterationCount == 0)
            {
                caveIterations++;
                GenerateCave();
            }
            else
            {
                caveIterationCount++;
                _caveNoiseGrid = _caveGridIterations[caveIterationCount];
                RenderTileWorld();
            }
            
        }

        if (Input.GetKeyDown(KeyCode.A) && caveIterationCount > 0)
        {
            caveIterationCount--;
            _caveNoiseGrid = CopyArray(_caveGridIterations[caveIterationCount]);
            RenderTileWorld();
        }
        #endregion
        
        #region Terrain
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (terrainIterations - terrainIterationCount == 1)
            {
                terrainIterationCount++;
                terrainIterations++;
                GenerateTerrain();
            }
            else if (terrainIterations == 0 && terrainIterationCount == 0)
            {
                terrainIterations++;
                GenerateTerrain();
            }
            else
            {
                terrainIterationCount++;
                _terrainNoiseGrid = _terrainGridIterations[terrainIterationCount];
                RenderTileWorld();
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && terrainIterationCount > 0)
        {
            terrainIterationCount--;
            _terrainNoiseGrid = CopyArray(_caveGridIterations[terrainIterationCount]);
            RenderTileWorld();
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.U))
        {
            yOffset++;
            RenderTileWorld();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            yOffset--;
            RenderTileWorld();
        }
    }

    private void GenerateCave()
    {
        for (int i = caveIterationCount; i < caveIterations; i++)
        {
            _caveGridIterations.Add(CopyArray(_caveNoiseGrid));
            _caveTempGrid = CopyArray(_caveNoiseGrid);
            
            for (int iPixel = 0; iPixel < caveWidth; iPixel++)
            {
                for (int jPixel = 0; jPixel < caveHeight; jPixel++)
                {
                    int neighbourCount = 0;

                    for (int dir = 0; dir < _xDirection.Length; dir++)
                    {
                        int x = iPixel + _xDirection[dir];
                        int y = jPixel + _yDirection[dir];
                        
                        if (x >= 0 && x < caveWidth && y >= 0 && y < caveHeight)
                        {
                            if (_caveTempGrid[x, y]) neighbourCount++;
                        }
                        else
                        {
                            neighbourCount++;
                        }
                    }

                    _caveNoiseGrid[iPixel, jPixel] = neighbourCount > neighbourBreakCave;
                }
            }
        }
        
        RenderTileWorld();
    }

    private void GenerateTerrain()
    {
        for (int i = terrainIterationCount; i < terrainIterations; i++)
        {
            _terrainGridIterations.Add(CopyArray(_terrainNoiseGrid));
            _terrainTempGrid = CopyArray(_terrainNoiseGrid);
            
            for (int iPixel = 0; iPixel < terrainWidth; iPixel++)
            {
                for (int jPixel = 0; jPixel < terrainHeight; jPixel++)
                {
                    int neighbourCount = 0;

                    for (int dir = 0; dir < _xDirection.Length; dir++)
                    {
                        int x = iPixel + _xDirection[dir];
                        int y = jPixel + _yDirection[dir];
                        
                        if (x >= 0 && x < terrainWidth && y >= 0 && y < terrainHeight)
                        {
                            if (_terrainTempGrid[x, y]) neighbourCount++;
                        }
                        else if (jPixel < (terrainHeight - 20))
                        {
                            neighbourCount++;
                        }
                    }

                    _terrainNoiseGrid[iPixel, jPixel] = neighbourCount > neighbourBreakTerrain;
                }
            }
        }
        
        RenderTileWorld();
    }
    
    private void GenerateNoiseGridForCave()
    {
        _caveNoiseGrid = new bool[caveWidth, caveHeight];
        _caveTempGrid = new bool[caveWidth, caveHeight];
        
        for (int i = 0; i < caveWidth; i++)
        {
            for (int j = 0; j < caveHeight; j++)
            {
                _caveNoiseGrid[i, j] = Random.Range(1, 101) <= caveDensity;
            }
        }
    }
    
    private void GenerateNoiseGridForTerrain()
    {
        _terrainNoiseGrid = new bool[terrainWidth, terrainHeight];
        _terrainTempGrid = new bool[terrainWidth, terrainHeight];
        
        for (int i = 0; i < terrainWidth; i++)
        {
            for (int j = 0; j < terrainHeight; j++)
            {
                if (j > terrainHeight/3*2)
                {
                    _terrainNoiseGrid[i, j] = Random.Range(1, 101) > terrainDensity;
                }
                else
                {
                    _terrainNoiseGrid[i, j] = Random.Range(1, 101) <= terrainDensity;
                }
            }
        }
    }

    private void RenderTileWorld()
    {
        groundTilemap.ClearAllTiles();
        backgroundTilemap.ClearAllTiles();
        
        for (int i = 0; i < caveWidth; i++)
        {
            for (int j = 0; j < caveHeight; j++)
            {
                if (_caveNoiseGrid[i,j])
                {
                    groundTilemap.SetTile(new Vector3Int(i, j, 0), blockData.stoneTile);
                }
                else
                {
                    backgroundTilemap.SetTile(new Vector3Int(i,j, 0), blockData.stoneTileBackground);
                }
            }
        }

        for (int i = 0; i < terrainWidth; i++)
        {
            for (int j = 0; j < terrainHeight; j++)
            {
                if (_terrainNoiseGrid[i,j])
                {
                    groundTilemap.SetTile(new Vector3Int(i, caveHeight + j + yOffset, 0), blockData.dirtTile);
                }
                else
                {
                    backgroundTilemap.SetTile(new Vector3Int(i, caveHeight + j + yOffset, 0), blockData.dirtTileBackground);
                }
            }
        }
    }

    private void ResetCellularAutomata()
    {
        caveIterations = 0;
        caveIterationCount = 0;
        _caveGridIterations = new List<bool[,]>();
        
        terrainIterations = 0;
        terrainIterationCount = 0;
        _terrainGridIterations = new List<bool[,]>();
        
        groundTilemap.ClearAllTiles();
        backgroundTilemap.ClearAllTiles();
    }

    private bool[,] CopyArray(bool[,] arrayToCopy)
    {
        var temp = new bool[arrayToCopy.GetLength(0), arrayToCopy.GetLength(1)];
        
        for (int i = 0; i < arrayToCopy.GetLength(0); i++)
        {
            for (int j = 0; j < arrayToCopy.GetLength(1); j++)
            {
                temp[i, j] = arrayToCopy[i, j];
            }
        }

        return temp;
    }
    
    private void Print2DArray(bool[,] arrayToPrint)
    {
        StringBuilder sb = new StringBuilder();
        for(int i = 0; i < arrayToPrint.GetLength(0); i++)
        {
            for(int j = 0; j < arrayToPrint.GetLength(1); j++)
            {
                sb.Append(arrayToPrint[i,j] ? 1 : 0);
                sb.Append(' ');				   
            }
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }
}
