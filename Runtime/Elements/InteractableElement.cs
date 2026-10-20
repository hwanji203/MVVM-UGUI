using MVVM.UGUI.Elements.Core;
using UnityEngine;

namespace MVVM.UGUI.Elements
{
    /// <summary>CanvasGroup 아래 전체를 잠그고 흐리게 (영역을 통째로 못 누르게).</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class InteractableElement : VisualElement<bool>
    {
        [SerializeField] private CanvasGroup group;
        [Tooltip("잠겼을 때 투명도 (1 이면 흐리게 하지 않음)")]
        [SerializeField, Range(0f, 1f)] private float lockedAlpha = 0.5f;

        protected override void HandleValueChanged(bool interactable)
        {
            group.interactable = interactable;
            group.alpha = interactable ? 1f : lockedAlpha;
        }

        private void Reset() => group = GetComponent<CanvasGroup>();
    }
}
