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
            ProcessRunners(deltaTime * timeScale);
        }

        public void PlayAll()
        {
            StopAndReset();
            
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
                // Note: Duration for scheduling is approximation.
                // If looping, we assume 1 cycle for scheduling logic usually, or just start time.
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
            // Note: State reset (rewind) is not implemented in modules yet.
            // This just stops execution.
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

            // Pass the local timeScale logic? 
            // We are handling delta time in ProcessRunners.
            // So we pass ignoreTimeScale=false (since we provide dt) and globalTimeScale=1f (since we scaled dt).
            // Wait, FlexTween uses Time.deltaTime internally if we don't pass dt.
            // BUT FlexTween accepts ignoreTimeScale and globalTimeScale parameters to calculate dt.
            // My RoutineRunner calls MoveNext(). FlexTween Loop does 'yield return null'.
            // When we yield null, FlexTween pauses. Next 'Step' calls MoveNext.
            // FlexTween calculates dt inside its loop.
            // FlexTween reads Time.deltaTime/unscaledDeltaTime directly.
            // IF we want FlexTween to respect OUR calculated dt (from ProcessRunners), we have a problem.
            // FlexTween is self-contained.
            
            // Solution: 
            // In Runtime: FlexTween reads Time.deltaTime. This matches ProcessRunners logic mostly.
            // In Editor: FlexTween reads Time.deltaTime which might be 0 or irregular?
            // Actually, in Editor, Time.deltaTime works if EditorApplication.update is driven correctly?
            // But FlexTween's "while (time < duration)" loop depends on accumulation.
            
            // Critical Issue: FlexTween calculates 'dt' internally.
            // If I am manually stepping it in EditorPreviewUpdate, FlexTween will read 'Time.deltaTime' which might be wrong (e.g. 0.02 const, or real frame time).
            // If I want to control time (e.g. for Pause/TimeScale), I passed params to FlexTween.
            
            // Runtime:
            // Update() calls ProcessRunners. ProcessRunners calls Step() -> MoveNext().
            // FlexTween loop runs ONCE per frame.
            // FlexTween reads Time.deltaTime * globalTimeScale.
            // This works for Runtime.
            
            // Editor:
            // EditorPreviewUpdate calls ProcessRunners.
            // FlexTween reads Time.deltaTime. 
            // In Edit Mode, Time.deltaTime is time since last editor update. It is valid.
            // So looping works.
            
            // Issue: Pause.
            // If _isPaused is true, ProcessRunners is NOT called.
            // FlexTween is NOT stepped.
            // So FlexTween pauses.
            // This works!
            
            // So I just need to pass the parameters correctly.
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

                // We try to execute logic.
                // If the top stack is done, pop.
                
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
                        // Recursively start the sub-routine? 
                        // Typically we wait for next frame to step into it? 
                        // Or step immediately? Unity steps immediately until yield.
                        // Let's step immediately to avoid 1-frame lags on nesting.
                        return Step(0f); 
                    }
                    else if (current == null)
                    {
                        // Yield return null; -> Wait for next frame.
                        return true;
                    }
                }
                else
                {
                    _stack.Pop();
                    if (_stack.Count > 0) return Step(0f); // Continue parent
                }
                
                return _stack.Count > 0;
            }
        }
    }
}
