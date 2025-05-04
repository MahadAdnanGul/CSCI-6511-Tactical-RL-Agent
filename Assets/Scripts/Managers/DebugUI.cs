using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpText;

    public void ShowDebugMessage(string message)
    {
        tmpText.text = message;
    }
}
