using System;
using System.Collections.Generic;

namespace MVVMGUI.Runtime.Core
{
    public abstract class AbstractViewModel
    {
        private List<IDisposable> _subscriptions = new List<IDisposable>();

        public virtual void Dispose()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
        }

        protected void Own(IDisposable unSub)
        {
            if (unSub == null)
                throw new ArgumentNullException(nameof(unSub));
            
            _subscriptions.Add(unSub);
        }
    }
}