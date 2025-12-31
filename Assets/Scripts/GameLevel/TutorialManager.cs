using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private int stepIndex = 0;

    public void NextStep()
    {
        stepIndex++;
    }

}

// [CreateAssetMenu(fileName = "TutorialStep", menuName = "Tutorial/Step")]
// public class TutorialStep : ScriptableObject
// {
//     public List<string> texts = new();
//     public TutorialAction action;
// }

// [CreateAssetMenu(fileName = "TutorialAction", menuName = "Tutorial/Action")]
// public class TutorialAction : ScriptableObject
// {
//     public bool allowDraw;
//     public bool allowDelete;
//     public bool allowSwapTool;
//     public bool allowSwap;
//     public bool allowChangeCard;
//     public bool allowReset;

//     public void Execute()
//     {
//         TutorialGate.AllowDraw = allowDraw;
//         TutorialGate.AllowDelete = allowDelete;
//         TutorialGate.AllowSwapTool = allowSwapTool;
//         TutorialGate.AllowSwap = allowSwap;
//         TutorialGate.AllowChangeCard = allowChangeCard;
//         TutorialGate.AllowReset = allowReset;
//     }
// }

public static class TutorialGate
{
    public static bool AllowDrawLine = false;
    public static bool AllowEraseLine= false;
    public static bool AllowSwapTool = false;
    public static bool AllowSwapNode = false;
    public static bool AllowChangeCard = false;
    public static bool AllowRestart = false;
}