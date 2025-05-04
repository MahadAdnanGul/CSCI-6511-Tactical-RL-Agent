using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

public interface IAgentRL
{
    public PlayerActions SelectAction(BaseState currentState);

    public void UpdateRL(BaseState prevState, PlayerActions action, BaseState newState, float reward);

    public void SaveTrainingData();

    public void LoadTrainingData();
}
