using MVVM.UGUI.Elements.Core;
using MVVM.UGUI.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.UGUI.Elements.Inputs
{
    /// <summary>토글 ↔ Bindable&lt;bool&gt;. 누르면 클릭 소리.</summary>
    [RequireComponent(typeof(Toggle))]
    public class ToggleElement : FormElement<bool>
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private UISoundSO clickSound;

        protected override void Awake()
        {
            base.Awake();
            toggle.onValueChanged.AddListener(OnToggled);
        }

        private void OnToggled(bool isOn)
        {
            if (clickSound)
                clickSound.Play();
            Write(isOn);
        }

        protected override void HandleValueChanged(bool value) => toggle.SetIsOnWithoutNotify(value);

        private void Reset() => toggle = GetComponent<Toggle>();
    }
}
