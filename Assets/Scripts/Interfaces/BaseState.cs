using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interfaces
{
    public abstract class BaseState : MonoBehaviour
    {
        public abstract Vector2Int GetPosition();
        public bool IsWall { get; set; } = false;
        public bool IsExposed { get; set; } = false;
        public bool IsGoal;
        public bool ContainsHealth;

        public abstract void UpdateState();
        public abstract void ResetState();
    }

    public struct StateValues
    {
        public bool IsWall;
        public bool IsExposed;
        public bool IsGoal;
        public bool ContainsHealth;
    }
}
