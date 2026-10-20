using MVVM.UGUI.Elements.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVVM.UGUI.Elements
{
    /// <summary>
    /// 포인터가 delay 초 머무르면 툴팁을 띄운다 ← Bindable&lt;string&gt; (빈 글이면 띄우지 않음). Raycast Target 인 Graphic 이 필요하다.
    /// PC 는 마우스를 올려 두면, 터치는 길게 누르면 뜬다 — 터치는 누르는 순간 들어오고 떼는 순간 나가므로 같은 규칙으로 된다.
    /// 툴팁 오브젝트는 프리팹에서 원하는 자리에 둔다.
    /// </summary>
    public class TooltipElement : VisualElement<string>, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("켜고 끌 툴팁 오브젝트")]
        [SerializeField] private GameObject tooltipRoot;
        [SerializeField] private TMP_Text tooltipText;
        [SerializeField] private float delay = 0.5f;

        private string _text;
        private bool _pointerOver;
        private float _waited;

        protected override void Awake()
        {
            base.Awake();
            Hide();
        }

        protected override void HandleValueChanged(string value)
        {
            _text = value;
            if (tooltipText != null)
                tooltipText.text = value;
            if (string.IsNullOrEmpty(value))
                Hide();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _pointerOver = true;
            _waited = 0f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _pointerOver = false;
            Hide();
        }

        private void Update()
        {
            if (!_pointerOver || tooltipRoot == null || tooltipRoot.activeSelf || string.IsNullOrEmpty(_text))
                return;

            _waited += Time.unscaledDeltaTime;
            if (_waited >= delay)
                tooltipRoot.SetActive(true);
        }

        private void OnDisable()
        {
            _pointerOver = false;
            Hide();
        }

        private void Hide()
        {
            if (tooltipRoot != null)
                tooltipRoot.SetActive(false);
        }
    }
}
