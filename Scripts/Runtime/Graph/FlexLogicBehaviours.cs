using UnityEngine;
using UnityEngine.Playables;

namespace FlexAnimation
{
    public class FlexWaitPlayableBehaviour : PlayableBehaviour
    {
        private float _duration;
        private float _elapsed;

        public void Initialize(float duration)
        {
            _duration = duration;
            _elapsed = 0f;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            _elapsed += info.deltaTime;
            float weight = (_elapsed >= _duration) ? 1.0f : 0.0f;
            
            // Pass input weight only after duration
            if (playable.GetInputCount() > 0)
            {
                weight *= playable.GetInputWeight(0);
            }
            
            PropagateWeight(playable, weight);
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

    public class FlexComparePlayableBehaviour : PlayableBehaviour
    {
        private float _threshold;
        private FlexCompareType _type;

        public void Initialize(float threshold, FlexCompareType type)
        {
            _threshold = threshold;
            _type = type;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            float input = (playable.GetInputCount() > 0) ? playable.GetInputWeight(0) : 0f;
            bool success = false;

            switch (_type)
            {
                case FlexCompareType.Greater: success = input > _threshold; break;
                case FlexCompareType.Less: success = input < _threshold; break;
                case FlexCompareType.Equal: success = Mathf.Approximately(input, _threshold); break;
                case FlexCompareType.NotEqual: success = !Mathf.Approximately(input, _threshold); break;
            }

            PropagateWeight(playable, success ? 1.0f : 0.0f);
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

    public class FlexSequencePlayableBehaviour : PlayableBehaviour
    {
        private float _duration;
        private float _elapsed;

        public void Initialize(float duration)
        {
            _duration = duration;
            _elapsed = 0f;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            int inputCount = playable.GetInputCount();
            if (inputCount <= 0) return;

            _elapsed += info.deltaTime;
            float totalTime = _duration * inputCount;
            if (_elapsed > totalTime) _elapsed = 0f; // Loop for sequence

            int activeIdx = Mathf.FloorToInt(_elapsed / _duration) % inputCount;

            for (int i = 0; i < inputCount; i++)
            {
                playable.SetInputWeight(i, (i == activeIdx) ? 1.0f : 0.0f);
            }
        }
    }
}
