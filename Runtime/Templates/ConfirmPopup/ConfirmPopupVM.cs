using System;
using MVVM.UGUI.Binding;
using MVVM.UGUI.Templates.Popup;

namespace MVVM.UGUI.Templates.ConfirmPopup
{
    public class ConfirmPopupVM : AbstractPopupVM
    {
        public Bindable<string> Title { get; } = new();
        public Bindable<string> Message { get; } = new();
        public Command ConfirmCmd { get; }
        public Command CancelCmd { get; }

        private Action _onConfirm;
        private Action _onCancel;

        public ConfirmPopupVM()
        {
            ConfirmCmd = new Command(Confirm);
            CancelCmd = new Command(Dismiss);
        }

        // 여는 쪽: ScreenManager.Open<ConfirmPopupVM>(vm => vm.Setup(...))
        public void Setup(string title, string message, Action onConfirm, Action onCancel = null)
        {
            Title.Value = title;
            Message.Value = message;
            _onConfirm = onConfirm;
            _onCancel = onCancel;
        }

        private void Confirm()
        {
            _onConfirm?.Invoke();
            RequestClose();
        }

        // 취소 버튼 · 닫기 버튼 · 배경 — 확인 없이 닫히면 모두 취소
        protected override void Dismiss()
        {
            _onCancel?.Invoke();
            base.Dismiss();
        }

        protected override void ResetForOpen()
        {
            base.ResetForOpen();
            ResetAll(Title, Message, ConfirmCmd, CancelCmd);
            _onConfirm = null;
            _onCancel = null;
        }
    }
}
