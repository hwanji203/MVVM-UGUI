using MVVM.UGUI.Binding;
using MVVM.UGUI.Elements.Core;
using TMPro;
using UnityEngine;

namespace MVVM.UGUI.Elements
{
    /// <summary>숫자를 형식에 맞춰 글로 (예: "{0:N0} G" → "1,200 G"). int 도 묶을 수 있다.</summary>
    [RequireComponent(typeof(TMP_Text))]
    public class NumberTextElement : VisualElement<float>
    {
        [SerializeField] private TMP_Text tmp;
        [Tooltip("string.Format 형식. {0:N0} = 천 단위 쉼표 정수, {0:0.0} = 소수 한 자리")]
        [SerializeField] private string format = "{0:N0}";

        public void Bind(IReadOnlyBindable<int> source)
        {
            var asFloat = Bindable.Select(source, v => (float)v);
            Bind(asFloat);
            // 변환 객체도 바구니에 — 다시 묶거나 파괴될 때 int 원본 구독이 같이 풀린다.
            // Bind 가 먼저 바구니를 비우므로 그 뒤에 담는다
            Bindings.Add(asFloat);
        }

        protected override void HandleValueChanged(float value) => tmp.text = string.Format(format, value);

        private void Reset() => tmp = GetComponent<TMP_Text>();
    }
}
