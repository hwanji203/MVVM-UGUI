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

        // Reset = 만든 직후로 — 생성자의 동작 · 켜짐
        [Test]
        public void Reset_RestoresConstructorActionAndActive()
        {
            int first = 0, later = 0;
            var command = new Command(() => first++, active: false);
            var received = new List<bool>();
            command.Active.Bind(received.Add);
            command.Action = () => later++;
            command.SetEnabled(true);

            command.Reset();
            command.SetEnabled(true);
            command.Execute();

            Assert.AreEqual(1, first);
            Assert.AreEqual(0, later);
            CollectionAssert.AreEqual(new[] { false, true, false, true }, received);
        }

        [Test]
        public void Generic_Reset_RestoresConstructorActionAndActive()
        {
            var received = new List<int>();
            var command = new Command<int>();
            command.Action = received.Add;
            command.SetEnabled(false);

            command.Reset();
            command.Execute(1);

            Assert.IsTrue(command.Active.Value);
            CollectionAssert.IsEmpty(received);
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
