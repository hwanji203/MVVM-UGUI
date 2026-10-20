using System.Linq;
using MVVM.UGUI.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MVVM.UGUI.Editor
{
    /// <summary>생성기를 부르는 곳: 프리팹 저장 · 인스펙터 머리의 버튼 · Tools 메뉴, 그리고 컴파일 뒤 이어 채우기.</summary>
    [InitializeOnLoad]
    internal static class ElementGeneratorHooks
    {
        private const string MenuPath = "Tools/MVVM UGUI/선택한 View 의 Element 속성 생성";

        static ElementGeneratorHooks()
        {
            PrefabStage.prefabSaving += OnPrefabSaving;
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnHeaderGUI;
        }

        // 생성된 파일이 컴파일된 직후 — 기다리던 배열 채우기를 마친다.
        // (EditorApplication.delayCall 은 에디터가 다음에 갱신될 때까지 돌지 않아 여기서 바로 부른다)
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void AfterScriptsReloaded() => ElementGenerator.CompletePending();

        private static void OnPrefabSaving(GameObject root)
        {
            foreach (var view in ElementScan.ViewsIn(root))
                ElementGenerator.Generate(view, ElementGenerator.Trigger.PrefabSaving);
        }

        // GameObject 인스펙터 머리 (이름 · 태그 아래) 에 버튼 — View 가 붙은 오브젝트 하나를 골랐을 때만
        private static void OnHeaderGUI(UnityEditor.Editor editor)
        {
            if (editor.targets.Length != 1 || editor.target is not GameObject go)
                return;

            var view = ElementScan.FindView(go);
            if (view == null)
                return;

            if (GUILayout.Button("MVVM UGUI · Element 속성 생성"))
                ElementGenerator.Generate(view);
        }

        [MenuItem(MenuPath)]
        private static void GenerateSelected()
        {
            foreach (var go in Selection.gameObjects)
            {
                var view = ElementScan.FindView(go);
                if (view != null)
                    ElementGenerator.Generate(view);
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool CanGenerateSelected() =>
            Selection.gameObjects.Any(go => ElementScan.FindView(go) != null);
    }
}
