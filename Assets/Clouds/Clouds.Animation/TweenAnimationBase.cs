using System;
using System.Collections.Generic;
using Clouds.Manager;
using Clouds.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Clouds.Animation
{
    /// <summary>
    /// Khung dùng chung cho mọi tween animation gắn trên GameObject: đếm loop, đảo chiều Yoyo,
    /// bắn OnStart/OnComplete. Không đụng tới RectTransform hay Renderer — lớp con quyết định
    /// tween gắn vào đâu bằng BuildAnimations().
    /// </summary>
    public abstract class TweenAnimationBase : MyBehaviour, IUIAnimation
    {
        // Giữ nguyên tên field cũ của TweenCUIAnimation: prefab serialize theo tên field, đổi tên khi
        // dời field lên lớp cha là mọi prefab đang gán data sẽ về null.
        public UIAnimationData UIAnimationData;

        // Mặc định false để giữ nguyên hành vi cũ. Animation của popup nên bật: popup hay mở đúng lúc
        // game pause (timeScale = 0), tween theo thời gian thường sẽ đứng im và await nó treo luôn.
        [Tooltip("Chạy theo thời gian thực, không bị Time.timeScale ảnh hưởng (bật cho UI mở khi pause).")]
        public bool IgnoreTimeScale;

        /// <summary>Trạng thái target trước lần phát gần nhất, do AnimationService ghi. Xem Restore().</summary>
        [NonSerialized] public AnimationSnapshot Snapshot;

        protected readonly List<IUIAnimation> _animations = new();

        /// <summary>Tween đã dựng (dùng bởi editor preview, nơi cần chuẩn bị từng tween một).</summary>
        public IReadOnlyList<IUIAnimation> Animations => _animations;

        public bool IsPlaying
        {
            get { foreach (var a in _animations) if (a.IsPlaying) return true; return false; }
        }

        public object NativeAnimation => _animations.Count > 0 ? _animations[0].NativeAnimation : null;

        public float Duration => UIAnimationData != null ? UIAnimationData.GetTotalDuration() : 0f;

        public event Action OnComplete;
        public event Action OnStart;

        private int  _completedLoops;
        private bool _isReverseCycle;

        protected override void Awake()
        {
            base.Awake();
            Rebuild();
        }

        /// <summary>
        /// Dựng lại tween từ trạng thái hiện tại của target.
        /// Bắt buộc gọi khi object được lấy ra từ PoolService: tween chụp vị trí/màu làm mốc ngay lúc
        /// build, nên instance spawn lại ở chỗ khác mà không rebuild sẽ chạy về đúng toạ độ của lần
        /// spawn đầu tiên.
        /// </summary>
        public void Rebuild()
        {
            foreach (var anim in _animations) anim.Stop();
            _animations.Clear();
            BuildAnimations();
        }

        /// <summary>Đọc UIAnimationData và nạp tween đã dựng vào _animations.</summary>
        protected abstract void BuildAnimations();

        // Mọi cách phát đều đi qua AnimationService — kể cả nút Odin này và editor preview — để việc
        // chụp snapshot và quyết định dựng lại tween chỉ nằm ở đúng một chỗ.
        [Button(ButtonSizes.Large)]
        public void Play() => AnimationService.Play(this);

        public void PlayReverse() => AnimationService.PlayReverse(this);

        internal void PlayInternal()
        {
            if (_animations.Count == 0) { ReportEmpty(); return; }
            _completedLoops  = 0;
            _isReverseCycle  = false;
            HookOneShot(_animations[0], isStart: true, () => OnStart?.Invoke());
            PlayCycle();
        }

        private void PlayCycle()
        {
            HookOneShot(_animations[^1], isStart: false, OnCycleComplete);
            if (_isReverseCycle)
                foreach (var anim in _animations) anim.PlayReverse();
            else
                foreach (var anim in _animations) anim.Restart();
        }

        private void OnCycleComplete()
        {
            _completedLoops++;
            bool shouldLoop = UIAnimationData != null && UIAnimationData.Loop &&
                              (UIAnimationData.LoopCount <= 0 || _completedLoops < UIAnimationData.LoopCount);

            if (shouldLoop)
            {
                if (UIAnimationData.LoopMode == Clouds.UI.LoopType.Yoyo) _isReverseCycle = !_isReverseCycle;
                PlayCycle();
            }
            else
                OnComplete?.Invoke();
        }

        internal void PlayReverseInternal()
        {
            if (_animations.Count == 0) { ReportEmpty(); return; }
            _completedLoops = 0;
            _isReverseCycle = true;
            HookOneShot(_animations[0],  isStart: true,  () => OnStart?.Invoke());
            HookOneShot(_animations[^1], isStart: false, () => OnComplete?.Invoke());
            foreach (var anim in _animations) anim.PlayReverse();
        }

        public void Stop()
        {
            foreach (var anim in _animations) anim.Stop();
        }

        public void Restart() => Play();

        // Không có tween nào để chạy thì vẫn phải bắn đủ cặp event, giống UIAnimationContainer.Play()
        // khi key không tồn tại. Im lặng bỏ qua sẽ làm mọi await trên animation này treo vĩnh viễn, và
        // callback onComplete của caller cũng không bao giờ chạy.
        private void ReportEmpty()
        {
            OnStart?.Invoke();
            OnComplete?.Invoke();
        }

        protected static void HookOneShot(IUIAnimation target, bool isStart, Action callback)
        {
            if (isStart)
            {
                void Wrapper() { callback(); target.OnStart -= Wrapper; }
                target.OnStart += Wrapper;
            }
            else
            {
                void Wrapper() { callback(); target.OnComplete -= Wrapper; }
                target.OnComplete += Wrapper;
            }
        }
    }
}
