using System;
using System.Collections.Generic;
using MVVMGUI.Runtime.Binding;
using NUnit.Framework;

namespace MVVMGUI.Tests
{
    public class DerivedBindableTests
    {
        [Test]
        public void Combine_ComputesInitialValue()
        {
            var hp = new Bindable<int> { Value = 30 };
            var maxHp = new Bindable<int> { Value = 120 };

            var ratio = Bindable.Combine(hp, maxHp, (h, m) => (float)h / m);

            Assert.AreEqual(0.25f, ratio.Value);
        }

        [Test]
        public void Combine_RecomputesWhenEitherSourceChanges()
        {
            var hp = new Bindable<int> { Value = 10 };
            var maxHp = new Bindable<int> { Value = 100 };
            var text = Bindable.Combine(hp, maxHp, (h, m) => $"{h}/{m}");
            var received = new List<string>();
            text.Bind(received.Add);

            hp.Value = 20;
            maxHp.Value = 200;

            CollectionAssert.AreEqual(new[] { "10/100", "20/100", "20/200" }, received);
        }

        [Test]
        public void Combine_SameResult_DoesNotNotify()
        {
            var a = new Bindable<int> { Value = 1 };
            var b = new Bindable<int> { Value = 2 };
            var positive = Bindable.Combine(a, b, (x, y) => x + y > 0);
            var count = 0;
            positive.Bind(_ => count++);

            a.Value = 5;

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Combine_Dispose_StopsRecomputing()
        {
            var a = new Bindable<int> { Value = 1 };
            var b = new Bindable<int> { Value = 1 };
            var sum = Bindable.Combine(a, b, (x, y) => x + y);

            sum.Dispose();
            a.Value = 10;

            Assert.AreEqual(2, sum.Value);
        }

        [Test]
        public void Combine_NullArgument_Throws()
        {
            var a = new Bindable<int>();

            Assert.Throws<ArgumentNullException>(() => Bindable.Combine<int, int, int>(a, null, (x, y) => x + y));
        }

        [Test]
        public void Select_ComputesAndFollowsSource()
        {
            var hp = new Bindable<int> { Value = 5 };
            var (dead, _) = Bindable.Select(hp, h => h <= 0);
            var received = new List<bool>();
            dead.Bind(received.Add);

            hp.Value = 0;

            CollectionAssert.AreEqual(new[] { false, true }, received);
        }

        [Test]
        public void Select_UnSub_StopsFollowing()
        {
            var hp = new Bindable<int> { Value = 5 };
            var (doubled, unSub) = Bindable.Select(hp, h => h * 2);

            unSub.Dispose();
            hp.Value = 50;

            Assert.AreEqual(10, doubled.Value);
        }
    }
}
