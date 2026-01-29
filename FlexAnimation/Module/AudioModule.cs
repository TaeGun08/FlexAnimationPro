using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace FlexAnimation
{
    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class AudioModule : AnimationModule
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool oneShot = true;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            return DOVirtual.DelayedCall(0, () =>
            {
                PlayAudio(target);
            }).SetDelay(duration); 
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target)
        {
            if (duration > 0) yield return new WaitForSeconds(duration);
            PlayAudio(target);
        }

        private void PlayAudio(Transform target)
        {
            if (clip == null) return;
                
            AudioSource source = target.GetComponent<AudioSource>();
            if (source == null) source = target.gameObject.AddComponent<AudioSource>();

            if (oneShot)
                source.PlayOneShot(clip, volume);
            else
            {
                source.clip = clip;
                source.volume = volume;
                source.Play();
            }
        }
    }
}