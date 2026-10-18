using System;
using System.Collections.Generic;

namespace MVVMGUI.Binding
{
    public interface IReadOnlyBindableList<T>
    {
        public IReadOnlyList<T> Items { get; }
        
        public T this[int index] { get; }
        
        public IDisposable Bind(Action<IReadOnlyList<T>> onChanged);
        public IDisposable Bind(Action<int, T> onChanged);
    }
}