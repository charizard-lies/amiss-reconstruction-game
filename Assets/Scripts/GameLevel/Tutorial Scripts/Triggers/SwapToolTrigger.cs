using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Triggers/SwapTool")]
public class SwapToolTrigger : TutorialTrigger
{
    public override void Subscribe(Action callback)
    {
        ToolManager.Instance.OnSwapTool += callback;
    }

    public override void Unsubscribe(Action callback)
    {
        ToolManager.Instance.OnSwapTool -= callback;
    }
}
