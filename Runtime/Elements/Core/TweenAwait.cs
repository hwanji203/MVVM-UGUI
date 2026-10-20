using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace MVVMGUI.Elements.Core
{
    /// <summary>트윈이 끝나거나 죽을 때까지 기다린다. token 이 취소되면 트윈을 Kill 한다.</summary>
    internal static class TweenAwait
    {
        public static UniTask Wait(Tween tween, CancellationToken token)
        {
            if (tween == null || !tween.IsActive())
                return UniTask.CompletedTask;

            var source = new UniTaskCompletionSource();
            // 끝까지 가도 (autoKill) 중간에 Kill 돼도 OnKill 이 불리므로 이것 하나로 기다림이 풀린다
            tween.OnKill(() => source.TrySetResult());
            var registration = token.Register(() => tween.Kill());
            return source.Task.ContinueWith(() => registration.Dispose());
        }
    }
}
