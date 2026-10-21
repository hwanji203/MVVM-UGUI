using DG.Tweening;
using MVVM.UGUI.Elements.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.UGUI.Elements.Display.Gauges
{
    /// <summary>0~1 값을 게이지로. Image 의 fillAmount 또는 RectTransform 크기로 그린다.</summary>
    public class GaugeElement : TweenValueElement<float>
    {
        public enum Mode
        {
            Fill,     // Image.fillAmount (Image Type = Filled)
            ScaleX,   // 가로 크기 (pivot 쪽이 고정)
            ScaleY,   // 세로 크기 (옛 GaugeForm 의 PosY 방식)
        }

        [SerializeField] private Mode mode = Mode.Fill;
        [Tooltip("Fill 모드일 때 채울 Image")]
        [SerializeField] private Image image;
        [Tooltip("Scale 모드일 때 크기를 바꿀 대상")]
        [SerializeField] private RectTransform scaleTarget;

        protected override void ApplyImmediate(float value) => Set(Mathf.Clamp01(value));

        protected override void AppendTween(Sequence seq, float value) =>
            seq.Append(DOTween.To(Get, Set, Mathf.Clamp01(value), duration).SetEase(ease));

        private float Get() => mode switch
        {
            Mode.Fill => image.fillAmount,
            Mode.ScaleX => scaleTarget.localScale.x,
            _ => scaleTarget.localScale.y,
        };

        private void Set(float ratio)
        {
            switch (mode)
            {
                case Mode.Fill:
                    image.fillAmount = ratio;
                    break;
                case Mode.ScaleX:
                    var sx = scaleTarget.localScale;
                    scaleTarget.localScale = new Vector3(ratio, sx.y, sx.z);
                    break;
                default:
                    var sy = scaleTarget.localScale;
                    scaleTarget.localScale = new Vector3(sy.x, ratio, sy.z);
                    break;
            }
        }

        private void Reset()
        {
            image = GetComponent<Image>();
            scaleTarget = GetComponent<RectTransform>();
        }
    }
}
