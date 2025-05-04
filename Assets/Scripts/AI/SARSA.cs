using System;
using System.Collections.Generic;
using System.IO;
using Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

public class SARSA : MonoBehaviour, IAgentRL
{
    private const string FileName = "sarsa_table.json";
    public float learningRate   = 0.1f;   
    public float discountFactor = 0.95f;  
    public float epsilon        = 0.1f;   

    private Dictionary<StateActionPair, float> SarsaTable = new Dictionary<StateActionPair, float>();

    [SerializeField] private PlayerMovement playerMovement;

    public PlayerActions SelectAction(BaseState currentState)
    {
        // Exploration: sometimes go for random action
        if (Random.value < epsilon)
        {
            return GetRandomAction(currentState);
        }

        float maxQValue = float.MinValue;
        PlayerActions bestAction = PlayerActions.None;
        foreach (PlayerActions action in playerMovement.GetLegalActions(currentState))
        {
            StateActionPair stateAction = new(currentState.GetPosition(), action);
            float QValue = SarsaTable.TryGetValue(stateAction, out var value) ? value : 0f;
            if (QValue > maxQValue)
            {
                maxQValue = QValue;
                bestAction = action;
            }
        }
        return bestAction;
    }
    
    private PlayerActions GetRandomAction(BaseState state)
    {
        PlayerActions[] all = playerMovement.GetLegalActions(state);
        return all[Random.Range(0, all.Length)];
    }

    public void UpdateRL(BaseState prevState, PlayerActions action, BaseState newState, float reward)
    {
        PlayerActions nextAction = SelectAction(newState);
        var prevStateActionPair = new StateActionPair(prevState.GetPosition(), action);
        float prevQ = SarsaTable.TryGetValue(prevStateActionPair, out var oldVal) ? oldVal : 0f;
        var nextStateActionPair = new StateActionPair(newState.GetPosition(), nextAction);
        float nextQ = SarsaTable.TryGetValue(nextStateActionPair, out var value) ? value : 0f;
        SarsaTable[prevStateActionPair] = prevQ + learningRate * (reward + discountFactor * nextQ - prevQ);
    }

    public void SaveTrainingData()
    {
        List<TrainingEntry> list = new List<TrainingEntry>();
        foreach (var kvp in SarsaTable)
        {
            list.Add(new TrainingEntry { stateActionPair = kvp.Key, value = kvp.Value });
        }
        TrainingTable trainingTable = new TrainingTable();
        trainingTable.entries = list.ToArray();
        string json = JsonUtility.ToJson( trainingTable, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, FileName), json);
        Debug.Log($"SARSA table saved: {FileName}");
    }

    public void LoadTrainingData()
    {
        string path = Path.Combine(Application.persistentDataPath, FileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No SARSA table at {path}");
            return;
        }
        string json = File.ReadAllText(path);
        TrainingTable trainingTable = JsonUtility.FromJson<TrainingTable>(json);
        SarsaTable.Clear();
        foreach (TrainingEntry entry in trainingTable.entries)
        {
            SarsaTable[entry.stateActionPair] = entry.value;
        }
        Debug.Log($"SARSA table loaded: {SarsaTable.Count} entries");
    }
}


