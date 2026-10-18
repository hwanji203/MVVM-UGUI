using System;

namespace MVVMSystem.Runtime.Binding
{
    public class CombineBindable<T, T1, T2> : IReadOnlyBindable<T>, IDisposable
    {
        private Action _unSubAll;

        private T _value = default;

        public T Value
        {
            get => _value;
            private set
            {
                if (Equals(value, _value))
                    return;
                _value = value;
                _valueChanged?.Invoke(_value);
            }
        }

        private Action<T> _valueChanged;

        public CombineBindable(IReadOnlyBindable<T1> item1, IReadOnlyBindable<T2> item2, Func<T1, T2, T> action)
        {
            if (item1 is null || item2 is null || action is null)
                throw new ArgumentNullException();

            IDisposable unSubItem1 = item1.Bind(i1 => Value = action(i1, item2.Value));
            IDisposable unSubItem2 = item2.Bind(i2 => Value = action(item1.Value, i2));

            _unSubAll = () =>
            {
                unSubItem1.Dispose();
                unSubItem2.Dispose();
            };
        }

        //UnBind를 할 수 있는 델리게이트를 반환.
        public IDisposable Bind(Action<T> onChanged)
        {
            _valueChanged += onChanged;
            onChanged?.Invoke(_value);
            return new UnSubscriber(() => _valueChanged -= onChanged);
        }

        public void Dispose()
        {
            _unSubAll?.Invoke();
            _unSubAll = null;
        }
    }
}