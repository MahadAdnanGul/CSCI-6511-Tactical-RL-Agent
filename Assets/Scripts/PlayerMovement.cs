using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using StarterAssets;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private StateSpaceManager stateSpaceManager;
    [SerializeField] private float playerHeightOffset = 0.5f;
    private BaseState currentState;


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
    
    

    private void MoveToNextState(Vector2Int direction)
    {
        if (currentState == null) return;

        // Get current grid position
        Vector2Int currentGridPos = stateSpaceManager.WorldToGrid(currentState.transform.position);

        // Calculate new grid position
        Vector2Int newGridPos = currentGridPos + direction;

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
    }

    private void Update()
    {
            Vector2Int moveDirection = Vector2Int.zero;

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
            
            if (moveDirection != Vector2Int.zero)
            {
                MoveToNextState(moveDirection);
            }
            
    }
}

public enum PlayerActions 
{
    None,
    Up,
    Down,
    Left,
    Right
}