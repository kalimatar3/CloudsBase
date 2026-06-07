using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
[CustomEditor(typeof(Spawner<>), true)] // Tạo editor cho các class kế thừa Spawner<T>
public class SpawnerEnumGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        // Thêm nút để tạo Enum từ spawnableObjects
        if (GUILayout.Button("Generate Enum"))
        {
            GenerateEnumFromTransforms();
        }
    }

private void GenerateEnumFromTransforms()
    {
        List<string> enumNames = new List<string>();
        GameObject gameObject = target as GameObject;
        // Lấy tất cả các Transform con trong đối tượng cha
        foreach (Transform child in gameObject?.GetComponent<Transform>().Find("Prefabs"))
        {
            enumNames.Add(child.name.Replace(" ", "_")); // Xử lý các tên để hợp lệ trong enum
        }

        // Tạo mã enum từ danh sách tên
        string enumCode = "public enum " + target.name + "Enum" + "\n{\n";
        foreach (string name in enumNames)
        {
            enumCode += "\t" + name + ",\n";
        }
        enumCode += "}";

        // Lưu mã enum vào file
        string path = Application.dataPath + "/1_Main/SCRIPTS/World/Spawner" + "/ " + target.name + "Enum.cs";
        File.WriteAllText(path, enumCode);
        AssetDatabase.Refresh();
        Debug.Log("Enum generated successfully at: " + path);
    }
}
