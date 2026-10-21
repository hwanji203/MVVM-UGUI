using System;
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
        public Action OnConfirm;   // 여는 쪽 콜백 자리 (확인 → 콜백 → 닫기)
        public int Dismissed;

        public TestPopupViewModel() => Confirm = new Command(() =>
        {
            OnConfirm?.Invoke();
            if (Valid) RequestClose();
        });

        public void CloseLater() => RequestClose();

        protected override void Dismiss()
        {
            Dismissed++;
            base.Dismiss();
        }

        protected override void ResetForOpen()
        {
            base.ResetForOpen();
            OnConfirm = null;
            Valid = true;
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

        // 꺼 둔 채 필드를 채우고 켠다 — Awake 의 Bind 가 닫기 버튼 · 배경을 VM 의 CloseCmd 에 묶는 것까지 확인된다
        private TestPopupView Spawn(ButtonElement closeButton = null, BackgroundDimElement backgroundDim = null)
        {
            _root = new GameObject("Popup");
            _root.SetActive(false);
            var window = new GameObject("Window").AddComponent<WindowElement>();
            window.transform.SetParent(_root.transform);
            closeButton?.transform.SetParent(_root.transform);
            backgroundDim?.transform.SetParent(_root.transform);

            var popup = _root.AddComponent<TestPopupView>();
            var serialized = new SerializedObject(popup);
            serialized.FindProperty("window").objectReferenceValue = window;
            serialized.FindProperty("closeButton").objectReferenceValue = closeButton;
            serialized.FindProperty("backgroundDim").objectReferenceValue = backgroundDim;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _root.SetActive(true);   // Awake: Initialize → Bind → 끄기

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
            var popup = Spawn(closeButton: button);

            button.GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual(1, popup.BoundWith.Dismissed);
            Assert.IsFalse(_root.activeSelf);
        }

        // 배경 클릭 = 닫기 버튼과 같은 길 (Dismiss) — 확인 팝업이면 취소가 된다
        [Test]
        public void BackgroundClick_GoesThroughDismiss()
        {
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image)).AddComponent<BackgroundDimElement>();
            var popup = Spawn(backgroundDim: dim);

            dim.OnPointerClick(null);

            Assert.AreEqual(1, popup.BoundWith.Dismissed);
            Assert.IsFalse(_root.activeSelf);
        }

        // 배경은 열려 있을 때만 클릭을 받는다 — 닫힐 때 raycast 를 꺼서 닫히는 팝업을 또 눌러 Dismiss 가 두 번 불리지 않게
        [Test]
        public void Background_TakesClicksOnlyWhileOpen()
        {
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image)).AddComponent<BackgroundDimElement>();
            var popup = Spawn(backgroundDim: dim);
            var group = dim.GetComponent<CanvasGroup>();

            Assert.IsTrue(group.blocksRaycasts);
            popup.CloseAsync().Forget();
            Assert.IsFalse(group.blocksRaycasts);
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
