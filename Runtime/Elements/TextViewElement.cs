using MVVMGUI.Elements.Core;
using TMPro;
using UnityEngine;

namespace MVVMGUI.Elements
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextViewElement : VisualElement<string>
    {
        [SerializeField] private TextMeshProUGUI tmp;
        
        protected override void HandleValueChanged(string value)
        {
            tmp.text = value;
        }

        // 컴포넌트를 붙이는 순간 (또는 인스펙터의 Reset) 같은 오브젝트의 TMP 를 잡는다
        private void Reset()
        {
            tmp = GetComponent<TextMeshProUGUI>();
        }
    }
}