using UnityEngine;
using System.Collections.Generic;
using System.Data;

[System.Serializable]
public class LevelState
{
    public string levelIndex;
    public int activeCardId;
    public Dictionary<int, CardState> idToCardStatesMap = new();
}