using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Clouds.Manager
{
    // Biến thư mục Assets/Game.Addressable/ thành nguồn sự thật của AssetService: thả asset vào thư mục
    // là nó nạp được bằng AssetService.LoadAsync với key = tên file, kéo ra khỏi thư mục là hết. Không
    // phải mở cửa sổ Addressables, không phải tự gõ address.
    //
    // Vì sao phải có tool: Addressables không tự biết tới thư mục nào cả. Muốn nạp được ở runtime thì
    // asset BUỘC phải có một entry trong Addressables với một address cụ thể — group và thư mục đều là
    // khái niệm chỉ tồn tại lúc Editor. Tool này bắc cầu: nó dựng đúng những entry đó theo nội dung thư
    // mục, để address trở thành chi tiết ẩn mà người dùng không cần đụng tới.
    //
    // Cùng triết lý với ConfigGroupLabeler nhưng khác điểm neo: bên đó lấy GROUP làm nguồn sự thật rồi
    // đồng bộ label, bên này lấy THƯ MỤC làm nguồn sự thật rồi đồng bộ entry. Khác vậy vì AssetService
    // nạp theo address chứ không theo label, nên tên file là thứ duy nhất người dùng cần biết.
    //
    // Đồng bộ 2 chiều: asset trong thư mục thì THÊM/sửa entry cho đúng tên file, entry trong group mà
    // asset đã rời thư mục thì GỠ. Không gỡ thì asset kéo ra ngoài vẫn nạp được như bóng ma, và
    // "thư mục là nguồn sự thật" chỉ đúng một nửa.
    [InitializeOnLoad]
    public static class AddressableFolderSync
    {
        // Thư mục, group Addressables và (qua AssetService) khái niệm mà người dùng nhìn thấy đều trùng
        // một cái tên — một khái niệm, một cái tên.
        public const string GROUP_NAME = AssetService.FOLDER_NAME;
        public const string FOLDER_PATH = "Assets/" + AssetService.FOLDER_NAME;

        static AddressableFolderSync()
        {
            // Hoãn 1 nhịp: lúc [InitializeOnLoad] chạy thì AssetDatabase và
            // AddressableAssetSettingsDefaultObject có thể chưa sẵn sàng.
            EditorApplication.delayCall += () => Sync(logResult: false);
        }

        [MenuItem("Tools/Clouds/Sync Addressable Folder")]
        private static void SyncFromMenu() => Sync(logResult: true);

        // Gọi lại mỗi khi có asset ra/vào thư mục — đây là thứ làm nên trải nghiệm "thả file vào là chạy".
        internal static void OnFolderChanged() => EditorApplication.delayCall += () => Sync(logResult: false);

        private static void Sync(bool logResult)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                if (logResult)
                    Debug.LogError("[AddressableFolderSync] Addressables chưa được khởi tạo (Window > Asset Management > Addressables > Groups).");
                return;
            }

            if (!AssetDatabase.IsValidFolder(FOLDER_PATH))
            {
                if (logResult)
                    Debug.LogWarning($"[AddressableFolderSync] Chưa có thư mục {FOLDER_PATH}. Tạo thư mục đó rồi thả asset vào.");
                return;
            }

            // So sánh bằng == chứ không dùng ?? : AddressableAssetGroup là UnityEngine.Object, chỉ toán
            // tử == của Unity mới coi object đã bị huỷ là null.
            AddressableAssetGroup group = settings.FindGroup(GROUP_NAME);
            if (group == null) group = CreateGroup(settings);
            if (group == null) return;

            Dictionary<string, string> wanted = CollectFolderAssets();

            // Gỡ trước, thêm sau: file vừa được đổi tên sẽ rơi vào cả hai vế (entry cũ đã lạc address,
            // asset mới cần entry), làm ngược thứ tự thì bước gỡ xoá luôn entry vừa thêm.
            int removed = RemoveStrayEntries(settings, group, wanted);
            int added = 0;
            int readdressed = 0;

            foreach (KeyValuePair<string, string> pair in wanted)
            {
                AddressableAssetEntry entry = group.GetAssetEntry(pair.Key);
                if (entry == null)
                {
                    // CreateOrMoveEntry: asset đang nằm ở group khác thì bị KÉO về đây, đúng tinh thần
                    // thư mục là nguồn sự thật.
                    entry = settings.CreateOrMoveEntry(pair.Key, group, false, false);
                    if (entry == null) continue;
                    added++;
                }

                if (entry.address != pair.Value)
                {
                    entry.SetAddress(pair.Value, false);
                    readdressed++;
                }
            }

            if (added > 0 || removed > 0 || readdressed > 0)
            {
                EditorUtility.SetDirty(settings);
                Debug.Log($"[AddressableFolderSync] Đồng bộ group '{GROUP_NAME}' theo {FOLDER_PATH}: +{added} asset, -{removed} asset, {readdressed} address được sửa.");
            }
            else if (logResult)
            {
                Debug.Log($"[AddressableFolderSync] Group '{GROUP_NAME}' đã khớp với {FOLDER_PATH} ({wanted.Count} asset), không có gì để đổi.");
            }
        }

        // GUID -> address mong muốn. Address là TÊN FILE không đuôi, kể cả với asset nằm trong thư mục
        // con: người dùng nhớ tên file chứ không nhớ đường dẫn. Đổi lại, hai file trùng tên ở hai thư
        // mục con khác nhau là xung đột thật — cảnh báo chứ không im lặng cho một cái đè cái kia.
        private static Dictionary<string, string> CollectFolderAssets()
        {
            var wanted = new Dictionary<string, string>();
            var addressOwners = new Dictionary<string, string>();

            foreach (string guid in AssetDatabase.FindAssets("t:Object", new[] { FOLDER_PATH }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path)) continue;

                string address = Path.GetFileNameWithoutExtension(path);
                if (addressOwners.TryGetValue(address, out string owner))
                {
                    Debug.LogWarning($"[AddressableFolderSync] Bỏ qua '{path}': trùng tên với '{owner}', mà address phải là duy nhất. Đổi tên một trong hai file.");
                    continue;
                }

                addressOwners[address] = path;
                wanted[guid] = address;
            }

            return wanted;
        }

        private static int RemoveStrayEntries(AddressableAssetSettings settings, AddressableAssetGroup group, Dictionary<string, string> wanted)
        {
            // Chép ra list trước: RemoveAssetEntry sửa đúng bộ sưu tập đang duyệt.
            var stray = new List<string>();
            foreach (AddressableAssetEntry entry in group.entries)
            {
                if (entry != null && !wanted.ContainsKey(entry.guid)) stray.Add(entry.guid);
            }

            foreach (string guid in stray)
                settings.RemoveAssetEntry(guid, false);

            return stray.Count;
        }

        private static AddressableAssetGroup CreateGroup(AddressableAssetSettings settings)
        {
            AddressableAssetGroup group = settings.CreateGroup(
                GROUP_NAME,
                setAsDefaultGroup: false,
                readOnly: false,
                postEvent: false,
                schemasToCopy: null,
                typeof(BundledAssetGroupSchema),
                typeof(ContentUpdateGroupSchema));

            if (group == null)
                Debug.LogError($"[AddressableFolderSync] Không tạo được Addressables group '{GROUP_NAME}'.");

            return group;
        }
    }

    // Class riêng chứ không lồng trong AddressableFolderSync: Unity dò AssetPostprocessor bằng phản
    // chiếu và chỉ nhận class ở cấp ngoài cùng.
    internal class AddressableFolderPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] movedTo, string[] movedFrom)
        {
            if (Touches(imported) || Touches(deleted) || Touches(movedTo) || Touches(movedFrom))
                AddressableFolderSync.OnFolderChanged();
        }

        private static bool Touches(string[] paths)
        {
            foreach (string path in paths)
            {
                if (path.StartsWith(AddressableFolderSync.FOLDER_PATH + "/")) return true;
            }
            return false;
        }
    }
}
