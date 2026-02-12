using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace FlexAnimation
{
    [System.Serializable]
    public class AudioModule : AnimationModule
    {
        public enum AudioPlayMode { Single, Playlist }
        public enum AudioVisualMode { Scale, Shake }
        
        [Header("Basic Settings")]
        public AudioPlayMode playMode = AudioPlayMode.Single;
        public AudioClip clip;
        public List<AudioClip> playlist = new List<AudioClip>();
        
        [Range(0f, 1f)] public float volume = 1f;
        public bool oneShot = true;

        [Header("Playback Options")]
        public bool loopPlayback = false;
        public bool autoCrossfade = false;
        public float fadeInDuration = 0f;
        public float fadeOutDuration = 0f;
        
        [Header("Playlist Options")]
        public bool shuffle = false;
        public bool loopPlaylist = true;
        public float crossfadeTime = 0f;
        public float songDuration = 0f; 

        public AudioModule()
        {
            // Set default duration to 0 to signify "play full clip" by default in AudioModule
            duration = 0f;
        }

        [Header("Visualizer")]
        public bool enableVisualizer = false;
        public AudioVisualMode visualMode = AudioVisualMode.Scale;
        public float vizSensitivity = 10f;
        public int frequencyBin = 0;
        public Vector3 axisWeight = Vector3.one;

        [Header("Color Sync")]
        public bool enableColorSync = false;
        public Color syncColor = Color.red;
        public float colorSensitivity = 50f;

        // Runtime State
        private AudioSource _sourceA;
        private AudioSource _sourceB;
        private bool _usingSourceA = true;
        private Color _initColor;
        private Vector3 _initScale;
        private Vector3 _initPos;
        private Graphic _graphic;
        private SpriteRenderer _spriteRenderer;
        private Material _material;
        private bool _isRoutineActive = false;
        private bool _skipRequested = false;
        private FlexAnimation _owner;

        public void Skip() => _skipRequested = true;

        public override IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            _isRoutineActive = true;
            _skipRequested = false;
            _owner = target.GetComponent<FlexAnimation>();
            
            InitializeRuntimeState(target);

            float effectiveDelay = delay;
            if (globalTimeScale > 0.0001f) effectiveDelay /= globalTimeScale;
            if (effectiveDelay > 0) yield return effectiveDelay;

            if (enableVisualizer || enableColorSync)
                RunParallel(AnalysisRoutine(target));

            if (playMode == AudioPlayMode.Single)
            {
                yield return SinglePlaybackRoutine(target);
            }
            else
            {
                yield return PlaylistPlaybackRoutine(target);
            }
        }

        private void InitializeRuntimeState(Transform target)
        {
            if (enableVisualizer)
            {
                _initScale = target.localScale;
                _initPos = target.localPosition;
            }
            if (enableColorSync) PrepareColorTarget(target);
        }

        private IEnumerator SinglePlaybackRoutine(Transform target)
        {
            if (clip == null) yield break;
            _sourceA = GetSource(target, 0);

            if (oneShot)
            {
                _sourceA.PlayOneShot(clip, volume);
                yield return WaitForSeconds(clip.length);
            }
            else
            {
                SetupAudioSource(_sourceA, clip, loopPlayback, fadeInDuration > 0 ? 0 : volume);
                
                if (autoCrossfade && _sourceA.isPlaying && _sourceA.clip != null && _sourceA.clip != clip)
                {
                    _sourceB = GetSource(target, 1);
                    SetupAudioSource(_sourceB, clip, loopPlayback, 0);
                    _sourceB.Play();
                    RunParallel(CrossfadeInternal(_sourceA, _sourceB, fadeInDuration, volume));
                    _usingSourceA = false;
                }
                else
                {
                    if (!_sourceA.isPlaying || _sourceA.clip != clip) _sourceA.Play();
                    if (fadeInDuration > 0) RunParallel(FadeVolume(_sourceA, volume, fadeInDuration));
                }

                // Wait logic
                if (duration > 0.001f)
                {
                    yield return WaitForSeconds(duration);
                    if (fadeOutDuration > 0 && _sourceA != null)
                        yield return StartFadeOut(_sourceA, fadeOutDuration);
                }
                else if (loopPlayback)
                {
                    while (_isRoutineActive && !_skipRequested) yield return null;
                }
                else
                {
                    yield return WaitForSeconds(clip.length);
                }
            }
            _skipRequested = false;
        }

        private void SetupAudioSource(AudioSource source, AudioClip audioClip, bool loop, float vol)
        {
            source.clip = audioClip;
            source.loop = loop;
            source.volume = vol;
        }

        private IEnumerator PlaylistPlaybackRoutine(Transform target)
        {
            if (playlist == null || playlist.Count == 0) yield break;

            List<int> order = new List<int>();
            for (int i = 0; i < playlist.Count; i++) order.Add(i);
            int idx = 0;

            _sourceA = GetSource(target, 0);
            _sourceB = GetSource(target, 1);

            while (_isRoutineActive)
            {
                if (shuffle && idx == 0) ShuffleList(order);
                AudioClip currentClip = playlist[order[idx]];
                
                if (currentClip == null)
                {
                    if (MoveNext(ref idx, order.Count)) continue; else break;
                }

                AudioSource active = _usingSourceA ? _sourceA : _sourceB;
                AudioSource next = _usingSourceA ? _sourceB : _sourceA;

                float totalPlayTime = (songDuration > 0.001f) ? songDuration : currentClip.length;
                float cf = (crossfadeTime > 0.001f && crossfadeTime < totalPlayTime) ? crossfadeTime : 0f;

                if (cf > 0)
                {
                    SetupAudioSource(next, currentClip, false, 0);
                    next.Play();
                    RunParallel(CrossfadeInternal(active, next, cf, volume));
                    _usingSourceA = !_usingSourceA;
                }
                else
                {
                    SetupAudioSource(active, currentClip, false, volume);
                    active.Play();
                }

                // 4. Wait for the song to finish (minus crossfade time)
                float waitTime = totalPlayTime - cf;
                if (waitTime > 0.001f)
                {
                    yield return WaitForSeconds(waitTime);
                }

                if (!MoveNext(ref idx, order.Count)) break;
            }
        }

        private IEnumerator WaitForSeconds(float seconds)
        {
            float elapsed = 0;
            while (elapsed < seconds && _isRoutineActive && !_skipRequested)
            {
                float dt = GetDeltaTime();
                if (_owner != null && _owner.IsPaused) dt = 0;
                elapsed += dt;
                yield return null;
            }
            _skipRequested = false;
        }

        private float GetDeltaTime()
        {
            if (_owner != null)
                return (_owner.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * _owner.timeScale;
            return Time.deltaTime;
        }

        private bool MoveNext(ref int idx, int count)
        {
            idx++;
            if (idx >= count)
            {
                if (loopPlaylist) { idx = 0; return true; }
                return false;
            }
            return true;
        }

        private IEnumerator AnalysisRoutine(Transform target)
        {
            float[] spectrum = new float[256];
            while (_isRoutineActive && target != null)
            {
                AudioSource active = GetActiveSource();
                if (active != null && active.isPlaying)
                {
                    active.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
                    float val = spectrum[Mathf.Clamp(frequencyBin, 0, 255)];
                    
                    if (enableVisualizer) UpdateVisualizer(target, val);
                    if (enableColorSync) SetColor(Color.Lerp(_initColor, syncColor, val * colorSensitivity));
                }
                yield return null;
            }
        }

        private AudioSource GetActiveSource()
        {
            if (_sourceA != null && _sourceA.isPlaying) return _sourceA;
            if (_sourceB != null && _sourceB.isPlaying) return _sourceB;
            return null;
        }

        private void UpdateVisualizer(Transform target, float value)
        {
            if (visualMode == AudioVisualMode.Scale)
                target.localScale = _initScale + (axisWeight * value * vizSensitivity);
            else
                target.localPosition = _initPos + (Vector3)Random.insideUnitSphere * value * vizSensitivity * 0.1f;
        }

        private void PrepareColorTarget(Transform target)
        {
            if (target.TryGetComponent(out _graphic)) _initColor = _graphic.color;
            else if (target.TryGetComponent(out _spriteRenderer)) _initColor = _spriteRenderer.color;
            else if (target.TryGetComponent(out Renderer r)) { _material = r.material; _initColor = _material.color; }
        }

        private void SetColor(Color c)
        {
            if (_graphic) _graphic.color = c; 
            else if (_spriteRenderer) _spriteRenderer.color = c; 
            else if (_material) _material.color = c;
        }

        private void ShuffleList(List<int> list) 
        { 
            for (int i = 0; i < list.Count; i++) 
            { 
                int t = list[i]; 
                int r = Random.Range(i, list.Count); 
                list[i] = list[r]; 
                list[r] = t; 
            } 
        }

        private IEnumerator CrossfadeInternal(AudioSource from, AudioSource to, float time, float vol)
        {
            float elapsed = 0; 
            float startVol = (from != null && from.isPlaying) ? from.volume : 0;
            
            while (elapsed < time && _isRoutineActive) 
            {
                if (to != null && !to.isPlaying && _owner != null && _owner.IsPaused) { yield return null; continue; } 
                
                elapsed += Time.deltaTime; 
                float t = elapsed / time; 
                
                if (from != null && from.isPlaying) from.volume = Mathf.Lerp(startVol, 0, t); 
                if (to != null) to.volume = Mathf.Lerp(0, vol, t); 
                
                yield return null; 
            }
            
            if (from != null && from.isPlaying) from.Stop(); 
            if (to != null && _isRoutineActive) to.volume = vol;
        }

        private IEnumerator FadeVolume(AudioSource s, float targetVol, float duration) 
        { 
            float elapsed = 0; 
            float startVol = s.volume; 
            while (elapsed < duration && _isRoutineActive) 
            { 
                if(!s.isPlaying) { yield return null; continue; } 
                elapsed += Time.deltaTime; 
                s.volume = Mathf.Lerp(startVol, targetVol, elapsed / duration); 
                yield return null; 
            } 
            if(_isRoutineActive && s != null) s.volume = targetVol; 
        }

        private IEnumerator StartFadeOut(AudioSource s, float duration) 
        { 
            float elapsed = 0; 
            float startVol = (s != null) ? s.volume : 0; 
            while (elapsed < duration && _isRoutineActive) 
            { 
                if(s != null && !s.isPlaying) { yield return null; continue; } 
                elapsed += Time.deltaTime; 
                if(s != null) s.volume = Mathf.Lerp(startVol, 0, elapsed / duration); 
                yield return null; 
            } 
            if(s != null) s.Stop(); 
        }

        private void RunParallel(IEnumerator routine)
        {
            if (_owner != null) _owner.AddParallelRoutine(routine);
            else StaticCoroutine.Start(routine);
        }

        private AudioSource GetSource(Transform target, int index)
        {
            var sources = target.GetComponents<AudioSource>();
            if (index < sources.Length) return sources[index];
            var s = target.gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            return s;
        }

        public override void OnStop(Transform t) 
        { 
            _isRoutineActive = false;
            
            var sources = t.GetComponents<AudioSource>();
            foreach (var s in sources) { s.Stop(); s.clip = null; }
            
            if (enableVisualizer) 
            { 
                t.localScale = _initScale; 
                t.localPosition = _initPos; 
            }
            if (enableColorSync) SetColor(_initColor); 
        }
        
        public override void OnPause(Transform t) { var sources = t.GetComponents<AudioSource>(); foreach(var s in sources) if(s.isPlaying) s.Pause(); }
        public override void OnResume(Transform t) { var sources = t.GetComponents<AudioSource>(); foreach(var s in sources) s.UnPause(); }
    }
}
