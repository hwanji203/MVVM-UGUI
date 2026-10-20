using DG.Tweening;
using MVVM.UGUI.Elements.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.UGUI.Elements
{
    /// <summary>Graphic (Image · TMP 등) 의 색. duration 0 이면 바로.</summary>
    [RequireComponent(typeof(Graphic))]
    public class ColorElement : TweenValueElement<Color>
    {
        [SerializeField] private Graphic graphic;

        protected override void ApplyImmediate(Color value) => graphic.color = value;

        protected override void AppendTween(Sequence seq, Color value) =>
            seq.Append(DOTween.To(() => graphic.color, c => graphic.color = c, value, duration).SetEase(ease));

        private void Reset() => graphic = GetComponent<Graphic>();
    }
}
