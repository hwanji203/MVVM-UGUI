using System;
using MVVMGUI.Binding;
using MVVMGUI.Core;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MVVMGUI.Tests
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
    }

    [ExecuteAlways]
    public class TestView : ScreenView<TestViewModel>
    {
        public TestViewModel Created;
        public TestViewModel BoundWith;

        protected override TestViewModel CreateViewModel() => Created = new TestViewModel();
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
            var vm = view.Created;

            Assert.IsNotNull(vm, "Awake 에서 CreateVM 이 불려야 한다");
            Assert.AreSame(vm, view.BoundWith, "만든 VM 으로 Bind 해야 한다");

            Object.DestroyImmediate(go);

            Assert.AreEqual(1, vm.DisposeCount, "파괴될 때 VM 을 Dispose 해야 한다");
        }
    }
}
