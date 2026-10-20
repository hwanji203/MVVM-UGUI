using System;

namespace MVVM.UGUI.Binding
{
    /// <summary>
    /// "방금 이런 일이 있었다" 를 알리는 통로 (토스트 · 데미지 숫자 · 한 번 하는 연출).
    /// 값을 저장하지 않아 같은 값도 매번 알리고, 구독할 때 지난 값을 다시 부르지 않는다.
    /// 구독 전에 Fire 된 것은 사라진다.
    /// </summary>
    public class Signal<T> : IReadOnlySignal<T>
    {
        private Action<T> _fired;

        public void Fire(T value) => _fired?.Invoke(value);

        public IDisposable Bind(Action<T> onFired)
        {
            _fired += onFired;
            return new UnSubscriber(() => _fired -= onFired);
        }
    }
}
