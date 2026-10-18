using System;
using MVVMGUI.Binding;

namespace MVVMGUI.Elements.Core
{
    public abstract class CommandElement : Element
    {
        protected Action CallCommand;
        private IDisposable _unSubValueChange;

        public void Bind(Command source)
        {
            _unSubValueChange?.Dispose();
            (CallCommand, _unSubValueChange) = source.Bind(HandleActiveChanged);
        }

        public override void OnDestroy()
        {
            _unSubValueChange?.Dispose();
            _unSubValueChange = null;
            CallCommand = null;

            base.OnDestroy();
        }

        protected abstract void HandleActiveChanged(bool enable);
    }
}