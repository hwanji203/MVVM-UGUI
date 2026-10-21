using MVVM.UGUI.Elements.Core;
using TMPro;
using UnityEngine;

namespace MVVM.UGUI.Elements.Display.Texts
{
    /// <summary>
    /// TMP 글. 프리팹에 적어 둔 원래 글에 <c>{0}</c> 이 있으면 그 자리에 값을 넣는다 (예: "HP {0}").
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TextViewElement : VisualElement<string>
    {
        [SerializeField] private TMP_Text tmp;

        // 원래 글은 처음 값을 받을 때 읽는다 — 꺼진 채 만들어진 Element 는 Awake 보다 Bind 가 먼저 올 수 있다
        private string _format;
        private bool _formatRead;

        protected override void HandleValueChanged(string value)
        {
            if (!_formatRead)
            {
                _formatRead = true;
                _format = tmp.text != null && tmp.text.Contains("{0}") ? tmp.text : null;
            }

            tmp.text = _format != null ? string.Format(_format, value) : value;
        }

        private void Reset() => tmp = GetComponent<TMP_Text>();
    }
}
