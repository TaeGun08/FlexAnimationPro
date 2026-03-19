using System.Collections.Generic;
using UnityEngine;

namespace FlexAnimation
{
    public class FlexGlobalManager : MonoBehaviour
    {
        private static FlexGlobalManager _instance;
        private Dictionary<string, float> _variables = new Dictionary<string, float>();

        public static FlexGlobalManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("FlexGlobalManager");
                    _instance = go.AddComponent<FlexGlobalManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public void SetValue(string key, float value)
        {
            _variables[key] = value;
        }

        public float GetValue(string key, float defaultValue = 0f)
        {
            if (_variables.TryGetValue(key, out float val)) return val;
            return defaultValue;
        }
    }
}
