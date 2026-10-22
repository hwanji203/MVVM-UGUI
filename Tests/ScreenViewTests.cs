using System.Threading;
using Cysharp.Threading.Tasks;
using MVVM.UGUI.Core;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MVVM.UGUI.Tests
{
    // 열기 · 닫기 연출이 끝나는 때를 테스트가 정한다 (Gate 가 null 이면 바로 끝)
    [ExecuteAlways]
    public class AnimatedTestView : ScreenView<TestViewModel>
    {
        public UniTaskCompletionSource OpenGate;
        public UniTaskCompletionSource CloseGate;

        protected override void Bind(TestViewModel vm) { }

        protected override UniTask PlayOpen(CancellationToken token) =>
            OpenGate == null ? UniTask.CompletedTask : OpenGate.Task.AttachExternalCancellation(token);

        protected override UniTask PlayClose(CancellationToken token) =>
            CloseGate == null ? UniTask.CompletedTask : CloseGate.Task.AttachExternalCancellation(token);
    }

    public class ScreenViewTests
    {
        private GameObject _go;
        private AnimatedTestView _view;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("View");
            _view = _go.AddComponent<AnimatedTestView>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go) Object.DestroyImmediate(_go);
        }

        private static bool IsDone(UniTask task) => task.Status == UniTaskStatus.Succeeded;

        [Test]
        public void StartsHidden()
        {
            Assert.IsFalse(_go.activeSelf);
        }

        [Test]
        public void Open_WithoutAnimation_ShowsAndCompletes()
        {
            var open = _view.OpenAsync();

            Assert.IsTrue(_go.activeSelf);
            Assert.IsTrue(IsDone(open));
        }

        [Test]
        public void Open_CompletesOnlyAfterAnimationEnds()
        {
            _view.OpenGate = new UniTaskCompletionSource();

            var open = _view.OpenAsync();
            Assert.IsTrue(_go.activeSelf, "연출 전에 켜져야 한다");
            Assert.IsFalse(IsDone(open));

            _view.OpenGate.TrySetResult();
            Assert.IsTrue(IsDone(open));
        }

        [Test]
        public void Close_HidesOnlyAfterAnimationEnds()
        {
            _view.OpenAsync().Forget();
            _view.CloseGate = new UniTaskCompletionSource();

            var close = _view.CloseAsync();
            Assert.IsTrue(_go.activeSelf, "닫기 연출 동안은 켜져 있어야 한다");
            Assert.IsFalse(IsDone(close));

            _view.CloseGate.TrySetResult();
            Assert.IsFalse(_go.activeSelf);
            Assert.IsTrue(IsDone(close));
        }

        [Test]
        public void Close_DuringOpenAnimation_CancelsOpen()
        {
            _view.OpenGate = new UniTaskCompletionSource();
            var open = _view.OpenAsync();

            _view.CloseAsync().Forget();

            Assert.IsFalse(_go.activeSelf);
            Assert.IsTrue(IsDone(open), "취소된 열기를 기다리던 쪽도 풀려야 한다");
        }

        [Test]
        public void Open_DuringCloseAnimation_CancelsClose()
        {
            _view.OpenAsync().Forget();
            _view.CloseGate = new UniTaskCompletionSource();
            _view.CloseAsync().Forget();

            _view.OpenAsync().Forget();
            _view.CloseGate.TrySetResult();

            Assert.IsTrue(_go.activeSelf, "취소된 닫기가 나중에 끄면 안 된다");
        }

        // 열림 = 열기를 시작한 때부터 닫기를 시작할 때까지 (사용자 10-22 — IsClosing 을 없애고 IsOpen 하나로)
        [Test]
        public void IsOpen_FromOpenUntilCloseStarts()
        {
            _view.CloseGate = new UniTaskCompletionSource();
            Assert.IsFalse(_view.IsOpen, "처음은 닫힘");

            _view.OpenAsync().Forget();
            Assert.IsTrue(_view.IsOpen);

            _view.CloseAsync().Forget();
            Assert.IsFalse(_view.IsOpen, "닫기를 시작하면 연출 중에도 닫힘");
            Assert.IsTrue(_go.activeSelf, "연출이 끝날 때까지는 켜져 있다");

            _view.CloseGate.TrySetResult();
            Assert.IsFalse(_view.IsOpen);
        }

        [Test]
        public void IsOpen_TrueAgainWhenOpenCancelsClose()
        {
            _view.OpenAsync().Forget();
            _view.CloseGate = new UniTaskCompletionSource();
            _view.CloseAsync().Forget();

            _view.OpenAsync().Forget();
            _view.CloseGate.TrySetResult();

            Assert.IsTrue(_view.IsOpen, "취소된 닫기가 끝나며 닫힘으로 되돌리면 안 된다");
        }

        [Test]
        public void Close_WhileClosing_DoesNothing()
        {
            _view.OpenAsync().Forget();
            _view.CloseGate = new UniTaskCompletionSource();
            var first = _view.CloseAsync();

            var second = _view.CloseAsync();
            Assert.IsTrue(IsDone(second), "이미 닫는 중이면 바로 끝난다 (연출을 다시 시작하지 않는다)");
            Assert.IsFalse(IsDone(first));

            _view.CloseGate.TrySetResult();
            Assert.IsTrue(IsDone(first));
            Assert.IsFalse(_go.activeSelf);
        }

        [Test]
        public void Close_WhenAlreadyClosed_DoesNothing()
        {
            var close = _view.CloseAsync();

            Assert.IsFalse(_go.activeSelf);
            Assert.IsTrue(IsDone(close));
        }

        // 재사용 (사용자 10-19): 닫힌 화면을 다시 연다
        [Test]
        public void Reopen_AfterClose_ShowsAgain()
        {
            _view.OpenAsync().Forget();
            _view.CloseAsync().Forget();

            _view.OpenAsync().Forget();

            Assert.IsTrue(_go.activeSelf);
        }

        [Test]
        public void Destroy_DuringOpenAnimation_ReleasesAwaiter()
        {
            _view.OpenGate = new UniTaskCompletionSource();
            var open = _view.OpenAsync();

            Object.DestroyImmediate(_go);
            _go = null;

            Assert.IsTrue(IsDone(open));
        }
    }
}
