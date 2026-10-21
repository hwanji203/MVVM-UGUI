using System;

namespace MVVM.UGUI.Binding
{
    public class Command
    {
        private readonly Bindable<bool> _active = new();
        private Action _action;

        // 버튼 같은 Element 는 이걸 구독해 눌 수 있는지 그린다
        public IReadOnlyBindable<bool> Active => _active;

        public Command(Action command = null, bool active = true)
        {
            _action = command;
            _active.Value = active;
        }

        // 꺼져 있거나 동작이 없으면 아무것도 하지 않는다
        public void Execute()
        {
            if (_active.Value)
                _action?.Invoke();
        }

        // Bind 는 Awake 에서 한 번이라 Command 객체는 그대로 두고, 열 때마다 바뀌는 동작만 VM 의 Setup 에서 갈아 끼운다
        public void SetAction(Action command)
            => _action = command;

        public void SetEnabled(bool active)
            => _active.Value = active;
    }

    public class Command<T>
    {
        private readonly Bindable<bool> _active = new();
        private Action<T> _action;

        // 버튼 같은 Element 는 이걸 구독해 눌 수 있는지 그린다
        public IReadOnlyBindable<bool> Active => _active;

        public Command(Action<T> command = null, bool active = true)
        {
            _action = command;
            _active.Value = active;
        }

        // 꺼져 있거나 동작이 없으면 아무것도 하지 않는다
        public void Execute(T argument)
        {
            if (_active.Value)
                _action?.Invoke(argument);
        }

        // Command 와 같다 — 객체는 그대로, 동작만 갈아 끼운다
        public void SetAction(Action<T> command)
            => _action = command;

        public void SetEnabled(bool active)
            => _active.Value = active;
    }
}
