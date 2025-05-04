using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RL/LearningParams", fileName = "LearningParams")]
public class TrainerSettings : ScriptableObject
{
    [Header("Learning Parameters")]
    [Range(0f, 1f)]
    public float learningRate = 0.2f;
    
    [Range(0f, 1f)]
    public float discountFactor = 0.95f;
    
    [Range(0f, 1f)]
    public float epsilon = 0.1f;

    [Range(0f, 1f)]
    public float lambda = 0.8f;

    public int stepsPerEpisode = 200;
    public int totalEpisodes = 50000;

    [Tooltip("Able to see training steps but much slower")]
    public bool visualizeTraining = false;
    
    [Tooltip("Add step delay to adjust simulation speed")]
    public float stepDelay = 0f;
    
    [Tooltip("Enable debug mode to see text overlays on top of each grid position")]
    public DebugLevel debugLevel = DebugLevel.Basic;


    public bool trainingMode = false;
    public bool ignoreSave = true;
    public bool evalMode = false;
}
