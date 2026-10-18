using System;
using System.Collections.Generic;

namespace MVVMSystem.Runtime.Binding
{
    public class BindableList<T> : IReadOnlyBindableList<T>
    {
        private readonly List<T> _value = new();
        
        public IReadOnlyList<T> Items => _value;
        public int Count => _value.Count;

        public T this[int index]
        {
            get => _value[index];
            set
            {
                if (Equals(_value[index], value))
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
            _valueChanged?.Invoke(_value);
        }

        public void Remove(T item)
        {
            _value.Remove(item);
            _valueChanged?.Invoke(_value);
        }

        public void RemoveAt(int index)
        {
            _value.RemoveAt(index);
            _valueChanged?.Invoke(_value);
        }

        public void Clear()
        {
            _value.Clear();
            _valueChanged?.Invoke(_value);
        }
    }
}