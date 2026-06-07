using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.SceneManagement;

namespace BFunCoreKit.Editor
{
    public class TMPFontChecker : EditorWindow
    {
        [MenuItem("BFun/Tools/Check Missing TMP Fonts")]
        public static void CheckMissingFonts()
        {
            int missingCount = 0;
            int totalChecked = 0;

            // Lấy tất cả các GameObject trong Scene hiện tại (bao gồm cả các đối tượng bị ẩn)
            GameObject[] allObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            Debug.Log("<color=cyan>--- Bắt đầu kiểm tra Missing TMP Fonts ---</color>");

            foreach (GameObject root in allObjects)
            {
                // Tìm cả TMP thông thường và TMP UI
                TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
                
                foreach (var tmp in texts)
                {
                    totalChecked++;
                    bool isMissing = false;
                    string reason = "";

                    if (tmp.font == null)
                    {
                        isMissing = true;
                        reason = "Font is NULL";
                    }
                    else if (tmp.font.name == "LiberationSans SDF")
                    {
                        isMissing = true;
                        reason = "Using default 'LiberationSans SDF' (No Vietnamese support)";
                    }

                    if (isMissing)
                    {
                        Debug.LogError($"[{reason}] GameObject: <color=yellow>{GetGameObjectPath(tmp.gameObject)}</color>", tmp.gameObject);
                        missingCount++;
                    }
                }
            }

            Debug.Log($"<color=cyan>--- Kiểm tra hoàn tất ---</color>");
            Debug.Log($"Tổng số TMP đã check: {totalChecked}");
            if (missingCount > 0)
                Debug.LogWarning($"<color=red>Tìm thấy {missingCount} đối tượng bị thiếu Font!</color>");
            else
                Debug.Log("<color=green>Tuyệt vời! Không có đối tượng nào bị thiếu Font.</color>");
        }

        // Hàm bổ trợ để lấy đường dẫn đầy đủ của Object trong Hierarchy
        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }
            return path;
        }
    }
}