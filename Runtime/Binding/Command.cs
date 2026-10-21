using System;

namespace MVVM.UGUI.Binding
{
    public class Command : IResettable
    {
        private readonly Bindable<bool> _active;
        //Action이 생성자에서 대입될 경우는 reset할 경우에 이 Action을 사용하도록 한다.
        private readonly Action _initialAction;
        public Action Action { get; set; }

        // 버튼 같은 Element 는 이걸 구독해 눌 수 있는지 그린다
        public IReadOnlyBindable<bool> Active => _active;

        public Command(Action command = null, bool active = true)
        {
            Action = _initialAction = command;
            _active = new Bindable<bool>(active);
        }

        // 꺼져 있거나 동작이 없으면 아무것도 하지 않는다
        public void Execute()
        {
            if (_active.Value)
                Action?.Invoke();
        }

        public void SetEnabled(bool active)
            => _active.Value = active;

        // 생성자로 준 동작 · 켜짐으로 — Setup 이 넣은 콜백 · 이번에 잠근 것을 한 번에 되돌린다
        public void Reset()
        {
            Action = _initialAction;
            _active.Reset();
        }
    }

    public class Command<T> : IResettable
    {
        private readonly Bindable<bool> _active;
        private readonly Action<T> _initialAction;
        public Action<T> Action { get; set; }

        // 버튼 같은 Element 는 이걸 구독해 눌 수 있는지 그린다
        public IReadOnlyBindable<bool> Active => _active;

        public Command(Action<T> command = null, bool active = true)
        {
            Action = _initialAction = command;
            _active = new Bindable<bool>(active);
        }

        // 꺼져 있거나 동작이 없으면 아무것도 하지 않는다
        public void Execute(T argument)
        {
            if (_active.Value)
                Action?.Invoke(argument);
        }

        public void SetEnabled(bool active)
            => _active.Value = active;

        // Command 와 같다
        public void Reset()
        {
            Action = _initialAction;
            _active.Reset();
        }
    }
}
