using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MVVMGUI.Core
{
    public abstract class ScreenView<T> : MonoBehaviour where T : AbstractViewModel
    {
        public Action OnOpen;
        public Action OnClose;

        private T _vm;

        // 지금 진행 중인 열기 · 닫기 — 새 열기 · 닫기가 오면 이전 것은 취소된다
        private CancellationTokenSource _transition;

        private void Awake()
        {
            _vm = CreateVM();
            Initialize();
            Bind(_vm);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Bind를 해서 값이 채워지기 전 정리
        /// </summary>
        protected virtual void Initialize() { }

        /// <summary>
        /// 여기서 vm에 있는 Bindable들을 Bind한다.
        /// </summary>
        protected virtual void Bind(T vm) { }

        /// <summary>켜고 → 열기 연출 (<see cref="PlayOpen"/>) 을 기다린 뒤 → OnOpen. 연출 중에 다른 열기 · 닫기가 오면 OnOpen 없이 끝난다.</summary>
        public async UniTask Open()
        {
            var token = BeginTransition();
            gameObject.SetActive(true);

            if (await PlayOpen(token).SuppressCancellationThrow())
                return;

            OnOpen?.Invoke();
        }

        /// <summary>닫기 연출 (<see cref="PlayClose"/>) 을 기다린 뒤 → 끄고 → OnClose. 이미 꺼져 있으면 아무것도 하지 않는다.</summary>
        public async UniTask Close()
        {
            if (!gameObject.activeSelf)
                return;

            var token = BeginTransition();

            if (await PlayClose(token).SuppressCancellationThrow())
                return;

            gameObject.SetActive(false);
            OnClose?.Invoke();
        }

        /// <summary>열기 연출. 하위 View (PopupView 등) 가 트윈을 돌리고 끝날 때까지 기다리게 한다. token 이 취소되면 멈춘다.</summary>
        protected virtual UniTask PlayOpen(CancellationToken token) => UniTask.CompletedTask;

        /// <summary>닫기 연출. 끝나면 오브젝트가 꺼진다.</summary>
        protected virtual UniTask PlayClose(CancellationToken token) => UniTask.CompletedTask;

        private CancellationToken BeginTransition()
        {
            CancelTransition();
            _transition = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            return _transition.Token;
        }

        private void CancelTransition()
        {
            _transition?.Cancel();
            _transition?.Dispose();
            _transition = null;
        }

        // 하위 View 가 OnDestroy 를 쓰면 override 하고 base.OnDestroy() 를 부른다
        protected virtual void OnDestroy()
        {
            CancelTransition();
            _vm?.Dispose();
            _vm = null;
        }

        protected abstract T CreateVM();
    }
}
