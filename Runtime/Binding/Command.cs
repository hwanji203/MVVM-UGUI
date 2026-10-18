using System;

namespace MVVMSystem.Runtime.Binding
{
    public class Command
    {
        private readonly Bindable<bool> _active = new();
        public event Action CommandAction;

        public Command(Action command, bool active = true)
        {
            CommandAction = command;
            _active.Value = active;
        }

        public (Action, IDisposable) Bind(Action<bool> action)
        {
            void Call()
            {
                if (_active.Value)
                    CommandAction?.Invoke();
            }
            
            IDisposable unSub = _active.Bind(action);
            return (Call, unSub);
        }

        public void SetEnabled(bool active)
            => _active.Value = active;
    }
}