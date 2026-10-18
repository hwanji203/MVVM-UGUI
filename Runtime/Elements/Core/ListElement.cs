using MVVMGUI.Runtime.Binding;

namespace MVVMGUI.Runtime.Elements.Core
{
    public abstract class ListElement<T> : ReadOnlyListElement<T>
    {
        protected BindableList<T> Source;
        
        public sealed override void Bind(IReadOnlyBindableList<T> source)
        {
            Source = source as BindableList<T>;
            base.Bind(source);
        }
    }
}