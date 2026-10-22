using MVVM.UGUI.Elements.Core;
using MVVM.UGUI.Sound;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MVVM.UGUI.Elements.Commands
{
    /// <summary>
    /// 길게 눌러야 실행되는 버튼 (삭제 · 포기 확인). 누르는 동안 progressFill 이 차고, 다 차면 Command 실행.
    /// 중간에 떼거나 밖으로 나가면 처음부터. Raycast Target 인 Graphic 이 필요하다.
    /// </summary>
    public class HoldButtonElement : CommandElement, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private float holdSeconds = 1f;
        [Tooltip("누른 정도를 보여줄 Image (Filled). 비워도 된다")]
        [SerializeField] private Image progressFill;
        [Tooltip("잠겼을 때 흐리게 할 CanvasGroup (비워도 된다)")]
        [SerializeField] private CanvasGroup group;
        [SerializeField, Range(0f, 1f)] private float lockedAlpha = 0.5f;
        [SerializeField] private UISoundSO completeSound;

        private bool _active;
        private bool _holding;
        private float _held;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_active)
                return;
            _holding = true;
            _held = 0f;
        }

        public void OnPointerUp(PointerEventData eventData) => Cancel();

        public void OnPointerExit(PointerEventData eventData) => Cancel();

        private void Update()
        {
            if (!_holding)
                return;

            // 일시정지 팝업 안에서도 눌리도록 timeScale 을 무시한다
            _held += Time.unscaledDeltaTime;
            SetProgress(_held / holdSeconds);
            if (_held < holdSeconds)
                return;

            // 실행 전에 먼저 멈춘다 — 실행이 화면을 닫아도 다음 프레임에 또 실행되지 않게
            Cancel();
            if (completeSound)
                completeSound.Play();
            Execute();
        }

        private void OnDisable() => Cancel();

        private void Cancel()
        {
            _holding = false;
            _held = 0f;
            SetProgress(0f);
        }

        private void SetProgress(float ratio)
        {
            if (progressFill)
                progressFill.fillAmount = Mathf.Clamp01(ratio);
        }

        protected override void HandleActiveChanged(bool enable)
        {
            _active = enable;
            if (!enable)
                Cancel();
            if (group)
                group.alpha = enable ? 1f : lockedAlpha;
        }

        private void Reset() => group = GetComponent<CanvasGroup>();
    }
}
