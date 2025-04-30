using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int health = 3;
    [SerializeField] private float damageProb = 0.8f;

    private bool _isAlive = true;

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
            health--;
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
