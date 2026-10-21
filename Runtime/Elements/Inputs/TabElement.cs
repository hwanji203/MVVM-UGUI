using UnityEngine;

namespace MVVM.UGUI.Elements.Inputs
{
    /// <summary>
    /// 탭 버튼 (토글) + 페이지. 고른 번호의 페이지만 켠다 ↔ Bindable&lt;int&gt;.
    /// 같은 일을 ToggleGroupElement + SwitchElement 를 같은 Bindable 에 묶어서도 할 수 있다 — 버튼과 페이지가 멀리 떨어져 있으면 그쪽.
    /// </summary>
    public class TabElement : ToggleGroupElement
    {
        [Tooltip("toggles 와 같은 순서")]
        [SerializeField] private GameObject[] pages;

        protected override void HandleValueChanged(int index)
        {
            base.HandleValueChanged(index);
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                    pages[i].SetActive(i == index);
            }
        }
    }
}
