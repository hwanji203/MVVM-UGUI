using System.Collections.Generic;
using MVVMGUI.Binding;

namespace MVVMGUI.Elements.Core
{
    public abstract class ReadOnlyListElement<T> : Element
    {
        public virtual void Bind(IReadOnlyBindableList<T> source)
        {
            Unbind();
            Bindings.Add(source.Bind(HandleValueChanged));
            Bindings.Add(source.Bind(HandleElementChanged));
        }

        protected abstract void HandleValueChanged(IReadOnlyList<T> value);
        protected abstract void HandleElementChanged(int index, T value);
    }
}
