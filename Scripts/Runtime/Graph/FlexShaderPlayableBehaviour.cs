using UnityEngine;
using UnityEngine.Playables;

namespace FlexAnimation
{
    public class FlexShaderPlayableBehaviour : PlayableBehaviour
    {
        private Material _material;
        private int _propertyID;
        private FlexShaderPropertyType _type;
        private float _floatValue;
        private Color _colorValue;

        public void Initialize(Material mat, string name, FlexShaderPropertyType type, float fVal, Color cVal)
        {
            _material = mat;
            _propertyID = Shader.PropertyToID(name);
            _type = type;
            _floatValue = fVal;
            _colorValue = cVal;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_material == null) return;

            // Get the value from the connection's weight
            float val = _floatValue;
            if (playable.GetInputCount() > 0)
            {
                val = playable.GetInputWeight(0);
            }
            
            if (_type == FlexShaderPropertyType.Float)
            {
                _material.SetFloat(_propertyID, val);
            }
            else
            {
                _material.SetColor(_propertyID, _colorValue * val);
            }
        }
    }
}
