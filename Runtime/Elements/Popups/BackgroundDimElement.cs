using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MVVM.UGUI.Elements.Core;
using MVVM.UGUI.Sound;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVVM.UGUI.Elements.Popups
{
    /// <summary>
    /// 팝업 뒤 어두운 배경 (옛 BackgroundForm) — 열고 닫을 때 페이드, 누르면 Command (보통 닫기).
    /// 어둡기는 Image 색의 알파로 정하고 이 Element 는 CanvasGroup 0 ↔ 1 만 한다. Command 를 묶지 않으면 눌러도 아무 일 없다.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class BackgroundDimElement : CommandElement, ITransitionElement, IPointerClickHandler
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private float openDuration = 0.2f;
        [SerializeField] private float closeDuration = 0.2f;
        [SerializeField] private UISoundSO clickSound;

        private bool _active;

        // 처음 (프리팹 알파 그대로) 에는 투명에서 시작한다
        private bool _faded;

        public UniTask PlayOpen(CancellationToken token) => Fade(1f, openDuration, token);

        public UniTask PlayClose(CancellationToken token) => Fade(0f, closeDuration, token);

        private UniTask Fade(float alpha, float time, CancellationToken token)
        {
            Seq?.Kill();
            bool opening = alpha > 0f;
            if (!_faded)
            {
                _faded = true;
                group.alpha = 0f;
            }
            group.blocksRaycasts = opening;
            group.interactable = opening;

            if (!Application.isPlaying || time <= 0f)
            {
                group.alpha = alpha;
                return UniTask.CompletedTask;
            }

            Seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            Seq.Append(DOTween.To(() => group.alpha, a => group.alpha = a, alpha, time).SetEase(Ease.Linear));
            return TweenAwait.Wait(Seq, token);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_active)
                return;
            if (clickSound)
                clickSound.Play();
            Execute();
        }

        protected override void HandleActiveChanged(bool enable) => _active = enable;

        private void Reset() => group = GetComponent<CanvasGroup>();
    }
}
