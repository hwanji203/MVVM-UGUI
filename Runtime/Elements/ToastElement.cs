using System.Collections.Generic;
using DG.Tweening;
using MVVMGUI.Elements.Core;
using TMPro;
using UnityEngine;

namespace MVVMGUI.Elements
{
    /// <summary>
    /// 잠깐 떴다 사라지는 알림 ← Signal&lt;string&gt; ("골드가 부족합니다"). 여러 개가 오면 차례로 보여준다.
    /// 같은 글이 연달아 와도 매번 뜬다. 꺼져 있을 때 온 것은 버린다.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ToastElement : SignalElement<string>
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text text;
        [SerializeField] private float fadeDuration = 0.2f;
        [SerializeField] private float showSeconds = 1.5f;
        [Tooltip("기다리는 알림이 이보다 많으면 오래된 것부터 버린다")]
        [SerializeField] private int maxQueue = 3;

        private readonly Queue<string> _queue = new();
        private bool _showing;

        protected override void Awake()
        {
            base.Awake();
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }

        protected override void HandleSignal(string message)
        {
            _queue.Enqueue(message);
            while (_queue.Count > maxQueue)
                _queue.Dequeue();

            if (!_showing)
                ShowNext();
        }

        private void ShowNext()
        {
            if (_queue.Count == 0)
            {
                _showing = false;
                return;
            }

            _showing = true;
            text.text = _queue.Dequeue();

            if (!Application.isPlaying)
            {
                group.alpha = 1f;
                _showing = false;
                return;
            }

            Seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            Seq.Append(DOTween.To(() => group.alpha, a => group.alpha = a, 1f, fadeDuration))
                .AppendInterval(showSeconds)
                .Append(DOTween.To(() => group.alpha, a => group.alpha = a, 0f, fadeDuration))
                .OnComplete(ShowNext);   // 다 사라지면 기다리던 다음 것
        }

        private void OnDisable()
        {
            Seq?.Kill();
            Seq = null;
            _queue.Clear();
            _showing = false;
            group.alpha = 0f;
        }

        private void Reset()
        {
            group = GetComponent<CanvasGroup>();
            text = GetComponentInChildren<TMP_Text>(true);
        }
    }
}
