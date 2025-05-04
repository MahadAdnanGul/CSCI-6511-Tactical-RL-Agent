using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private TrainerSettings settings;

    [SerializeField] private Button testButton;

    [SerializeField] private Button evalButton;

    [SerializeField] private Button manualButton;

    public void OnTestButton()
    {
        settings.debugLevel = DebugLevel.None;
        settings.trainingMode = true;
        settings.stepDelay = 0.125f;
        settings.visualizeTraining = true;
        SceneManager.LoadScene(1);
    }

    public void OnEvalButton()
    {
        settings.debugLevel = DebugLevel.None;
        settings.trainingMode = true;
        settings.stepDelay = 0;
        settings.visualizeTraining = false;
        settings.epsilon = 0;
        settings.learningRate = 0;
        settings.ignoreSave = true;
        settings.evalMode = true;
        SceneManager.LoadScene(1);
    }

    public void OnManualButton()
    {
        settings.debugLevel = DebugLevel.Basic;
        settings.trainingMode = false;
        settings.stepDelay = 0.25f;
        settings.ignoreSave = true;
        SceneManager.LoadScene(1);
    }
    
    
}
