using UnityEngine;
using UnityEngine.Playables;

namespace FlexAnimation
{
    public class FlexShaderEffectBehaviour : PlayableBehaviour
    {
        private Material _material;
        private int _propertyID;
        private FlexShaderPropertyType _propType;
        private FlexShaderEffectType _effectType;
        private float _freq, _intensity, _min, _max;
        private float _time;
        private Color _baseColor;
        private float _baseFloat;

        public void Initialize(Material mat, string prop, FlexShaderPropertyType pType, 
            FlexShaderEffectType eType, float freq, float intensity, float min, float max, Color cVal, float fVal)
        {
            _material = mat;
            _propertyID = Shader.PropertyToID(prop);
            _propType = pType;
            _effectType = eType;
            _freq = freq;
            _intensity = intensity;
            _min = min;
            _max = max;
            _baseColor = cVal;
            _baseFloat = fVal;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_material == null) return;

            _time += info.deltaTime * _freq;
            float val = 0f;

            switch (_effectType)
            {
                case FlexShaderEffectType.Pulse:
                    val = Mathf.Lerp(_min, _max, (Mathf.Sin(_time * Mathf.PI * 2f) + 1f) * 0.5f);
                    break;
                case FlexShaderEffectType.Blink:
                    val = (Mathf.Sin(_time * Mathf.PI * 2f) > 0) ? _max : _min;
                    break;
                case FlexShaderEffectType.SineWave:
                    val = Mathf.Lerp(_min, _max, Mathf.Sin(_time));
                    break;
                case FlexShaderEffectType.Flash:
                    float t = (float)(playable.GetTime() % (1.0f / _freq)) * _freq;
                    val = Mathf.Lerp(_max, _min, t);
                    break;
            }

            val *= _intensity;

            if (_propType == FlexShaderPropertyType.Float)
            {
                _material.SetFloat(_propertyID, val);
            }
            else
            {
                _material.SetColor(_propertyID, _baseColor * val);
            }
        }
    }
}
