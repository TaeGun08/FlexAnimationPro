using UnityEngine;
using UnityEngine.Playables;

namespace FlexAnimation
{
    public class FlexSpringPlayableBehaviour : PlayableBehaviour
    {
        private float _stiffness = 100.0f;
        private float _damping = 10.0f;
        private float _currentValue;
        private float _velocity;

        public void Initialize(float stiffness, float damping)
        {
            _stiffness = stiffness;
            _damping = damping;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            // Read target value from input weight
            float targetValue = 0f;
            if (playable.GetInputCount() > 0)
            {
                targetValue = playable.GetInputWeight(0);
            }

            // Spring physics: F = -kx - bv
            float force = (targetValue - _currentValue) * _stiffness;
            _velocity += (force - _velocity * _damping) * info.deltaTime;
            _currentValue += _velocity * info.deltaTime;

            // Propagate to outputs
            PropagateWeight(playable, _currentValue);
        }

        private void PropagateWeight(Playable playable, float value)
        {
            int outputCount = playable.GetOutputCount();
            for (int i = 0; i < outputCount; i++)
            {
                Playable output = playable.GetOutput(i);
                int inputCount = output.GetInputCount();
                for (int j = 0; j < inputCount; j++)
                {
                    if (output.GetInput(j).Equals(playable))
                    {
                        output.SetInputWeight(j, value);
                    }
                }
            }
        }
    }

    public class FlexPointerPlayableBehaviour : PlayableBehaviour
    {
        private FlexPointerMode _mode;
        private float _range;
        private RectTransform _rectTarget;
        private Transform _worldTarget;

        public void Initialize(FlexPointerMode mode, float range, Transform target)
        {
            _mode = mode;
            _range = range;
            _worldTarget = target;
            _rectTarget = target as RectTransform;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            float result = 0f;
            Vector2 mousePos = Input.mousePosition;

            if (_mode == FlexPointerMode.Distance)
            {
                float dist;
                if (_rectTarget != null)
                {
                    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, _rectTarget.position);
                    dist = Vector2.Distance(mousePos, screenPos);
                }
                else
                {
                    if (Camera.main != null)
                    {
                        Vector2 screenPos = Camera.main.WorldToScreenPoint(_worldTarget.position);
                        dist = Vector2.Distance(mousePos, screenPos);
                    }
                    else dist = _range + 1f;
                }
                result = Mathf.Clamp01(1.0f - (dist / _range));
            }
            else if (_mode == FlexPointerMode.Click)
            {
                result = Input.GetMouseButton(0) ? 1.0f : 0.0f;
            }
            else if (_mode == FlexPointerMode.Hover)
            {
                if (_rectTarget != null)
                {
                    result = RectTransformUtility.RectangleContainsScreenPoint(_rectTarget, mousePos) ? 1.0f : 0.0f;
                }
            }

            PropagateWeight(playable, result);
        }

        private void PropagateWeight(Playable playable, float value)
        {
            int outputCount = playable.GetOutputCount();
            for (int i = 0; i < outputCount; i++)
            {
                Playable output = playable.GetOutput(i);
                int inputCount = output.GetInputCount();
                for (int j = 0; j < inputCount; j++)
                {
                    if (output.GetInput(j).Equals(playable))
                    {
                        output.SetInputWeight(j, value);
                    }
                }
            }
        }
    }
}
