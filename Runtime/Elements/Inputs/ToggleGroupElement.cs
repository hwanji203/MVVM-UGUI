using MVVM.UGUI.Elements.Core;
using MVVM.UGUI.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.UGUI.Elements.Inputs
{
    /// <summary>
    /// 여러 토글 중 하나만 고른다 ↔ Bindable&lt;int&gt; (난이도 · 정렬 기준). Unity ToggleGroup 없이 이 Element 가 하나만 켠다.
    /// 이미 켜진 것을 다시 눌러도 꺼지지 않는다.
    /// </summary>
    public class ToggleGroupElement : FormElement<int>
    {
        [Tooltip("붙이는 순간 자식들의 Toggle 로 채운다")]
        [SerializeField] protected Toggle[] toggles;
        [SerializeField] private UISoundSO clickSound;

        protected override void Awake()
        {
            base.Awake();
            for (int i = 0; i < toggles.Length; i++)
            {
                int index = i;   // 람다가 반복 변수 대신 이 칸의 번호를 잡게
                toggles[i].onValueChanged.AddListener(isOn => OnToggled(index, isOn));
            }
        }

        private void OnToggled(int index, bool isOn)
        {
            if (!isOn)
            {
                // 고른 것을 다시 눌러 끄려 하면 되돌린다
                if (Source != null && Source.Value == index)
                    toggles[index].SetIsOnWithoutNotify(true);
                return;
            }

            if (clickSound)
                clickSound.Play();
            Write(index);
        }

        protected override void HandleValueChanged(int index)
        {
            for (int i = 0; i < toggles.Length; i++)
                toggles[i].SetIsOnWithoutNotify(i == index);
        }

        private void Reset() => toggles = GetComponentsInChildren<Toggle>(true);
    }
}
