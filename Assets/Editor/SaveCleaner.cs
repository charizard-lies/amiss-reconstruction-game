using UnityEditor;
using UnityEngine;
using System.IO;

public static class SaveCleaner
{
    [MenuItem("Tools/Clear All Saved Levels")]
    public static void ClearAllLevelSaves()
    {
        string folder = Application.persistentDataPath;
        string[] files = Directory.GetFiles(folder, "level_*.json");

        int count = 0;
        foreach (string file in files)
        {
            File.Delete(file);
            count++;
        }

        Debug.Log($"Deleted {count} saved level files from: {folder}");
    }
}
