using System;
using MVVMGUI.Binding;

namespace MVVMGUI.Elements.Core
{
    public abstract class CommandElement : Element
    {
        protected Action CallCommand;

        public void Bind(Command source)
        {
            Unbind();
            var (call, unSub) = source.Bind(HandleActiveChanged);
            CallCommand = call;
            Bindings.Add(unSub);
        }

        public override void Unbind()
        {
            base.Unbind();
            CallCommand = null;
        }

        protected abstract void HandleActiveChanged(bool enable);
    }
}
