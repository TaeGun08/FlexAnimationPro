using UnityEngine;
using UnityEngine.Playables;

namespace FlexAnimation
{
    public class FlexAudioPlayableBehaviour : PlayableBehaviour
    {
        private FlexAudioBand _band;
        private float _sensitivity;
        private float _threshold;
        private static float[] _spectrumData = new float[512];

        public void Initialize(FlexAudioBand band, float sensitivity, float threshold)
        {
            _band = band;
            _sensitivity = sensitivity;
            _threshold = threshold;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            AudioListener.GetSpectrumData(_spectrumData, 0, FFTWindow.BlackmanHarris);

            float avg = 0f;
            int startIdx = 0, endIdx = 0;

            switch (_band)
            {
                case FlexAudioBand.Bass: startIdx = 0; endIdx = 40; break;
                case FlexAudioBand.Mid: startIdx = 41; endIdx = 250; break;
                case FlexAudioBand.Treble: startIdx = 251; endIdx = 511; break;
                case FlexAudioBand.All: startIdx = 0; endIdx = 511; break;
            }

            for (int i = startIdx; i <= endIdx; i++)
            {
                avg += _spectrumData[i];
            }
            avg /= (endIdx - startIdx + 1);

            float result = Mathf.Clamp01((avg - _threshold) * _sensitivity);
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
