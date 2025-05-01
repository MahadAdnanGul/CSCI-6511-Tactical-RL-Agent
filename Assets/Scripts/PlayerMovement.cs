using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using StarterAssets;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private StateSpaceManager stateSpaceManager;
    [SerializeField] private float playerHeightOffset = 0.5f;
    [SerializeField] private GameObject smokePrefab;
    private BaseState currentState;

    private Vector2Int moveDirection = Vector2Int.zero;
    private bool useSmoke = false;
    private bool hasSmoke = true;

    private void OnEnable()
    {
        StateSpaceManager.PlayerStateUpdate += MoveToNextState;
    }

    private void OnDisable()
    {
        StateSpaceManager.PlayerStateUpdate -= MoveToNextState;
    }

    private void Start()
    {
        Vector3 playerPos = transform.position;

        // Convert world position to grid indices
        Vector2Int gridPos = stateSpaceManager.WorldToGrid(playerPos);

        // Get the closest BaseState
        currentState = stateSpaceManager.GetStateAt(gridPos.x, gridPos.y);
        
        if (currentState != null)
        {
            Debug.Log($"Player starting at State ({gridPos.x}, {gridPos.y})");
        }
        else
        {
            Debug.LogError($"No valid state found at starting position ({gridPos.x}, {gridPos.y})");
        }
    }

    private void PlayerStateUpdate()
    {
        if (currentState.IsGoal)
        {
            Debug.Log("Agent Wins!");
            return;
        }
        
        if (currentState.IsExposed)
        {
            //Player may take damage
            player.TakeDamage();
        }

        if (currentState.ContainsHealth)
        {
            currentState.ContainsHealth = false;
            player.Heal();

        }
    }

    private void MoveToNextState()
    {
        if (currentState == null || !player.IsAlive() || currentState.IsGoal) return;

        // Get current grid position
        Vector2Int currentGridPos = stateSpaceManager.WorldToGrid(currentState.transform.position);

        // Calculate new grid position
        Vector2Int newGridPos = currentGridPos + moveDirection;

        // Get the state at the new position
        BaseState nextState = stateSpaceManager.GetStateAt(newGridPos.x, newGridPos.y);

        if (nextState != null && !nextState.IsWall) // Only move if the next state is valid and not a wall
        {
            currentState = nextState;
            transform.position = new Vector3(currentState.transform.position.x + 0.5f,
                currentState.transform.position.y + playerHeightOffset, currentState.transform.position.z + 0.5f); // Move player GameObject to new position

            Debug.Log($"Moved to State ({newGridPos.x}, {newGridPos.y})");
            if (currentState.IsExposed)
            {
                Debug.Log("Player at EXPOSED STATE");
            }
            
        }
        else
        {
            Debug.Log("Blocked: Cannot move into wall or out of bounds.");
        }

        if (useSmoke)
        {
            useSmoke = false;
            hasSmoke = false;
            int smokeRange = Mathf.RoundToInt(smokePrefab.GetComponent<ParticleSystem>().shape.scale.y/2);
            for (int i = smokeRange; i >= 0; i--)
            {
                Vector3 pos = new Vector3(currentState.transform.position.x, currentState.transform.position.y + 0.5f,
                    currentState.transform.position.z + smokeRange);
                BaseState state = stateSpaceManager.GetStateAt((int)pos.x, (int)pos.z);
                if (state != null)
                {
                    GameObject smoke = Instantiate(smokePrefab);
                    smoke.transform.position = pos;
                    break;
                }
            }

        }
        
        PlayerStateUpdate();
    }

    private void Update()
    {
        if (!player.IsAlive())
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            moveDirection = Vector2Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            moveDirection = Vector2Int.down; 
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            moveDirection = Vector2Int.left; 
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            moveDirection = Vector2Int.right;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            moveDirection = Vector2Int.zero;
            if (hasSmoke)
            {
                useSmoke = true;
            }
        }
            
    }
}

public enum PlayerActions 
{
    None,
    Up,
    Down,
    Left,
    Right,
    Smoke
}