using MVVM.UGUI.Elements.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.UGUI.Elements
{
    /// <summary>
    /// 남은 쿨다운 0~1 을 덮개 (Image Filled, 보통 Radial 360) 로. 트윈 없음 — ViewModel 이 매 프레임 값을 넣는다.
    /// 쿨다운 버튼은 버튼 오브젝트에 ButtonElement, 자식에 이것을 붙여 조합한다.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CooldownElement : VisualElement<float>
    {
        [SerializeField] private Image cover;
        [Tooltip("0 (준비됨) 이면 덮개 Image 를 끈다")]
        [SerializeField] private bool hideWhenReady = true;

        protected override void HandleValueChanged(float remaining)
        {
            remaining = Mathf.Clamp01(remaining);
            cover.fillAmount = remaining;
            if (hideWhenReady)
                cover.enabled = remaining > 0f;
        }

        private void Reset() => cover = GetComponent<Image>();
    }
}
