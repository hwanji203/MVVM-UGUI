using System;

namespace MVVMGUI.Binding
{
    public class Command
    {
        private readonly Bindable<bool> _active = new();
        public event Action CommandAction;

        // 버튼 같은 Element 는 이걸 구독해 눌 수 있는지 그린다
        public IReadOnlyBindable<bool> Active => _active;

        public Command(Action command, bool active = true)
        {
            CommandAction = command;
            _active.Value = active;
        }

        // 꺼져 있으면 아무것도 하지 않는다
        public void Execute()
        {
            if (_active.Value)
                CommandAction?.Invoke();
        }

        public void SetEnabled(bool active)
            => _active.Value = active;
    }

    public class Command<T>
    {        
        private readonly Bindable<bool> _active = new();
        public event Action<T> CommandAction;

        // 버튼 같은 Element 는 이걸 구독해 눌 수 있는지 그린다
        public IReadOnlyBindable<bool> Active => _active;

        public Command(Action<T> command, bool active = true)
        {
            CommandAction = command;
            _active.Value = active;
        }

        // 꺼져 있으면 아무것도 하지 않는다
        public void Execute(T argument)
        {
            if (_active.Value)
                CommandAction?.Invoke(argument);
        }

        public void SetEnabled(bool active)
            => _active.Value = active;
    }
}
