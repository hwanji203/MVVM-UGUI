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
        public void Action_ReplacesAction()
        {
            var oldRuns = 0;
            var newRuns = 0;
            var command = new Command(() => oldRuns++);

            command.Action = () => newRuns++;
            command.Execute();

            Assert.AreEqual(0, oldRuns);
            Assert.AreEqual(1, newRuns);
        }

        [Test]
        public void NoAction_ExecuteDoesNothing()
        {
            var command = new Command();

            Assert.DoesNotThrow(command.Execute);

            command.Action = () => { };
            command.Action = null;
            Assert.DoesNotThrow(command.Execute);
        }

        [Test]
        public void Generic_Action_ReplacesAction()
        {
            var received = new List<int>();
            var command = new Command<int>();

            command.Execute(1);
            command.Action = received.Add;
            command.Execute(2);

            CollectionAssert.AreEqual(new[] { 2 }, received);
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
