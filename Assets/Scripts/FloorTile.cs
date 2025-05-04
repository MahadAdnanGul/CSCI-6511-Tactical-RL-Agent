using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

public class FloorTile : BaseState
{
    [SerializeField] private Material exposedMaterial;
    [SerializeField] private Material regularMaterial;
    [SerializeField] private Material wallMaterial;
    [SerializeField] private Material goalMaterial;
    [SerializeField] private GameObject healthItem;
    [SerializeField] private MeshRenderer _meshRenderer;
    private StateValues initValues;
    
    

    private void Awake()
    {
       
        
    }

    private void Start()
    {
        initValues.ContainsHealth = ContainsHealth;
        initValues.IsExposed = IsExposed;
        initValues.IsWall = IsWall;
        initValues.IsGoal = IsGoal;
    }

    public override Vector2Int GetPosition()
    {
        return new Vector2Int((int)transform.position.x, (int)transform.position.z);
    }

    public override void UpdateState()
    {
        if (IsGoal)
        {
            _meshRenderer.material = goalMaterial;
        }
        else if (IsExposed)
        {
            _meshRenderer.material = exposedMaterial;
        }
        else if (IsWall)
        {
            _meshRenderer.material = wallMaterial;
        }
        else
        {
            _meshRenderer.material = regularMaterial;
        }

        healthItem.SetActive(ContainsHealth);
    }

    public override void ResetState()
    {
        IsExposed = initValues.IsExposed;
        IsWall = initValues.IsWall;
        IsGoal = initValues.IsGoal;
        ContainsHealth = initValues.ContainsHealth;
    }
}
