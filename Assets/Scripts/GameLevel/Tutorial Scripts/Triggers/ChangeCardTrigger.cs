using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Triggers/ChangeCard")]
public class ChangeCardTrigger : TutorialTrigger
{
    public override void Subscribe(Action callback)
    {
        LevelScript.Instance.OnCardChanged += callback;
    }

    public override void Unsubscribe(Action callback)
    {
        LevelScript.Instance.OnCardChanged -= callback;
    }
}
