using System.Threading;
using Cysharp.Threading.Tasks;
using MVVM.UGUI.Binding;
using MVVM.UGUI.Core;
using MVVM.UGUI.Elements.Commands;
using MVVM.UGUI.Elements.Core;
using MVVM.UGUI.Elements.Popups;
using UnityEngine;

namespace MVVM.UGUI.Templates
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class AbstractPopupView<T> : ScreenView<T> where T : AbstractViewModel, new()
    {
        //팝업은 일단 열리고 닫힐 때 DOTween이 들어감.
        // 인터페이스 (ITransitionElement) 는 직렬화가 안 돼 바탕 클래스로 받는다 — WindowElement · 옆 패널 등
        [SerializeField] private TransitionElement window;
        [SerializeField, Nullable] private ButtonElement closeButton;
        [SerializeField, Nullable] private BackgroundDimElement backgroundDim;
        
        private CanvasGroup _canvasGroup;

        protected override void Initialize()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void Bind(T vm)
        {
            Command closeCmd = new Command(() => CloseAsync().Forget());
            closeButton?.Bind(closeCmd);
            backgroundDim?.Bind(closeCmd);
        }

        protected override async UniTask PlayOpen(CancellationToken token)
        {
            SetClickable(_canvasGroup, false);
            
            if (backgroundDim == null)
                await window.PlayOpen(token);
            else
                await UniTask.WhenAll(window.PlayOpen(token), backgroundDim.PlayOpen(token));
            
            if (!token.IsCancellationRequested) 
                SetClickable(_canvasGroup, true);
        }
        
        protected override async UniTask PlayClose(CancellationToken token)
        {
            SetClickable(_canvasGroup, false);
            
            if (backgroundDim == null)
                await window.PlayClose(token);
            else
                await UniTask.WhenAll(window.PlayClose(token), backgroundDim.PlayClose(token));
        }
    }
}