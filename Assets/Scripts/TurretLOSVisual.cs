using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TurretLOSVisual : MonoBehaviour
{
    private Mesh mesh;

    [SerializeField] 
    [Range(1,100)]
    private int coneResolution = 20;
    
    [SerializeField]
    private LayerMask wallLayerMask;

    private float _range;
    private float _angle;

    private Vector3[] _vertices;
    private int[] _triangles;
    
    public float GetRange () => _range;
    public float GetAngle() => _angle;

    private void Start()
    {
        _vertices = new Vector3[coneResolution + 2]; // center + outer ring
        _triangles = new int[coneResolution * 3];
    }
    

    //Inspiration: Code Monkey
    public void GenerateLOSCone(float losAngle, float losRange)
    {
        _range = losAngle;
        _angle = losAngle;
        
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        // Center vertex
        _vertices[0] = Vector3.zero;
        float angleStep = losAngle / coneResolution;
        float halfAngle = losAngle / 2f;

        for (int i = 0; i <= coneResolution; i++)
        {
            float currentAngle = -halfAngle + i * angleStep;
            Quaternion rot = Quaternion.AngleAxis(currentAngle, transform.up);
            Vector3 dir = rot * Vector3.forward;
            Vector3 worldDir = transform.rotation * dir;

            Ray ray = new Ray(transform.position, worldDir);
            if (Physics.Raycast(ray, out RaycastHit hit, losRange, wallLayerMask))
            {
                _vertices[i + 1] = dir * hit.distance;
            }
            else
            {
                _vertices[i + 1] = dir * losRange;
            }
            
            if (i != coneResolution)
            {
                _triangles[i * 3 + 0] = 0;
                _triangles[i * 3 + 1] = i + 1;
                _triangles[i * 3 + 2] = i + 2;
            }
        }
        mesh.vertices = _vertices;
        mesh.triangles = _triangles;
        mesh.RecalculateNormals();
    }
}
