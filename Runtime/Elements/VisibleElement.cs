using DG.Tweening;
using MVVMGUI.Elements.Core;
using UnityEngine;

namespace MVVMGUI.Elements
{
    /// <summary>
    /// 켜고 끄기. Active = SetActive (트윈 없음), Fade = CanvasGroup 투명도, Scale = 크기 0 ↔ 1.
    /// Fade · Scale 은 오브젝트를 끄지 않는다 — 꺼진 Element 는 트윈하지 않아 다시 나타날 때 연출이 사라지기 때문.
    /// 숨은 동안은 CanvasGroup (있으면) 이 클릭을 막는다.
    /// </summary>
    public class VisibleElement : TweenValueElement<bool>
    {
        public enum Mode { Active, Fade, Scale }

        [SerializeField] private Mode mode = Mode.Fade;
        [Tooltip("Active 모드에서 켜고 끌 오브젝트 (비우면 자기 자신)")]
        [SerializeField] private GameObject target;
        [Tooltip("Fade 모드의 투명도, 다른 모드에서도 있으면 숨은 동안 클릭을 막는다")]
        [SerializeField] private CanvasGroup group;
        [Tooltip("Scale 모드에서 크기를 바꿀 대상")]
        [SerializeField] private RectTransform scaleTarget;

        // SetActive 는 중간이 없어 늘 바로
        protected override bool CanTween => mode != Mode.Active && base.CanTween;

        protected override void ApplyImmediate(bool visible)
        {
            switch (mode)
            {
                case Mode.Active:
                    (target != null ? target : gameObject).SetActive(visible);
                    break;
                case Mode.Fade:
                    group.alpha = visible ? 1f : 0f;
                    break;
                default:
                    scaleTarget.localScale = visible ? Vector3.one : Vector3.zero;
                    break;
            }

            SetInput(visible);
        }

        protected override void AppendTween(Sequence seq, bool visible)
        {
            // 나타날 때는 바로 누를 수 있게, 사라질 때는 바로 못 누르게
            SetInput(visible);

            if (mode == Mode.Fade)
                seq.Append(DOTween.To(() => group.alpha, a => group.alpha = a, visible ? 1f : 0f, duration).SetEase(ease));
            else
                seq.Append(scaleTarget.DOScale(visible ? Vector3.one : Vector3.zero, duration).SetEase(ease));
        }

        private void SetInput(bool visible)
        {
            if (group == null)
                return;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        private void Reset()
        {
            group = GetComponent<CanvasGroup>();
            scaleTarget = GetComponent<RectTransform>();
        }
    }
}
