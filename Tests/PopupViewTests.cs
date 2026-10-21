using Cysharp.Threading.Tasks;
using MVVM.UGUI.Binding;
using MVVM.UGUI.Elements.Commands;
using MVVM.UGUI.Elements.Popups;
using MVVM.UGUI.Templates.Popup;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MVVM.UGUI.Tests
{
    // 닫을지를 VM 이 정하는 팝업 — 확인은 검증에 통과할 때만 닫는다 (검증 · 비동기 닫기 자리)
    public class TestPopupViewModel : AbstractPopupVM
    {
        public readonly Command Confirm;
        public bool Valid = true;
        public int Dismissed;

        public TestPopupViewModel() => Confirm = new Command(() => { if (Valid) RequestClose(); });

        public void CloseLater() => RequestClose();

        protected override void Dismiss()
        {
            Dismissed++;
            base.Dismiss();
        }
    }

    [ExecuteAlways]
    public class TestPopupView : AbstractPopupView<TestPopupViewModel>
    {
        public TestPopupViewModel BoundWith;

        protected override void Bind(TestPopupViewModel vm)
        {
            base.Bind(vm);
            BoundWith = vm;
        }
    }

    public class PopupViewTests
    {
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        // Awake 에서 Bind 가 불린다 — 필드는 그 뒤에 채우므로 닫기 버튼은 테스트가 직접 묶는다
        private TestPopupView Spawn(ButtonElement closeButton = null)
        {
            _root = new GameObject("Popup");
            var window = new GameObject("Window").AddComponent<WindowElement>();
            window.transform.SetParent(_root.transform);

            var popup = _root.AddComponent<TestPopupView>();
            var serialized = new SerializedObject(popup);
            serialized.FindProperty("window").objectReferenceValue = window;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            closeButton?.Bind(popup.BoundWith.CloseCmd);

            popup.OpenAsync().Forget();
            return popup;
        }

        [Test]
        public void Popup_WithoutNullableElements_OpensAndCloses()
        {
            var popup = Spawn();

            Assert.IsTrue(_root.activeSelf);
            Assert.AreEqual(UniTaskStatus.Succeeded, popup.CloseAsync().Status);
            Assert.IsFalse(_root.activeSelf);
        }

        [Test]
        public void VmRequestsClose_Closes()
        {
            var popup = Spawn();

            popup.BoundWith.Confirm.Execute();

            Assert.IsFalse(_root.activeSelf);
        }

        [Test]
        public void VmDoesNotRequestClose_StaysOpen()
        {
            var popup = Spawn();
            popup.BoundWith.Valid = false;

            popup.BoundWith.Confirm.Execute();

            Assert.IsTrue(_root.activeSelf, "검증에 실패하면 VM 이 닫기를 요청하지 않는다");
        }

        [Test]
        public void VmRequestsCloseLater_ClosesThen()
        {
            var popup = Spawn();
            popup.BoundWith.Valid = false;
            popup.BoundWith.Confirm.Execute();

            popup.BoundWith.CloseLater();   // 저장이 끝난 뒤 같은 자리

            Assert.IsFalse(_root.activeSelf);
        }

        [Test]
        public void CloseButton_GoesThroughDismiss()
        {
            var go = new GameObject("Close", typeof(RectTransform));
            go.SetActive(false);   // 켜면서 Awake 가 Button 에 리스너를 잇는다 (ElementsTests 와 같은 방식)
            var button = go.AddComponent<TestButtonElement>();
            go.SetActive(true);
            var popup = Spawn(button);
            go.transform.SetParent(_root.transform);

            button.GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual(1, popup.BoundWith.Dismissed);
            Assert.IsFalse(_root.activeSelf);
        }

        [Test]
        public void RequestClose_WhenClosed_DoesNothing()
        {
            var popup = Spawn();
            popup.CloseAsync().Forget();

            Assert.DoesNotThrow(popup.BoundWith.CloseLater);
            Assert.IsFalse(_root.activeSelf);
        }
    }
}
