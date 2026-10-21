using DG.Tweening;
using MVVM.UGUI.Elements.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.UGUI.Elements.Display.Gauges
{
    /// <summary>
    /// 잔상이 따라오는 게이지 (격투게임 HP 바). 앞 · 뒤 Image 두 장 (Filled).
    /// 줄 때: 앞이 먼저 줄고 뒤 (잔상) 가 delay 뒤에 따라온다. 늘 때: 뒤가 먼저 차고 앞이 따라온다.
    /// </summary>
    public class DelayedGaugeElement : TweenValueElement<float>
    {
        [SerializeField] private Image front;
        [Tooltip("잔상 — front 뒤 (계층에서 위) 에 두고 다른 색으로")]
        [SerializeField] private Image trail;
        [SerializeField] private float delay = 0.4f;
        [SerializeField] private float frontDuration = 0.1f;

        protected override void ApplyImmediate(float value)
        {
            value = Mathf.Clamp01(value);
            front.fillAmount = value;
            trail.fillAmount = value;
        }

        protected override void AppendTween(Sequence seq, float value)
        {
            value = Mathf.Clamp01(value);
            bool decreasing = value < front.fillAmount;
            Image leading = decreasing ? front : trail;
            Image following = decreasing ? trail : front;

            seq.Append(Fill(leading, value, frontDuration).SetEase(Ease.OutQuad));
            // 뒤따르는 바는 앞 바가 끝나는 때와 상관없이 delay 초에 출발한다
            seq.Insert(delay, Fill(following, value, duration).SetEase(ease));
        }

        private static Tween Fill(Image image, float to, float time) =>
            DOTween.To(() => image.fillAmount, v => image.fillAmount = v, to, time);

        private void Reset() => front = GetComponent<Image>();
    }
}
