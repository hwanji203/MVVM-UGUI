using UnityEngine;

namespace MVVM.UGUI.Sound
{
    /// <summary>패키지 기본 구현 — AudioClip 하나를 공용 2D AudioSource 로 PlayOneShot.</summary>
    [CreateAssetMenu(menuName = "MVVM UGUI/UI Sound (AudioSource)", fileName = "UISound")]
    public class AudioSourceUISoundSO : UISoundSO
    {
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        private static AudioSource _shared;

        public override void Play()
        {
            if (!clip || !Application.isPlaying)
                return;

            SharedSource().PlayOneShot(clip, volume);
        }

        // 처음 소리를 낼 때 씬 전환에도 남는 AudioSource 하나를 만든다
        private static AudioSource SharedSource()
        {
            if (_shared)
                return _shared;

            var go = new GameObject("[MVVM UGUI Sound]");
            DontDestroyOnLoad(go);
            _shared = go.AddComponent<AudioSource>();
            _shared.playOnAwake = false;
            _shared.spatialBlend = 0f;
            return _shared;
        }
    }
}
