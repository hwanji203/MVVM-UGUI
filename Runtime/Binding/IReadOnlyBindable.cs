using System;

namespace MVVMSystem.Runtime.Binding
{
    public interface IReadOnlyBindable<T>
    {
        public T Value { get; }

        public IDisposable Bind(Action<T> onChanged);
    }
}