using System.Collections.Generic;
using MVVM.UGUI.Binding;
using MVVM.UGUI.Core;
using MVVM.UGUI.Elements.Core;
using UnityEngine;

namespace MVVM.UGUI.Elements.Lists
{
    /// <summary>
    /// BindableList 의 항목마다 항목 프리팹 (<see cref="ItemView{TItem}"/>) 을 하나씩 그린다 (인벤토리 · 손패 · 상점 목록).
    /// 만든 항목 View 는 지우지 않고 꺼 두었다가 다시 쓴다. 배치는 container 의 LayoutGroup 이 정한다.
    /// 프리팹의 항목 타입이 목록과 다르면 Bind 할 때 오류를 낸다.
    /// </summary>
    public class ItemListElement : Element
    {
        [Tooltip("항목 하나의 프리팹 (ItemView<T> 가 루트에 붙은 것)")]
        [SerializeField] private ItemView itemPrefab;
        [Tooltip("항목을 넣을 부모 (비우면 자기 자신)")]
        [SerializeField] private Transform container;

        private readonly List<ItemView> _views = new();

        public void Bind<TItem>(IReadOnlyBindableList<TItem> source)
        {
            Unbind();

            if (itemPrefab == null || itemPrefab.ItemType != typeof(TItem))
            {
                Debug.LogError($"[MVVM UGUI] {name}: 항목 프리팹이 ItemView<{typeof(TItem).Name}> 가 아니다 " +
                               $"({(itemPrefab == null ? "비어 있음" : itemPrefab.ItemType.Name)}).", this);
                return;
            }

            // 개수가 바뀌면 전부 다시 맞추고, 한 칸만 바뀌면 그 View 만 다시 Bind
            Bindings.Add(source.Bind(Sync));
            Bindings.Add(source.Bind((index, item) => _views[index].SetItem(item)));
        }

        public override void Unbind()
        {
            base.Unbind();
            for (int i = 0; i < _views.Count; i++)
                Hide(_views[i]);
        }

        private void Sync<TItem>(IReadOnlyList<TItem> items)
        {
            // i 번째 항목은 늘 i 번째 View — 모자라면 만들고, 남으면 끈다
            for (int i = 0; i < items.Count; i++)
            {
                var view = i < _views.Count ? _views[i] : Create();
                view.gameObject.SetActive(true);
                view.SetItem(items[i]);
            }

            for (int i = items.Count; i < _views.Count; i++)
                Hide(_views[i]);
        }

        private ItemView Create()
        {
            var view = Instantiate(itemPrefab, container != null ? container : transform);
            _views.Add(view);
            return view;
        }

        private static void Hide(ItemView view)
        {
            if (view == null || !view.gameObject.activeSelf)
                return;
            view.ClearItem();
            view.gameObject.SetActive(false);
        }
    }
}
