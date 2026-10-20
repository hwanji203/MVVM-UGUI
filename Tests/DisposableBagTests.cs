using System;
using NUnit.Framework;

namespace MVVM.UGUI.Tests
{
    public class DisposableBagTests
    {
        [Test]
        public void Clear_DisposesAll()
        {
            var bag = new DisposableBag();
            var a = new CountingDisposable();
            var b = new CountingDisposable();
            bag.Add(a);
            bag.Add(b);

            bag.Clear();

            Assert.AreEqual(1, a.Count);
            Assert.AreEqual(1, b.Count);
        }

        [Test]
        public void Clear_Twice_DoesNotDisposeAgain()
        {
            var bag = new DisposableBag();
            var a = new CountingDisposable();
            bag.Add(a);

            bag.Clear();
            bag.Dispose();

            Assert.AreEqual(1, a.Count);
        }

        [Test]
        public void AfterClear_CanAddAgain()
        {
            var bag = new DisposableBag();
            bag.Add(new CountingDisposable());
            bag.Clear();
            var next = new CountingDisposable();

            bag.Add(next);
            bag.Clear();

            Assert.AreEqual(1, next.Count);
        }

        [Test]
        public void Add_Null_Throws()
        {
            var bag = new DisposableBag();

            Assert.Throws<ArgumentNullException>(() => bag.Add(null));
        }
    }
}
