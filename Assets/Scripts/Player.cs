using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int health = 3;
    [SerializeField] private float damageProb = 0.8f;

    private bool _isAlive = true;

    public void Heal()
    {
        health = math.min(3, health + 1);
        Debug.Log("Player Consumed Health Item.");
    }
    public void TakeDamage()
    {
        if (!_isAlive)
        {
            Debug.LogError("Agent Already Dead!");
            return;
        }
        bool takeDamage = Utilties.SimulateBernoulli(damageProb);
        if (takeDamage)
        {
            Debug.Log("Agent Took Damage");
            health = math.max(0, health - 1);
            if (health <= 0)
            {
                OnDeath();
            }
        }
        else
        {
            Debug.Log("Turret Attack Missed");
        }

    }

    public void OnDeath()
    {
        health = 0;
        _isAlive = false;
        Debug.Log("Your Agent just kicked the bucket :(");
    }

    public bool IsAlive()
    {
        return _isAlive;
    }
    
}
