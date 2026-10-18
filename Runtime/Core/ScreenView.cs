using UnityEngine;

namespace MVVMGUI.Core
{
    public abstract class ScreenView<T> : MonoBehaviour where T : AbstractViewModel
    {
        private T _vm;

        private void Awake()
        {
            _vm = CreateVM();
            Bind(_vm);
        }

        // 하위 View 가 OnDestroy 를 쓰면 override 하고 base.OnDestroy() 를 부른다
        protected virtual void OnDestroy()
        {
            _vm?.Dispose();
            _vm = null;
        }

        protected abstract T CreateVM();

        protected abstract void Bind(T vm);
    }
}