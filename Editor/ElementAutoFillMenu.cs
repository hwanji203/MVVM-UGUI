using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MVVM.UGUI.Editor
{
    /// <summary>"이름으로 채우기" 를 부르는 곳: GameObject 인스펙터 머리의 버튼 · Tools 메뉴.</summary>
    [InitializeOnLoad]
    internal static class ElementAutoFillMenu
    {
        private const string MenuPath = "Tools/MVVM UGUI/선택한 View 의 Element 이름으로 채우기";
        private const string LogPrefix = "[MVVM UGUI] ";

        static ElementAutoFillMenu()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnHeaderGUI;
        }

        // View 가 붙은 오브젝트 하나를 골랐을 때만 버튼을 보인다
        private static void OnHeaderGUI(UnityEditor.Editor editor)
        {
            if (editor.targets.Length != 1 || editor.target is not GameObject go)
                return;

            var views = ViewsOn(go);
            if (views.Count == 0)
                return;

            if (GUILayout.Button("MVVM UGUI · Element 이름으로 채우기"))
                FillAndLog(views);
        }

        [MenuItem(MenuPath)]
        private static void FillSelected()
        {
            FillAndLog(Selection.gameObjects.SelectMany(ViewsOn).ToList());
        }

        [MenuItem(MenuPath, true)]
        private static bool CanFillSelected() => Selection.gameObjects.Any(go => ViewsOn(go).Count > 0);

        private static List<MonoBehaviour> ViewsOn(GameObject go) =>
            go.GetComponents<MonoBehaviour>().Where(c => c != null && ElementAutoFill.IsView(c)).ToList();

        private static void FillAndLog(List<MonoBehaviour> views)
        {
            foreach (var view in views)
            {
                var report = ElementAutoFill.Fill(view);
                string message = $"{LogPrefix}{view.GetType().Name}: 채움 {report.Filled.Count}개" +
                                 List(" ", report.Filled) +
                                 (report.NotFound.Count > 0 ? $"\n- 같은 이름의 자식이 없음 (끌어다 놓기): {string.Join(", ", report.NotFound)}" : "") +
                                 (report.Ambiguous.Count > 0 ? $"\n- 같은 이름이 둘 이상이라 비워 둠: {string.Join(", ", report.Ambiguous)}" : "");

                if (report.NotFound.Count > 0 || report.Ambiguous.Count > 0)
                    Debug.LogWarning(message, view);
                else
                    Debug.Log(message, view);
            }
        }

        private static string List(string prefix, List<string> names) =>
            names.Count > 0 ? prefix + "(" + string.Join(", ", names) + ")" : "";
    }
}
