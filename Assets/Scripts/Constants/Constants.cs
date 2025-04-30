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
}
