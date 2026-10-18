using UnityEngine;

namespace MVVMSystem.Runtime.Core
{
    public abstract class AbstractView<T> : MonoBehaviour where T : AbstractViewModel, new()
    {
        private T _vm;

        private void Awake()
        {
            _vm = CreateVM();
            Bind(_vm);
        }

        protected abstract T CreateVM();

        protected abstract void Bind(T vm);
    }
}