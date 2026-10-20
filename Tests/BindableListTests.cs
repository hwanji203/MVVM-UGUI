using System.Collections.Generic;
using System.Linq;
using MVVM.UGUI.Binding;
using NUnit.Framework;

namespace MVVM.UGUI.Tests
{
    public class BindableListTests
    {
        [Test]
        public void BindList_CallsImmediatelyAndOnEveryChange()
        {
            var cards = new BindableList<string>();
            var snapshots = new List<string[]>();
            cards.Bind(items => snapshots.Add(items.ToArray()));

            cards.Add("A");
            cards.Add("B");
            cards.RemoveAt(0);
            cards.Add("C");
            cards.Remove("B");
            cards.Clear();

            Assert.AreEqual(7, snapshots.Count);
            CollectionAssert.AreEqual(new string[0], snapshots[0]);
            CollectionAssert.AreEqual(new[] { "A", "B" }, snapshots[2]);
            CollectionAssert.AreEqual(new[] { "B" }, snapshots[3]);
            CollectionAssert.AreEqual(new[] { "C" }, snapshots[5]);
            CollectionAssert.AreEqual(new string[0], snapshots[6]);
        }

        [Test]
        public void Indexer_NotifiesElementChangeWithIndex()
        {
            var cards = new BindableList<string>();
            cards.Add("A");
            cards.Add("B");
            var changes = new List<(int, string)>();
            cards.Bind((index, value) => changes.Add((index, value)));

            cards[1] = "X";
            cards[1] = "X";

            CollectionAssert.AreEqual(new[] { (1, "X") }, changes);
            Assert.AreEqual("X", cards[1]);
        }

        [Test]
        public void ElementBind_DoesNotCallImmediately()
        {
            var cards = new BindableList<string>();
            cards.Add("A");
            var calls = 0;

            cards.Bind((_, _) => calls++);

            Assert.AreEqual(0, calls);
        }

        [Test]
        public void UnSub_StopsListNotifications()
        {
            var cards = new BindableList<int>();
            var calls = 0;
            var binding = cards.Bind(_ => calls++);

            binding.Dispose();
            cards.Add(1);

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void Remove_Missing_ReturnsFalseAndDoesNotNotify()
        {
            var cards = new BindableList<string>();
            cards.Add("A");
            var calls = 0;
            cards.Bind(_ => calls++);

            var removed = cards.Remove("Z");

            Assert.IsFalse(removed);
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void Remove_Present_ReturnsTrueAndNotifies()
        {
            var cards = new BindableList<string>();
            cards.Add("A");
            var calls = 0;
            cards.Bind(_ => calls++);

            var removed = cards.Remove("A");

            Assert.IsTrue(removed);
            Assert.AreEqual(2, calls);
        }
    }
}
