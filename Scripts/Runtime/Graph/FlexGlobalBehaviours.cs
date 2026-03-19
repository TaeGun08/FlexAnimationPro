using UnityEngine;
using UnityEngine.Playables;

namespace FlexAnimation
{
    public class FlexGlobalSetBehaviour : PlayableBehaviour
    {
        private string _key;

        public void Initialize(string key)
        {
            _key = key;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (string.IsNullOrEmpty(_key)) return;

            float weight = (playable.GetInputCount() > 0) ? playable.GetInputWeight(0) : 0f;
            FlexGlobalManager.Instance.SetValue(_key, weight);
        }
    }

    public class FlexGlobalGetBehaviour : PlayableBehaviour
    {
        private string _key;

        public void Initialize(string key)
        {
            _key = key;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (string.IsNullOrEmpty(_key)) return;

            float val = FlexGlobalManager.Instance.GetValue(_key);
            PropagateWeight(playable, val);
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
