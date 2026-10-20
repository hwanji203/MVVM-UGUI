using MVVM.UGUI.Binding;

namespace MVVM.UGUI.Elements.Core
{
    /// <summary>
    /// 일회성 알림 (<see cref="Signal{T}"/>) 을 받는 Element. 꺼져 있을 때 온 알림은 버린다 —
    /// 닫혀 숨은 화면에 토스트 · 데미지 숫자가 쌓이지 않게 (사용자 10-20).
    /// </summary>
    public abstract class SignalElement<T> : Element
    {
        public void Bind(IReadOnlySignal<T> source)
        {
            Unbind();
            Bindings.Add(source.Bind(OnFired));
        }

        private void OnFired(T value)
        {
            if (isActiveAndEnabled)
                HandleSignal(value);
        }

        protected abstract void HandleSignal(T value);
    }
}
