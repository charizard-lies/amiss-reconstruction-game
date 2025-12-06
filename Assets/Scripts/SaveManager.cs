using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static LevelState currentState;

    public static LevelState CurrentState
    {
        get { return currentState; }
        set { currentState = value; }
    }

    private static string SavePath(string levelIndex)
    {
        return Application.persistentDataPath + $"/level_{levelIndex}.json";
    }
    public static void Save(string levelIndex)
    {
        currentState.EnsureList();
        foreach(var kv in currentState.idToCardStatesMap)
        {
            kv.Value.EnsureList();
        } 
        string json = JsonUtility.ToJson(currentState, true);
        File.WriteAllText(SavePath(levelIndex), json);
    }
    public static LevelState Load(string levelIndex)
    {
        string path = SavePath(levelIndex);
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        var state = JsonUtility.FromJson<LevelState>(json);

        state.EnsureDict();
        foreach (var kv in state.idToCardStatesMap) kv.Value.EnsureDict();

        return state;
    }

    public static void Delete(string levelIndex)
    {
        if (File.Exists(SavePath(levelIndex)))
        {
            File.Delete(SavePath(levelIndex));
            Debug.Log("Save deleted.");
        }
    }
}
