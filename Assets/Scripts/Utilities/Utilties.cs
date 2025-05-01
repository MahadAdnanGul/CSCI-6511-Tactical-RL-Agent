using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilties
{
    public const float EPSILON = 1e-5f;
    public static bool IsFloatEqual(this float a, float b, float epsilon = EPSILON)
    {
        return Mathf.Abs(a - b) < epsilon;
    }

    public static bool SimulateBernoulli(float successProb)
    {
        float random = Random.Range(0f,1f);
        return random <= successProb;
    }
    
    public static void DebugDrawBox(Vector3 center, Vector3 halfExtents, Quaternion rotation, Color color, float duration = 1f)
    {
        Vector3[] corners = new Vector3[8];

        Vector3 right = rotation * Vector3.right * halfExtents.x;
        Vector3 up = rotation * Vector3.up * halfExtents.y;
        Vector3 forward = rotation * Vector3.forward * halfExtents.z;
        
        corners[0] = center + right + up + forward;
        corners[1] = center + right + up - forward;
        corners[2] = center + right - up + forward;
        corners[3] = center + right - up - forward;
        corners[4] = center - right + up + forward;
        corners[5] = center - right + up - forward;
        corners[6] = center - right - up + forward;
        corners[7] = center - right - up - forward;
        
        Debug.DrawLine(corners[0], corners[1], color, duration);
        Debug.DrawLine(corners[0], corners[2], color, duration);
        Debug.DrawLine(corners[0], corners[4], color, duration);

        Debug.DrawLine(corners[7], corners[6], color, duration);
        Debug.DrawLine(corners[7], corners[5], color, duration);
        Debug.DrawLine(corners[7], corners[3], color, duration);

        Debug.DrawLine(corners[1], corners[3], color, duration);
        Debug.DrawLine(corners[1], corners[5], color, duration);

        Debug.DrawLine(corners[2], corners[3], color, duration);
        Debug.DrawLine(corners[2], corners[6], color, duration);

        Debug.DrawLine(corners[4], corners[5], color, duration);
        Debug.DrawLine(corners[4], corners[6], color, duration);
    }
}
