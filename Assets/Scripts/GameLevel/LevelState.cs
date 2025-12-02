using UnityEngine;
using System.Collections.Generic;
using System.Data;
using System.Linq;

[System.Serializable]
public class LevelState
{
    public string levelIndex;
    public int activeCardId;
    public List<idCardStatePair> idCardStatePairs = new List<idCardStatePair>();

    [System.NonSerialized]
    public Dictionary<int, CardState> idToCardStatesMap = new Dictionary<int, CardState>();

    public void EnsureList()
    {
        idCardStatePairs = idToCardStatesMap
            .Select(kv => new idCardStatePair { cardId = kv.Key, cardState = kv.Value })
            .ToList();
    }
    
    public void EnsureDict()
    {
        idToCardStatesMap = idCardStatePairs.ToDictionary(p => p.cardId, p => p.cardState);
    }
}

[System.Serializable]
public class idCardStatePair
{
    public int cardId;
    public CardState cardState;
}