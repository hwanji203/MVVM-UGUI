using MVVMGUI.Runtime.Elements.Core;
using TMPro;
using UnityEngine;

namespace MVVMGUI.Runtime.Elements
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextViewElement : VisualElement<string>
    {
        [SerializeField] private TextMeshProUGUI tmp;
        
        protected override void HandleValueChanged(string value)
        {
            tmp.text = value;
        }

        private void OnValidate()
        {
            if (tmp == null)
                tmp = GetComponent<TextMeshProUGUI>();
        }
    }
}