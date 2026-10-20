using MVVMGUI.Binding;
using UnityEngine;

namespace MVVMGUI.Elements.Core
{
    public abstract class FormElement<T> : VisualElement<T>
    {
        protected Bindable<T> Source;

        public sealed override void Bind(IReadOnlyBindable<T> source)
        {
            // base.Bind 가 먼저 Unbind 하며 Source 를 비우므로 그 뒤에 잡는다
            base.Bind(source);
            Source = source as Bindable<T>;
            Debug.Assert(Source != null, "양방향 Element 는 Bindable<T> 를 받아야 한다 (파생 값은 쓸 수 없다)", this);
        }

        public override void Unbind()
        {
            base.Unbind();
            Source = null;
        }

        // 조작 → ViewModel. 알림이 나가 같은 값을 보는 다른 곳도 따라온다 (DESIGN 3.2)
        protected void Write(T value)
        {
            if (Source != null)
                Source.Value = value;
        }
    }
}
