using System.Collections.Generic;
using System.IO;
using Interfaces;
using UnityEngine;

public class SARSALambda : MonoBehaviour, IAgentRL
{
    private const string FileName = "sarsa_lambda_table.json";
    [SerializeField] private TrainerSettings settings;
    private Dictionary<StateActionPair, float> SarsaTable       = new Dictionary<StateActionPair, float>();
    private Dictionary<StateActionPair, float> eligibilityTrace = new Dictionary<StateActionPair, float>();
    [SerializeField] private PlayerMovement playerMovement;

    public PlayerActions SelectAction(BaseState currentState)
    {
        // Exploration: sometimes go for random action
        if (Random.value < settings.epsilon)
        {
            return GetRandomAction(currentState);
        }
        
        float maxQValue = float.MinValue;
        PlayerActions bestAction = PlayerActions.None;
        foreach (PlayerActions action in playerMovement.GetLegalActions(currentState))
        {
            StateActionPair stateAction = new StateActionPair(currentState.GetPosition(), action);
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
        StateActionPair prevStateActionPair = new StateActionPair(prevState.GetPosition(), action);
        float prevQ = SarsaTable.TryGetValue(prevStateActionPair, out var oldVal) ? oldVal : 0f;
        var nextStateActionPair = new StateActionPair(newState.GetPosition(), nextAction);
        float nextQ = SarsaTable.TryGetValue(nextStateActionPair, out var nextVal) ? nextVal : 0f;
        eligibilityTrace[prevStateActionPair] = eligibilityTrace.TryGetValue(prevStateActionPair, out var eVal) ? eVal + 1f : 1f;
        var stateActionPairs = new List<StateActionPair>(eligibilityTrace.Keys);
        float diff = reward + settings.discountFactor * nextQ - prevQ;
        
        foreach (StateActionPair stateActionPair in stateActionPairs)
        {
            // Q(S,A) = Q(S,A) + learningRate * diff * E(S,A)
            // E(S,A) = discountFactor * lambda * E(S,A)
            float eSA = eligibilityTrace[stateActionPair];
            float currentQ = SarsaTable.TryGetValue(stateActionPair, out var qVal) ? qVal : 0f;
            float updatedQ = currentQ + settings.learningRate * diff * eSA;
            SarsaTable[stateActionPair] = updatedQ;
            
            float postDecayESA = settings.discountFactor * settings.lambda * eSA;
            if (postDecayESA < Utilties.EPSILON)
                eligibilityTrace.Remove(stateActionPair);
            else
                eligibilityTrace[stateActionPair] = postDecayESA;
        }
    }

    public void SaveTrainingData()
    {
        /*List<TrainingEntry> list = new List<TrainingEntry>();
        foreach (var kvp in SarsaTable)
        {
            list.Add(new TrainingEntry { stateActionPair = kvp.Key, value = kvp.Value });
        }

        TrainingTable trainingTable = new TrainingTable();
        trainingTable.entries = list.ToArray();
        string json = JsonUtility.ToJson(trainingTable, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, FileName), json);
        Debug.Log($"SARSA-Lambda table saved: {FileName}");*/
    }

    public void LoadTrainingData()
    {
        /*string path = Path.Combine(Application.persistentDataPath, FileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No SARSA Lambda table at {path}");
            return;
        }*/
        string json = Resources.Load<TextAsset>("sarsa_lambda_table").text;
        TrainingTable trainingTable = JsonUtility.FromJson<TrainingTable>(json);
        SarsaTable.Clear();
        eligibilityTrace.Clear();
        foreach (TrainingEntry entry in trainingTable.entries)
        {
            SarsaTable[entry.stateActionPair] = entry.value;
        }
        Debug.Log($"SARSA Lambda table loaded: {SarsaTable.Count} entries");
    }
}