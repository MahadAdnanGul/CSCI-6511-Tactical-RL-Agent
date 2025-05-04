using System;
using System.Collections.Generic;
using System.IO;
using Interfaces;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class QLearning : MonoBehaviour, IAgentRL
{
    private const string FileName = "qtable.json";

    public TrainerSettings learningParams;

    private Dictionary<StateActionPair, float> QTable = new Dictionary<StateActionPair, float>();

    [SerializeField] private PlayerMovement playerMovement;

    public PlayerActions SelectAction(BaseState currentState)
    {
        // Exploration: sometimes go for random action
        if (Random.value < learningParams.epsilon)
        {
            return GetRandomAction(currentState);
        }

        float maxQValue = float.MinValue;
        PlayerActions bestAction = PlayerActions.None;

        foreach (PlayerActions action in playerMovement.GetLegalActions(currentState))
        {
            StateActionPair stateAction = new(currentState.GetPosition(), action);
            float QValue = QTable.TryGetValue(stateAction, out var value) ? value : 0f;
            if (QValue > maxQValue)
            {
                maxQValue = QValue;
                bestAction = action;
            }
        }

        return bestAction;
    }

    public void UpdateRL(BaseState prevState, PlayerActions action, BaseState newState, float reward)
    {
        var prevStateAction = new StateActionPair(prevState.GetPosition(), action);
        var maxNextQ = float.MinValue;

        foreach (PlayerActions a in playerMovement.GetLegalActions(newState))
        {
            var nextStateAction = new StateActionPair(newState.GetPosition(), a);
            float q = QTable.ContainsKey(nextStateAction) ? QTable[nextStateAction] : 0f;
            maxNextQ = Mathf.Max(maxNextQ, q);
        }

        float oldQ = QTable.ContainsKey(prevStateAction) ? QTable[prevStateAction] : 0f;
        float newQ = oldQ + learningParams.learningRate * (reward + learningParams.discountFactor * maxNextQ - oldQ);
        QTable[prevStateAction] = newQ;
    }

    private PlayerActions GetRandomAction(BaseState state)
    {
        PlayerActions[] all = playerMovement.GetLegalActions(state);
        return all[Random.Range(0, all.Length)];
    }
    
    public void SaveTrainingData()
    {
        List<TrainingEntry> list = new List<TrainingEntry>(QTable.Count);
        foreach (var kvp in QTable)
        {
            list.Add(new TrainingEntry { stateActionPair = kvp.Key, value = kvp.Value });
        }

        TrainingTable trainingTable = new TrainingTable();
        trainingTable.entries = list.ToArray();
        string json = JsonUtility.ToJson(trainingTable, prettyPrint: true);
        string path = Path.Combine(Application.persistentDataPath, FileName);
        File.WriteAllText(path, json);
        Debug.Log($"Q-table saved to {path}");
    }
    
    public void LoadTrainingData()
    {
        string path = Path.Combine(Application.persistentDataPath, FileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No Q-table file found at {path}");
            return;
        }
        string json = File.ReadAllText(path);
        TrainingTable data = JsonUtility.FromJson<TrainingTable>(json);
        QTable.Clear();
        foreach (TrainingEntry entry in data.entries)
        {
            QTable[entry.stateActionPair] = entry.value;
        }
        Debug.Log($"Q-table loaded from {path}, {QTable.Count} entries");
    }
}

[Serializable]
public struct TrainingEntry
{
    public StateActionPair stateActionPair;
    public float value;
}

[Serializable]
public class TrainingTable
{
    public TrainingEntry[] entries;
}
