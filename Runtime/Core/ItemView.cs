using System;
using MVVM.UGUI.Elements.Core;
using UnityEngine;

namespace MVVM.UGUI.Core
{
    /// <summary>
    /// 목록 (<see cref="MVVM.UGUI.Elements.Lists.ItemListElement"/>) 의 항목 하나를 그리는 View 의 바탕 (제네릭 아님).
    /// 목록은 이것만 보고 항목을 넣고 뺀다. 항목 View 는 <see cref="ItemView{TItem}"/> 를 상속한다.
    /// </summary>
    public abstract class ItemView : MonoBehaviour
    {
        internal abstract Type ItemType { get; }

        /// <summary>목록에서 빠질 때 — 안의 Element 연결을 끊어 숨은 항목이 지난 값을 계속 받지 않게 한다.</summary>
        internal virtual void ClearItem()
        {
            foreach (var element in GetComponentsInChildren<Element>(true))
                element.Unbind();
        }
    }

    /// <summary>
    /// 항목 하나의 View. ScreenView 와 달리 ViewModel (항목) 을 스스로 만들지 않고 목록이 넣어 준다 —
    /// 항목은 목록을 가진 ViewModel 의 것이라 여기서 Dispose 하지 않는다.
    /// Element 는 ScreenView 처럼 [SerializeField] 필드로 들고, 에디터의 "이름으로 채우기" 로 채울 수 있다.
    /// </summary>
    public abstract class ItemView<TItem> : ItemView
    {
        /// <summary>지금 그리는 항목.</summary>
        protected TItem Item { get; private set; }

        internal override Type ItemType => typeof(TItem);

        // 목록이 ItemView<TItem> 로 형변환해 바로 부른다 — object 를 거치지 않아 struct 항목도 박싱되지 않는다
        internal void SetItem(TItem item)
        {
            Item = item;
            Bind(Item);
        }

        internal override void ClearItem()
        {
            base.ClearItem();
            Item = default;
        }

        /// <summary>여기서 항목의 Bindable 들을 Element 에 Bind 한다. 같은 View 가 다른 항목으로 여러 번 불릴 수 있다.</summary>
        protected abstract void Bind(TItem item);
    }
}
