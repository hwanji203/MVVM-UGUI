using MVVM.UGUI.Elements.Commands;
using MVVM.UGUI.Elements.Display.Texts;
using UnityEngine;

namespace MVVM.UGUI.Templates.ConfirmPopup
{
    public class ConfirmPopupView : AbstractPopupView<ConfirmPopupVM>
    {
        [SerializeField] private TextViewElement titleText;
        [SerializeField, Nullable] private TextViewElement messageText;
        [SerializeField] private ButtonElement confirmBtn;
        [SerializeField, Nullable] private ButtonElement cancelBtn;

        protected override void Bind(ConfirmPopupVM vm)
        {
            base.Bind(vm);
            
            titleText.Bind(vm.Title);
            messageText?.Bind(vm.Message);
            confirmBtn.Bind(vm.ConfirmCmd);
            cancelBtn?.Bind(vm.CancelCmd);
        }
    }
}