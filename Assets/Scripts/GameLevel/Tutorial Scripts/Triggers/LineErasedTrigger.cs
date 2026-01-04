using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Triggers/LineErased")]
public class LineErasedTrigger : TutorialTrigger
{
    public override void Subscribe(Action callback)
    {
        ToolManager.Instance.OnLineErased += callback;
    }

    public override void Unsubscribe(Action callback)
    {
        ToolManager.Instance.OnLineErased -= callback;
    }
}
