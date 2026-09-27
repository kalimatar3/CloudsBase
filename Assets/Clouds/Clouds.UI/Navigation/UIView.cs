using System;
using Clouds.Animation;
using Clouds.Manager;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Clouds.UI
{
    /// <summary>
    /// View nhận tham số có kiểu. Service gọi SetArgs() trước OnWillShowAsync(), nên sai kiểu là lỗi
    /// biên dịch chứ không phải lỗi ép kiểu lúc chạy:
    ///   await PopupService.ShowAsync&lt;ConfirmPopup, ConfirmArgs&gt;(args);
    /// </summary>
    public interface IViewArgs<in TArgs>
    {
        void SetArgs(TArgs args);
    }

    public enum UIViewState { Hidden, Showing, Shown, Hiding }

    /// <summary>
    /// Lớp gốc của ScreenView, PopupView, ActivityView. Layer sở hữu vòng đời; view chỉ override hook:
    ///
    ///   OnWillShowAsync → [transition Show] → OnDidShow → … → OnWillHideAsync → [transition Hide] → OnDidHide
    ///
    /// Transition Show/Hide: key "Show"/"Hide" của UIAnimationSequencer trên root nếu có, không thì
    /// transition mặc định trong UINavigatorConfig.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public abstract class UIView : MyBehaviour
    {
        [Tooltip("Key khi view được đặt sẵn trong layer ở scene. Để trống = tên GameObject. " +
                 "View nạp từ Addressables thì key chính là address đã dùng để nạp.")]
        [SerializeField] private string _sceneKey;

        private CanvasGroup _canvasGroup;
        private UIAnimationSequencer _sequencer;
        private UniTaskCompletionSource _hiddenSource;
        private AnimationSnapshot _hideSnapshot;

        public string Key { get; internal set; }
        public UILayer Layer { get; internal set; }
        public UIViewState State { get; private set; } = UIViewState.Hidden;

        internal bool IsSceneResident { get; set; }
        internal string SceneKey => string.IsNullOrEmpty(_sceneKey) ? gameObject.name : _sceneKey;

        // Lấy lười chứ không đợi LoadComponents: view đặt sẵn trong scene bị layer tắt trước khi Awake
        // kịp chạy, mà layer cần CanvasGroup ngay lần hiện đầu tiên.
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        public UIAnimationSequencer Sequencer
        {
            get
            {
                if (_sequencer == null) _sequencer = GetComponent<UIAnimationSequencer>();
                return _sequencer;
            }
        }

        public event Action<UIView> Shown;
        public event Action<UIView> Hidden;

        // ── Hook cho lớp con ─────────────────────────────────────────────────────
        // KHÔNG await một lệnh điều hướng khác của CÙNG layer trong các hook async: layer xử lý tuần tự
        // và đang đợi chính hook này, nên hai bên chờ nhau mãi. Cần mở tiếp thì gọi .Forget() hoặc làm
        // trong OnDidShow/OnDidHide.

        /// <summary>View đã active nhưng còn trong suốt. Bind dữ liệu ở đây.</summary>
        protected virtual UniTask OnWillShowAsync() => UniTask.CompletedTask;
        protected virtual void OnDidShow() { }
        protected virtual UniTask OnWillHideAsync() => UniTask.CompletedTask;
        /// <summary>Transition Hide đã xong, view sắp bị tắt/trả về pool/huỷ.</summary>
        protected virtual void OnDidHide() { }

        // ── API ──────────────────────────────────────────────────────────────────

        /// <summary>Đóng view này: Popup đóng, Screen pop (hoặc rút khỏi lịch sử), Activity ẩn.</summary>
        public UniTask CloseAsync() => Layer != null ? Layer.CloseAsync(this) : UniTask.CompletedTask;

        /// <summary>Đợi tới khi view ẩn hẳn. Trả kết quả từ popup: set field rồi CloseAsync(), bên gọi đọc sau khi await.</summary>
        public UniTask WaitHiddenAsync() => _hiddenSource != null ? _hiddenSource.Task : UniTask.CompletedTask;

        // ── Vòng đời, do layer gọi ───────────────────────────────────────────────

        internal void PrepareShow()
        {
            State = UIViewState.Showing;
            _hiddenSource ??= new UniTaskCompletionSource();
            CanvasGroup.alpha = 0f;
            gameObject.SetActive(true);
        }

        internal UniTask WillShowAsync() => OnWillShowAsync();

        internal void DidShow()
        {
            State = UIViewState.Shown;
            OnDidShow();
            Shown?.Invoke(this);
        }

        internal UniTask WillHideAsync()
        {
            State = UIViewState.Hiding;
            return OnWillHideAsync();
        }

        /// <summary>Hook OnDidHide, đưa object về trạng thái trước Hide, rồi báo cho người đang đợi.</summary>
        internal void DidHide()
        {
            State = UIViewState.Hidden;
            OnDidHide();
            RestoreAfterHide();
            Hidden?.Invoke(this);

            UniTaskCompletionSource source = _hiddenSource;
            _hiddenSource = null;
            source?.TrySetResult();
        }

        internal async UniTask PlayTransitionAsync(bool show, UIAnimationData fallback, bool ignoreTimeScale)
        {
            string key = show ? UIAnimationSequencer.SHOW : UIAnimationSequencer.HIDE;
            if (show) CanvasGroup.alpha = 1f;
            else      _hideSnapshot = AnimationSnapshot.Capture(gameObject, AnimationSnapshot.ChannelsOf(fallback));

            if (Sequencer != null && Sequencer.HasKey(key))
                await Sequencer.PlayAsync(key, destroyCancellationToken);
            else
                await UITransition.PlayAsync(fallback, gameObject, ignoreTimeScale, destroyCancellationToken);
        }

        // Hide để object ở trạng thái đã ẩn (alpha 0, thu nhỏ…). View lấy lại từ pool hay view đặt sẵn
        // trong scene sẽ mở lần sau từ chính trạng thái đó nếu không kéo về — và transition Show chỉ chạm
        // những kênh nó cần, kênh còn lại vẫn kẹt ở giá trị của Hide.
        private void RestoreAfterHide()
        {
            if (Sequencer != null && Sequencer.HasKey(UIAnimationSequencer.HIDE))
                Sequencer.Restore(UIAnimationSequencer.HIDE);
            else
                _hideSnapshot.Restore(gameObject);
            _hideSnapshot = default;
        }
    }
}
