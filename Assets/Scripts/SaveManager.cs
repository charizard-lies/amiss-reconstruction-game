using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;


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
        FileSync.Sync();
    }
    
    public static LevelState Load(string levelIndex)
    {
        string path = SavePath(levelIndex);
        if (!File.Exists(path)) return null;
        
        string json = File.ReadAllText(path);
        var state = JsonUtility.FromJson<LevelState>(json);

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

public static class FileSync {
    [DllImport("__Internal")]
    private static extern void SyncFs();

    public static void Sync() {
#if UNITY_WEBGL && !UNITY_EDITOR
        SyncFs();
#endif
    }
}
