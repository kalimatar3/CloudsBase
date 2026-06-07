using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MissingScriptFinder : EditorWindow
{
    [MenuItem("Tools/Check Missing Scripts in Scene")]
    static void FindMissingScriptsInScene()
    {
        int goCount = 0;
        int componentsCount = 0;
        int missingCount = 0;

        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);

        foreach (GameObject go in allObjects)
        {
            goCount++;
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                componentsCount++;
                if (components[i] == null)
                {
                    missingCount++;
                    Debug.LogWarning($"⚠️ Missing script on: {GetFullPath(go)}", go);
                }
            }
        }

        Debug.Log($"✅ Scan complete. Checked {goCount} GameObjects, {componentsCount} Components, found {missingCount} missing scripts.");
    }

    static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform current = go.transform;
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }
        return path;
    }

    [MenuItem("Tools/Remove Missing Scripts in Scene")]
    static void RemoveMissingScriptsInScene()
    {
        int count = 0;
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);

        foreach (GameObject go in allObjects)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
            {
                Debug.Log($"🧹 Removed {removed} missing script(s) from: {GetFullPath(go)}", go);
                count += removed;
            }
        }

        Debug.Log($"✅ Done! Removed {count} missing scripts in total.");
    }

    // ============================
    // PROJECT PREFAB SCAN + REMOVE
    // ============================

    [MenuItem("Clouds/CheckMissingScripts (Project)")]
    static void CheckMissingScriptsInProject()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        int prefabCount = 0;
        int missingCount = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            prefabCount++;

            GameObject instance = PrefabUtility.LoadPrefabContents(path);
            bool hasMissing = false;

            Transform[] allTransforms = instance.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in allTransforms)
            {
                Component[] comps = t.GetComponents<Component>();
                foreach (var c in comps)
                {
                    if (c == null)
                    {
                        hasMissing = true;
                        missingCount++;
                        Debug.LogWarning($"⚠️ Missing Script in Prefab: {path} | Object: {GetFullPath(t.gameObject)}");
                    }
                }
            }

            PrefabUtility.UnloadPrefabContents(instance);
        }

        Debug.Log($"✅ Project Scan Done! Checked {prefabCount} prefabs. Found {missingCount} missing scripts.");
    }

    [MenuItem("Clouds/RemoveMissingScripts (Project)")]
    static void RemoveMissingScriptsInProject()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        int prefabCount = 0;
        int removedCount = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            GameObject instance = PrefabUtility.LoadPrefabContents(path);
            if (instance == null) continue;

            prefabCount++;

            Transform[] allTransforms = instance.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in allTransforms)
            {
                int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                if (removed > 0)
                {
                    removedCount += removed;
                    Debug.Log($"🧹 Removed {removed} missing script(s) in Prefab: {path} | Object: {GetFullPath(t.gameObject)}");
                }
            }

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Done! Checked {prefabCount} prefabs. Removed {removedCount} missing scripts in total.");
    }
}