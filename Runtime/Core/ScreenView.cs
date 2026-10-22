using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MVVM.UGUI.Elements.Core;
using UnityEngine;

namespace MVVM.UGUI.Core
{
    /// <summary>
    /// 모든 화면의 바탕 (제네릭 아님) — 열기 · 닫기와 그 연출만 안다. ScreenManager 는 이것만 보고 화면을 다룬다.
    /// ViewModel 을 쓰는 화면은 <see cref="ScreenView{T}"/> 를 상속한다.
    /// </summary>
    public abstract class ScreenView : MonoBehaviour
    {
        // 지금 진행 중인 열기 · 닫기 — 새 열기 · 닫기가 오면 이전 것은 취소된다
        private CancellationTokenSource _transition;

        /// <summary>열기를 시작한 때부터 닫기를 시작할 때까지 true. 누가 닫든 (매니저 · 자기 버튼) 닫기를 시작하면 연출 중에도 false (사용자 10-22).</summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// 켜고 → 열기 연출 (<see cref="PlayOpen"/>) 을 기다린 뒤 → OnOpen. 켜기 전 vm을 setup하고 켠다.
        /// </summary>
        public async UniTask OpenAsync()
        {
            IsOpen = true;
            
            var token = BeginTransition();
            gameObject.SetActive(true);

            await PlayOpen(token).SuppressCancellationThrow();
        }

        /// <summary>닫기 연출 (<see cref="PlayClose"/>) 을 기다린 뒤 → 끈다. 열려 있지 않으면 (이미 닫혔거나 닫는 중) 아무것도 하지 않는다.</summary>
        public async UniTask CloseAsync()
        {
            if (!IsOpen)
                return;
            IsOpen = false;

            var token = BeginTransition();

            //취소됐을 때 try-catch 문으로 잡으면 코드가 길어지니 취소됐을 때 true를 반환하도록 .SuppressCancellationThrow()를 사용
            //취소됐으면 새 열기가 취소한 것이다 — 그 열기가 이미 IsOpen 을 켰으니 끄지 않는다
            if (await PlayClose(token).SuppressCancellationThrow())
                return;

            gameObject.SetActive(false);
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
        
        #region Helper

        protected void SetClickable(CanvasGroup canvasGroup, bool value)
        {
            canvasGroup.interactable = value;
            canvasGroup.blocksRaycasts = value;
        }
        
        #endregion
    }

    /// <summary>ViewModel 을 쓰는 화면. T 덕분에 CreateVM · Bind 가 형변환 없이 자기 ViewModel 을 받는다.</summary>
    public abstract class  ScreenView<T> : ScreenView where T : AbstractViewModel, new()
    {
        [SerializeField] internal T viewModel = new T();

        /// <summary>
        /// 중요한 내용이기 때문에 자식에서 구현하지 않는 것을 대비해 Initialize를 따로 만듦.
        /// </summary>
        private void Awake()
        {
            Initialize();
            if (viewModel is IInitializable initializable)
                initializable.Initialize();
            Bind(viewModel);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Bind를 해서 값이 채워지기 전 정리
        /// </summary>
        protected virtual void Initialize() { }

        /// <summary>
        /// 여기서 vm에 있는 Bindable들을 Bind한다.
        /// </summary>
        protected abstract void Bind(T vm);

        protected override void OnDestroy()
        {
            viewModel?.Dispose();
            viewModel = null;
            base.OnDestroy();
        }
    }
}
