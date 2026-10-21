using MVVM.UGUI.Elements.Core;
using UnityEngine;

namespace MVVM.UGUI.Elements.Display
{
    /// <summary>번호에 맞는 오브젝트 하나만 켠다 (상태별 아이콘 · 페이지). 범위 밖이면 전부 끈다.</summary>
    public class SwitchElement : VisualElement<int>
    {
        [Tooltip("붙이는 순간 자식들로 채운다")]
        [SerializeField] private GameObject[] targets;

        protected override void HandleValueChanged(int index)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                    targets[i].SetActive(i == index);
            }
        }

        private void Reset()
        {
            targets = new GameObject[transform.childCount];
            for (int i = 0; i < targets.Length; i++)
                targets[i] = transform.GetChild(i).gameObject;
        }
    }
}
