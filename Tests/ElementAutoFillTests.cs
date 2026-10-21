using System.Collections.Generic;
using MVVM.UGUI.Core;
using MVVM.UGUI.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MVVM.UGUI.Tests
{
    // ExecuteAlways 를 붙이지 않는다 — 에디트 모드에서 Awake 가 돌면 View 가 스스로 꺼진다
    public class FillTestView : ScreenView<TestViewModel>
    {
        [SerializeField] private TestValueElement hpText;
        [SerializeField] private TestCommandElement closeButton;
        [SerializeField] private TestValueElement missingText;

        public TestValueElement HpText => hpText;
        public TestCommandElement CloseButton => closeButton;
        public TestValueElement MissingText => missingText;

        protected override TestViewModel CreateViewModel() => new();
    }

    // PopupView 처럼 부모 클래스의 private 필드
    public abstract class FillBaseView<T> : ScreenView<T> where T : AbstractViewModel
    {
        [SerializeField] private TestCommandElement baseButton;
        public TestCommandElement BaseButton => baseButton;
    }

    public class FillDerivedView : FillBaseView<TestViewModel>
    {
        protected override TestViewModel CreateViewModel() => new();
    }

    public class ElementAutoFillTests
    {
        private readonly List<GameObject> _created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created)
                if (go != null) Object.DestroyImmediate(go);
            _created.Clear();
        }

        private GameObject Node(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            else _created.Add(go);
            return go;
        }

        [TestCase("closeButton", "closebutton")]
        [TestCase("Close Button", "closebutton")]
        [TestCase("close_button", "closebutton")]
        [TestCase("m_CloseButton", "closebutton")]
        [TestCase("HP-Bar 2", "hpbar2")]
        public void Normalize(string name, string expected)
        {
            Assert.AreEqual(expected, ElementAutoFill.Normalize(name));
        }

        [Test]
        public void Fill_MatchesChildByName_IncludingInactiveAndDeep()
        {
            var root = Node("Hud");
            var view = root.AddComponent<FillTestView>();
            var group = Node("Group", root.transform);
            group.SetActive(false);
            var hp = Node("HP Text", group.transform).AddComponent<TestValueElement>();
            var close = Node("CloseButton", root.transform).AddComponent<TestCommandElement>();

            var report = ElementAutoFill.Fill(view);

            Assert.AreSame(hp, view.HpText);
            Assert.AreSame(close, view.CloseButton);
            CollectionAssert.AreEquivalent(new[] { "hpText", "closeButton" }, report.Filled);
            CollectionAssert.AreEqual(new[] { "missingText" }, report.NotFound);
        }

        [Test]
        public void Fill_DoesNotOverwriteFilledField()
        {
            var root = Node("Hud");
            var view = root.AddComponent<FillTestView>();
            var chosen = Node("Other", root.transform).AddComponent<TestValueElement>();
            Node("HpText", root.transform).AddComponent<TestValueElement>();
            var so = new SerializedObject(view);
            so.FindProperty("hpText").objectReferenceValue = chosen;
            so.ApplyModifiedPropertiesWithoutUndo();

            ElementAutoFill.Fill(view);

            Assert.AreSame(chosen, view.HpText, "손으로 넣은 것은 그대로");
        }

        [Test]
        public void Fill_SkipsNestedViewSubtree()
        {
            var root = Node("Shop");
            var view = root.AddComponent<FillTestView>();
            var item = Node("ItemTemplate", root.transform);
            item.AddComponent<TestItemView>();
            Node("HpText", item.transform).AddComponent<TestValueElement>();

            var report = ElementAutoFill.Fill(view);

            Assert.IsNull(view.HpText, "안쪽 View 아래의 Element 는 그 View 의 것");
            CollectionAssert.Contains(report.NotFound, "hpText");
        }

        [Test]
        public void Fill_SameNameTwice_LeavesEmptyAndReports()
        {
            var root = Node("Hud");
            var view = root.AddComponent<FillTestView>();
            Node("HpText", root.transform).AddComponent<TestValueElement>();
            Node("hp_text", root.transform).AddComponent<TestValueElement>();

            var report = ElementAutoFill.Fill(view);

            Assert.IsNull(view.HpText);
            CollectionAssert.Contains(report.Ambiguous, "hpText");
        }

        [Test]
        public void Fill_NameMatchesButWrongType_NotFilled()
        {
            var root = Node("Hud");
            var view = root.AddComponent<FillTestView>();
            Node("CloseButton", root.transform).AddComponent<TestValueElement>();

            var report = ElementAutoFill.Fill(view);

            Assert.IsNull(view.CloseButton);
            CollectionAssert.Contains(report.NotFound, "closeButton");
        }

        [Test]
        public void Fill_FindsPrivateFieldOfGenericBase()
        {
            var root = Node("Popup");
            var view = root.AddComponent<FillDerivedView>();
            var button = Node("BaseButton", root.transform).AddComponent<TestCommandElement>();

            ElementAutoFill.Fill(view);

            Assert.AreSame(button, view.BaseButton);
        }

        [Test]
        public void IsView_ScreenAndItemViewOnly()
        {
            var go = Node("Views");
            Assert.IsTrue(ElementAutoFill.IsView(go.AddComponent<FillTestView>()));
            Assert.IsTrue(ElementAutoFill.IsView(Node("Item").AddComponent<TestItemView>()));
            Assert.IsFalse(ElementAutoFill.IsView(Node("Element").AddComponent<TestValueElement>()));
        }
    }
}
