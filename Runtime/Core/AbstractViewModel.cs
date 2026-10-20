using System;

namespace MVVM.UGUI.Core
{
    public abstract class AbstractViewModel : IDisposable
    {
        private readonly DisposableBag _owned = new();

        public virtual void Dispose() => _owned.Clear();

        protected void Own(IDisposable unSub) => _owned.Add(unSub);
    }
}
