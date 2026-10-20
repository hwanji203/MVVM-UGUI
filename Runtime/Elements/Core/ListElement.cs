using MVVM.UGUI.Binding;

namespace MVVM.UGUI.Elements.Core
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