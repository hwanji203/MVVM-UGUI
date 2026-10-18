using System;

namespace MVVMGUI.Binding
{
    public class SelectBindable<T, T1> : IReadOnlyBindable<T>, IDisposable
    {
        // 값 저장 · 같은 값 무시 · 알림은 Bindable 에 맡긴다
        private readonly Bindable<T> _result = new();
        private IDisposable _source;

        public T Value => _result.Value;

        public SelectBindable(IReadOnlyBindable<T1> item1, Func<T1, T> action)
        {
            if (item1 is null || action is null)
                throw new ArgumentNullException();

            _source = item1.Bind(i1 => _result.Value = action(i1));
        }

        //UnBind를 할 수 있는 델리게이트를 반환.
        public IDisposable Bind(Action<T> onChanged) => _result.Bind(onChanged);

        public void Dispose()
        {
            _source?.Dispose();
            _source = null;
        }
    }
}
