using System.Collections.Generic;
using MVVM.UGUI.Binding;
using MVVM.UGUI.Elements.Core;
using TMPro;
using UnityEngine;

namespace MVVM.UGUI.Elements.Inputs
{
    /// <summary>
    /// 드롭다운 ↔ 고른 번호 Bindable&lt;int&gt;.
    /// 항목은 인스펙터 (TMP_Dropdown Options) 에 적어 두거나, 해상도처럼 실행 중에 정해지면 <see cref="Bind(IReadOnlyBindableList{string}, Bindable{int})"/> 로 같이 넘긴다.
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public class DropdownElement : FormElement<int>
    {
        [SerializeField] private TMP_Dropdown dropdown;

        protected override void Awake()
        {
            base.Awake();
            dropdown.onValueChanged.AddListener(Write);
        }

        /// <summary>
        /// 항목 목록과 고른 번호를 한 번에 묶는다. 둘을 따로 Bind 하면 두 번째 Bind 가 첫 번째 연결을 끊는다 (Element 의 연결 바구니는 하나).
        /// </summary>
        public void Bind(IReadOnlyBindableList<string> options, Bindable<int> selected)
        {
            Bind(selected);
            Bindings.Add(options.Bind(SetOptions));
            // 항목 하나가 바뀌어도 드롭다운은 통째로 다시 채운다 (항목 수가 적다)
            Bindings.Add(options.Bind((_, _) => SetOptions(options.Items)));
        }

        private void SetOptions(IReadOnlyList<string> items)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>(items));
            // 항목이 오기 전에 맞춘 번호는 잘렸을 수 있어 다시 맞춘다
            if (Source != null)
                dropdown.SetValueWithoutNotify(Source.Value);
            dropdown.RefreshShownValue();
        }

        protected override void HandleValueChanged(int index) => dropdown.SetValueWithoutNotify(index);

        private void Reset() => dropdown = GetComponent<TMP_Dropdown>();
    }
}
