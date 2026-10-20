using MVVMGUI.Elements.Core;
using UnityEngine;

namespace MVVMGUI.Elements
{
    /// <summary>
    /// 월드 좌표를 화면에서 따라간다 (공 위 HP 바 · 이름표). 값은 월드 위치, 카메라가 움직여도 매 프레임 다시 맞춘다.
    /// 부모는 RectTransform 이어야 한다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class WorldFollowElement : VisualElement<Vector3>
    {
        [Tooltip("월드를 찍는 카메라 (비우면 Camera.main)")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Vector3 worldOffset;
        [Tooltip("카메라 뒤로 가면 CanvasGroup 투명도를 0 으로 (CanvasGroup 이 있을 때)")]
        [SerializeField] private CanvasGroup hideGroup;

        private Vector3 _worldPosition;
        private Canvas _canvas;

        protected override void HandleValueChanged(Vector3 worldPosition)
        {
            _worldPosition = worldPosition;
            if (isActiveAndEnabled)
                Follow();
        }

        // 월드 오브젝트 · 카메라가 Update 에서 움직인 뒤에 맞춘다
        private void LateUpdate() => Follow();

        private void Follow()
        {
            var cam = worldCamera != null ? worldCamera : Camera.main;
            if (cam == null || transform.parent is not RectTransform parent)
                return;

            Vector3 screen = cam.WorldToScreenPoint(_worldPosition + worldOffset);
            bool behind = screen.z < 0f;
            if (hideGroup != null)
                hideGroup.alpha = behind ? 0f : 1f;
            if (behind)
                return;

            if (_canvas == null)
                _canvas = GetComponentInParent<Canvas>().rootCanvas;
            // Overlay Canvas 는 화면 좌표를 그대로 써서 카메라를 넘기지 않는다
            var uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, uiCamera, out var local))
            {
                var position = transform.localPosition;
                transform.localPosition = new Vector3(local.x, local.y, position.z);
            }
        }

        private void Reset() => hideGroup = GetComponent<CanvasGroup>();
    }
}
