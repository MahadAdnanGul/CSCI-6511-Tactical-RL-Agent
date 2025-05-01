using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Unity.Mathematics;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private LayerMask stateLayerMask;
    private int length;
    private int width;
    private Collider[] overlapStates;

    private void OnEnable()
    {
        StateSpaceManager.SmokeStateUpdate += SmokeStateUpdate;
    }

    private void OnDisable()
    {
        StateSpaceManager.SmokeStateUpdate -= SmokeStateUpdate;
    }

    private void Start()
    {
        length = Mathf.RoundToInt(smokeParticles.shape.scale.y);
        width = Mathf.RoundToInt(smokeParticles.shape.scale.x);
        overlapStates = new Collider[length * width];
    }

    private void SmokeStateUpdate()
    {
        Vector3 halfExtents = new Vector3((length / 2) - 0.1f, 3, (width/2) - 0.1f);
        Physics.OverlapBoxNonAlloc(transform.position, halfExtents, overlapStates, transform.rotation, stateLayerMask);
        Utilties.DebugDrawBox(transform.position, halfExtents, transform.rotation, Color.red, 1f);
        foreach (var col in overlapStates)
        {
            BaseState state = col.gameObject.GetComponent<BaseState>();
            state.IsSmoked = true;
            state.IsExposed = false;
            Debug.Log($"State ({state.transform.position.x},{state.transform.position.z}) is Smoked!");
        }
    }
}
