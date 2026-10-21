using System.Collections.Generic;
using DG.Tweening;
using MVVM.UGUI.Elements.Core;
using TMPro;
using UnityEngine;

namespace MVVM.UGUI.Elements.Feedback
{
    /// <summary>
    /// 떠오르며 사라지는 글 ← Signal&lt;string&gt; (데미지 숫자 "-10" · "CRIT!"). 같은 값이 연달아 와도 매번 뜬다.
    /// 꺼 둔 자식 TMP 하나 (template) 를 복제해 쓰고, 끝난 것은 꺼 두었다가 다시 쓴다.
    /// 월드 위치를 따라가려면 같은 오브젝트에 WorldFollowElement 를 붙인다.
    /// </summary>
    public class FloatingTextElement : SignalElement<string>
    {
        [Tooltip("복제할 글 (꺼 둔 자식)")]
        [SerializeField] private TMP_Text template;
        [SerializeField] private float riseDistance = 60f;
        [SerializeField] private float duration = 0.8f;
        [Tooltip("가로로 흩어지는 폭 (±)")]
        [SerializeField] private float randomX = 20f;

        // 복제한 글과 그 트윈 — 꺼질 때 남은 트윈을 멈추려고 같이 든다
        private class Entry
        {
            public TMP_Text Label;
            public Sequence Seq;
        }

        private readonly List<Entry> _pool = new();

        protected override void Awake()
        {
            base.Awake();
            template.gameObject.SetActive(false);
        }

        protected override void HandleSignal(string message)
        {
            if (!Application.isPlaying)
                return;

            var entry = Take();
            var label = entry.Label;
            label.text = message;
            label.alpha = 1f;
            var start = template.transform.localPosition + new Vector3(Random.Range(-randomX, randomX), 0f, 0f);
            label.transform.localPosition = start;
            label.gameObject.SetActive(true);

            entry.Seq = DOTween.Sequence().SetUpdate(true).SetLink(label.gameObject)
                .Append(label.transform.DOLocalMoveY(start.y + riseDistance, duration).SetEase(Ease.OutCubic))
                .Join(DOTween.To(() => label.alpha, a => label.alpha = a, 0f, duration).SetEase(Ease.InQuad))
                .OnComplete(() => label.gameObject.SetActive(false));
        }

        private Entry Take()
        {
            foreach (var entry in _pool)
            {
                if (!entry.Label.gameObject.activeSelf)   // 끝나서 꺼진 것을 다시 쓴다
                    return entry;
            }

            var created = new Entry { Label = Instantiate(template, template.transform.parent) };
            _pool.Add(created);
            return created;
        }

        private void OnDisable()
        {
            foreach (var entry in _pool)
            {
                entry.Seq?.Kill();
                entry.Seq = null;
                if (entry.Label != null)
                    entry.Label.gameObject.SetActive(false);
            }
        }
    }
}
