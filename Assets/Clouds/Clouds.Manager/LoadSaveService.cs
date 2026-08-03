using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
using Clouds.Data;

namespace Clouds.Manager
{
    public static class LoadSaveService
    {
        public static T JsonToData<T>(string jsonString) where T : new()
        {
            try
            {
                T data = JsonUtility.FromJson<T>(jsonString);
                return data != null ? data : new T();
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoadSaveService] Failed to parse JSON for {typeof(T)}: {e}");
                return new T();
            }
        }

        public static bool SaveDatatofile(string filename, DynamicData dynamicData)
        {
            string filePath = Path.Combine(Application.persistentDataPath, filename);
            try
            {
                string jsonString = JsonUtility.ToJson(dynamicData);
                File.WriteAllText(filePath, jsonString);
                Debug.Log(dynamicData + " saved to: " + filePath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoadSaveService] Failed to save {filename}: {e}");
                return false;
            }
        }

        public static bool LoadDataFromFile<T>(string filename, out T outdata) where T : DynamicData, new()
        {
            string filePath = Path.Combine(Application.persistentDataPath, filename);
            try
            {
                if (!File.Exists(filePath))
                {
                    outdata = new T();
                    return false;
                }
                string jsonString = File.ReadAllText(filePath);
                T data = JsonUtility.FromJson<T>(jsonString);
                outdata = data != null ? data : new T();
                return data != null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoadSaveService] Failed to load {filename}: {e}");
                outdata = new T();
                return false;
            }
        }

        public static string DataToJson<T>(T dynamicData) where T : DynamicData
        {
            return JsonUtility.ToJson(dynamicData);
        }

        public static string DatatoJsonConvert<T>(T dynamicData) where T : DynamicData
        {
            return JsonConvert.SerializeObject(dynamicData);
        }

        public static bool LoadDataFromJson<T>(string jsonString, T originData, out T outdata)
        {
            try
            {
                T data = JsonUtility.FromJson<T>(jsonString);
                outdata = data != null ? data : originData;
                return data != null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoadSaveService] Failed to parse JSON for {typeof(T)}: {e}");
                outdata = originData;
                return false;
            }
        }

        public static bool LoadDataFromPlayerPref<T>(string playerPrefName, T originData, out T outdata)
        {
            try
            {
                string jsonString = PlayerPrefs.GetString(playerPrefName);
                T data = JsonUtility.FromJson<T>(jsonString);
                outdata = data != null ? data : originData;
                return data != null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoadSaveService] Failed to parse PlayerPref '{playerPrefName}' for {typeof(T)}: {e}");
                outdata = originData;
                return false;
            }
        }

        public static bool SaveDataToPlayerPref(string playerPrefName, DynamicData dynamicData)
        {
            try
            {
                string jsonString = JsonUtility.ToJson(dynamicData);
                PlayerPrefs.SetString(playerPrefName, jsonString);
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoadSaveService] Failed to save PlayerPref '{playerPrefName}': {e}");
                return false;
            }
        }

        public static bool LoadDataFromJsonConvert<T>(string jsonString, T originData, out T outdata)
        {
            try
            {
                T data = string.IsNullOrEmpty(jsonString) ? default : JsonConvert.DeserializeObject<T>(jsonString);
                outdata = data != null ? data : originData;
                return data != null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoadSaveService] Failed to parse JSON for {typeof(T)}: {e}");
                outdata = originData;
                return false;
            }
        }
    }
}
