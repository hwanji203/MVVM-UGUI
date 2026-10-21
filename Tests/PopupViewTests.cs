using Cysharp.Threading.Tasks;
using MVVM.UGUI.Elements.Popups;
using MVVM.UGUI.Templates;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MVVM.UGUI.Tests
{
    [ExecuteAlways]
    public class TestPopupView : AbstractPopupView<TestViewModel>
    {
    }

    public class PopupViewTests
    {
        [Test]
        public void Popup_WithoutNullableElements_OpensAndCloses()
        {
            var go = new GameObject("Popup");
            var window = new GameObject("Window").AddComponent<WindowElement>();
            window.transform.SetParent(go.transform);

            // Awake 에서 Bind 가 불린다 — closeButton · backgroundDim 은 비어 있다
            var popup = go.AddComponent<TestPopupView>();
            var serialized = new SerializedObject(popup);
            serialized.FindProperty("window").objectReferenceValue = window;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(UniTaskStatus.Succeeded, popup.OpenAsync().Status);
            Assert.IsTrue(go.activeSelf);
            Assert.AreEqual(UniTaskStatus.Succeeded, popup.CloseAsync().Status);
            Assert.IsFalse(go.activeSelf);

            Object.DestroyImmediate(go);
        }
    }
}
