using System;

namespace MVVM.UGUI.Binding
{
    public class Command
    {
        private readonly Bindable<bool> _active = new();
        public Action Action { get; set; }

        // 버튼 같은 Element 는 이걸 구독해 눌 수 있는지 그린다
        public IReadOnlyBindable<bool> Active => _active;

        public Command(Action command = null, bool active = true)
        {
            Action = command;
            _active.Value = active;
        }

        // 꺼져 있거나 동작이 없으면 아무것도 하지 않는다
        public void Execute()
        {
            if (_active.Value)
                Action?.Invoke();
        }

        public void SetEnabled(bool active)
            => _active.Value = active;
    }

    public class Command<T>
    {
        private readonly Bindable<bool> _active = new();
        public Action<T> Action { get; set; }

        // 버튼 같은 Element 는 이걸 구독해 눌 수 있는지 그린다
        public IReadOnlyBindable<bool> Active => _active;

        public Command(Action<T> command = null, bool active = true)
        {
            Action = command;
            _active.Value = active;
        }

        // 꺼져 있거나 동작이 없으면 아무것도 하지 않는다
        public void Execute(T argument)
        {
            if (_active.Value)
                Action?.Invoke(argument);
        }

        public void SetEnabled(bool active)
            => _active.Value = active;
    }
}
