using MVVM.UGUI.Binding;
using MVVM.UGUI.Core;

namespace MVVM.UGUI.Templates.Popup
{
    /// <summary>
    /// 팝업 VM 의 바탕. VM 은 View 를 모르므로 닫기를 직접 못 하고 <see cref="RequestClose"/> 로 요청만 한다 —
    /// <see cref="AbstractPopupView{T}"/> 가 받아 닫는다. 그래서 닫을지 · 언제 닫을지 (검증 실패면 안 닫기 · 저장 뒤 닫기) 는 VM 이 정한다.
    /// </summary>
    public abstract class AbstractPopupVM : AbstractViewModel
    {
        // 값은 쓰지 않는다 — 인자 없는 Signal 이 없어서 bool 로 둔다
        private readonly Signal<bool> _closeRequested = new();

        public IReadOnlySignal<bool> CloseRequested => _closeRequested;

        /// <summary>닫기 버튼 · 배경. 누르면 <see cref="Dismiss"/>.</summary>
        public Command CloseCmd { get; }

        protected AbstractPopupVM() => CloseCmd = new Command(Dismiss);

        protected void RequestClose() => _closeRequested.Fire(true);

        /// <summary>
        /// 확인 없이 닫힐 때 (닫기 버튼 · 배경). 기본은 닫기만 — 취소 콜백이 있는 팝업은 덮어써서 부른 뒤 <c>base.Dismiss()</c>.
        /// </summary>
        protected virtual void Dismiss() => RequestClose();

        // 덮어쓰면 base.ResetForOpen() 을 부른다 (AbstractPopupView 의 Initialize · Bind 와 같은 규칙)
        protected override void ResetForOpen() => CloseCmd.Reset();
    }
}
