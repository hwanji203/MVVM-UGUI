using System;
using System.Collections.Generic;
using System.Text;
using MVVM.UGUI.Core;
using MVVM.UGUI.Elements.Core;
using UnityEngine;

namespace MVVM.UGUI.Editor
{
    /// <summary>View 아래의 Element 를 찾고, 속성 이름을 정하고, 문제를 알린다 (생성기의 읽기 부분).</summary>
    public static class ElementScan
    {
        public static bool IsScreenView(Type type) => typeof(ScreenView).IsAssignableFrom(type);

        /// <summary>생성기가 속성을 만들어 주는 View — 화면 (ScreenView) 과 목록 항목 (ItemView).</summary>
        public static bool IsView(Type type) => IsScreenView(type) || typeof(ItemView).IsAssignableFrom(type);

        public static MonoBehaviour FindView(GameObject go)
        {
            var screen = go.GetComponent<ScreenView>();
            if (screen != null)
                return screen;
            var item = go.GetComponent<ItemView>();
            return item != null ? item : null;
        }

        /// <summary>root 와 그 아래 (꺼진 것 포함) 의 View 전부.</summary>
        public static IEnumerable<MonoBehaviour> ViewsIn(GameObject root)
        {
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour != null && IsView(behaviour.GetType()))
                    yield return behaviour;
            }
        }

        /// <summary>
        /// View 오브젝트의 자식 (꺼진 것 포함) 에서 Element 를 계층 순서대로 모은다.
        /// View 자신에 붙은 Element 는 빼고, 안쪽에 다른 View (ScreenView · ItemView) 가 있으면 그 아래는 그 View 의 것이라 들어가지 않는다.
        /// </summary>
        public static List<Element> Collect(Transform viewRoot)
        {
            var found = new List<Element>();
            foreach (Transform child in viewRoot)
                Visit(child, found);
            return found;
        }

        private static void Visit(Transform node, List<Element> found)
        {
            if (FindView(node.gameObject) != null)
                return;

            found.AddRange(node.GetComponents<Element>());
            foreach (Transform child in node)
                Visit(child, found);
        }

        /// <summary>오브젝트 이름 → C# 이름. 쓸 수 없는 글자는 빼고, 숫자로 시작하면 `_`, 키워드면 `@` 를 붙인다.</summary>
        public static string ToIdentifier(string objectName)
        {
            var sb = new StringBuilder(objectName.Length);
            foreach (char c in objectName)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
            }

            if (sb.Length == 0)
                return string.Empty;
            if (char.IsDigit(sb[0]))
                sb.Insert(0, '_');

            string name = sb.ToString();
            return Keywords.Contains(name) ? "@" + name : name;
        }

        /// <summary>Element 마다 속성 이름을 정한다. 이름이 비거나 겹치면 errors 에 담는다 (그때는 생성하지 않는다).</summary>
        public static List<string> NamesFor(IReadOnlyList<Element> elements, List<string> errors)
        {
            var names = new List<string>(elements.Count);
            var firstOwner = new Dictionary<string, Element>();

            foreach (var element in elements)
            {
                string name = ToIdentifier(element.gameObject.name);
                names.Add(name);

                if (name.Length == 0)
                {
                    errors.Add($"'{element.gameObject.name}' 은 C# 이름으로 쓸 수 있는 글자가 없다 — 오브젝트 이름을 바꿔 주세요.");
                    continue;
                }

                if (firstOwner.TryGetValue(name, out var owner))
                    errors.Add($"속성 이름 '{name}' 이 겹친다: '{Path(owner.transform)}' · '{Path(element.transform)}' ({element.GetType().Name}) — 한쪽 이름을 바꿔 주세요.");
                else
                    firstOwner.Add(name, element);
            }

            return names;
        }

        public static string Path(Transform t)
        {
            var sb = new StringBuilder(t.name);
            for (var p = t.parent; p != null; p = p.parent)
                sb.Insert(0, p.name + "/");
            return sb.ToString();
        }

        private static readonly HashSet<string> Keywords = new()
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
            "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
            "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
            "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
            "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
            "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
        };
    }
}
