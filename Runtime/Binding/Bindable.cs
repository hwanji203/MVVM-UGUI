using System;
using System.Collections.Generic;

namespace MVVM.UGUI.Binding
{
    public class Bindable<T> : IReadOnlyBindable<T>, IResettable
    {
        private readonly T _initial;
        private T _value;

        // Reset 이 돌아갈 값. 객체 초기화 ({ Value = 1 }) 는 생성 뒤라 초기값이 아니다
        public Bindable(T initial = default)
        {
            _initial = initial;
            _value = initial;
        }

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(value, _value))
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

        // 생성자로 준 값으로 — 이미 그 값이면 알리지 않는다 (Value 와 같은 규칙)
        public void Reset() => Value = _initial;
    }

    public static class Bindable
    {
        //Bindable을 받아서 그 값이 변경되면 Select하여 값의 변경을 알리는 Bindable을 반환하기
        public static SelectBindable<TR, T1> Select<TR, T1>(IReadOnlyBindable<T1> item1, Func<T1, TR> action)
            => new (item1, action);
        
        //Bindable을 2개 받아서 둘 중 하나가 변경되면 action에 따라 값을 변경하는 Bindable 반환.
        public static CombineBindable<TR, T1, T2> Combine<TR, T1, T2>(IReadOnlyBindable<T1> item1, 
            IReadOnlyBindable<T2> item2, Func<T1, T2, TR> action) =>
            new (item1, item2, action);
    }
}
