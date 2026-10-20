using System.Threading;
using Cysharp.Threading.Tasks;

namespace MVVMGUI.Elements.Core
{
    /// <summary>
    /// 화면이 열리고 닫힐 때 연출하는 Element (창 · 배경). PopupView 의 PlayOpen · PlayClose 가 기다린다.
    /// token 이 취소되면 연출을 멈추고 바로 끝난다.
    /// </summary>
    public interface ITransitionElement
    {
        UniTask PlayOpen(CancellationToken token);
        UniTask PlayClose(CancellationToken token);
    }
}
