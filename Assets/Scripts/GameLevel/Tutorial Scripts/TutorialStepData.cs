using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStepData", menuName = "Tutorial/StepData")]
public class TutorialStepData : ScriptableObject
{
    public List<TutorialSlide> slides;
    public TutorialTrigger trigger;

    public ITutorialStep CreateRuntimeStep(TutorialManager manager)
    {
        return new TutorialStep(manager, this);
    }
}

[System.Serializable]
public class TutorialSlide
{
    [TextArea(3,10)]
    public string text;
    public Sprite image;
}