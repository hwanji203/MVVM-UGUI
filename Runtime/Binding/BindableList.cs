using System;
using System.Collections.Generic;

namespace MVVM.UGUI.Binding
{
    public class BindableList<T> : IReadOnlyBindableList<T>, IResettable
    {
        private readonly List<T> _value = new();
        
        public IReadOnlyList<T> Items => _value;
        public int Count => _value.Count;

        public T this[int index]
        {
            get => _value[index];
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value[index], value))
                    return;
                
                _value[index] = value;
                _elementChanged?.Invoke(index, value);
            }
        }

        private Action<IReadOnlyList<T>> _valueChanged;
        private Action<int, T> _elementChanged;

        //UnBind를 할 수 있는 델리게이트를 반환.
        public IDisposable Bind(Action<IReadOnlyList<T>> onChanged)
        {
            _valueChanged += onChanged;
            onChanged?.Invoke(_value);
            return new UnSubscriber(() => _valueChanged -= onChanged);
        }

        public IDisposable Bind(Action<int, T> onChanged)
        {
            _elementChanged += onChanged;
            return new UnSubscriber(() => _elementChanged -= onChanged);
        }

        public void Add(T item)
        {
            _value.Add(item);
            NotifyList();
        }

        // 목록에 없으면 바뀐 게 없으므로 알리지 않는다
        public bool Remove(T item)
        {
            if (!_value.Remove(item))
                return false;

            NotifyList();
            return true;
        }

        public void RemoveAt(int index)
        {
            _value.RemoveAt(index);
            NotifyList();
        }

        public void Clear()
        {
            _value.Clear();
            NotifyList();
        }

        // 만든 직후 (빈 목록) 로 — 이미 비어 있으면 알리지 않는다
        public void Reset()
        {
            if (_value.Count > 0)
                Clear();
        }

        private void NotifyList() => _valueChanged?.Invoke(_value);
    }
}