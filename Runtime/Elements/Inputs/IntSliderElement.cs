using MVVM.UGUI.Elements.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.UGUI.Elements.Inputs
{
    /// <summary>정수 슬라이더 ↔ Bindable&lt;int&gt; (품질 단계 · 개수). 붙이는 순간 Whole Numbers 를 켠다.</summary>
    [RequireComponent(typeof(Slider))]
    public class IntSliderElement : FormElement<int>
    {
        [SerializeField] private Slider slider;

        protected override void Awake()
        {
            base.Awake();
            slider.onValueChanged.AddListener(v => Write(Mathf.RoundToInt(v)));
        }

        protected override void HandleValueChanged(int value) => slider.SetValueWithoutNotify(value);

        private void Reset()
        {
            slider = GetComponent<Slider>();
            slider.wholeNumbers = true;
        }
    }
}
