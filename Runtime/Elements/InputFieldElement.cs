using MVVM.UGUI.Elements.Core;
using TMPro;
using UnityEngine;

namespace MVVM.UGUI.Elements
{
    /// <summary>글 입력 ↔ Bindable&lt;string&gt; (이름 · 검색). 칠 때마다 또는 입력을 마칠 때 ViewModel 에 넣는다.</summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldElement : FormElement<string>
    {
        public enum WriteTiming { EveryChange, EndEdit }

        [SerializeField] private TMP_InputField input;
        [SerializeField] private WriteTiming writeTiming = WriteTiming.EndEdit;

        protected override void Awake()
        {
            base.Awake();
            if (writeTiming == WriteTiming.EveryChange)
                input.onValueChanged.AddListener(Write);
            else
                input.onEndEdit.AddListener(Write);
        }

        protected override void HandleValueChanged(string value) => input.SetTextWithoutNotify(value ?? string.Empty);

        private void Reset() => input = GetComponent<TMP_InputField>();
    }
}
