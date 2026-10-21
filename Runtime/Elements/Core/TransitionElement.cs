using System.Threading;
using Cysharp.Threading.Tasks;

namespace MVVM.UGUI.Elements.Core
{
    /// <summary>
    /// 열기 · 닫기 연출만 하는 Element 의 바탕 (창 · 옆 패널). <see cref="ITransitionElement"/> 는 인터페이스라
    /// 인스펙터 필드로 받을 수 없어서, 팝업 같은 틀은 이 타입으로 받는다 — 인스펙터가 이 타입만 받아 잘못 넣을 수 없다.
    /// Command 도 받아야 하는 것 (<c>BackgroundDimElement</c>) 은 단일 상속 때문에 이걸 못 쓰고 인터페이스만 구현한다.
    /// </summary>
    public abstract class TransitionElement : Element, ITransitionElement
    {
        public abstract UniTask PlayOpen(CancellationToken token);
        public abstract UniTask PlayClose(CancellationToken token);
    }
}
