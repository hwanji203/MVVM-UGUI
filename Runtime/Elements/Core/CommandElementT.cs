using MVVM.UGUI.Binding;

namespace MVVM.UGUI.Elements.Core
{
    /// <summary>인자 있는 Command 를 실행하는 Element (끌어다 놓은 곳 번호 등).</summary>
    public abstract class CommandElement<T> : Element
    {
        private Command<T> _command;

        // 눌림을 직접 받는 Element (끌기) 가 시작할지 정할 때 본다 — 묶인 Command 가 없으면 꺼진 것으로
        protected bool IsActive => _command != null && _command.Active.Value;

        public void Bind(Command<T> source)
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

        protected void Execute(T argument) => _command?.Execute(argument);

        protected abstract void HandleActiveChanged(bool enable);
    }
}
