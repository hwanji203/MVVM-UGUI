using MVVM.UGUI.AssistComponent;
using MVVM.UGUI.Elements.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVVM.UGUI.Elements.Commands
{
    /// <summary>
    /// 끌어다 놓기 (카드 끌기). 놓으면 그 아래 <see cref="DropZone"/> 의 번호로 Command&lt;int&gt; 를 실행한다 — 놓은 곳이 없으면 -1.
    /// 무엇을 끌었는지는 항목마다 자기 Command 를 묶어서 ViewModel 이 이미 안다.
    /// 놓은 뒤에는 늘 제자리로 돌아간다. 옮겨 갈지는 ViewModel 이 정하고 목록이 다시 그린다.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DragElement : CommandElement<int>, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public const int NoZone = -1;

        [SerializeField] private CanvasGroup group;
        [Tooltip("끄는 동안 맨 위 Canvas 로 옮겨 다른 UI 위에 그린다")]
        [SerializeField] private bool moveToTop = true;

        private bool _dragging;
        private Transform _parent;
        private int _siblingIndex;
        private Vector2 _anchoredPosition;
        private Vector3 _localScale;
        private Vector2 _grabOffset;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsActive)
            {
                eventData.pointerDrag = null;   // 이번 끌기의 나머지 이벤트를 받지 않는다
                return;
            }

            var rect = (RectTransform)transform;
            _dragging = true;
            _parent = rect.parent;
            _siblingIndex = rect.GetSiblingIndex();
            _anchoredPosition = rect.anchoredPosition;
            _localScale = rect.localScale;

            if (moveToTop)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas)
                {
                    rect.SetParent(canvas.rootCanvas.transform, true);
                    rect.SetAsLastSibling();
                }
            }

            // 아래의 DropZone 이 포인터를 받도록 끄는 동안 자기는 Raycast 를 막지 않는다
            group.blocksRaycasts = false;

            // 잡은 지점이 끄는 동안 포인터 아래 그대로 있게 (가운데로 튀지 않게)
            _grabOffset = PointerInParent(eventData, out var pointer) ? (Vector2)rect.localPosition - pointer : Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging || !PointerInParent(eventData, out var pointer))
                return;

            var position = transform.localPosition;
            transform.localPosition = new Vector3(pointer.x + _grabOffset.x, pointer.y + _grabOffset.y, position.z);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            var hit = eventData.pointerCurrentRaycast.gameObject;
            var zone = hit ? hit.GetComponentInParent<DropZone>() : null;
            int zoneId = zone && zone.isActiveAndEnabled ? zone.ZoneId : NoZone;

            // 먼저 제자리로 — Command 가 목록을 바꿔 이 오브젝트를 다시 쓰거나 끌 수 있다
            Restore();
            Execute(zoneId);
        }

        // 끄는 도중 꺼지면 (화면이 닫힘) 실행 없이 제자리로
        private void OnDisable()
        {
            if (_dragging)
                Restore();
        }

        private void Restore()
        {
            _dragging = false;
            group.blocksRaycasts = true;

            var rect = (RectTransform)transform;
            if (_parent && rect.parent != _parent)
            {
                // 원래 부모 기준 값으로 되돌리므로 worldPositionStays 는 false
                rect.SetParent(_parent, false);
                rect.SetSiblingIndex(_siblingIndex);
            }
            rect.localScale = _localScale;
            rect.anchoredPosition = _anchoredPosition;
        }

        private bool PointerInParent(PointerEventData eventData, out Vector2 local)
        {
            local = default;
            return transform.parent is RectTransform parent &&
                   RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, eventData.position, eventData.pressEventCamera, out local);
        }

        protected override void HandleActiveChanged(bool enable) { }

        private void Reset() => group = GetComponent<CanvasGroup>();
    }
}
