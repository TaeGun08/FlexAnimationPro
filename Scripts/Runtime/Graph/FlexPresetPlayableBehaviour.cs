using UnityEngine;
using UnityEngine.Playables;

namespace FlexAnimation
{
    public class FlexPresetPlayableBehaviour : PlayableBehaviour
    {
        private FlexAnimationPreset _preset;
        private FlexAnimation _playerInstance;
        private bool _isPlaying = false;
        private GameObject _targetObject;

        public void Initialize(FlexAnimationPreset preset, GameObject targetObject)
        {
            _preset = preset;
            _targetObject = targetObject;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (_preset == null || _targetObject == null) return;
            
            if (_playerInstance == null)
            {
                _playerInstance = _targetObject.AddComponent<FlexAnimation>();
                _playerInstance.playOnEnable = false;
                _playerInstance.preset = _preset;
            }

            if (!_isPlaying)
            {
                _playerInstance.PlayAll();
                _isPlaying = true;
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (_playerInstance != null && _isPlaying)
            {
                _playerInstance.StopAndReset();
                _isPlaying = false;
            }
        }

        public override void OnGraphStop(Playable playable)
        {
            if (_playerInstance != null)
            {
                _playerInstance.StopAndReset();
                if (Application.isPlaying)
                {
                    Object.Destroy(_playerInstance);
                }
                else
                {
                    Object.DestroyImmediate(_playerInstance);
                }
            }
        }
    }
}
