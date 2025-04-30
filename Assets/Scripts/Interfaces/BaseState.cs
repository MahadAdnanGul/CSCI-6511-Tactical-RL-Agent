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
        public bool IsGoal { get; set; }
        public bool ContainsHealth { get; set; }

        public abstract void UpdateState();
    }
}
