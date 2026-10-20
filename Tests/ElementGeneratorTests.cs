using System.Collections.Generic;
using MVVM.UGUI.Core;
using MVVM.UGUI.Editor;
using MVVM.UGUI.Elements.Core;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MVVM.UGUI.Tests
{
    // 생성된 partial 이 넣는 배열을 손으로 둔 View — TryFill 확인용
    public class FilledTestView : ScreenView<TestViewModel>
    {
        [SerializeField] private Element[] generatedElements;
        public Element[] Generated => generatedElements;
        protected override TestViewModel CreateViewModel() => new();
    }

    public class ElementGeneratorTests
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

        [Test]
        public void IsScreenView_FindsGenericBase()
        {
            Assert.IsTrue(ElementScan.IsScreenView(typeof(TestView)));
            Assert.IsFalse(ElementScan.IsScreenView(typeof(TestValueElement)));
        }

        [Test]
        public void Collect_HierarchyOrder_IncludesInactive_SkipsRootAndNestedViews()
        {
            var root = Node("Hud");
            root.AddComponent<TestView>();
            root.AddComponent<TestValueElement>();                 // View 자신 — 빠진다
            var hp = Node("Hp", root.transform).AddComponent<TestValueElement>();
            var group = Node("Group", root.transform);
            var hidden = Node("Hidden", group.transform);
            hidden.SetActive(false);
            var hiddenElement = hidden.AddComponent<TestValueElement>();
            var nested = Node("Nested", root.transform);
            nested.AddComponent<TestView>();                        // 안쪽 View — 그 아래는 그 View 의 것
            Node("Inner", nested.transform).AddComponent<TestValueElement>();

            var found = ElementScan.Collect(root.transform);

            CollectionAssert.AreEqual(new Element[] { hp, hiddenElement }, found);
        }

        [Test]
        public void ItemView_IsView_AndNestedItemTemplateIsBoundary()
        {
            Assert.IsTrue(ElementScan.IsView(typeof(TestItemView)));

            var root = Node("Shop");
            root.AddComponent<TestView>();
            var title = Node("Title", root.transform).AddComponent<TestValueElement>();
            var template = Node("ItemTemplate", root.transform);
            template.AddComponent<TestItemView>();                 // 화면 안에 놓인 항목 템플릿 — 그 아래는 항목 View 의 것
            var price = Node("Price", template.transform).AddComponent<TestValueElement>();

            CollectionAssert.AreEqual(new Element[] { title }, ElementScan.Collect(root.transform));
            CollectionAssert.AreEqual(new Element[] { price }, ElementScan.Collect(template.transform));
            Assert.AreEqual(template.GetComponent<TestItemView>(), ElementScan.FindView(template));
        }

        [TestCase("HpText", "HpText")]
        [TestCase("HP Text", "HPText")]
        [TestCase("hp-bar.fill", "hpbarfill")]
        [TestCase("1st", "_1st")]
        [TestCase("class", "@class")]
        [TestCase("체력", "체력")]
        [TestCase("!!!", "")]
        public void ToIdentifier(string objectName, string expected)
        {
            Assert.AreEqual(expected, ElementScan.ToIdentifier(objectName));
        }

        [Test]
        public void NamesFor_DuplicateOrEmpty_ReportsErrors()
        {
            var root = Node("Hud");
            var a = Node("Hp", root.transform).AddComponent<TestValueElement>();
            var b = Node("H p", root.transform).AddComponent<TestValueElement>();
            var c = Node("???", root.transform).AddComponent<TestValueElement>();
            var errors = new List<string>();

            var names = ElementScan.NamesFor(new Element[] { a, b, c }, errors);

            CollectionAssert.AreEqual(new[] { "Hp", "Hp", "" }, names);
            Assert.AreEqual(2, errors.Count);
        }

        [Test]
        public void Write_ProducesPartialWithArrayAndTypedProperties()
        {
            var root = Node("Hud");
            var hp = Node("Hp", root.transform).AddComponent<TestValueElement>();
            var pause = Node("Pause", root.transform).AddComponent<TestCommandElement>();

            string code = ElementCodeWriter.Write(typeof(TestView), new Element[] { hp, pause }, new[] { "Hp", "Pause" });

            StringAssert.Contains("namespace MVVM.UGUI.Tests", code);
            StringAssert.Contains("partial class TestView", code);
            StringAssert.Contains("private global::MVVM.UGUI.Elements.Core.Element[] generatedElements;", code);
            StringAssert.Contains("private global::MVVM.UGUI.Tests.TestValueElement Hp => (global::MVVM.UGUI.Tests.TestValueElement)generatedElements[0];", code);
            StringAssert.Contains("private global::MVVM.UGUI.Tests.TestCommandElement Pause => (global::MVVM.UGUI.Tests.TestCommandElement)generatedElements[1];", code);
        }

        [Test]
        public void TypeName_HandlesGenerics()
        {
            Assert.AreEqual("global::System.Collections.Generic.List<global::System.Int32>", ElementCodeWriter.TypeName(typeof(List<int>)));
        }

        [Test]
        public void TryFill_WritesReferencesInOrder()
        {
            var root = Node("Hud");
            var view = root.AddComponent<FilledTestView>();
            var hp = Node("Hp", root.transform).AddComponent<TestValueElement>();
            var mp = Node("Mp", root.transform).AddComponent<TestValueElement>();

            bool filled = ElementGenerator.TryFill(view, new Element[] { hp, mp });

            Assert.IsTrue(filled);
            CollectionAssert.AreEqual(new Element[] { hp, mp }, view.Generated);
        }

        [Test]
        public void TryFill_WithoutArrayField_ReturnsFalse()
        {
            var root = Node("Hud");
            var view = root.AddComponent<TestView>();

            Assert.IsFalse(ElementGenerator.TryFill(view, new Element[0]));
        }
    }
}
