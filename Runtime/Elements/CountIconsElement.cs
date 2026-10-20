using MVVMGUI.Elements.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MVVMGUI.Elements
{
    /// <summary>
    /// N 개 중 앞의 k 개를 켠다 — 하트 3개 중 2개, 별점, 칸 게이지 (에너지 칸 · 탄창).
    /// 꺼진 칸은 숨기거나 (Hide) 색 · 그림을 바꾼다 (Tint · Swap).
    /// </summary>
    public class CountIconsElement : VisualElement<int>
    {
        public enum OffStyle { Hide, Tint, Swap }

        [Tooltip("붙이는 순간 바로 아래 자식들의 Graphic 으로 채운다")]
        [SerializeField] private Graphic[] icons;
        [SerializeField] private OffStyle offStyle = OffStyle.Tint;

        [Header("Tint")]
        [SerializeField] private Color onColor = Color.white;
        [SerializeField] private Color offColor = new(1f, 1f, 1f, 0.25f);

        [Header("Swap (icons 가 Image 일 때)")]
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;

        protected override void HandleValueChanged(int count)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                var icon = icons[i];
                if (icon == null)
                    continue;

                bool on = i < count;
                switch (offStyle)
                {
                    case OffStyle.Hide:
                        icon.gameObject.SetActive(on);
                        break;
                    case OffStyle.Tint:
                        icon.color = on ? onColor : offColor;
                        break;
                    default:
                        if (icon is Image image)
                            image.sprite = on ? onSprite : offSprite;
                        break;
                }
            }
        }

        private void Reset()
        {
            icons = new Graphic[transform.childCount];
            for (int i = 0; i < icons.Length; i++)
                icons[i] = transform.GetChild(i).GetComponent<Graphic>();
        }
    }
}
