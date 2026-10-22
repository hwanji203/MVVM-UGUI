using MVVM.UGUI.Elements.Core;
using MVVM.UGUI.Sound;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVVM.UGUI.Elements.Commands
{
    /// <summary>
    /// 버튼 모양 없이 아무 Graphic 이나 누르면 Command (카드 · 배경 · 그림). Raycast Target 인 Graphic 이 필요하다.
    /// 끌다가 놓은 것은 클릭으로 치지 않는다 (EventSystem 규칙).
    /// </summary>
    public class ClickAreaElement : CommandElement, IPointerClickHandler
    {
        [SerializeField] private UISoundSO clickSound;

        private bool _active;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_active)
                return;
            if (clickSound)
                clickSound.Play();
            Execute();
        }

        protected override void HandleActiveChanged(bool enable) => _active = enable;
    }
}
