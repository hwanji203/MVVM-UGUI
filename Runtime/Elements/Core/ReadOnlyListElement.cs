using System.Collections.Generic;
using MVVMSystem.Runtime.Binding;

namespace MVVMSystem.Runtime.Elements.Core
{
    public abstract class ReadOnlyListElement<T> : Element
    {
        private IDisposable _unSubListChange;
        private IDisposable _unSubElementChange;
        
        public virtual void Bind(IReadOnlyBindableList<T> source)
        {
            _unSubListChange?.Dispose();
            _unSubListChange = source.Bind(HandleValueChanged);
            _unSubElementChange?.Dispose();
            _unSubElementChange = source.Bind(HandleElementChanged);
        }

        public override void OnDestroy()
        {
            _unSubListChange?.Dispose();
            _unSubListChange = null;
            _unSubElementChange?.Dispose();
            _unSubElementChange = null;
            
            base.OnDestroy();
        }

        protected abstract void HandleValueChanged(IReadOnlyList<T> value);
        protected abstract void HandleElementChanged(int index, T value);
    }
}