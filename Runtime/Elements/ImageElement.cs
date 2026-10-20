using MVVM.UGUI.Elements.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.UGUI.Elements
{
    /// <summary>그림 교체 (카드 그림 · 아이템 아이콘 · 얼굴). null 이면 Image 를 끈다.</summary>
    [RequireComponent(typeof(Image))]
    public class ImageElement : VisualElement<Sprite>
    {
        [SerializeField] private Image image;
        [SerializeField] private bool hideWhenNull = true;
        [Tooltip("스프라이트 원래 크기로 맞춘다")]
        [SerializeField] private bool setNativeSize;

        protected override void HandleValueChanged(Sprite value)
        {
            image.sprite = value;
            if (hideWhenNull)
                image.enabled = value != null;
            if (setNativeSize && value != null)
                image.SetNativeSize();
        }

        private void Reset() => image = GetComponent<Image>();
    }
}
