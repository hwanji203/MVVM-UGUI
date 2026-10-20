using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MVVM.UGUI.Elements.Core;
using UnityEngine;

namespace MVVM.UGUI.Elements
{
    /// <summary>
    /// 팝업 창의 열기 · 닫기 연출 (옛 DoTweenWindowForm) — 크기 closedScale ↔ 1, CanvasGroup 이 있으면 투명도도.
    /// PopupView 가 PlayOpen · PlayClose 를 기다린다. 닫는 중에 다시 열면 지금 크기에서 이어서 커진다.
    /// 값을 묶지 않는 Element 다 (생성된 속성으로 꺼내 PopupView 가 쓴다).
    /// </summary>
    public class WindowElement : Element, ITransitionElement
    {
        [SerializeField] private float openDuration = 0.25f;
        [SerializeField] private float closeDuration = 0.2f;
        [SerializeField] private Ease openEase = Ease.OutBack;
        [SerializeField] private Ease closeEase = Ease.InBack;
        [SerializeField] private float closedScale;
        [Tooltip("있으면 투명도도 함께 바꾸고, 닫는 동안 클릭을 막는다")]
        [SerializeField] private CanvasGroup group;

        // 닫기 연출이 끝까지 간 상태 (처음 포함) — 이때만 열기를 닫힌 모양에서 시작한다
        private bool _atClosed = true;

        public UniTask PlayOpen(CancellationToken token)
        {
            Seq?.Kill();
            if (_atClosed)
                Apply(closedScale, 0f);
            _atClosed = false;
            SetInput(true);

            if (!Application.isPlaying || openDuration <= 0f)
            {
                Apply(1f, 1f);
                return UniTask.CompletedTask;
            }

            Seq = MakeSequence(1f, 1f, openDuration, openEase);
            return TweenAwait.Wait(Seq, token);
        }

        public UniTask PlayClose(CancellationToken token)
        {
            Seq?.Kill();
            SetInput(false);

            if (!Application.isPlaying || closeDuration <= 0f)
            {
                Apply(closedScale, 0f);
                _atClosed = true;
                return UniTask.CompletedTask;
            }

            // OnComplete 는 끝까지 갔을 때만 — 중간에 취소되면 지금 크기에서 다시 열린다
            Seq = MakeSequence(closedScale, 0f, closeDuration, closeEase).OnComplete(() => _atClosed = true);
            return TweenAwait.Wait(Seq, token);
        }

        private Sequence MakeSequence(float scale, float alpha, float time, Ease curve)
        {
            var seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            seq.Append(transform.DOScale(scale, time).SetEase(curve));
            if (group != null)
                seq.Join(DOTween.To(() => group.alpha, a => group.alpha = a, alpha, time));
            return seq;
        }

        private void Apply(float scale, float alpha)
        {
            transform.localScale = Vector3.one * scale;
            if (group != null)
                group.alpha = alpha;
        }

        private void SetInput(bool enable)
        {
            if (group == null)
                return;
            group.interactable = enable;
            group.blocksRaycasts = enable;
        }

        private void Reset() => group = GetComponent<CanvasGroup>();
    }
}
