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
    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public override Vector3 GetPosition()
    {
        return transform.position;
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
}
