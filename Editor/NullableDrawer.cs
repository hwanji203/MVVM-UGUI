using UnityEditor;
using UnityEngine;

namespace MVVM.UGUI.Editor
{
    /// <summary><see cref="NullableAttribute"/> 필드의 이름을 기울임꼴로 그리고, 툴팁에 비워도 된다고 덧붙인다.</summary>
    [CustomPropertyDrawer(typeof(NullableAttribute))]
    public class NullableDrawer : PropertyDrawer
    {
        private const string Hint = "비워 둬도 된다 ([Nullable])";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 받은 label 은 Unity 가 다시 쓸 수 있어 툴팁을 덧붙이면 그릴 때마다 길어진다 — 새로 만든다
            string tooltip = string.IsNullOrEmpty(label.tooltip) ? Hint : $"{label.tooltip}\n{Hint}";
            var content = new GUIContent(label.text, label.image, tooltip);

            // 이름은 PropertyField 안에서 EditorStyles.label 로 그려진다. 잠깐 바꿨다가 되돌린다 —
            // 이름을 직접 그리면 프리팹 덮어쓰기 굵은 글씨 · 우클릭 메뉴 · 끌어다 놓기를 다시 만들어야 한다
            GUIStyle style = EditorStyles.label;
            FontStyle previous = style.fontStyle;
            style.fontStyle = FontStyle.Italic;
            try
            {
                EditorGUI.PropertyField(position, property, content, true);
            }
            finally
            {
                style.fontStyle = previous;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);
    }
}
