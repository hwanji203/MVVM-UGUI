using System;

namespace MVVMGUI.Binding
{
    public class CombineBindable<T, T1, T2> : IReadOnlyBindable<T>, IDisposable
    {
        // 값 저장 · 같은 값 무시 · 알림은 Bindable 에 맡긴다
        private readonly Bindable<T> _result = new();
        private readonly DisposableBag _sources = new();

        public T Value => _result.Value;

        public CombineBindable(IReadOnlyBindable<T1> item1, IReadOnlyBindable<T2> item2, Func<T1, T2, T> action)
        {
            if (item1 is null || item2 is null || action is null)
                throw new ArgumentNullException();

            _sources.Add(item1.Bind(i1 => _result.Value = action(i1, item2.Value)));
            _sources.Add(item2.Bind(i2 => _result.Value = action(item1.Value, i2)));
        }

        //UnBind를 할 수 있는 델리게이트를 반환.
        public IDisposable Bind(Action<T> onChanged) => _result.Bind(onChanged);

        public void Dispose() => _sources.Clear();
    }
}
