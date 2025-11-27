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
        string json = JsonUtility.ToJson(currentState, true);
        File.WriteAllText(SavePath(levelIndex), json);
    }
    public static LevelState Load(string levelIndex)
    {
        string path = SavePath(levelIndex);

        if (!File.Exists(path))
            return null; // No save exists

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<LevelState>(json);
    }
}
