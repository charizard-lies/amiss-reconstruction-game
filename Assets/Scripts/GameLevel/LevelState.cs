using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

[System.Serializable]
public class LevelState
{
    public string levelIndex;
    public int activeCardId;
    public bool solved;
    
    public List<CardState> cardStates = new List<CardState>();
}

[System.Serializable]
public class CardState {
    public int id;
    public List<int> scramble = new List<int>();
    public List<int> drawnEdges = new List<int>();
}