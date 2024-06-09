using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class CellularAutomata : MonoBehaviour
{
    [Header("Cellular Automata")]
    [SerializeField] 
    private int iterations;
    [SerializeField] [Range(1, 8)] 
    private int neighbourBreak;

    private bool[,] _noiseGrid;
    private bool[,] _tempGrid;

    [Header("Noise Grid Generator")]
    [SerializeField] [Range(1, 100)]
    private int density;
    [SerializeField] 
    private int height;
    [SerializeField] 
    private int width;

    [Header("Renderer")] 
    [SerializeField] 
    private Tilemap groundTilemap;
    [SerializeField] 
    private BlockData blockData;


    public int iterationCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        _noiseGrid = new bool[height, width];
        _tempGrid = new bool[height, width];
        
        GenerateNosieGrid();
        SmoothNoiseGird();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            iterationCount++;
            iterations++;
            if (iterations == 1 && iterationCount == 1)
            {
                iterationCount = 0;
            }
            AddOneIteration();
        }
    }

    private void SmoothNoiseGird()
    {
        int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

        for (int i = iterationCount; i < iterations; i++)
        {
            CopyArray();
            
            for (int iPixel = 0; iPixel < height; iPixel++)
            {
                for (int jPixel = 0; jPixel < width; jPixel++)
                {
                    // for (int l = iPixel-1; l < iPixel+2; l++)
                    // {
                    //     for (int m = jPixel-1; m < jPixel+2; m++)
                    //     {
                    //         // if ((l < 0 || l >= height) || (m < 0 || m >= width) ||
                    //         //     (tempGrid[l, m] == true && (l != iPixel && m != jPixel)))
                    //         // {
                    //         //     neighbourCount++;
                    //         // }
                    //
                    //         // if (l < 0)
                    //         // {
                    //         //     Debug.Log("l < 0");
                    //         //     neighbourCount++;
                    //         // }
                    //         // else if (l >= height)
                    //         // {
                    //         //     Debug.Log("l >= height");
                    //         //     neighbourCount++;
                    //         // }
                    //         // else if (m < 0) 
                    //         // {
                    //         //     Debug.Log("m < 0");
                    //         //     neighbourCount++;
                    //         // }
                    //         // else if (m >= width)
                    //         // {
                    //         //     Debug.Log("m >= width");
                    //         //     neighbourCount++;
                    //         // }
                    //         // else if (_tempGrid[l,m] == true && l != iPixel && m != jPixel)
                    //         // {
                    //         //     Debug.Log("tempGrid[l,m] == true && l != iPixel && m != jPixel");
                    //         //     neighbourCount++;
                    //         // }
                    //
                    //         if ((l < 0 || l >= height) || (m < 0 || m >= width))
                    //         {
                    //             border = true;
                    //         }
                    //         else
                    //         {
                    //             if (_tempGrid[l,m] == true && l != iPixel && m != jPixel)
                    //             {
                    //                 neighbourCount++;
                    //             }
                    //         }
                    //     }
                    // }
                    //
                    // if (neighbourCount > neighbourBreak || border)
                    // {
                    //     _noiseGrid[iPixel, jPixel] = true;
                    // }
                    // else
                    // {
                    //     _noiseGrid[iPixel, jPixel] = false;   
                    // }
                    //
                    // border = false;
                    // neighbourCount = 0;

                    int neighbourCount = 0;

                    for (int dir = 0; dir < dx.Length; dir++)
                    {
                        int x = iPixel + dx[dir];
                        int y = jPixel + dy[dir];
                        
                        if (x >= 0 && x < height && y >= 0 && y < width)
                        {
                            if (_tempGrid[x, y]) neighbourCount++;
                        }
                        else
                        {
                            neighbourCount++;
                        }
                    }

                    _noiseGrid[iPixel, jPixel] = neighbourCount > neighbourBreak;
                }
            }
        }
        
        RenderTileWorld();
    }
    
    private void GenerateNosieGrid()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                _noiseGrid[i, j] = Random.Range(1, 101) <= density;
            }
        }
    }

    private void RenderTileWorld()
    {
        groundTilemap.ClearAllTiles();
        
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (_noiseGrid[i,j])
                {
                    groundTilemap.SetTile(new Vector3Int(j, height - i - 1, 0), blockData.stoneTile);
                }
            }
        }
    }

    private void Print2DArray()
    {
        StringBuilder sb = new StringBuilder();
        for(int i=0; i < _noiseGrid.GetLength(1); i++)
        {
            for(int j=0; j< _noiseGrid.GetLength(0); j++)
            {
                sb.Append(_noiseGrid[i,j] ? 1 : 0);
                sb.Append(' ');				   
            }
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }

    private void CopyArray()
    {
        for (int i = 0; i < _noiseGrid.GetLength(0); i++)
        {
            for (int j = 0; j < _noiseGrid.GetLength(1); j++)
            {
                _tempGrid[i, j] = _noiseGrid[i, j];
            }
        }
    }

    private void AddOneIteration()
    {
        SmoothNoiseGird();
    }
}
