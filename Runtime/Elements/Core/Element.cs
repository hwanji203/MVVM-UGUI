using DG.Tweening;
using UnityEngine;

namespace MVVM.UGUI.Elements.Core
{
    public class Element : MonoBehaviour
    {
        protected RectTransform RectTrm;
        protected Sequence Seq;

        // 하위 Element 는 Bind 할 때 먼저 Unbind() 하고 여기에 구독을 담는다 — 파괴될 때 한 번에 풀린다
        protected readonly DisposableBag Bindings = new();

        protected virtual void Awake()
        {
            RectTrm = GetComponent<RectTransform>();
        }

        public virtual void Unbind() => Bindings.Clear();

        protected virtual void OnDestroy()
        {
            Unbind();
            Seq?.Kill();
            Seq = null;
        }
    }
}
