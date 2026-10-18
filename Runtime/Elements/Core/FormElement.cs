using MVVMGUI.Binding;
using UnityEngine;

namespace MVVMGUI.Elements.Core
{
    public abstract class FormElement<T> : VisualElement<T>
    {
        protected Bindable<T> Source;

        public sealed override void Bind(IReadOnlyBindable<T> source)
        {
            Source = source as Bindable<T>;
            base.Bind(source);
        }
    }
}