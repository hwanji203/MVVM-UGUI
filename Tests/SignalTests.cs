using System.Collections.Generic;
using MVVM.UGUI.Binding;
using NUnit.Framework;

namespace MVVM.UGUI.Tests
{
    public class SignalTests
    {
        [Test]
        public void Fire_SameValueTwice_NotifiesTwice()
        {
            var signal = new Signal<int>();
            var received = new List<int>();
            signal.Bind(received.Add);

            signal.Fire(10);
            signal.Fire(10);

            CollectionAssert.AreEqual(new[] { 10, 10 }, received);
        }

        [Test]
        public void Bind_DoesNotCallImmediately()
        {
            var signal = new Signal<string>();
            signal.Fire("before");
            var calls = 0;

            signal.Bind(_ => calls++);

            Assert.AreEqual(0, calls, "구독 전에 Fire 된 것은 사라진다");
        }

        [Test]
        public void Dispose_StopsNotifications()
        {
            var signal = new Signal<int>();
            var calls = 0;
            var handle = signal.Bind(_ => calls++);

            handle.Dispose();
            signal.Fire(1);

            Assert.AreEqual(0, calls);
        }
    }
}
