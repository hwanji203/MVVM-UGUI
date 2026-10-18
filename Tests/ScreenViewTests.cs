using System.Threading;
using Cysharp.Threading.Tasks;
using MVVMGUI.Core;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MVVMGUI.Tests
{
    // 열기 · 닫기 연출이 끝나는 때를 테스트가 정한다 (Gate 가 null 이면 바로 끝)
    [ExecuteAlways]
    public class AnimatedTestView : ScreenView<TestViewModel>
    {
        public UniTaskCompletionSource OpenGate;
        public UniTaskCompletionSource CloseGate;
        public int Opened;
        public int Closed;

        protected override TestViewModel CreateVM() => new();

        protected override void Initialize()
        {
            OnOpen += () => Opened++;
            OnClose += () => Closed++;
        }

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
            if (_go != null) Object.DestroyImmediate(_go);
        }

        [Test]
        public void StartsHidden()
        {
            Assert.IsFalse(_go.activeSelf);
        }

        [Test]
        public void Open_WithoutAnimation_ShowsAndNotifies()
        {
            _view.Open().Forget();

            Assert.IsTrue(_go.activeSelf);
            Assert.AreEqual(1, _view.Opened);
        }

        [Test]
        public void Open_NotifiesOnlyAfterAnimationEnds()
        {
            _view.OpenGate = new UniTaskCompletionSource();

            _view.Open().Forget();
            Assert.IsTrue(_go.activeSelf, "연출 전에 켜져야 한다");
            Assert.AreEqual(0, _view.Opened);

            _view.OpenGate.TrySetResult();
            Assert.AreEqual(1, _view.Opened);
        }

        [Test]
        public void Close_HidesOnlyAfterAnimationEnds()
        {
            _view.Open().Forget();
            _view.CloseGate = new UniTaskCompletionSource();

            _view.Close().Forget();
            Assert.IsTrue(_go.activeSelf, "닫기 연출 동안은 켜져 있어야 한다");

            _view.CloseGate.TrySetResult();
            Assert.IsFalse(_go.activeSelf);
            Assert.AreEqual(1, _view.Closed);
        }

        [Test]
        public void Close_DuringOpenAnimation_CancelsOpen()
        {
            _view.OpenGate = new UniTaskCompletionSource();
            _view.Open().Forget();

            _view.Close().Forget();
            _view.OpenGate.TrySetResult();

            Assert.IsFalse(_go.activeSelf);
            Assert.AreEqual(0, _view.Opened, "취소된 열기는 OnOpen 을 부르지 않는다");
            Assert.AreEqual(1, _view.Closed);
        }

        [Test]
        public void Close_WhenAlreadyClosed_DoesNothing()
        {
            _view.Close().Forget();

            Assert.AreEqual(0, _view.Closed);
        }

        [Test]
        public void Destroy_DuringOpenAnimation_DoesNotNotify()
        {
            _view.OpenGate = new UniTaskCompletionSource();
            _view.Open().Forget();
            var view = _view;

            Object.DestroyImmediate(_go);
            _go = null;

            Assert.AreEqual(0, view.Opened);
        }
    }
}
