using MVVMSystem.Runtime.Binding;

namespace MVVMSystem.Runtime.Elements.Core
{
    public abstract class VisualElement<T> : Element
    {
        private IDisposable _unSubValueChange;
        
        public virtual void Bind(IReadOnlyBindable<T> source)
        {
            _unSubValueChange?.Dispose();
            _unSubValueChange = source.Bind(HandleValueChanged);
        }

        public override void OnDestroy()
        {
            _unSubValueChange?.Dispose();
            _unSubValueChange = null;
            
            base.OnDestroy();
        }

        protected abstract void HandleValueChanged(T value);
    }
}