using Clouds.Manager;
using Clouds.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Khởi động scene UINavigationTest khi bấm Play thẳng từ scene đó: tự nạp config (bình thường
    /// Bootstrap làm việc này) rồi mở màn ingame.
    /// </summary>
    public class IngameTestBoot : MonoBehaviour
    {
        private async UniTaskVoid Start()
        {
            if (!ConfigService.IsLoaded<UINavigatorConfig>()) await ConfigLoader.LoadAllAsync();
            await ScreenService.PushAsync<IngameScreen>();
        }
    }
}
