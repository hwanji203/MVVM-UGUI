using DG.Tweening;
using MVVM.UGUI.Elements.Core;
using TMPro;
using UnityEngine;

namespace MVVM.UGUI.Elements.Display.Texts
{
    /// <summary>
    /// 새 글을 한 글자씩 보여준다 (대사 · 튜토리얼). 꺼져 있거나 플레이 중이 아니면 바로 전부.
    /// 눌러서 넘기기는 View 가 <see cref="Skip"/> 을 부른다.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TypewriterTextElement : VisualElement<string>
    {
        [SerializeField] private TMP_Text tmp;
        [SerializeField] private float charactersPerSecond = 30f;
        [SerializeField] private bool ignoreTimeScale = true;

        public bool IsTyping => Seq != null && Seq.IsActive();

        protected override void HandleValueChanged(string value)
        {
            Seq?.Kill();
            Seq = null;

            tmp.text = value;
            tmp.maxVisibleCharacters = int.MaxValue;

            if (string.IsNullOrEmpty(value) || charactersPerSecond <= 0f || !Application.isPlaying || !isActiveAndEnabled)
                return;

            // 글자 수는 메시를 만들어야 나온다 (리치 텍스트 태그는 세지 않는다)
            tmp.ForceMeshUpdate();
            int total = tmp.textInfo.characterCount;
            tmp.maxVisibleCharacters = 0;

            Seq = DOTween.Sequence().SetUpdate(ignoreTimeScale).SetLink(gameObject);
            Seq.Append(DOTween.To(() => tmp.maxVisibleCharacters, v => tmp.maxVisibleCharacters = v, total,
                total / charactersPerSecond).SetEase(Ease.Linear));
        }

        /// <summary>치는 중이면 바로 전부 보여준다.</summary>
        public void Skip()
        {
            Seq?.Kill();
            Seq = null;
            tmp.maxVisibleCharacters = int.MaxValue;
        }

        private void OnDisable() => Skip();

        private void Reset() => tmp = GetComponent<TMP_Text>();
    }
}
