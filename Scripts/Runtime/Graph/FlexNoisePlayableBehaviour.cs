using UnityEngine;
using UnityEngine.Playables;

namespace FlexAnimation
{
    public class FlexNoisePlayableBehaviour : PlayableBehaviour
    {
        private float _frequency = 1.0f;
        private float _amplitude = 0.5f;
        private Vector3 _axis = Vector3.up;
        private Transform _target;
        private float _time;
        private Vector3 _lastOffset;

        public void Initialize(float freq, float amp, Vector3 axis, Transform target)
        {
            _frequency = freq;
            _amplitude = amp;
            _axis = axis;
            _target = target;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_target == null) return;

            // Simple Perlin Noise for organic movement
            _time += info.deltaTime * _frequency;
            float noiseValue = (Mathf.PerlinNoise(_time, 0) - 0.5f) * 2.0f;
            Vector3 currentOffset = _axis * noiseValue * _amplitude;
            
            // Apply as additive offset to localPosition
            _target.localPosition += (currentOffset - _lastOffset);
            _lastOffset = currentOffset;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            // Reset offset on stop
            if (_target != null)
            {
                _target.localPosition -= _lastOffset;
            }
            _lastOffset = Vector3.zero;
        }
    }
}
