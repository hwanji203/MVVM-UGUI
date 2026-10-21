namespace MVVM.UGUI.Binding
{
    /// <summary>
    /// 만든 직후 상태로 되돌릴 수 있는 바인딩 (Bindable · BindableList · Command).
    /// VM 의 ResetForOpen 에서 <c>ResetAll(Title, ConfirmCmd, …)</c> 한 줄로 쓴다.
    /// 파생 값 (Select · Combine) 은 원본을 따라가므로 구현하지 않는다 — 넘기면 컴파일 오류.
    /// </summary>
    public interface IResettable
    {
        void Reset();
    }
}
