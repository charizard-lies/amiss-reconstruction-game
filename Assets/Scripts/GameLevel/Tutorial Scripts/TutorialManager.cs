using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private int stepIndex = 0;
    private ITutorialStep current;
    public List<TutorialStepData> tutorialFlow = new();

    public void StartTutorial()
    {
        EnterStep(0);
    }

    void EnterStep(int i)
    {
        current?.Exit();
        stepIndex = i;

        if (stepIndex >= tutorialFlow.Count)
        {
            EndTutorial();
            return;
        }

        current = tutorialFlow[stepIndex].CreateRuntimeStep(this);
        current.Enter();
    }

    public void CompleteCurrentStep()
    {
        EnterStep(stepIndex + 1);
    }

    public void EndTutorial()
    {
        LevelScript.Instance.isTutorial = false;
    } 
}

public interface ITutorialStep
{
    void Enter();
    void Exit();
}

public class TutorialStep: ITutorialStep
{
    LevelScript levelManager => LevelScript.Instance;
    TutorialManager tutorialManager;
    TutorialStepData stepData;

    int textIndex;
    public TutorialStep(TutorialManager tutManager, TutorialStepData data)
    {
        tutorialManager = tutManager;
        stepData = data;
    }

    public void Enter()
    {
        textIndex = 0;
        ShowNextText();

        levelManager.OnTutorialTextClicked += ShowNextText;
    }
    public void Exit()
    {
        levelManager.OnTutorialTextClicked -= ShowNextText;
        stepData.trigger?.Unsubscribe(tutorialManager.CompleteCurrentStep);
    }

    void ShowNextText()
    {
        if(textIndex < stepData.slides.Count)
        {
            levelManager.ShowTutorialSlide(stepData.slides[textIndex]);
            textIndex++;
            return;
        }
        
        levelManager.CloseTutorialText();
        levelManager.OnTutorialTextClicked -= ShowNextText;
        if(stepData.trigger != null) stepData.trigger.Subscribe(tutorialManager.CompleteCurrentStep);
        else tutorialManager.CompleteCurrentStep();
    }
}
