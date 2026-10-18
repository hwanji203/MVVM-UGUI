using DG.Tweening;
using UnityEngine;

namespace MVVMGUI.Elements.Core
{
    public class Element : MonoBehaviour
    {
        protected RectTransform RectTrm;
        protected Sequence Seq;

        protected virtual void Awake()
        {
            RectTrm = GetComponent<RectTransform>();
        }

        public virtual void OnDestroy()
        {
            Seq?.Kill();
            Seq = null;
        }
    }
}