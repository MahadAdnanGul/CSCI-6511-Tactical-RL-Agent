using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Interfaces;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using Update = UnityEngine.PlayerLoop.Update;

public enum DebugLevel
{
    None,
    Basic,
    Extra
}
public class StateSpaceManager : MonoBehaviour
{
    public bool trainingMode = true;
    public static Action TurretStateUpdate { get; set; }
    public static Action PlayerStateUpdate { get; set; }

    [Tooltip("Enable debug mode to see text overlays on top of each grid position")]
    [SerializeField] private DebugLevel debugLevel = DebugLevel.None;
    
    private int gridWidth;
    private int gridHeight;
    private BaseState[][] stateSpace;
    public BaseState goalState;
    
    private float minX;
    private float minZ;
    public int numberOfExposedStatesSmoked = 0;
    
    [SerializeField] private float timeStepUpdateRate = 1f;
    private float elapsedTime = 0;

    private void Awake()
    {
        InitializeStateSpace();
    }

    public void ResetAllStates()
    {
        Smoke[] smokes = FindObjectsByType<Smoke>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var smoke in smokes)
        {
            smoke.gameObject.SetActive(false);
            //Destroy(smoke.gameObject);
        }
        foreach (var stateArr in stateSpace)
        {
            foreach (var state in stateArr)
            {
                state.ResetState();
            }
        }

        numberOfExposedStatesSmoked = 0;
    }
    

    private void InitializeStateSpace()
    {
        // Get all states in scene
        List<BaseState> states = FindObjectsByType<BaseState>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .ToList();
        List<Wall> walls = FindObjectsByType<Wall>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();


        if (states.Count == 0)
        {
            Debug.LogError("No states implementing BaseState found in the scene!");
            return;
        }

        minX = states.Min(s => s.GetPosition().x);
        minZ = states.Min(s => s.GetPosition().y);
        float maxX = states.Max(s => s.GetPosition().x);
        float maxZ = states.Max(s => s.GetPosition().y);

        gridWidth = Mathf.RoundToInt(maxX - minX) + 1;
        gridHeight = Mathf.RoundToInt(maxZ - minZ) + 1;

        //Debug.Log($"State Space Size: {gridWidth} x {gridHeight}");
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();

        foreach (var wall in walls)
        {
            Vector3 pos = wall.transform.position;
            Vector2Int gridPos =
                new Vector2Int(Mathf.RoundToInt(pos.x - minX), Mathf.RoundToInt(pos.z - minZ)); // Assuming XZ plane
            wallPositions.Add(gridPos);
        }

        // Initialize stateSpace
        stateSpace = new BaseState[gridWidth][];
        for (int x = 0; x < gridWidth; x++)
            stateSpace[x] = new BaseState[gridHeight];

        // Assign states
        foreach (var state in states)
        {
            Vector2Int pos = state.GetPosition();
            int x = Mathf.RoundToInt(pos.x - minX);
            int z = Mathf.RoundToInt(pos.y - minZ);
            if (state.IsGoal)
            {
                goalState = state;
            }

            if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight)
            {
                Vector2Int stateGridPos = new Vector2Int(x, z);
                stateSpace[x][z] = state;
                if (wallPositions.Contains(stateGridPos))
                {
                    state.IsWall = true;
                }

                if (debugLevel != DebugLevel.None)
                {
                    state.UpdateState();
                }
                
            }
            else
            {
                Debug.LogWarning($"State at world position ({pos.x},{pos.y}) is out of adjusted bounds.");
            }
        }
    }

    public BaseState GetStateAt(int x, int z)
    {
        if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight)
            return stateSpace[x][z];
        return null;
    }
    
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x - minX);
        int z = Mathf.RoundToInt(worldPosition.z - minZ);
        return new Vector2Int(x, z);
    }

    public Vector3 GridToWorld(int x, int z)
    {
        float worldX = x + minX;
        float worldZ = z + minZ;
        return new Vector3(worldX, 0f, worldZ); // Assuming y=0 for grid world
    }
    
    void OnDrawGizmos()
    {
        if (stateSpace == null) return;
        if (debugLevel != DebugLevel.Extra) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        style.alignment = TextAnchor.MiddleCenter;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                BaseState state = stateSpace[x][z];
                if (state != null)
                {
                    Vector3 worldPos = state.transform.position + new Vector3(0, 0.5f, 0); 

#if UNITY_EDITOR
                    Handles.Label(worldPos, $"({x},{z})\n Wall: {state.IsWall}", style);
#endif
                }
            }
        }
    }


    public void ExecuteAction()
    {
        numberOfExposedStatesSmoked = 0;
        PlayerStateUpdate?.Invoke();
        
        // Reset exposed states
        foreach (var stateArr in stateSpace)
        {
            foreach (var state in stateArr)
            {
                state.IsExposed = false;
            }
        }
        TurretStateUpdate?.Invoke();
            
            
        if (debugLevel != DebugLevel.None)
        {
            foreach (var stateArr in stateSpace)
            {
                foreach (var state in stateArr)
                {
                    state.UpdateState();
                }
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(trainingMode)
            return;
        
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= timeStepUpdateRate)
        {
            elapsedTime = 0;
            ExecuteAction();
        }

        
        
    }
}
