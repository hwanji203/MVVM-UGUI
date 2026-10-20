using MVVM.UGUI.Elements.Core;
using MVVM.UGUI.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.UGUI.Elements
{
    /// <summary>버튼 → Command. Command 의 Active 가 꺼지면 잠긴다. 눌리면 스스로 클릭 소리를 낸다 (DESIGN 3.5).</summary>
    [RequireComponent(typeof(Button))]
    public class ButtonElement : CommandElement
    {
        [SerializeField] protected Button button;
        [SerializeField] private UISoundSO clickSound;

        protected override void Awake()
        {
            base.Awake();
            button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            if (clickSound != null)
                clickSound.Play();
            Execute();
        }

        protected override void HandleActiveChanged(bool enable) => button.interactable = enable;

        // 컴포넌트를 붙이는 순간 (또는 인스펙터의 Reset) 같은 오브젝트의 Button 을 잡는다
        private void Reset() => button = GetComponent<Button>();
    }
}
