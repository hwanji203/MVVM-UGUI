using System;
using UnityEngine;

namespace MVVM.UGUI
{
    /// <summary>
    /// 비워 둬도 되는 직렬화 필드 (팝업의 닫기 버튼 · 배경 등). 인스펙터에서 이름이 기울임꼴로 보인다.
    /// 표시만 한다 — 비었는지 검사하지 않으니 쓰는 쪽에서 null 을 확인한다. 목록 필드에 붙이면 각 요소에 적용된다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NullableAttribute : PropertyAttribute
    {
    }
}
