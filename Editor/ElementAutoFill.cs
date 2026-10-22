using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MVVM.UGUI.Core;
using MVVM.UGUI.Elements.Core;
using UnityEditor;
using UnityEngine;

namespace MVVM.UGUI.Editor
{
    /// <summary>
    /// View 의 비어 있는 Element 필드를 같은 이름의 자식 오브젝트로 채운다 (필드 <c>closeButton</c> ↔ 오브젝트 <c>CloseButton</c>).
    /// 코드에 필드를 먼저 쓰고 프리팹을 만든 뒤 한 번 누르는 용도 — 이름이 안 맞는 것은 인스펙터에서 끌어다 놓는다.
    /// <list type="bullet">
    /// <item>이미 채워진 필드는 건드리지 않는다</item>
    /// <item>이름은 대소문자 · 띄어쓰기 · <c>_</c> · 앞의 <c>m_</c> 를 무시하고 비교한다</item>
    /// <item>그 이름의 자식에 필드 타입의 컴포넌트가 있어야 한다. 둘 이상이면 채우지 않고 알린다</item>
    /// <item>안쪽에 다른 View (ScreenView · ItemView) 가 붙은 오브젝트는 그 아래까지 그 View 의 것이라 찾지 않는다</item>
    /// </list>
    /// </summary>
    public static class ElementAutoFill
    {
        public class Report
        {
            public readonly List<string> Filled = new();
            public readonly List<string> NotFound = new();
            public readonly List<string> Ambiguous = new();
        }

        public static bool IsView(Component component) => component is ScreenView || component is ItemView;

        public static Report Fill(MonoBehaviour view)
        {
            var report = new Report();
            var serialized = new SerializedObject(view);
            var property = serialized.GetIterator();

            // 맨 위 필드만 훑는다 (첫 호출만 안으로 들어간다)
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue)
                    continue;

                var field = FindField(view.GetType(), property.name);
                if (field == null || !typeof(Element).IsAssignableFrom(field.FieldType))
                    continue;

                var matches = new List<Component>();
                Collect(view.transform, Normalize(property.name), field.FieldType, matches);

                if (matches.Count == 1)
                {
                    property.objectReferenceValue = matches[0];
                    report.Filled.Add(property.name);
                }
                else if (matches.Count == 0)
                {
                    report.NotFound.Add(property.name);
                }
                else
                {
                    report.Ambiguous.Add(property.name);
                }
            }

            // Undo 에 남고 프리팹 · 씬이 바뀐 것으로 표시된다
            serialized.ApplyModifiedProperties();
            return report;
        }

        /// <summary>비교용 이름 — 앞의 m_ 를 떼고, 글자 · 숫자만 남겨 소문자로.</summary>
        public static string Normalize(string name)
        {
            if (name.StartsWith("m_"))
                name = name.Substring(2);

            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        // 부모 클래스의 private 필드 (PopupView 의 window 등) 도 찾도록 위로 올라간다
        private static FieldInfo FindField(Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (var t = type; t != null; t = t.BaseType)
            {
                var field = t.GetField(name, flags);
                if (field != null)
                    return field;
            }
            return null;
        }

        private static void Collect(Transform parent, string key, Type type, List<Component> found)
        {
            foreach (Transform child in parent)
            {
                if (HasView(child.gameObject))
                    continue;

                if (Normalize(child.name) == key)
                {
                    var component = child.GetComponent(type);
                    if (component)
                        found.Add(component);
                }

                Collect(child, key, type, found);
            }
        }

        private static bool HasView(GameObject go)
        {
            foreach (var component in go.GetComponents<MonoBehaviour>())
            {
                if (component && IsView(component))
                    return true;
            }
            return false;
        }
    }
}
