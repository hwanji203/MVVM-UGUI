using MVVM.UGUI.Elements.Core;
using TMPro;
using UnityEngine;

namespace MVVM.UGUI.Elements.Display
{
    /// <summary>알림 배지. 0 이면 숨기고, max 를 넘으면 "99+" 처럼.</summary>
    public class BadgeElement : VisualElement<int>
    {
        [Tooltip("켜고 끌 배지 오브젝트 (비우면 자기 자신)")]
        [SerializeField] private GameObject badge;
        [Tooltip("비우면 숫자 없이 점만")]
        [SerializeField] private TMP_Text countText;
        [SerializeField] private int max = 99;

        protected override void HandleValueChanged(int count)
        {
            (badge ? badge : gameObject).SetActive(count > 0);

            if (countText && count > 0)
                countText.text = count > max ? $"{max}+" : count.ToString();
        }

        private void Reset() => countText = GetComponentInChildren<TMP_Text>(true);
    }
}
