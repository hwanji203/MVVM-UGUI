using MVVM.UGUI.Elements.Core;
using TMPro;
using UnityEngine;

namespace MVVM.UGUI.Elements.Display.Texts
{
    /// <summary>초를 시계 글로 (mm:ss 등). 일정 초 아래로 내려가면 색을 바꾼다.</summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TimerTextElement : VisualElement<float>
    {
        public enum Style
        {
            MinutesSeconds,         // 01:05
            HoursMinutesSeconds,    // 0:01:05
            Seconds,                // 65
            SecondsTenths,          // 65.0
        }

        [SerializeField] private TMP_Text tmp;
        [SerializeField] private Style style = Style.MinutesSeconds;
        [Tooltip("남은 시간 표시라면 켠다 — 0.2초 남았을 때 00:00 이 아니라 00:01 로 보인다")]
        [SerializeField] private bool roundUp = true;

        [Header("경고 색 (warningBelow <= 0 이면 안 씀)")]
        [SerializeField] private float warningBelow;
        [SerializeField] private Color warningColor = Color.red;

        private Color _normalColor;
        private bool _colorRead;

        protected override void HandleValueChanged(float seconds)
        {
            if (!_colorRead)
            {
                _colorRead = true;
                _normalColor = tmp.color;
            }

            seconds = Mathf.Max(0f, seconds);
            tmp.text = Format(seconds);

            if (warningBelow > 0f)
                tmp.color = seconds < warningBelow ? warningColor : _normalColor;
        }

        private string Format(float seconds)
        {
            if (style == Style.SecondsTenths)
                return seconds.ToString("0.0");

            int whole = roundUp ? Mathf.CeilToInt(seconds) : Mathf.FloorToInt(seconds);
            return style switch
            {
                Style.MinutesSeconds => $"{whole / 60:00}:{whole % 60:00}",
                Style.HoursMinutesSeconds => $"{whole / 3600}:{whole / 60 % 60:00}:{whole % 60:00}",
                _ => whole.ToString(),
            };
        }

        private void Reset() => tmp = GetComponent<TMP_Text>();
    }
}
