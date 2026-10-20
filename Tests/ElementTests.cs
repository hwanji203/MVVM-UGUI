using System.Collections.Generic;
using MVVM.UGUI.Binding;
using MVVM.UGUI.Elements.Core;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MVVM.UGUI.Tests
{
    [ExecuteAlways]
    public class TestValueElement : VisualElement<int>
    {
        public readonly List<int> Received = new();
        protected override void HandleValueChanged(int value) => Received.Add(value);
    }

    [ExecuteAlways]
    public class TestListElement : ReadOnlyListElement<int>
    {
        public int ListCalls;
        public int ElementCalls;
        protected override void HandleValueChanged(IReadOnlyList<int> value) => ListCalls++;
        protected override void HandleElementChanged(int index, int value) => ElementCalls++;
    }

    [ExecuteAlways]
    public class TestCommandElement : CommandElement
    {
        public readonly List<bool> Active = new();
        public void Click() => Execute();
        protected override void HandleActiveChanged(bool enable) => Active.Add(enable);
    }

    public class ElementTests
    {
        private static T Make<T>() where T : Component => new GameObject(typeof(T).Name).AddComponent<T>();

        [Test]
        public void Rebind_DropsPreviousSource()
        {
            var element = Make<TestValueElement>();
            var a = new Bindable<int> { Value = 1 };
            var b = new Bindable<int> { Value = 10 };

            element.Bind(a);
            element.Bind(b);
            a.Value = 2;
            b.Value = 20;

            CollectionAssert.AreEqual(new[] { 1, 10, 20 }, element.Received);
            Object.DestroyImmediate(element.gameObject);
        }

        [Test]
        public void Unbind_StopsUpdates()
        {
            var element = Make<TestValueElement>();
            var a = new Bindable<int> { Value = 1 };
            element.Bind(a);

            element.Unbind();
            a.Value = 2;

            CollectionAssert.AreEqual(new[] { 1 }, element.Received);
            Object.DestroyImmediate(element.gameObject);
        }

        [Test]
        public void Destroy_StopsUpdates()
        {
            var element = Make<TestValueElement>();
            var a = new Bindable<int> { Value = 1 };
            element.Bind(a);
            var received = element.Received;

            Object.DestroyImmediate(element.gameObject);
            a.Value = 2;

            CollectionAssert.AreEqual(new[] { 1 }, received);
        }

        [Test]
        public void Destroy_WithoutBind_DoesNotThrow()
        {
            var element = Make<TestValueElement>();

            Assert.DoesNotThrow(() => Object.DestroyImmediate(element.gameObject));
        }

        [Test]
        public void ListElement_Rebind_DropsBothSubscriptionsOfPrevious()
        {
            var element = Make<TestListElement>();
            var first = new BindableList<int>();
            first.Add(0);
            var second = new BindableList<int>();

            element.Bind(first);
            element.Bind(second);
            element.ListCalls = 0;
            element.ElementCalls = 0;
            first.Add(1);
            first[0] = 5;

            Assert.AreEqual(0, element.ListCalls);
            Assert.AreEqual(0, element.ElementCalls);
            Object.DestroyImmediate(element.gameObject);
        }

        [Test]
        public void CommandElement_Unbind_ClearsCall()
        {
            var element = Make<TestCommandElement>();
            var runs = 0;
            element.Bind(new Command(() => runs++));

            element.Unbind();
            element.Click();

            Assert.AreEqual(0, runs);
            Object.DestroyImmediate(element.gameObject);
        }

        [Test]
        public void CommandElement_Rebind_RunsOnlyNewCommand()
        {
            var element = Make<TestCommandElement>();
            var oldRuns = 0;
            var newRuns = 0;
            var oldCommand = new Command(() => oldRuns++);

            element.Bind(oldCommand);
            element.Bind(new Command(() => newRuns++));
            element.Click();
            oldCommand.SetEnabled(false);

            Assert.AreEqual(0, oldRuns);
            Assert.AreEqual(1, newRuns);
            CollectionAssert.AreEqual(new[] { true, true }, element.Active, "옛 Command 의 상태 변화는 오지 않아야 한다");
            Object.DestroyImmediate(element.gameObject);
        }
    }
}
