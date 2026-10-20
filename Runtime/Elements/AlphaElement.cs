using DG.Tweening;
using MVVM.UGUI.Elements.Core;
using UnityEngine;

namespace MVVM.UGUI.Elements
{
    /// <summary>CanvasGroup 투명도 0~1.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class AlphaElement : TweenValueElement<float>
    {
        [SerializeField] private CanvasGroup group;

        protected override void ApplyImmediate(float value) => group.alpha = Mathf.Clamp01(value);

        protected override void AppendTween(Sequence seq, float value) =>
            seq.Append(DOTween.To(() => group.alpha, a => group.alpha = a, Mathf.Clamp01(value), duration).SetEase(ease));

        private void Reset() => group = GetComponent<CanvasGroup>();
    }
}
