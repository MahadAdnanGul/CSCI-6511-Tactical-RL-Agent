using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using Interfaces;
using Unity.Mathematics;
using UnityEngine;

public class Trainer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TrainerSettings settings;
    

    [Header("Dependencies")] 
    [SerializeField] private DebugUI debugUI;
    [SerializeField] private MonoBehaviour agentBehaviour; 
    private IAgentRL agentRL => agentBehaviour as IAgentRL;
    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private Player player;
    [SerializeField] private StateSpaceManager stateManager;
    [SerializeField] private Vector3 playerStartWorldPos;
    
    private int wins = 0;

    private void Start()
    {
        if (agentRL is not IAgentRL)
        {
            Debug.LogError("Agent Behavior is not of type IAgentRL...Invalid Agent!");
            return;
        }
        
        if (!stateManager.trainingMode)
        {
            Debug.LogWarning("Training Mode Disabled... Trainer will not work.");
            return;
        }
        agentRL.LoadTrainingData();
        if (settings.visualizeTraining)
        {
            StartCoroutine(TrainingLoop());
        }
        else
        {
            StartCoroutine(FastTrainingLoop());
        }
    }
    

    private IEnumerator FastTrainingLoop()
    {
        // To ensure everything is subscribed to events
        yield return new WaitForEndOfFrame();
        
        for (int i = 1; i <= settings.totalEpisodes; i++)
        {
            // Init Episode
            ResetEpisode();
            yield return new WaitForEndOfFrame();
            //yield return new WaitForEndOfFrame();
            float totalReward = 0f;
            BaseState prevState = player.currentState;

            for (int j = 0; j < settings.stepsPerEpisode; j++)
            {
                // Select next action based on agent policy
                PlayerActions action = agentRL.SelectAction(prevState);
                playerMovement.SetAction(action);        
                
                //yield return new WaitForEndOfFrame();  
                
                stateManager.ExecuteAction();
                float reward = ComputeReward(player.currentState);
                totalReward += reward;

                // Model Update
                agentRL.UpdateRL(prevState, action, player.currentState, reward);

                // Episode ends on terminal state
                if (!player.IsAlive())
                    break;

                if (player.reachedGoal)
                {
                    wins++;
                    break;
                }
                

                prevState = player.currentState;

                // Simulation Delay
                //if (stepDelay > 0f)
                    //yield return new WaitForSeconds(stepDelay);
            }
            
            // Saving data periodically
            if (i % 100 == 0)
            {
                agentRL.SaveTrainingData();
                Debug.Log($"Win Rate: {(wins/100f)*100f}%");
                wins = 0;
            }

            string msg = $"Episode {i}/{settings.totalEpisodes} Completed: Reward = {totalReward}";
            Debug.Log(msg);
            debugUI.ShowDebugMessage(msg);
        }

        Debug.Log($"Training complete!");
        
        agentRL.SaveTrainingData();
    }
    
    private IEnumerator TrainingLoop()
    {
        for (int i = 1; i <= settings.totalEpisodes; i++)
        {
            // Init Episode
            ResetEpisode();
            float totalReward = 0f;
            BaseState prevState = player.currentState;

            for (int j = 0; j < settings.stepsPerEpisode; j++)
            {
                // Select next action based on agent policy
                PlayerActions action = agentRL.SelectAction(prevState);
                playerMovement.SetAction(action);
                
                // Simulation Delay
                if (settings.stepDelay > 0f)
                    yield return new WaitForSeconds(settings.stepDelay);
                else
                {
                    yield return new WaitForEndOfFrame();  
                }

                
                stateManager.ExecuteAction();
                float reward = ComputeReward(player.currentState);
                totalReward += reward;

                // Model Update
                agentRL.UpdateRL(prevState, action, player.currentState, reward);

                if (!player.IsAlive())
                    break;

                if (player.reachedGoal)
                {
                    wins++;
                    break;
                }

                prevState = player.currentState;
                
            }
            
            // Saving data periodically
            if (i % 100 == 0)
            {
                if (!settings.visualizeTraining)
                {
                    agentRL.SaveTrainingData();
                }
                Debug.Log($"Win Rate: {(wins/100f)*100f}%");
                wins = 0;
            }

            string msg = $"Episode {i}/{settings.totalEpisodes} Completed: Reward = {totalReward}";
            Debug.Log(msg);
            debugUI.ShowDebugMessage(msg);
        }

        Debug.Log("Training complete!");
        if (!settings.visualizeTraining)
        {
            agentRL.SaveTrainingData();
        }

    }

    private void ResetEpisode()
    {
        player.transform.position = playerStartWorldPos;
        player.ResetState();
        stateManager.ResetAllStates();
    }

    private float ComputeReward(BaseState state)
    {
        float reward = 0;
        
        // Encourage using smoke sparingly and in a useful way
        if (playerMovement.useSmoke)
        {
            reward -= 0.2f;
            reward += Mathf.Min((stateManager.numberOfExposedStatesSmoked * 0.05f), 1.0f);
        }

        // Encourage the agent to try to save their smoke
        if (!player.hasSmoke)
        {
            reward -= 0.01f;
        }

        // Big penalty for dying
        if (!player.IsAlive())
        {
            reward -= 5f;
        }
        
        // Penalty for being exposed
        if (state.IsExposed)
        {
            reward -= 0.5f;
        }

        // Reward for collecting health if it benefits player, otherwise penalize wastage
        if (state.ContainsHealth )
        {
            if (player.IsHealthFull())
            {
                reward -= 0.2f;
            }
            else
            {
                reward += 0.5f;
            }
         
        }

        // Reward for reaching goal state
        if (state.IsGoal)
        {
            reward += 5f;
        }

        // Reward for inching closer to goal state
        Vector2Int goalPos = stateManager.goalState.GetPosition();
        float currentGoalDist = Vector2Int.Distance(goalPos, state.GetPosition());
        if (currentGoalDist < player.bestDistanceToGoal)
        {
            player.bestDistanceToGoal = currentGoalDist;
            reward += 0.05f;
        }
        else
        {
            reward -= 0.05f;
        }
        
        return reward;
    }
}
