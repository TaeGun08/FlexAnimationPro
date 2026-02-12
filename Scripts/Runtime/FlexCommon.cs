namespace FlexAnimation
{
    public enum FlexLinkType { Append, Join, Insert }

    public enum LoopMode { None, Loop, Yoyo, Incremental }

    public enum Ease
    {
        Linear,
        InSine, OutSine, InOutSine,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InQuart, OutQuart, InOutQuart,
        InQuint, OutQuint, InOutQuint,
        InExpo, OutExpo, InOutExpo,
        InCirc, OutCirc, InOutCirc,
        InBack, OutBack, InOutBack,
        InElastic, OutElastic, InOutElastic,
        InBounce, OutBounce, InOutBounce
    }

    internal class StaticCoroutine : UnityEngine.MonoBehaviour
    {
        private static StaticCoroutine _instance;
        public static void Start(System.Collections.IEnumerator routine)
        {
            if (_instance == null)
            {
                UnityEngine.GameObject go = new UnityEngine.GameObject("FlexAudioCoroutine");
                _instance = go.AddComponent<StaticCoroutine>();
                if (UnityEngine.Application.isPlaying)
                    UnityEngine.Object.DontDestroyOnLoad(go);
            }
            _instance.StartCoroutine(routine);
        }
    }
}
