using MVVM.UGUI.Sound;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVVM.UGUI.Elements.Commands
{
    /// <summary>
    /// 잠기면 모양이 바뀌는 버튼 (옛 LockButtonForm) — 자물쇠 아이콘을 켜고, 글을 "잠김" 으로 바꾼다.
    /// 잠긴 채 누르면 lockedSound (막힌 소리).
    /// </summary>
    public class LockButtonElement : ButtonElement, IPointerClickHandler
    {
        [Tooltip("잠겼을 때 켤 오브젝트 (자물쇠 아이콘)")]
        [SerializeField] private GameObject lockedMark;
        [Tooltip("잠겼을 때 글을 바꿀 TMP (비우면 글은 그대로)")]
        [SerializeField] private TMP_Text label;
        [SerializeField] private string lockedText = "잠김";
        [SerializeField] private UISoundSO lockedSound;

        private string _unlockedText;
        private bool _textRead;

        protected override void HandleActiveChanged(bool enable)
        {
            base.HandleActiveChanged(enable);

            if (lockedMark != null)
                lockedMark.SetActive(!enable);

            if (label == null)
                return;
            if (!_textRead)
            {
                _textRead = true;
                _unlockedText = label.text;
            }
            label.text = enable ? _unlockedText : lockedText;
        }

        // Button 이 잠기면 onClick 이 오지 않으므로 클릭 자체를 따로 받는다
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!button.interactable && lockedSound != null)
                lockedSound.Play();
        }
    }
}
