using System.Collections.Generic;
using MVVM.UGUI.Binding;
using NUnit.Framework;

namespace MVVM.UGUI.Tests
{
    public class CommandTests
    {
        [Test]
        public void DefaultActive_ExecuteRunsAction()
        {
            var runs = 0;
            var command = new Command(() => runs++);

            command.Execute();

            Assert.AreEqual(1, runs);
        }

        [Test]
        public void Disabled_ExecuteDoesNothing()
        {
            var runs = 0;
            var command = new Command(() => runs++);

            command.SetEnabled(false);
            command.Execute();

            Assert.AreEqual(0, runs);
        }

        [Test]
        public void Active_ReceivesStateNowAndOnChange()
        {
            var command = new Command(() => { }, active: false);
            var received = new List<bool>();

            command.Active.Bind(received.Add);
            command.SetEnabled(true);

            CollectionAssert.AreEqual(new[] { false, true }, received);
        }

        [Test]
        public void Active_Dispose_StopsNotifications()
        {
            var command = new Command(() => { });
            var received = new List<bool>();
            var binding = command.Active.Bind(received.Add);

            binding.Dispose();
            command.SetEnabled(false);

            CollectionAssert.AreEqual(new[] { true }, received);
        }
    }
}
