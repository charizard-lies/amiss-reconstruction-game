using System;
using UnityEngine;

public abstract class TutorialTrigger : ScriptableObject
{
    public abstract void Subscribe(Action callback);
    public abstract void Unsubscribe(Action callback);
}