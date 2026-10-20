using MVVMGUI.Elements.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MVVMGUI.Elements
{
    /// <summary>슬라이더 ↔ Bindable&lt;float&gt; (볼륨 · 감도). 범위 제한 · 반올림은 ViewModel 이 값을 넣을 때 한다.</summary>
    [RequireComponent(typeof(Slider))]
    public class SliderElement : FormElement<float>
    {
        [SerializeField] private Slider slider;

        protected override void Awake()
        {
            base.Awake();
            slider.onValueChanged.AddListener(Write);
        }

        protected override void HandleValueChanged(float value) => slider.SetValueWithoutNotify(value);

        private void Reset() => slider = GetComponent<Slider>();
    }
}
