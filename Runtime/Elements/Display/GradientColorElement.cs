using DG.Tweening;
using MVVM.UGUI.Elements.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.UGUI.Elements.Display
{
    /// <summary>0~1 값으로 Gradient 에서 색을 골라 칠한다 (HP 가 낮으면 빨강).</summary>
    [RequireComponent(typeof(Graphic))]
    public class GradientColorElement : TweenValueElement<float>
    {
        [SerializeField] private Graphic graphic;
        [SerializeField] private Gradient gradient = new();

        // 지금 칠한 비율 — 색에서 거꾸로 구할 수 없어 따로 든다 (트윈 시작 값)
        private float _ratio;

        protected override void ApplyImmediate(float value) => Paint(value);

        protected override void AppendTween(Sequence seq, float value) =>
            seq.Append(DOTween.To(() => _ratio, Paint, value, duration).SetEase(ease));

        private void Paint(float ratio)
        {
            _ratio = Mathf.Clamp01(ratio);
            graphic.color = gradient.Evaluate(_ratio);
        }

        private void Reset() => graphic = GetComponent<Graphic>();
    }
}
