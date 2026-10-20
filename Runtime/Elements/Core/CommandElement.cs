using MVVM.UGUI.Binding;

namespace MVVM.UGUI.Elements.Core
{
    public abstract class CommandElement : Element
    {
        private Command _command;

        public void Bind(Command source)
        {
            Unbind();
            _command = source;
            Bindings.Add(source.Active.Bind(HandleActiveChanged));
        }

        public override void Unbind()
        {
            base.Unbind();
            _command = null;
        }

        // 눌렸을 때 하위 Element 가 부른다
        protected void Execute() => _command?.Execute();

        protected abstract void HandleActiveChanged(bool enable);
    }
}
