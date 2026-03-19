using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

namespace FlexAnimation
{
    public class FlexTimeRewindBehaviour : PlayableBehaviour
    {
        private struct TransformState
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
            public float time;
        }

        private float _bufferSeconds = 5.0f;
        private Transform _target;
        private LinkedList<TransformState> _buffer = new LinkedList<TransformState>();
        private float _currentTime = 0f;

        public void Initialize(float bufferSeconds, Transform target)
        {
            _bufferSeconds = bufferSeconds;
            _target = target;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_target == null) return;

            float weight = (playable.GetInputCount() > 0) ? playable.GetInputWeight(0) : 0f;
            _currentTime += info.deltaTime;

            if (weight <= 0.01f)
            {
                // Record Mode
                RecordState();
            }
            else
            {
                // Rewind Mode
                ApplyRewind(weight);
            }
        }

        private void RecordState()
        {
            _buffer.AddFirst(new TransformState 
            {
                position = _target.localPosition,
                rotation = _target.localRotation,
                scale = _target.localScale,
                time = _currentTime
            });

            // Remove old states beyond buffer size
            while (_buffer.Count > 0 && (_currentTime - _buffer.Last.Value.time) > _bufferSeconds)
            {
                _buffer.RemoveLast();
            }
        }

        private void ApplyRewind(float weight)
        {
            if (_buffer.Count < 2) return;

            // Simple rewind logic: move back in the list based on weight
            int targetIdx = Mathf.FloorToInt(weight * (_buffer.Count - 1));
            var node = _buffer.First;
            for (int i = 0; i < targetIdx && node.Next != null; i++)
            {
                node = node.Next;
            }

            var state = node.Value;
            _target.localPosition = Vector3.Lerp(_target.localPosition, state.position, weight);
            _target.localRotation = Quaternion.Slerp(_target.localRotation, state.rotation, weight);
            _target.localScale = Vector3.Lerp(_target.localScale, state.scale, weight);
        }
    }
}
