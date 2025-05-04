using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using StarterAssets;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private StateSpaceManager stateSpaceManager;
    [SerializeField] private float playerHeightOffset = 0.5f;
    [SerializeField] private GameObject smokePrefab;

    private Vector2Int moveDirection = Vector2Int.zero;
    public bool useSmoke = false;

    private void OnEnable()
    {
        StateSpaceManager.PlayerStateUpdate += MoveToNextState;
    }

    private void OnDisable()
    {
        StateSpaceManager.PlayerStateUpdate -= MoveToNextState;
    }

    
    public void SetAction(PlayerActions action)
    {
        useSmoke = false;
        moveDirection = Vector2Int.zero;
        switch (action)
        {
            case PlayerActions.None:
                moveDirection = Vector2Int.zero;
                break;
            case PlayerActions.Up:
                moveDirection = Vector2Int.up;
                break;
            case PlayerActions.Down:
                moveDirection = Vector2Int.down;
                break;
            case PlayerActions.Left:
                moveDirection = Vector2Int.left;
                break;
            case PlayerActions.Right:
                moveDirection = Vector2Int.right;
                break;
            case PlayerActions.Smoke:
                if (player.hasSmoke)
                {
                    moveDirection = Vector2Int.zero;
                    useSmoke = true;
                }
                break;
        }
    }
    

    private void PlayerStateUpdate()
    {
        if (player.currentState.IsGoal)
        {
            player.reachedGoal = true;
            Debug.Log("Agent Wins!");
            return;
        }
        
        if (player.currentState.IsExposed)
        {
            //Player may take damage
            player.TakeDamage();
        }

        if (player.currentState.ContainsHealth)
        {
            player.currentState.ContainsHealth = false;
            player.Heal();

        }
    }

    private bool CheckIfMoveLegal(Vector2Int moveDir, BaseState state)
    {
        Vector2Int currentGridPos = stateSpaceManager.WorldToGrid(state.transform.position);
        Vector2Int newGridPos = currentGridPos + moveDir;
        BaseState nextState = stateSpaceManager.GetStateAt(newGridPos.x, newGridPos.y);

        return (nextState != null && !nextState.IsWall);

    }

    private void MoveToNextState()
    {
        if (player.currentState == null || !player.IsAlive() || player.currentState.IsGoal) return;
        
        Vector2Int currentGridPos = stateSpaceManager.WorldToGrid(player.currentState.transform.position);
        Vector2Int newGridPos = currentGridPos + moveDirection;
        BaseState nextState = stateSpaceManager.GetStateAt(newGridPos.x, newGridPos.y);

        if (nextState != null && !nextState.IsWall) // Only move if the next state is valid and not a wall
        {
            player.currentState = nextState;
            transform.position = new Vector3(player.currentState.transform.position.x + 0.5f,
                player.currentState.transform.position.y + playerHeightOffset, player.currentState.transform.position.z + 0.5f); // Move player GameObject to new position

            /*Debug.Log($"Moved to State ({newGridPos.x}, {newGridPos.y})");
            if (player.currentState.IsExposed)
            {
                Debug.Log("Player at EXPOSED STATE");
            }*/

        }
        else
        {
            Debug.Log("Blocked: Cannot move into wall or out of bounds.");
            
        }

        if (useSmoke)
        {
            player.hasSmoke = false;
            int smokeRange = Mathf.RoundToInt(smokePrefab.GetComponent<ParticleSystem>().shape.scale.y/2) + 1;
            for (int i = smokeRange; i >= 0; i--)
            {
                Vector3 pos = new Vector3(player.currentState.transform.position.x, player.currentState.transform.position.y + 0.5f,
                    player.currentState.transform.position.z + i);
                Vector2Int gridPos = stateSpaceManager.WorldToGrid(pos);
                BaseState state = stateSpaceManager.GetStateAt(gridPos.x, gridPos.y);
                if (state != null)
                {
                    smokePrefab.SetActive(true);
                    smokePrefab.transform.position = pos;
                    //GameObject smoke = Instantiate(smokePrefab, pos, quaternion.identity);
                    break;
                }
            }

        }
        
        PlayerStateUpdate();
    }

    private void Update()
    {
        if(!stateSpaceManager.trainingMode)
            HandlePlayerInput();
    }

    public PlayerActions[] GetLegalActions(BaseState state, bool excludeSmoke = false)
    {
        List<PlayerActions> legalActions = new List<PlayerActions>();
        if (CheckIfMoveLegal(Vector2Int.up, state))
        {
            legalActions.Add(PlayerActions.Up);
        }

        if (CheckIfMoveLegal(Vector2Int.down, state))
        {
            legalActions.Add(PlayerActions.Down);
        }
        
        if (CheckIfMoveLegal(Vector2Int.left, state))
        {
            legalActions.Add(PlayerActions.Left);
        }
        
        if (CheckIfMoveLegal(Vector2Int.right, state))
        {
            legalActions.Add(PlayerActions.Right);
        }

        if (!excludeSmoke)
        {
            if (player.hasSmoke)
            {
                
                Vector2Int nextPos = stateSpaceManager.WorldToGrid(new Vector3(player.currentState.transform.position.x, 0, player.currentState.transform.position.z + 1f));
                BaseState nextState = stateSpaceManager.GetStateAt(nextPos.x, nextPos.y);
                
                if (player.currentState.IsExposed || (nextState != null && nextState.IsExposed))
                {
                    legalActions.Add(PlayerActions.Smoke);
                }
            }
        }


        legalActions.Add(PlayerActions.None);

        return legalActions.ToArray();
    }

    private void HandlePlayerInput()
    {
        if (!player.IsAlive())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            SetAction(PlayerActions.Up);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SetAction(PlayerActions.Down);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            SetAction(PlayerActions.Left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            SetAction(PlayerActions.Right);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            SetAction(PlayerActions.Smoke);
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