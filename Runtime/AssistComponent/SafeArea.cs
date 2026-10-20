using UnityEngine;

namespace MVVMGUI.AssistComponent
{
    /// <summary>
    /// 휴대폰의 노치 · 둥근 모서리 아래로 UI 가 들어가지 않게 이 RectTransform 을 Screen.safeArea 에 맞춘다 (모바일용).
    /// 화면 프리팹 루트 바로 아래의 "내용" 오브젝트에 붙이고, 배경은 그 밖에 두어 끝까지 칠한다. 화면을 돌리면 다시 맞춘다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        private Rect _applied;
        private Vector2Int _screen;

        private void OnEnable() => Apply();

        // 화면 회전 · 창 크기 변화를 알려 주는 이벤트가 없어 매 프레임 비교한다 (값 비교라 싸다)
        private void Update()
        {
            if (Screen.safeArea != _applied || Screen.width != _screen.x || Screen.height != _screen.y)
                Apply();
        }

        private void Apply()
        {
            _applied = Screen.safeArea;
            _screen = new Vector2Int(Screen.width, Screen.height);
            if (_screen.x <= 0 || _screen.y <= 0)
                return;

            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(_applied.xMin / _screen.x, _applied.yMin / _screen.y);
            rect.anchorMax = new Vector2(_applied.xMax / _screen.x, _applied.yMax / _screen.y);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
