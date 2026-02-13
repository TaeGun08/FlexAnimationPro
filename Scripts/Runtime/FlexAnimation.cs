using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace FlexAnimation
{
    public class FlexAnimation : MonoBehaviour
    {
        [SerializeReference]
        public List<AnimationModule> modules = new List<AnimationModule>();
        
        public bool playOnEnable = true;
        public FlexAnimationPreset preset;
        public float timeScale = 1f;
        public bool ignoreTimeScale = false;

        public UnityEngine.Events.UnityEvent OnPlay;
        public UnityEngine.Events.UnityEvent OnComplete;

        public bool IsPlaying => _isPlaying;
        public bool IsPaused => _isPaused;

        private List<RoutineRunner> _runners = new List<RoutineRunner>(16);
        private bool _isPlaying = false;
        private bool _isPaused = false;

        // State Backup
        private bool _hasSavedState = false;
        private Vector3 _initPos, _initScale;
        private Quaternion _initRot;
        private Vector2 _initAnchoredPos, _initSizeDelta;
        private float _initAlpha = 1f;
        private Color _initColor = Color.white;
        private bool _hasCanvasGroup, _hasGraphic, _hasSpriteRenderer;

        private void OnEnable() { if (playOnEnable) PlayAll(); }
        private void OnDisable() { StopAndReset(); }

        private void Update()
        {
            if (!_isPlaying || _isPaused) return;
            float dt = (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * timeScale;
            ProcessRunners(dt);
        }

        public void EditorPreviewUpdate(float deltaTime)
        {
            if (!_isPlaying || _isPaused) return;
            global::FlexAnimation.Internal.FlexTween.OverrideDeltaTime = deltaTime * timeScale;
            ProcessRunners(deltaTime * timeScale);
            global::FlexAnimation.Internal.FlexTween.OverrideDeltaTime = null;
        }

        public void PlayAll()
        {
            StopAndReset();
            SaveInitialState();
            
            var targetModules = (modules != null && modules.Count > 0) ? modules : (preset != null ? preset.modules : null);
            if (targetModules == null || targetModules.Count == 0) return;

            _isPlaying = true;
            _isPaused = false;
            OnPlay?.Invoke();

            float cursor = 0f, maxEndTimeInBlock = 0f, maxTotalDuration = 0f;

            foreach (var module in targetModules)
            {
                if (module == null || !module.enabled) continue;
                if (module.linkType == FlexLinkType.Append) cursor = maxEndTimeInBlock;

                float startTime = cursor + module.delay;
                float endTime = startTime + module.duration;
                if (endTime > maxEndTimeInBlock) maxEndTimeInBlock = endTime;
                if (endTime > maxTotalDuration) maxTotalDuration = endTime;

                _runners.Add(new RoutineRunner(RunModuleRoutine(module, startTime)));
            }
            _runners.Add(new RoutineRunner(WaitAndComplete(maxTotalDuration)));
        }

        public void PauseAll() { _isPaused = true; NotifyModules(m => m.OnPause(transform)); }
        public void ResumeAll() { _isPaused = false; NotifyModules(m => m.OnResume(transform)); }

        public void StopAndReset()
        {
            if (!_isPlaying && !_hasSavedState) return;
            _isPlaying = false; _isPaused = false;
            NotifyModules(m => m.OnStop(transform));
            RestoreInitialState();
            _runners.Clear();
        }

        public void AddParallelRoutine(IEnumerator routine) { if (routine != null) _runners.Add(new RoutineRunner(routine)); }

        private void NotifyModules(System.Action<AnimationModule> action)
        {
            var targetModules = (modules != null && modules.Count > 0) ? modules : (preset != null ? preset.modules : null);
            if (targetModules != null) { foreach (var m in targetModules) if (m != null) action(m); }
        }

        private void SaveInitialState()
        {
            if (_hasSavedState) return;
            _initPos = transform.localPosition; _initRot = transform.localRotation; _initScale = transform.localScale;
            if (transform is RectTransform rect) { _initAnchoredPos = rect.anchoredPosition; _initSizeDelta = rect.sizeDelta; }
            if (TryGetComponent(out CanvasGroup cg)) { _initAlpha = cg.alpha; _hasCanvasGroup = true; }
            if (TryGetComponent(out UnityEngine.UI.Graphic gr)) { _initColor = gr.color; _hasGraphic = true; }
            if (TryGetComponent(out SpriteRenderer sr)) { _initColor = sr.color; _hasSpriteRenderer = true; }
            _hasSavedState = true;
        }

        private void RestoreInitialState()
        {
            if (!_hasSavedState) return;
            transform.localPosition = _initPos; transform.localRotation = _initRot; transform.localScale = _initScale;
            if (transform is RectTransform rect) { rect.anchoredPosition = _initAnchoredPos; rect.sizeDelta = _initSizeDelta; }
            if (_hasCanvasGroup && TryGetComponent(out CanvasGroup cg)) cg.alpha = _initAlpha;
            if (_hasGraphic && TryGetComponent(out UnityEngine.UI.Graphic gr)) gr.color = _initColor;
            if (_hasSpriteRenderer && TryGetComponent(out SpriteRenderer sr)) sr.color = _initColor;
            _hasSavedState = false;
        }

        private void ProcessRunners(float dt)
        {
            int count = _runners.Count;
            if (count == 0) { _isPlaying = false; return; }
            for (int i = count - 1; i >= 0; i--) { if (_runners[i].IsDone || !_runners[i].Step(dt)) _runners.RemoveAt(i); }
        }

        private IEnumerator RunModuleRoutine(AnimationModule module, float delay) { if (delay > 0) yield return delay; yield return module.CreateRoutine(transform, ignoreTimeScale, timeScale); }
        private IEnumerator WaitAndComplete(float duration) { if (duration > 0) yield return duration; OnComplete?.Invoke(); }

        private class RoutineRunner
        {
            private Stack<IEnumerator> _stack = new Stack<IEnumerator>(4);
            private float _waitTime = 0f;
            public bool IsDone => _stack.Count == 0 && _waitTime <= 0;
            public RoutineRunner(IEnumerator root) { _stack.Push(root); }
            public bool Step(float dt)
            {
                if (_waitTime > 0) { _waitTime -= dt; return true; }
                if (_stack.Count == 0) return false;
                IEnumerator top = _stack.Peek();
                if (top.MoveNext()) {
                    object cur = top.Current;
                    if (cur is float f) _waitTime = f;
                    else if (cur is IEnumerator sub) { _stack.Push(sub); return Step(0f); }
                    return true;
                }
                _stack.Pop();
                if (_stack.Count > 0) return Step(0f);
                return false;
            }
        }
    }
}
