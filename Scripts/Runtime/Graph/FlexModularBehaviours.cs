using UnityEngine;
using UnityEngine.Playables;
using System.Reflection;

namespace FlexAnimation
{
    public class FlexMathPlayableBehaviour : PlayableBehaviour
    {
        private FlexMathOp _op;

        public void Initialize(FlexMathOp op)
        {
            _op = op;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            int inputCount = playable.GetInputCount();
            if (inputCount <= 0) return;

            float result = playable.GetInputWeight(0);

            for (int i = 1; i < inputCount; i++)
            {
                float val = playable.GetInputWeight(i);
                switch (_op)
                {
                    case FlexMathOp.Add: result += val; break;
                    case FlexMathOp.Multiply: result *= val; break;
                    case FlexMathOp.Subtract: result -= val; break;
                    case FlexMathOp.Divide: if (val != 0) result /= val; break;
                    case FlexMathOp.Min: result = Mathf.Min(result, val); break;
                    case FlexMathOp.Max: result = Mathf.Max(result, val); break;
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

    public class FlexCustomNodeBehaviour : PlayableBehaviour
    {
        private Component _comp;
        private MethodInfo _method;

        public void Initialize(Component comp, string methodName)
        {
            _comp = comp;
            if (_comp != null && !string.IsNullOrEmpty(methodName))
            {
                _method = _comp.GetType().GetMethod(methodName);
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_comp == null || _method == null) return;

            float weight = (playable.GetInputCount() > 0) ? playable.GetInputWeight(0) : 0f;
            
            // Invoke custom method with the weight as an argument
            _method.Invoke(_comp, new object[] { weight });
        }
    }
}
