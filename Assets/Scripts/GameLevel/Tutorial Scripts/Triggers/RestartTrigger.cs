using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Triggers/Restart")]
public class RestartTrigger : TutorialTrigger
{
    public override void Subscribe(Action callback)
    {
        LevelScript.Instance.OnRestart += callback;
    }

    public override void Unsubscribe(Action callback)
    {
        LevelScript.Instance.OnRestart -= callback;
    }
}