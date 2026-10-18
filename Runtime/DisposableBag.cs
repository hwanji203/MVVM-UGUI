using System;
using System.Collections.Generic;

namespace MVVMGUI
{
    /// <summary>해제 핸들 여러 개를 담았다가 한 번에 푼다. 비운 뒤에도 다시 담을 수 있다 (다시 묶을 때).</summary>
    public class DisposableBag : IDisposable
    {
        private readonly List<IDisposable> _items = new();

        public void Add(IDisposable item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _items.Add(item);
        }

        public void Clear()
        {
            foreach (var item in _items)
                item.Dispose();
            _items.Clear();
        }

        public void Dispose() => Clear();
    }
}
