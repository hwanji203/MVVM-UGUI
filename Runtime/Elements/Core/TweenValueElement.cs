using DG.Tweening;
using UnityEngine;

namespace MVVMGUI.Elements.Core
{
    /// <summary>
    /// 값이 바뀌면 트윈으로 따라가는 Element 의 바탕. 언제 트윈하고 언제 바로 맞출지는 여기서만 정한다 (DESIGN 3.4).
    /// 하위 Element 는 <see cref="ApplyImmediate"/> (바로 맞추기) 와 <see cref="AppendTween"/> (트윈 만들기) 만 쓴다.
    /// <list type="bullet">
    /// <item>첫 값 (Bind 직후) · 꺼져 있음 · 플레이 중이 아님 · duration 0 → 바로 맞춘다</item>
    /// <item>트윈 중 새 값 → 이전 트윈을 Kill (Complete 아님) — 지금 위치에서 새 목표로 간다</item>
    /// <item>트윈 중 꺼짐 → 마지막 값으로 바로 맞춘다 (DOTween 은 꺼진 오브젝트의 트윈도 돌려서 어중간한 값이 남는다)</item>
    /// </list>
    /// </summary>
    public abstract class TweenValueElement<T> : VisualElement<T>
    {
        [SerializeField] protected float duration = 0.25f;
        [SerializeField] protected Ease ease = Ease.OutQuart;
        [Tooltip("켜면 Time.timeScale 을 무시한다 (일시정지 팝업 안에서도 움직인다)")]
        [SerializeField] protected bool ignoreTimeScale = true;

        private bool _hasValue;

        /// <summary>마지막으로 받은 값 (트윈의 목표).</summary>
        protected T Target { get; private set; }

        protected sealed override void HandleValueChanged(T value)
        {
            // 목표를 먼저 적어 둔다 — 트윈 중 꺼지면 OnDisable 이 이 값으로 맞춘다
            Target = value;
            // Kill 이라 이전 트윈은 멈춘 자리에 남고, 새 트윈의 getter 가 그 자리를 시작 값으로 읽는다
            KillTween();

            if (!_hasValue || !CanTween)
            {
                _hasValue = true;
                ApplyImmediate(value);
                return;
            }

            Seq = DOTween.Sequence().SetUpdate(ignoreTimeScale).SetLink(gameObject);
            AppendTween(Seq, value);
        }

        /// <summary>지금 트윈해도 되나. 하위 Element 가 "이 설정에선 항상 바로" 를 더할 때만 덮는다 (base 를 함께 본다).</summary>
        protected virtual bool CanTween => duration > 0f && Application.isPlaying && isActiveAndEnabled;

        /// <summary>트윈 없이 화면을 value 로 맞춘다.</summary>
        protected abstract void ApplyImmediate(T value);

        /// <summary>
        /// 지금 화면 상태에서 value 로 가는 트윈을 seq 에 넣는다 (<see cref="duration"/> · <see cref="ease"/> 사용).
        /// 시작 값은 DOTween.To 의 getter 로 지금 값을 읽는다.
        /// </summary>
        protected abstract void AppendTween(Sequence seq, T value);

        // 다시 묶으면 새 원본의 첫 값도 바로 맞춘다 (Bind 가 먼저 Unbind 를 부른다)
        public override void Unbind()
        {
            base.Unbind();
            KillTween();
            _hasValue = false;
        }

        protected virtual void OnDisable()
        {
            if (Seq == null || !Seq.IsActive())
                return;

            KillTween();
            ApplyImmediate(Target);
        }

        private void KillTween()
        {
            Seq?.Kill();
            Seq = null;
        }
    }
}
