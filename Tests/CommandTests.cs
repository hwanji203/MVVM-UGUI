using System.Collections.Generic;
using MVVMGUI.Binding;
using NUnit.Framework;

namespace MVVMGUI.Tests
{
    public class CommandTests
    {
        [Test]
        public void DefaultActive_CallRunsAction()
        {
            var runs = 0;
            var command = new Command(() => runs++);
            var (call, _) = command.Bind(_ => { });

            call();

            Assert.AreEqual(1, runs);
        }

        [Test]
        public void Disabled_CallDoesNothing()
        {
            var runs = 0;
            var command = new Command(() => runs++);
            var (call, _) = command.Bind(_ => { });

            command.SetEnabled(false);
            call();

            Assert.AreEqual(0, runs);
        }

        [Test]
        public void Bind_ReceivesActiveStateNowAndOnChange()
        {
            var command = new Command(() => { }, active: false);
            var received = new List<bool>();

            command.Bind(received.Add);
            command.SetEnabled(true);

            CollectionAssert.AreEqual(new[] { false, true }, received);
        }

        [Test]
        public void UnSub_StopsActiveNotifications()
        {
            var command = new Command(() => { });
            var received = new List<bool>();
            var (_, unSub) = command.Bind(received.Add);

            unSub.Dispose();
            command.SetEnabled(false);

            CollectionAssert.AreEqual(new[] { true }, received);
        }
    }
}
