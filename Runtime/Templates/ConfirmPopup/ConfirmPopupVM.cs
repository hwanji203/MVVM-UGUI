using System;
using MVVM.UGUI.Binding;
using MVVM.UGUI.Core;

namespace MVVM.UGUI.Templates.ConfirmPopup
{
    public class ConfirmPopupVM : AbstractViewModel
    {
        public Bindable<string> Title { get; } = new();
        public Bindable<string> Message { get; } = new();
        public Command ConfirmCmd { get; } = new();
        public Command CancelCmd { get; } = new();

        // 여는 쪽: ScreenManager.Open<ConfirmPopupVM>(vm => vm.Setup(...))
        public void Setup(string title, string message, Action onConfirm, Action onCancel = null)
        {
            Title.Value = title;
            Message.Value = message;
            ConfirmCmd.Action = onConfirm;
            CancelCmd.Action = onCancel;
        }

        protected override void ResetForOpen()
        {
            ResetAll(Title, Message, ConfirmCmd, CancelCmd);
        }
    }
}
