using UnityEngine;
using TMPro;
using UnityEngine.Scripting.APIUpdating;
using FlexAnimation.Internal;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace FlexAnimation
{
    public enum TextAnimMode { Typewriter }

    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class TextModule : AnimationModule
    {
        [Header("Text Settings")]
        public string customText; // If empty, uses existing text
        public TextAnimMode mode = TextAnimMode.Typewriter;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            if (target.TryGetComponent(out TMP_Text tmp))
            {
                if (!string.IsNullOrEmpty(customText))
                {
                    tmp.text = customText;
                }

                if (mode == TextAnimMode.Typewriter)
                {
                    int totalChars = tmp.textInfo.characterCount;
                    if (totalChars == 0) 
                    {
                        tmp.ForceMeshUpdate();
                        totalChars = tmp.textInfo.characterCount;
                    }

                    tmp.maxVisibleCharacters = 0;
                    return DOTween.To(
                        () => tmp.maxVisibleCharacters,
                        x => tmp.maxVisibleCharacters = x,
                        totalChars,
                        duration
                    );
                }
            }
            return null;
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target)
        {
            if (target.TryGetComponent(out TMP_Text tmp))
            {
                if (!string.IsNullOrEmpty(customText)) tmp.text = customText;

                if (mode == TextAnimMode.Typewriter)
                {
                    tmp.ForceMeshUpdate();
                    int totalChars = tmp.textInfo.characterCount;
                    tmp.maxVisibleCharacters = 0;

                    yield return FlexTween.To(
                        () => (float)tmp.maxVisibleCharacters,
                        val => tmp.maxVisibleCharacters = (int)val,
                        (float)totalChars,
                        duration,
                        ease
                    );
                }
            }
        }
    }
}