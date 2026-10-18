using UnityEngine;

namespace MVVMGUI.Core
{
    public abstract class ScreenView<T> : MonoBehaviour where T : AbstractViewModel
    {
        private T _vm;

        private void Awake()
        {
            _vm = CreateVM();
            Initialize();
            Bind(_vm);
        }

        /// <summary>
        /// Bind를 해서 값이 채워지기 전 정리
        /// </summary>
        protected virtual void Initialize() { }

        /// <summary>
        /// 여기서 vm에 있는 Bindable들을 Bind한다.
        /// </summary>
        protected virtual void Bind(T vm) { }

        // 하위 View 가 OnDestroy 를 쓰면 override 하고 base.OnDestroy() 를 부른다
        protected virtual void OnDestroy()
        {
            _vm?.Dispose();
            _vm = null;
        }

        protected abstract T CreateVM();
    }
}