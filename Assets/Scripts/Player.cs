using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private StateSpaceManager stateSpaceManager;
    [SerializeField] private int health = 3;
    [SerializeField] private float damageProb = 0.8f;
    private BaseState _startState;
    public BaseState currentState;
    public bool hasSmoke = true;

    private bool _isAlive = true;
    public bool reachedGoal { get; set; } = false;
    public float bestDistanceToGoal = Single.PositiveInfinity;
    

    public void ResetState()
    {
        hasSmoke = true;
        reachedGoal = false;
        _isAlive = true;
        health = 3;
        currentState = _startState;
        bestDistanceToGoal = Single.PositiveInfinity;

    }

    public bool IsHealthFull()
    {
        return health == 3;
    }
    private void Awake()
    {
        Vector3 playerPos = transform.position;

        // Convert world position to grid indices
        Vector2Int gridPos = stateSpaceManager.WorldToGrid(playerPos);

        // Get the closest BaseState
        currentState = stateSpaceManager.GetStateAt(gridPos.x, gridPos.y);
        _startState = currentState;
        
        if (currentState != null)
        {
            Debug.Log($"Player starting at State ({gridPos.x}, {gridPos.y})");
        }
        else
        {
            Debug.LogError($"No valid state found at starting position ({gridPos.x}, {gridPos.y})");
        }
    }
    
    public void Heal()
    {
        health = math.min(3, health + 1);
        Debug.Log("Player Consumed Health Item.");
    }
    public void TakeDamage()
    {
        if (!_isAlive)
        {
            Debug.LogError("Agent Already Dead!");
            return;
        }
        bool takeDamage = Utilties.SimulateBernoulli(damageProb);
        if (takeDamage)
        {
            Debug.Log("Agent Took Damage");
            health = math.max(0, health - 1);
            if (health <= 0)
            {
                OnDeath();
            }
        }
        else
        {
            Debug.Log("Turret Attack Missed");
        }

    }

    public void OnDeath()
    {
        health = 0;
        _isAlive = false;
        Debug.Log("Your Agent just kicked the bucket :(");
    }

    public bool IsAlive()
    {
        return _isAlive;
    }
    
}
