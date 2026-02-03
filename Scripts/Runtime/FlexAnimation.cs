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

        // Events
        public UnityEngine.Events.UnityEvent OnPlay;
        public UnityEngine.Events.UnityEvent OnComplete;

        private List<RoutineRunner> _runners = new List<RoutineRunner>();
        private bool _isPlaying = false;
        private bool _isPaused = false;

        // State Backup
        private bool _hasSavedState = false;
        private Vector3 _initPos;
        private Quaternion _initRot;
        private Vector3 _initScale;
        private Vector2 _initAnchoredPos;
        private Vector2 _initSizeDelta;

        private void OnEnable()
        {
            if (playOnEnable)
            {
                PlayAll();
            }
        }

        private void OnDisable()
        {
            StopAndReset();
        }

        private void Update()
        {
            if (!_isPlaying || _isPaused) return;

            float dt = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            dt *= timeScale;

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
            // If already playing, stop first (resets to start)
            if (_isPlaying) StopAndReset();

            // Save state if strictly new start
            SaveInitialState();
            
            var targetModules = modules;
            if ((targetModules == null || targetModules.Count == 0) && preset != null)
            {
                targetModules = preset.modules;
            }

            if (targetModules == null || targetModules.Count == 0) return;

            _isPlaying = true;
            _isPaused = false;
            OnPlay?.Invoke();

            float cursor = 0f;
            float maxEndTimeInBlock = 0f;
            float maxTotalDuration = 0f;

            foreach (var module in targetModules)
            {
                if (!module.enabled) continue;

                if (module.linkType == FlexLinkType.Append)
                {
                    cursor = maxEndTimeInBlock;
                }
                // Join keeps the current cursor

                float startTime = cursor + module.delay;
                float duration = module.duration;
                
                float endTime = startTime + duration;
                if (endTime > maxEndTimeInBlock) maxEndTimeInBlock = endTime;
                if (endTime > maxTotalDuration) maxTotalDuration = endTime;

                _runners.Add(new RoutineRunner(RunModuleRoutine(module, startTime)));
            }

            // Schedule OnComplete
            _runners.Add(new RoutineRunner(WaitAndComplete(maxTotalDuration)));
        }

        public void PauseAll()
        {
            _isPaused = true;
        }
        
        public void ResumeAll()
        {
            _isPaused = false;
        }

        public void StopAndReset()
        {
            _isPlaying = false;
            _isPaused = false;
            _runners.Clear();
            RestoreInitialState();
        }

        private void SaveInitialState()
        {
            if (_hasSavedState) return;

            _initPos = transform.localPosition;
            _initRot = transform.localRotation;
            _initScale = transform.localScale;

            if (transform is RectTransform rect)
            {
                _initAnchoredPos = rect.anchoredPosition;
                _initSizeDelta = rect.sizeDelta;
            }

            _hasSavedState = true;
        }

        private void RestoreInitialState()
        {
            if (!_hasSavedState) return;

            transform.localPosition = _initPos;
            transform.localRotation = _initRot;
            transform.localScale = _initScale;

            if (transform is RectTransform rect)
            {
                rect.anchoredPosition = _initAnchoredPos;
                rect.sizeDelta = _initSizeDelta;
            }

            _hasSavedState = false;
        }

        private void ProcessRunners(float dt)
        {
            if (_runners.Count == 0)
            {
                _isPlaying = false;
                return;
            }

            for (int i = _runners.Count - 1; i >= 0; i--)
            {
                if (_runners[i].IsDone || !_runners[i].Step(dt))
                {
                    _runners.RemoveAt(i);
                }
            }
        }

        private IEnumerator RunModuleRoutine(AnimationModule module, float delay)
        {
            if (delay > 0)
            {
                yield return delay;
            }

            yield return module.CreateRoutine(transform, ignoreTimeScale, timeScale);
        }

        private IEnumerator WaitAndComplete(float duration)
        {
            if (duration > 0)
            {
                yield return duration;
            }
            OnComplete?.Invoke();
        }

        // --- Inner Class ---
        private class RoutineRunner
        {
            private Stack<IEnumerator> _stack = new Stack<IEnumerator>();
            private float _waitTime = 0f;
            public bool IsDone => _stack.Count == 0 && _waitTime <= 0;

            public RoutineRunner(IEnumerator root) { _stack.Push(root); }

            public bool Step(float dt)
            {
                if (_waitTime > 0)
                {
                    _waitTime -= dt;
                    return true;
                }

                if (_stack.Count == 0) return false;

                IEnumerator top = _stack.Peek();
                bool hasMore = false;
                
                try 
                {
                    hasMore = top.MoveNext();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[FlexAnimation] Error in routine: " + e);
                    return false;
                }

                if (hasMore)
                {
                    object current = top.Current;
                    
                    if (current is float f)
                    {
                        _waitTime = f;
                    }
                    else if (current is IEnumerator sub)
                    {
                        _stack.Push(sub);
                        return Step(0f); 
                    }
                    else if (current == null)
                    {
                        return true;
                    }
                }
                else
                {
                    _stack.Pop();
                    if (_stack.Count > 0) return Step(0f);
                }
                
                return _stack.Count > 0;
            }
        }
    }
}
