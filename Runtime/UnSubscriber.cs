using System;

namespace MVVM.UGUI
{
    public class UnSubscriber : IDisposable
    {
        private Action _unSub;
        
        public UnSubscriber(Action unSub)
        {
            _unSub = unSub;
        }
        
        public void Dispose()
        {
            _unSub?.Invoke();
            _unSub = null;
        }
    }
}