using System;

namespace MVVMSystem.Runtime.Binding
{
    public class Bindable<T> : IReadOnlyBindable<T>
    {
        private T _value = default;

        public T Value
        {
            get => _value;
            set
            {
                if (Equals(value, _value))
                    return;
                _value = value;
                _valueChanged?.Invoke(_value);
            }
        }

        private Action<T> _valueChanged;

        //UnBind를 할 수 있는 델리게이트를 반환.
        public IDisposable Bind(Action<T> onChanged)
        {
            _valueChanged += onChanged;
            onChanged?.Invoke(_value);
            return new UnSubscriber(() => _valueChanged -= onChanged);
        }
    }
    
    public static class Bindable
    {
        //Bindable을 받아서 그 값이 변경되면 Select하여 값의 변경을 알리는 Bindable을 반환하기
        public static (IReadOnlyBindable<TR> bindable, IDisposable unSub) Select<TR, T1>(IReadOnlyBindable<T1> item1, Func<T1, TR> action)
        {
            if (item1 is null || action is null)
                throw new ArgumentNullException();

            Bindable<TR> newBindable = new Bindable<TR>();
            
            IDisposable unSubItem1 = item1.Bind(i1 => newBindable.Value = action(i1));
            
            return (newBindable, unSubItem1);
        }
        
        //Bindable을 2개 받아서 둘 중 하나가 변경되면 action에 따라 값을 변경하는 Bindable 반환.
        public static CombineBindable<TR, T1, T2> Combine<TR, T1, T2>(IReadOnlyBindable<T1> item1, 
            IReadOnlyBindable<T2> item2, Func<T1, T2, TR> action) =>
            new (item1, item2, action);
    }
}
