using UnityEditor;
using UnityEngine;

public class MissingScriptCleaner
{
    [MenuItem("Tools/Cleanup/Remove Missing Scripts in Scene")]
    static void RemoveMissingScripts()
    {
        int count = 0;
        foreach (GameObject go in Object.FindObjectsOfType<GameObject>(true))
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
            {
                count += removed;
                Debug.Log($"Removed {removed} missing script(s) from GameObject: {go.name}");
            }
        }

        Debug.Log($"Total missing scripts removed: {count}");
    }
}