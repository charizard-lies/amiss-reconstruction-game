using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Triggers/SwapNode")]
public class SwapNodeTrigger : TutorialTrigger
{
    public override void Subscribe(Action callback)
    {
        ToolManager.Instance.OnSwapNode += callback;
    }

    public override void Unsubscribe(Action callback)
    {
        ToolManager.Instance.OnSwapNode -= callback;
    }
}
