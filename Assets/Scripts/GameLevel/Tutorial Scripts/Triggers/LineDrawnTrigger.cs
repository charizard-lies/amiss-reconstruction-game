using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Triggers/LineDrawn")]
public class LineDrawnTrigger : TutorialTrigger
{
    public override void Subscribe(Action callback)
    {
        ToolManager.Instance.OnLineDrawn += callback;
    }

    public override void Unsubscribe(Action callback)
    {
        ToolManager.Instance.OnLineDrawn -= callback;
    }
}
