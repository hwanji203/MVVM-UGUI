using System;
using System.Reflection;
using MVVM.UGUI.Binding;
using MVVM.UGUI.Core;
using MVVM.UGUI.Templates.ConfirmPopup;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MVVM.UGUI.Tests
{
    public class CountingDisposable : IDisposable
    {
        public int Count;
        public void Dispose() => Count++;
    }

    public class TestViewModel : AbstractViewModel
    {
        public int DisposeCount;
        public readonly Bindable<int> Hp = new();

        public void OwnForTest(IDisposable unSub) => Own(unSub);

        public override void Dispose()
        {
            DisposeCount++;
            base.Dispose();
        }

        protected override void ResetForOpen() { }
    }

    [ExecuteAlways]
    public class TestView : ScreenView<TestViewModel>
    {
        public TestViewModel BoundWith;

        protected override void Bind(TestViewModel vm) => BoundWith = vm;
    }

    public class ViewModelTests
    {
        [Test]
        public void Dispose_DisposesOwned()
        {
            var vm = new TestViewModel();
            var a = new CountingDisposable();
            var b = new CountingDisposable();
            vm.OwnForTest(a);
            vm.OwnForTest(b);

            vm.Dispose();

            Assert.AreEqual(1, a.Count);
            Assert.AreEqual(1, b.Count);
        }

        [Test]
        public void Dispose_Twice_DoesNotDisposeOwnedAgain()
        {
            var vm = new TestViewModel();
            var a = new CountingDisposable();
            vm.OwnForTest(a);

            vm.Dispose();
            vm.Dispose();

            Assert.AreEqual(1, a.Count);
        }

        [Test]
        public void Own_Null_Throws()
        {
            var vm = new TestViewModel();

            Assert.Throws<ArgumentNullException>(() => vm.OwnForTest(null));
        }

        [Test]
        public void Own_DerivedValueFromLongLivedSource_StopsAfterDispose()
        {
            var setting = new Bindable<int> { Value = 1 };   // VM 보다 오래 사는 원본 (SO 자리)
            var vm = new TestViewModel();
            var doubled = Bindable.Combine(setting, vm.Hp, (s, _) => s * 2);
            vm.OwnForTest(doubled);

            vm.Dispose();
            setting.Value = 5;

            Assert.AreEqual(2, doubled.Value);
        }

        [Test]
        public void View_CreatesAndBindsOnAwake_DisposesOnDestroy()
        {
            var go = new GameObject("TestView");
            var view = go.AddComponent<TestView>();
            var vm = view.BoundWith;

            Assert.IsNotNull(vm, "Awake 에서 만든 VM 으로 Bind 해야 한다");

            Object.DestroyImmediate(go);

            Assert.AreEqual(1, vm.DisposeCount, "파괴될 때 VM 을 Dispose 해야 한다");
        }

        // ScreenManager 가 열 때마다 setup 직전에 부르는 되돌리기 — internal 이라 리플렉션으로 부른다
        private static void PrepareOpen(AbstractViewModel vm) =>
            typeof(AbstractViewModel).GetMethod("PrepareOpen", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(vm, null);

        [Test]
        public void ConfirmPopupVM_PrepareOpen_ClearsLastSetup()
        {
            var vm = new ConfirmPopupVM();
            int confirms = 0, cancels = 0;
            vm.Setup("title", "message", () => confirms++, () => cancels++);
            vm.ConfirmCmd.SetEnabled(false);

            PrepareOpen(vm);
            vm.ConfirmCmd.Execute();
            vm.CancelCmd.Execute();

            Assert.IsNull(vm.Title.Value);
            Assert.IsNull(vm.Message.Value);
            Assert.AreEqual(0, confirms, "지난번 확인 콜백이 남으면 안 된다");
            Assert.AreEqual(0, cancels, "지난번 취소 콜백이 남으면 안 된다");
            Assert.IsTrue(vm.ConfirmCmd.Active.Value, "이번에 잠근 버튼은 다음 열기에서 풀려야 한다");
        }

        [Test]
        public void ConfirmPopupVM_Setup_FillsValuesAndCommands()
        {
            var vm = new ConfirmPopupVM();
            int confirms = 0;

            vm.Setup("title", "message", () => confirms++);
            vm.ConfirmCmd.Execute();
            vm.CancelCmd.Execute();

            Assert.AreEqual("title", vm.Title.Value);
            Assert.AreEqual("message", vm.Message.Value);
            Assert.AreEqual(1, confirms);
        }

        // 확인 · 취소 · 닫기 버튼 · 배경은 콜백 뒤에 닫기를 요청한다 (사용자 10-21, B)
        [Test]
        public void ConfirmPopupVM_EachButton_RunsCallbackThenRequestsClose()
        {
            var vm = new ConfirmPopupVM();
            var log = new System.Collections.Generic.List<string>();
            vm.CloseRequested.Bind(_ => log.Add("close"));
            vm.Setup("title", null, () => log.Add("confirm"), () => log.Add("cancel"));

            vm.ConfirmCmd.Execute();
            vm.CancelCmd.Execute();
            vm.CloseCmd.Execute();

            CollectionAssert.AreEqual(new[] { "confirm", "close", "cancel", "close", "cancel", "close" }, log);
        }

        [Test]
        public void ConfirmPopupVM_WithoutSetup_ButtonsStillClose()
        {
            var vm = new ConfirmPopupVM();
            int closes = 0;
            vm.CloseRequested.Bind(_ => closes++);

            PrepareOpen(vm);
            vm.ConfirmCmd.Execute();
            vm.CancelCmd.Execute();

            Assert.AreEqual(2, closes, "setup 없이 열어도 팝업이 갇히면 안 된다");
        }
    }
}
