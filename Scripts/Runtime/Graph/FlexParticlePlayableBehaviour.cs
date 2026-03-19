using UnityEngine;
using UnityEngine.Playables;

namespace FlexAnimation
{
    public class FlexParticlePlayableBehaviour : PlayableBehaviour
    {
        private ParticleSystem _ps;
        private FlexParticleControlField _field;
        private float _min, _max;
        private Color _targetColor;

        public void Initialize(ParticleSystem ps, FlexParticleControlField field, float min, float max, Color color)
        {
            _ps = ps;
            _field = field;
            _min = min;
            _max = max;
            _targetColor = color;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_ps == null) return;

            float weight = (playable.GetInputCount() > 0) ? playable.GetInputWeight(0) : 1f;
            float mappedValue = Mathf.Lerp(_min, _max, weight);

            switch (_field)
            {
                case FlexParticleControlField.EmissionRate:
                    var emission = _ps.emission;
                    emission.rateOverTime = mappedValue;
                    break;
                case FlexParticleControlField.StartSpeed:
                    var mainSpeed = _ps.main;
                    mainSpeed.startSpeed = mappedValue;
                    break;
                case FlexParticleControlField.StartSize:
                    var mainSize = _ps.main;
                    mainSize.startSize = mappedValue;
                    break;
                case FlexParticleControlField.StartColor:
                    var mainColor = _ps.main;
                    mainColor.startColor = Color.Lerp(Color.white, _targetColor, weight);
                    break;
            }
        }
    }
}
