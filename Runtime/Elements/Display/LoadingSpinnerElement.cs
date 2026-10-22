using MVVM.UGUI.Elements.Core;
using UnityEngine;

namespace MVVM.UGUI.Elements.Display
{
    /// <summary>true 인 동안 보이고 돈다 (불러오는 중). steps 가 0 보다 크면 칸 단위로 끊어 돈다 (점 8개 스피너 그림).</summary>
    public class LoadingSpinnerElement : VisualElement<bool>
    {
        [Tooltip("켜고 끌 오브젝트 (비우면 자기 자신)")]
        [SerializeField] private GameObject root;
        [Tooltip("돌릴 그림 (비우면 자기 자신)")]
        [SerializeField] private RectTransform spinner;
        [SerializeField] private float degreesPerSecond = 360f;
        [SerializeField] private int steps;

        private bool _spinning;
        private float _angle;

        protected override void HandleValueChanged(bool loading)
        {
            _spinning = loading;
            (root ? root : gameObject).SetActive(loading);
        }

        private void Update()
        {
            if (!_spinning)
                return;

            _angle = (_angle + degreesPerSecond * Time.unscaledDeltaTime) % 360f;
            // steps 가 있으면 한 칸 (360 / steps 도) 단위로 끊어 보여준다
            float shown = steps > 0 ? Mathf.Floor(_angle / (360f / steps)) * (360f / steps) : _angle;
            (spinner ? spinner : transform).localRotation = Quaternion.Euler(0f, 0f, -shown);
        }
    }
}
