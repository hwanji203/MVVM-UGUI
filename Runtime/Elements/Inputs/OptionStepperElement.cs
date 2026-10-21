using System.Collections.Generic;
using MVVM.UGUI.Binding;
using MVVM.UGUI.Elements.Core;
using MVVM.UGUI.Sound;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.UGUI.Elements.Inputs
{
    /// <summary>
    /// "◀ 보통 ▶" 콘솔식 선택 ↔ 고른 번호 Bindable&lt;int&gt; (언어 · 해상도 · 화면 모드).
    /// 항목은 인스펙터에 적거나, 실행 중에 정해지면 <see cref="Bind(IReadOnlyBindableList{string}, Bindable{int})"/> 로 같이 넘긴다.
    /// </summary>
    public class OptionStepperElement : FormElement<int>
    {
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text label;
        [SerializeField] private string[] options = { };
        [Tooltip("끝에서 넘기면 처음으로. 끄면 끝에서 그쪽 버튼이 잠긴다")]
        [SerializeField] private bool wrap = true;
        [SerializeField] private UISoundSO clickSound;

        // 실행 중에 받은 항목 — 없으면 인스펙터의 options
        private IReadOnlyList<string> _options;

        private IReadOnlyList<string> Options => _options ?? options;

        protected override void Awake()
        {
            base.Awake();
            previousButton.onClick.AddListener(() => Step(-1));
            nextButton.onClick.AddListener(() => Step(+1));
        }

        /// <summary>항목 목록과 고른 번호를 한 번에 묶는다 (따로 Bind 하면 두 번째가 첫 번째를 끊는다).</summary>
        public void Bind(IReadOnlyBindableList<string> optionList, Bindable<int> selected)
        {
            Bind(selected);
            Bindings.Add(optionList.Bind(SetOptions));
            Bindings.Add(optionList.Bind((_, _) => SetOptions(optionList.Items)));
        }

        public override void Unbind()
        {
            base.Unbind();
            _options = null;
        }

        private void SetOptions(IReadOnlyList<string> items)
        {
            _options = items;
            if (Source != null)
                HandleValueChanged(Source.Value);
        }

        private void Step(int delta)
        {
            int count = Options.Count;
            if (count == 0 || Source == null)
                return;

            int next = Source.Value + delta;
            // C# 의 % 는 음수를 남겨서 (-1 % 3 == -1) count 를 더해 한 번 더 나눈다
            next = wrap ? (next % count + count) % count : Mathf.Clamp(next, 0, count - 1);
            if (next == Source.Value)
                return;

            if (clickSound != null)
                clickSound.Play();
            Write(next);
        }

        protected override void HandleValueChanged(int index)
        {
            var list = Options;
            label.text = index >= 0 && index < list.Count ? list[index] : string.Empty;

            if (!wrap)
            {
                previousButton.interactable = index > 0;
                nextButton.interactable = index < list.Count - 1;
            }
        }
    }
}
