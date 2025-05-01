using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interfaces
{
    public abstract class BaseState : MonoBehaviour
    {
        public abstract Vector3 GetPosition();
        public bool IsWall { get; set; }
        public bool IsExposed { get; set; }
        public bool IsGoal;
        public bool ContainsHealth;
        public bool IsSmoked { get; set; } = false;

        public abstract void UpdateState();
    }
}
