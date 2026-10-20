using MVVM.UGUI.Elements.Core;
using UnityEngine.EventSystems;

namespace MVVM.UGUI.Elements
{
    /// <summary>
    /// 포인터가 올라와 있는지를 ViewModel 에 알린다 → Bindable&lt;bool&gt; (미리보기 · 강조). Raycast Target 인 Graphic 이 필요하다.
    /// 터치에는 "올려 둔 상태" 가 없다 — 누르는 동안만 true.
    /// </summary>
    public class HoverElement : FormElement<bool>, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData) => Write(true);

        public void OnPointerExit(PointerEventData eventData) => Write(false);

        // 올라온 채 꺼지면 Exit 가 오지 않아 true 로 남는다
        private void OnDisable() => Write(false);

        // 입력만 받는 Element — 값을 그리지 않는다
        protected override void HandleValueChanged(bool value) { }
    }
}
