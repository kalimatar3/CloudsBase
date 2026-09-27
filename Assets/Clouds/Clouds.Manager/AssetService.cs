using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Clouds.Manager
{
    // Nạp asset lẻ qua Addressables theo key. Dành cho NỘI DUNG không phải config: bộ màn chơi, bảng số
    // liệu, asset dữ liệu chỉ dùng ở một màn... — những thứ mà nạp hết ngay lúc boot là lãng phí.
    //
    // Khác ConfigService chỗ nào (hai cái bổ sung nhau, không thay thế nhau):
    //   - ConfigService: nạp HẾT lúc boot theo label, tra cứu ĐỒNG BỘ theo KIỂU, mỗi kiểu đúng 1 asset.
    //     Hợp với tinh chỉnh gameplay dùng chung — luôn cần tới, và luôn chỉ có đúng một bản.
    //   - AssetService: nạp KHI CẦN theo KEY, một kiểu có bao nhiêu asset cũng được, giải phóng được.
    //     Hợp với nội dung — nhiều asset cùng kiểu, mỗi lúc chỉ dùng tới một phần.
    //
    // QUY ƯỚC KEY: asset đặt trong thư mục Assets/Game.Addressable/ sẽ được AddressableFolderSync
    // (Editor) tự đăng ký vào Addressables với address = TÊN FILE. Nên key truyền vào đây chính là tên
    // file, không cần đuôi mở rộng, không cần đường dẫn, không phải mở cửa sổ Addressables lần nào.
    // Key nào khác cũng nạp được (đây là Addressables thuần), thư mục chỉ là đường dẫn thuận tiện.
    //
    // Cache theo key: gọi LoadAsync cùng một key nhiều lần chỉ tốn đúng 1 lần nạp, và nhiều lời gọi
    // song song cùng chờ trên MỘT handle chứ không mỗi lời gọi nạp một bản riêng.
    public static class AssetService
    {
        // Thư mục quy ước. Là hằng ở đây vì cả AddressableFolderSync lẫn thông báo lỗi bên dưới đều
        // phải nói về cùng một chỗ — chép tay 2 nơi thì sớm muộn lệch nhau.
        public const string FOLDER_NAME = "Game.Addressable";

        // Handle không generic: một key chỉ có một handle, không phân mảnh theo kiểu T mà người gọi
        // tình cờ yêu cầu.
        private static readonly Dictionary<string, AsyncOperationHandle> _handles = new();

        public static async UniTask<T> LoadAsync<T>(string key) where T : Object
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[AssetService] Key rỗng — không nạp được gì.");
                return null;
            }

            if (!_handles.TryGetValue(key, out AsyncOperationHandle handle))
            {
                handle = Addressables.LoadAssetAsync<Object>(key);
                _handles[key] = handle;
            }

            // Handle đã xong (lần nạp trước) thì không await gì cả — lời gọi trả về ngay trong frame
            // hiện tại, nên caller không cần tự cache lại kết quả cho khỏi trễ.
            //
            // Bắt exception chứ không chỉ xem Status: ToUniTask() NÉM khi thao tác hỏng (key sai thì
            // Addressables ném InvalidKeyException), nên không bọc try thì lỗi bay thẳng qua đầu người
            // gọi — mà người gọi thường .Forget(), tức chỉ còn lại một dòng unhandled exception thay vì
            // thông báo chỉ đúng chỗ sai.
            if (!handle.IsDone)
            {
                try
                {
                    await handle.ToUniTask();
                }
                catch (System.Exception e)
                {
                    LogLoadFailure(key, e.Message);
                    return null;
                }
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                // Tới được đây là handle hỏng từ lần gọi TRƯỚC (đã IsDone nên không await lần nữa). Cố ý
                // GIỮ handle hỏng lại trong cache: key sai là lỗi nội dung chứ không phải trục trặc tạm
                // thời, thử lại chỉ tổ nạp hỏng thêm lần nữa. Gỡ nó ra ở đây còn sinh nguy cơ Release
                // hai lần khi có nhiều lời gọi song song cùng chờ trên chính handle này.
                LogLoadFailure(key, handle.OperationException?.Message);
                return null;
            }

            if (handle.Result is T asset) return asset;

            Debug.LogError($"[AssetService] Asset '{key}' không phải kiểu {typeof(T).Name} (thực tế là {handle.Result?.GetType().Name ?? "null"}).");
            return null;
        }

        private static void LogLoadFailure(string key, string reason)
            => Debug.LogError($"[AssetService] Không nạp được asset với key '{key}'. Asset đã nằm trong Assets/{FOLDER_NAME}/ chưa? ({reason})");

        // Lấy asset đã nạp xong mà không cần await. false khi key chưa từng được nạp, đang nạp dở, hoặc
        // nạp hỏng — dùng cho code chạy trong Update, nơi không await được.
        public static bool TryGet<T>(string key, out T asset) where T : Object
        {
            asset = null;
            if (!_handles.TryGetValue(key, out AsyncOperationHandle handle)) return false;
            if (!handle.IsDone || handle.Status != AsyncOperationStatus.Succeeded) return false;

            asset = handle.Result as T;
            return asset != null;
        }

        public static bool IsLoaded(string key)
            => _handles.TryGetValue(key, out AsyncOperationHandle handle)
               && handle.IsDone && handle.Status == AsyncOperationStatus.Succeeded;

        // Trả asset về cho Addressables. Mọi tham chiếu tới asset đang giữ ở ngoài sẽ thành không hợp lệ
        // — chỉ gọi khi chắc chắn không còn ai dùng (đổi màn, thoát khu vực).
        public static void Release(string key)
        {
            if (!_handles.TryGetValue(key, out AsyncOperationHandle handle)) return;
            _handles.Remove(key);
            if (handle.IsValid()) Addressables.Release(handle);
        }

        public static void ReleaseAll()
        {
            foreach (AsyncOperationHandle handle in _handles.Values)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            _handles.Clear();
        }

#if UNITY_EDITOR
        // _handles là state C# thuần: tắt domain reload thì nó sống sót qua các lần Play và giữ lại
        // handle trỏ vào Addressables đã bị dọn từ lần chạy trước. Cùng lý do với hook reset trong
        // ConfigService/PoolService/SignalBus.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic() => _handles.Clear();
#endif
    }
}
