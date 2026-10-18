using MVVMGUI.Elements.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MVVMGUI.Elements
{
    [RequireComponent(typeof(Button))]
    public class ButtonElement : CommandElement
    {
        [SerializeField] private Button button;

        protected override void Awake()
        {
            base.Awake();

            button.onClick.AddListener(() => CallCommand?.Invoke());
        }

        protected override void HandleActiveChanged(bool enable)
            => button.interactable = enable;
        
        private void OnValidate()
        {
            if (button == null)
                button = GetComponent<Button>();
        }
    }
}