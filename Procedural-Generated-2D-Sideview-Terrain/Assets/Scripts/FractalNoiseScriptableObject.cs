using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FractalNoiseScriptableObject", menuName = "Data/FractalNoiseScriptableObject")]
public class FractalNoiseScriptableObject : ScriptableObject
{
    [Header("Noise")]
    [Range(0, 1)]
    public float amplitude = 0.4f;
    [Range(0, 0.1f)] 
    public float frequency = 0.05f;
    
    public int octaves = 3;
    
    public float lacunarity = 2.0f;
    
    public float persistence = 0.5f;
    
    
    [Header("Stone Noise")]
    [Range(0, 1)]
    public float stoneAmplitude = 0.4f;
    [Range(0, 0.1f)] 
    public float stoneFrequency = 0.02f;
    
    public int stoneOctaves = 3;
     
    public float stoneLacunarity = 2.0f;
    
    public float stonePersistence = 0.5f;
    
    [Header("World")]
    public int width = 150;
    
    public int seed = 1;
    [Range(1, 500)]
    public int heightModifier = 100;
    [Range(0, 1)] 
    public float caveModifier = 0.2f;
}
