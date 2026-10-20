using DG.Tweening;
using MVVMGUI.Binding;
using MVVMGUI.Elements.Core;
using TMPro;
using UnityEngine;

namespace MVVMGUI.Elements
{
    /// <summary>값이 바뀌면 숫자가 굴러가며 따라간다 (골드 · 점수). 첫 값과 꺼져 있을 때는 바로.</summary>
    [RequireComponent(typeof(TMP_Text))]
    public class CountUpTextElement : TweenValueElement<float>
    {
        [SerializeField] private TMP_Text tmp;
        [Tooltip("string.Format 형식. 굴러가는 중에도 이 형식으로 보인다")]
        [SerializeField] private string format = "{0:N0}";

        // 지금 보이는 숫자 — 트윈이 여기서 출발한다 (글에서 다시 읽으면 형식 때문에 값이 어긋난다)
        private float _shown;

        public void Bind(IReadOnlyBindable<int> source)
        {
            var asFloat = Bindable.Select(source, v => (float)v);
            Bind(asFloat);
            // 변환 객체도 바구니에 — 다시 묶거나 파괴될 때 int 원본 구독이 같이 풀린다.
            // Bind 가 먼저 바구니를 비우므로 그 뒤에 담는다
            Bindings.Add(asFloat);
        }

        protected override void ApplyImmediate(float value) => Show(value);

        protected override void AppendTween(Sequence seq, float value) =>
            seq.Append(DOTween.To(() => _shown, Show, value, duration).SetEase(ease));

        private void Show(float value)
        {
            _shown = value;
            tmp.text = string.Format(format, value);
        }

        private void Reset() => tmp = GetComponent<TMP_Text>();
    }
}
