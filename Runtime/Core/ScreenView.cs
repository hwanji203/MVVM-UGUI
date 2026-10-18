using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MVVMGUI.Core
{
    /// <summary>
    /// 모든 화면의 바탕 (제네릭 아님) — 열기 · 닫기와 그 연출만 안다. ScreenManager 는 이것만 보고 화면을 다룬다.
    /// ViewModel 을 쓰는 화면은 <see cref="ScreenView{T}"/> 를 상속한다.
    /// </summary>
    public abstract class ScreenView : MonoBehaviour
    {
        public Action OnOpen;
        public Action OnClose;

        // 지금 진행 중인 열기 · 닫기 — 새 열기 · 닫기가 오면 이전 것은 취소된다
        private CancellationTokenSource _transition;

        /// <summary>
        /// 켜고 → 열기 연출 (<see cref="PlayOpen"/>) 을 기다린 뒤 → OnOpen. 연출 중에 다른 열기 · 닫기가 오면 OnOpen 없이 끝난다.
        /// </summary>
        public async UniTask Open()
        {
            var token = BeginTransition();
            gameObject.SetActive(true);

            //취소됐을 때 try-catch 문으로 잡으면 코드가 길어지니 취소됐을 때 true를 반환하도록 .SuppressCancellationThrow()를 사용
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
            //CreateLinkedTokenSource로 인수로 넣은 토큰(게임오브젝트 파괴시 Cancel)이 Cancel될 때 생성한 토큰도 같이 Cancel되도록 생성
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
            //CreateLinkedTokenSource가 있기 때문에 파괴될 때 이미 Cancel을 부르고 있기는 하지만, 자식이 OnDestroy를 빼먹을 수도 있기 때문에 안전코드
            CancelTransition();
        }
    }

    /// <summary>ViewModel 을 쓰는 화면. T 덕분에 CreateVM · Bind 가 형변환 없이 자기 ViewModel 을 받는다.</summary>
    public abstract class ScreenView<T> : ScreenView where T : AbstractViewModel
    {
        private T _vm;

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

        protected abstract T CreateVM();

        protected override void OnDestroy()
        {
            _vm?.Dispose();
            _vm = null;
            base.OnDestroy();
        }
    }
}
