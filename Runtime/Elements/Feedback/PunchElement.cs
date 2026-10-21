using DG.Tweening;
using MVVM.UGUI.Binding;
using MVVM.UGUI.Elements.Core;
using UnityEngine;

namespace MVVM.UGUI.Elements.Feedback
{
    /// <summary>
    /// 값이 바뀌면 통통 튄다 (골드 · 점수 글 옆에 함께 붙인다). 어떤 타입의 Bindable 이든 Signal 이든 묶을 수 있다.
    /// Bindable 은 첫 값 (Bind 직후) 에는 튀지 않는다. 꺼져 있을 때는 튀지 않는다.
    /// </summary>
    public class PunchElement : Element
    {
        [Tooltip("튈 대상 (비우면 자기 자신)")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 punch = new(0.2f, 0.2f, 0f);
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private int vibrato = 6;
        [SerializeField, Range(0f, 1f)] private float elasticity = 0.5f;

        private Vector3 _baseScale;
        private bool _baseRead;

        private Transform Target => target != null ? target : transform;

        public void Bind<T>(IReadOnlyBindable<T> source)
        {
            Unbind();
            bool first = true;
            Bindings.Add(source.Bind(_ =>
            {
                if (first)
                {
                    first = false;
                    return;
                }
                Play();
            }));
        }

        public void Bind<T>(IReadOnlySignal<T> source)
        {
            Unbind();
            Bindings.Add(source.Bind(_ => Play()));
        }

        /// <summary>한 번 튄다. 튀는 중이면 처음 크기에서 다시.</summary>
        public void Play()
        {
            if (!Application.isPlaying || !isActiveAndEnabled)
                return;

            // 튀는 중에 또 튀면 커진 크기에서 시작해 점점 커지므로 원래 크기로 되돌리고 시작
            StopAndRestore();
            Seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            Seq.Append(Target.DOPunchScale(punch, duration, vibrato, elasticity));
        }

        private void OnDisable() => StopAndRestore();

        private void StopAndRestore()
        {
            if (!_baseRead)
            {
                _baseRead = true;
                _baseScale = Target.localScale;
            }

            Seq?.Kill();
            Seq = null;
            Target.localScale = _baseScale;
        }
    }
}
