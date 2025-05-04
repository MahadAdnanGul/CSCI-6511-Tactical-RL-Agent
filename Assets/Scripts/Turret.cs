using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField] 
    private TurretLOSVisual LOSVisual;

    [SerializeField] private StateSpaceManager stateSpaceManager;

    [SerializeField] private GameObject Target;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private LayerMask smokeMask;
    
    [SerializeField] 
    private float losRange = 5.0f;
    [SerializeField] 
    private float losAngle = 45.0f;

    [SerializeField] 
    private float rotationSpeed = 5f;
    [SerializeField] 
    private float maxRotation = 15f;

    private float _currentRotation = 0f;
    private float _startRotation = 0f;
    private bool _postiveRotation = true;

    [SerializeField] private LayerMask stateMask;

    private void OnEnable()
    {
        StateSpaceManager.TurretStateUpdate += HandleRotation;
    }

    private void OnDisable()
    {
        StateSpaceManager.TurretStateUpdate -= HandleRotation;
    }

    private void CheckStates()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, losRange, stateMask);

        foreach (var hit in hits)
        {
            var state = hit.GetComponent<BaseState>();
            Vector3 dirToTarget = (hit.transform.position - transform.position);
            dirToTarget = new Vector3(dirToTarget.x, 0, dirToTarget.z).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) <= losAngle / 2)
            {
                if (!state.IsExposed)
                {
                    float distance = Vector3.Distance(transform.position, state.transform.position);

                    // Raycast to check if there is a wall or smoke in the way
                    RaycastHit hitInfo;
                    //First check wall
                    if (!Physics.Raycast(transform.position, dirToTarget,out hitInfo, distance, wallMask,
                            QueryTriggerInteraction.Collide))
                    {
                        // Now check smoke
                        if (Physics.Raycast(transform.position, dirToTarget,out hitInfo, distance, smokeMask,
                                QueryTriggerInteraction.Collide))
                        {
                            // Add smoked state
                            stateSpaceManager.numberOfExposedStatesSmoked++;
                        }
                        else
                        {
                            // Blocked by neither so mark exposed
                            state.IsExposed = true;
                        }
                        
                    }
                   
                }
            }
        }
        //Debug.Log($"Number of smoke covers: {stateSpaceManager.numberOfExposedStatesSmoked}");
    }
    
    private void Start()
    {
        _startRotation = transform.rotation.eulerAngles.y;
        if (Target == null)
        {
            Debug.LogWarning("Target not specified!");
        }
        if (LOSVisual == null)
        {
           Debug.LogWarning("No LOS Visual Component Assigned! Visuals disabled!");
        }
    }

    private void Update()
    {
        //HandleRotation();
        //CheckTarget();
       // CheckStates();
    }

    private void CheckTarget()
    {
        if (Target == null)
            return;

        Vector3 forward = transform.forward;
        Vector3 toTarget = (Target.transform.position - transform.position);
        float targetDistance = toTarget.magnitude;
        Vector3 toTargetDir = toTarget.normalized;
        float angle = Vector3.Angle(forward, toTargetDir);
        float halfAngle = losAngle / 2f;

        if (angle <= halfAngle && targetDistance <= losRange)
        {
            // Raycast but ONLY against the wall layer
            if (Physics.Raycast(transform.position, toTargetDir, out RaycastHit hitInfo, targetDistance, wallMask))
            {
                //Debug.Log("Blocked by wall!");
            }
            else
            {
               // Debug.Log("Target Found and Unobstructed!");
            }
        }
    }
    
    private void HandleRotation()
    {
        if (_postiveRotation)
        {
            _currentRotation += rotationSpeed;
            transform.Rotate(Vector3.up, rotationSpeed);
        }
        else
        {
            _currentRotation -= rotationSpeed;
            transform.Rotate(Vector3.up, -rotationSpeed);
        }


        if (_currentRotation > maxRotation)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.x, _startRotation + maxRotation, transform.rotation.z);
            _currentRotation = maxRotation;
            _postiveRotation = false;
        }

        if (_currentRotation < -maxRotation)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.x, _startRotation - maxRotation, transform.rotation.z);
            _currentRotation = -maxRotation;
            _postiveRotation = true;
        }
        
        CheckStates();
    }
    private void LateUpdate()
   {
       if (LOSVisual != null)
       {
           LOSVisual.GenerateLOSCone(losAngle, losRange);
       }
   }
}
