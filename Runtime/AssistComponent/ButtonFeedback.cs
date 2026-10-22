using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MVVM.UGUI.AssistComponent
{
    /// <summary>
    /// 누를 수 있는 것에 붙이는 손맛 연출 (옛 FadeButtonForm) — 올리면 · 누르면 크기 · 투명도가 바뀐다.
    /// 값을 묶지 않아 Element 가 아니다. 같은 오브젝트의 Selectable (Button 등) 이 잠겨 있으면 반응하지 않는다.
    /// </summary>
    public class ButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("크기를 바꿀 대상 (비우면 자기 자신)")]
        [SerializeField] private Transform target;
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float pressScale = 0.95f;
        [Tooltip("투명도를 바꿀 CanvasGroup (비우면 투명도는 그대로)")]
        [SerializeField] private CanvasGroup group;
        [SerializeField, Range(0f, 1f)] private float hoverAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float pressAlpha = 0.8f;
        [SerializeField] private float duration = 0.1f;

        private Selectable _selectable;
        private Vector3 _baseScale;
        private bool _baseRead;
        private bool _over;
        private bool _pressed;
        private Sequence _seq;

        private Transform Target => target ? target : transform;

        private void Awake() => _selectable = GetComponent<Selectable>();

        public void OnPointerEnter(PointerEventData eventData) { _over = true; Refresh(); }

        public void OnPointerExit(PointerEventData eventData) { _over = false; _pressed = false; Refresh(); }

        public void OnPointerDown(PointerEventData eventData) { _pressed = true; Refresh(); }

        public void OnPointerUp(PointerEventData eventData) { _pressed = false; Refresh(); }

        private void Refresh()
        {
            if (!_baseRead)
            {
                _baseRead = true;
                _baseScale = Target.localScale;
            }

            bool interactable = !_selectable || _selectable.IsInteractable();
            // 누름 > 올림 > 보통 순으로 고른다. 잠겨 있으면 늘 보통
            float scale = !interactable ? 1f : _pressed ? pressScale : _over ? hoverScale : 1f;
            float alpha = !interactable ? 1f : _pressed ? pressAlpha : _over ? hoverAlpha : 1f;

            _seq?.Kill();
            _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            _seq.Append(Target.DOScale(_baseScale * scale, duration));
            if (group)
                _seq.Join(DOTween.To(() => group.alpha, a => group.alpha = a, alpha, duration));
        }

        private void OnDisable()
        {
            _over = false;
            _pressed = false;
            _seq?.Kill();
            _seq = null;
            if (_baseRead)
                Target.localScale = _baseScale;
            if (group)
                group.alpha = 1f;
        }

        private void OnDestroy() => _seq?.Kill();
    }
}
