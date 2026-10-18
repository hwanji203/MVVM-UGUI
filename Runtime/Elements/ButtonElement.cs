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

            button.onClick.AddListener(Execute);
        }

        protected override void HandleActiveChanged(bool enable)
            => button.interactable = enable;

        // 컴포넌트를 붙이는 순간 (또는 인스펙터의 Reset) 같은 오브젝트의 Button 을 잡는다
        private void Reset()
        {
            button = GetComponent<Button>();
        }
    }
}
