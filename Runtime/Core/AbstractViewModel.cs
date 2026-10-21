using System;

namespace MVVM.UGUI.Core
{
    public abstract class AbstractViewModel : IDisposable
    {
        private readonly DisposableBag _owned = new();

        public virtual void Dispose() => _owned.Clear();

        protected void Own(IDisposable unSub) => _owned.Add(unSub);

        /// <summary>
        /// 열 때마다 setup 직전에 불린다 — 지난번 setup 이 넣은 값 · 콜백을 처음 상태로 되돌린다.
        /// 값만 바꾸고 Bindable · Command 를 새로 만들지 않는다 (View 는 Awake 에서 한 번 묶은 객체를 든다).
        /// 한 번만 할 일 (구독 · Own) 은 여기가 아니라 <see cref="IInitializable.Initialize"/> 에서.
        /// </summary>
        protected abstract void ResetForOpen();

        // ScreenManager 만 부른다 — 하위 VM 은 어느 어셈블리에 있든 protected override 로 쓴다
        internal void PrepareOpen() => ResetForOpen();
    }
}
