using System;
using UnityEngine;

[Serializable]
public struct StateActionPair : IEquatable<StateActionPair>
{
    public Vector2Int statePos;
    public PlayerActions action;

    public StateActionPair(Vector2Int pos, PlayerActions act)
    {
        statePos = pos;
        action = act;
    }

    public bool Equals(StateActionPair other)
    {
        return statePos.Equals(other.statePos) && action == other.action;
    }

    public override int GetHashCode()
    {
        return statePos.GetHashCode() ^ action.GetHashCode();
    }
}