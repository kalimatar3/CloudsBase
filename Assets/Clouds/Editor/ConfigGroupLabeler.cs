using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Clouds.Manager
{
    // Làm cho Addressables group tên "Game.Config" trở thành nguồn sự thật của ConfigService: kéo một
    // config SO vào group đó là nó được đăng ký, kéo ra là hết — không phải nhớ gán label thủ công.
    //
    // Vì sao cần tool này: runtime KHÔNG đọc được group. Addressables chỉ nhận address, label,
    // AssetReference hoặc GUID làm key; group thuần là khái niệm build-time quyết định asset nằm ở
    // bundle nào, và tên group không tồn tại lúc game chạy. Nên ConfigLoader buộc phải load theo label.
    // Tool này bắc cầu: ở Editor, nó đồng bộ label ConfigLoader.LABEL đúng theo thành viên của group,
    // biến label thành chi tiết ẩn mà người dùng không cần đụng tới.
    //
    // Đồng bộ theo 2 chiều: entry trong group thì THÊM label, entry ngoài group mà còn dính label thì
    // GỠ label. Không gỡ thì asset từng kéo ra khỏi group vẫn tiếp tục được nạp như bóng ma, và
    // "group là nguồn sự thật" chỉ đúng một nửa.
    [InitializeOnLoad]
    public static class ConfigGroupLabeler
    {
        // Group trùng tên với label và với thư mục Assets/Game.Config/ — một khái niệm, một cái tên.
        public const string GROUP_NAME = ConfigLoader.LABEL;

        // Chống đệ quy: chính việc gắn/gỡ label lại phát ra ModificationEvent.
        private static bool _isSyncing;

        static ConfigGroupLabeler()
        {
            AddressableAssetSettings.OnModificationGlobal -= OnSettingsModified;
            AddressableAssetSettings.OnModificationGlobal += OnSettingsModified;
        }

        private static void OnSettingsModified(AddressableAssetSettings settings, AddressableAssetSettings.ModificationEvent modificationEvent, object data)
        {
            if (_isSyncing) return;

            switch (modificationEvent)
            {
                case AddressableAssetSettings.ModificationEvent.EntryCreated:
                case AddressableAssetSettings.ModificationEvent.EntryAdded:
                case AddressableAssetSettings.ModificationEvent.EntryMoved:
                case AddressableAssetSettings.ModificationEvent.EntryRemoved:
                    Sync(settings, logResult: false);
                    break;
            }
        }

        [MenuItem("Tools/Clouds/Sync Config Group Labels")]
        private static void SyncFromMenu()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[ConfigGroupLabeler] Addressables chưa được khởi tạo (Window > Asset Management > Addressables > Groups).");
                return;
            }

            Sync(settings, logResult: true);
        }

        private static void Sync(AddressableAssetSettings settings, bool logResult)
        {
            if (settings == null) return;

            AddressableAssetGroup group = settings.FindGroup(GROUP_NAME);
            if (group == null)
            {
                if (logResult)
                    Debug.LogWarning($"[ConfigGroupLabeler] Chưa có Addressables group tên '{GROUP_NAME}'. Tạo group đó rồi kéo các config SO vào.");
                return;
            }

            _isSyncing = true;
            try
            {
                settings.AddLabel(ConfigLoader.LABEL, false);

                var inGroup = new HashSet<AddressableAssetEntry>(group.entries);
                int labeled = 0;
                int unlabeled = 0;

                foreach (AddressableAssetEntry entry in inGroup)
                {
                    // postEvent: false để việc gắn label không kích hoạt lại chính callback này.
                    if (entry.SetLabel(ConfigLoader.LABEL, true, false, false)) labeled++;
                }

                foreach (AddressableAssetGroup otherGroup in settings.groups)
                {
                    if (otherGroup == null || otherGroup == group) continue;

                    foreach (AddressableAssetEntry entry in otherGroup.entries)
                    {
                        if (entry.SetLabel(ConfigLoader.LABEL, false, false, false)) unlabeled++;
                    }
                }

                if (labeled > 0 || unlabeled > 0)
                {
                    EditorUtility.SetDirty(settings);
                    Debug.Log($"[ConfigGroupLabeler] Đồng bộ label '{ConfigLoader.LABEL}': +{labeled} trong group '{GROUP_NAME}', -{unlabeled} ngoài group.");
                }
                else if (logResult)
                {
                    Debug.Log($"[ConfigGroupLabeler] Label '{ConfigLoader.LABEL}' đã khớp với group '{GROUP_NAME}' ({inGroup.Count} asset), không có gì để đổi.");
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }
}
