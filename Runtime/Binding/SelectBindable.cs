using System;

namespace MVVMGUI.Binding
{
    public class SelectBindable<T, T1> : IReadOnlyBindable<T>, IDisposable
    {
        private IDisposable _unSub;

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

        public SelectBindable(IReadOnlyBindable<T1> item1, Func<T1, T> action)
        {
            if (item1 is null || action is null)
                throw new ArgumentNullException();

            _unSub = item1.Bind(i1 => Value = action(i1));
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
            _unSub?.Dispose();
            _unSub = null;
        }
    }
}