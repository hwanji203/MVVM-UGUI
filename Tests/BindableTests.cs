using System.Collections.Generic;
using MVVM.UGUI.Binding;
using NUnit.Framework;

namespace MVVM.UGUI.Tests
{
    public class BindableTests
    {
        [Test]
        public void Bind_CallsImmediatelyWithCurrentValue()
        {
            var hp = new Bindable<int> { Value = 7 };
            var received = new List<int>();

            hp.Bind(received.Add);

            CollectionAssert.AreEqual(new[] { 7 }, received);
        }

        [Test]
        public void SetValue_NotifiesOnlyWhenChanged()
        {
            var hp = new Bindable<int>();
            var received = new List<int>();
            hp.Bind(received.Add);

            hp.Value = 3;
            hp.Value = 3;
            hp.Value = 5;

            CollectionAssert.AreEqual(new[] { 0, 3, 5 }, received);
        }

        [Test]
        public void NewSubscriber_DoesNotReplayToExistingOnes()
        {
            var hp = new Bindable<int> { Value = 1 };
            var first = new List<int>();
            hp.Bind(first.Add);

            hp.Bind(_ => { });

            CollectionAssert.AreEqual(new[] { 1 }, first);
        }

        [Test]
        public void Dispose_StopsNotifications()
        {
            var hp = new Bindable<int>();
            var received = new List<int>();
            var binding = hp.Bind(received.Add);

            binding.Dispose();
            hp.Value = 9;

            CollectionAssert.AreEqual(new[] { 0 }, received);
        }

        [Test]
        public void Dispose_Twice_IsSafe()
        {
            var hp = new Bindable<int>();
            var binding = hp.Bind(_ => { });

            binding.Dispose();

            Assert.DoesNotThrow(() => binding.Dispose());
        }
    }
}
