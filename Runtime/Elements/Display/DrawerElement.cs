using DG.Tweening;
using MVVM.UGUI.Elements.Core;
using UnityEngine;

namespace MVVM.UGUI.Elements.Display
{
    /// <summary>
    /// 서랍처럼 한 방향으로 미끄러져 들어오고 나간다 (옆 메뉴 · 아래 패널). true = 열림.
    /// 열린 자리는 프리팹에 놓아 둔 위치 (처음 값을 받을 때 읽는다). LayoutGroup 이 위치를 정하는 자식에는 쓰지 않는다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DrawerElement : TweenValueElement<bool>
    {
        public enum Direction { Up, Down, Left, Right }

        [Tooltip("닫힐 때 빠져나가는 방향")]
        [SerializeField] private Direction hideTo = Direction.Left;
        [Tooltip("자기 크기에 더해 더 밀어낼 거리 (그림자 · 테두리까지 숨기려면)")]
        [SerializeField] private float extraDistance;

        private RectTransform _rect;
        private Vector2 _openPosition;
        private bool _positionRead;

        protected override void ApplyImmediate(bool open) => Rect.anchoredPosition = Destination(open);

        protected override void AppendTween(Sequence seq, bool open) =>
            seq.Append(DOTween.To(() => Rect.anchoredPosition, p => Rect.anchoredPosition = p, Destination(open), duration)
                .SetEase(ease));

        private RectTransform Rect => _rect != null ? _rect : _rect = (RectTransform)transform;

        private Vector2 Destination(bool open)
        {
            if (!_positionRead)
            {
                _positionRead = true;
                _openPosition = Rect.anchoredPosition;
            }

            if (open)
                return _openPosition;

            // 자기 크기만큼 밀어내 화면 밖으로 — 크기가 바뀌어도 매번 다시 잰다
            var size = Rect.rect.size;
            return _openPosition + hideTo switch
            {
                Direction.Up => new Vector2(0f, size.y + extraDistance),
                Direction.Down => new Vector2(0f, -(size.y + extraDistance)),
                Direction.Left => new Vector2(-(size.x + extraDistance), 0f),
                _ => new Vector2(size.x + extraDistance, 0f),
            };
        }
    }
}
