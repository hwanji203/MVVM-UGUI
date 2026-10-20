using System;

namespace MVVMGUI.Binding
{
    /// <summary>값을 들지 않는 일회성 알림의 읽기 쪽. Element 는 이것만 받는다.</summary>
    public interface IReadOnlySignal<T>
    {
        /// <summary>구독한다. Bindable 과 달리 구독하는 순간에는 부르지 않는다.</summary>
        public IDisposable Bind(Action<T> onFired);
    }
}
