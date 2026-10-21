using MVVM.UGUI.Elements.Commands;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVVM.UGUI.AssistComponent
{
    /// <summary>
    /// <see cref="DragElement"/> 를 놓을 수 있는 곳. 놓이면 DragElement 가 이 번호로 Command 를 실행한다.
    /// 값을 묶지 않아 Element 가 아니다. Raycast Target 인 Graphic 이 필요하다.
    /// </summary>
    public class DropZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("ViewModel 이 받는 번호 (예: 0 = 내 칸, 1 = 버리기)")]
        [SerializeField] private int zoneId;
        [Tooltip("끌고 있는 것이 위에 올라오면 켤 오브젝트 (비워도 된다)")]
        [SerializeField] private GameObject highlight;

        public int ZoneId => zoneId;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (highlight != null && eventData.dragging && eventData.pointerDrag != null &&
                eventData.pointerDrag.GetComponent<DragElement>() != null)
                highlight.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData) => HideHighlight();

        private void OnDisable() => HideHighlight();

        private void HideHighlight()
        {
            if (highlight != null)
                highlight.SetActive(false);
        }
    }
}
