using MVVM.UGUI.Binding;

namespace MVVM.UGUI.Elements.Core
{
    public abstract class VisualElement<T> : Element
    {
        public virtual void Bind(IReadOnlyBindable<T> source)
        {
            Unbind();
            Bindings.Add(source.Bind(HandleValueChanged));
        }

        protected abstract void HandleValueChanged(T value);
    }
}
