using UnityEngine;
using System;

namespace FlexAnimation
{
    [Serializable]
    public class FlexMoveModule : AnimationModule
    {
        public Vector3 targetPosition;
    }

    [Serializable]
    public class FlexRotateModule : AnimationModule
    {
        public Vector3 targetRotation;
    }

    [Serializable]
    public class FlexScaleModule : AnimationModule
    {
        public Vector3 targetScale = Vector3.one;
    }

    [Serializable]
    public class FlexFadeModule : AnimationModule
    {
        public float targetAlpha = 1f;
        public Color targetColor = Color.white;
    }

    [Serializable]
    public class FlexUIModule : AnimationModule
    {
        public Vector2 anchoredPosition;
    }
}
