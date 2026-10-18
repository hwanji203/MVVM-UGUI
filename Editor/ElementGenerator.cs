using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MVVMGUI.Elements.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MVVMGUI.Editor
{
    /// <summary>
    /// View 의 `&lt;View&gt;.Elements.g.cs` 를 쓰고, 프리팹 · 씬의 숨은 배열에 Element 참조를 채운다.
    /// 배열 필드는 생성된 파일에 있어서, 처음 한 번은 "파일 쓰기 → 컴파일 → 배열 채우기" 두 단계다 —
    /// 컴파일이 끝나면 <see cref="CompletePending"/> 가 이어서 채운다.
    /// </summary>
    public static class ElementGenerator
    {
        public enum Result { Failed, Done, WaitingForCompile }

        public enum Trigger { Manual, PrefabSaving }

        private const string PendingKey = "MVVMGUI.ElementGenerator.Pending";
        private const string LogPrefix = "[MVVMGUI] ";

        public static Result Generate(MonoBehaviour view, Trigger trigger = Trigger.Manual)
        {
            var type = view.GetType();

            if (type.IsGenericType || type.IsNested)
                return Fail(view, $"{type.Name}: 제네릭 · 중첩 클래스 View 는 생성하지 않는다.");

            var script = MonoScript.FromMonoBehaviour(view);
            string scriptPath = script != null ? AssetDatabase.GetAssetPath(script) : null;
            if (string.IsNullOrEmpty(scriptPath) || !scriptPath.EndsWith(".cs"))
                return Fail(view, $"{type.Name}: View 스크립트 파일을 찾지 못했다 (파일 이름 = 클래스 이름이어야 한다).");

            if (!Regex.IsMatch(File.ReadAllText(scriptPath), $@"\bpartial\s+class\s+{Regex.Escape(type.Name)}\b"))
                return Fail(view, $"{type.Name}: View 클래스에 partial 을 붙여 주세요 — 생성된 속성은 같은 클래스의 다른 파일에 들어간다 ({scriptPath}).");

            var elements = ElementScan.Collect(view.transform);
            var errors = new List<string>();
            var names = ElementScan.NamesFor(elements, errors);
            if (errors.Count > 0)
                return Fail(view, $"{type.Name}: 속성 생성을 멈췄다 (파일은 그대로)\n- " + string.Join("\n- ", errors));

            string generatedPath = Path.Combine(Path.GetDirectoryName(scriptPath), ElementCodeWriter.FileName(type)).Replace('\\', '/');
            if (WriteIfChanged(generatedPath, ElementCodeWriter.Write(type, elements, names)))
            {
                // 프리팹 저장 도중에는 가져오기를 미룬다
                if (trigger == Trigger.PrefabSaving)
                    EditorApplication.delayCall += () => AssetDatabase.ImportAsset(generatedPath);
                else
                    AssetDatabase.ImportAsset(generatedPath);
            }

            if (TryFill(view, elements))
            {
                if (trigger == Trigger.Manual)
                    SaveOrMarkDirty(view);
                Debug.Log($"{LogPrefix}{type.Name}: Element {elements.Count}개 → {generatedPath}", view);
                return Result.Done;
            }

            AddPending(PendingId(view));
            Debug.Log($"{LogPrefix}{type.Name}: Element {elements.Count}개 → {generatedPath} (배열은 컴파일이 끝나면 채운다)", view);
            return Result.WaitingForCompile;
        }

        /// <summary>숨은 배열에 Element 를 넣는다. 배열 필드가 아직 없으면 (생성된 파일이 컴파일 전) false.</summary>
        public static bool TryFill(MonoBehaviour view, IReadOnlyList<Element> elements)
        {
            var so = new SerializedObject(view);
            var array = so.FindProperty(ElementCodeWriter.ArrayField);
            if (array == null || !array.isArray)
                return false;

            bool same = array.arraySize == elements.Count;
            for (int i = 0; same && i < elements.Count; i++)
                same = array.GetArrayElementAtIndex(i).objectReferenceValue == elements[i];
            if (same)
                return true;

            array.arraySize = elements.Count;
            for (int i = 0; i < elements.Count; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = elements[i];
            so.ApplyModifiedProperties();
            return true;
        }

        /// <summary>컴파일 뒤 (도메인을 다시 불러온 뒤) 기다리던 배열 채우기를 마친다.</summary>
        public static void CompletePending()
        {
            string stored = SessionState.GetString(PendingKey, "");
            if (stored.Length == 0)
                return;
            SessionState.EraseString(PendingKey);

            foreach (string id in stored.Split('\n').Where(s => s.Length > 0).Distinct())
            {
                if (id.StartsWith("asset:"))
                    FillPrefabAsset(id.Substring("asset:".Length));
                else if (id.StartsWith("gid:"))
                    FillSceneObject(id.Substring("gid:".Length));
            }
        }

        private static void FillPrefabAsset(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                return;

            // 프리팹 모드가 열려 있어도 디스크의 프리팹을 채운다 — 열린 쪽은 다음 저장 때 생성기가 다시 채운다
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                bool filled = false, waiting = false;
                foreach (var view in Views(root))
                {
                    if (TryFill(view, ElementScan.Collect(view.transform)))
                        filled = true;
                    else
                        waiting = true;
                }

                if (filled)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    Debug.Log($"{LogPrefix}{path}: Element 배열을 채웠다.");
                }
                if (waiting)
                    Debug.LogWarning($"{LogPrefix}{path}: 컴파일 뒤에도 배열 필드가 없는 View 가 있다 — 컴파일 오류가 없는지 보고 버튼으로 다시 생성해 주세요.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void FillSceneObject(string globalId)
        {
            if (!GlobalObjectId.TryParse(globalId, out var gid))
                return;
            if (GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid) is not MonoBehaviour view)
                return;

            if (TryFill(view, ElementScan.Collect(view.transform)))
                SaveOrMarkDirty(view);
        }

        private static IEnumerable<MonoBehaviour> Views(GameObject root) =>
            root.GetComponentsInChildren<MonoBehaviour>(true).Where(c => c != null && ElementScan.IsScreenView(c.GetType()));

        private static void SaveOrMarkDirty(MonoBehaviour view)
        {
            if (EditorUtility.IsPersistent(view))
            {
                PrefabUtility.SavePrefabAsset(view.transform.root.gameObject);
                return;
            }

            var stage = PrefabStageUtility.GetPrefabStage(view.gameObject);
            EditorSceneManager.MarkSceneDirty(stage != null ? stage.scene : view.gameObject.scene);
        }

        private static string PendingId(MonoBehaviour view)
        {
            if (EditorUtility.IsPersistent(view))
                return "asset:" + AssetDatabase.GetAssetPath(view);

            var stage = PrefabStageUtility.GetPrefabStage(view.gameObject);
            if (stage != null)
                return "asset:" + stage.assetPath;

            return "gid:" + GlobalObjectId.GetGlobalObjectIdSlow(view);
        }

        private static void AddPending(string id)
        {
            string stored = SessionState.GetString(PendingKey, "");
            SessionState.SetString(PendingKey, stored.Length == 0 ? id : stored + "\n" + id);
        }

        /// <summary>내용이 같으면 쓰지 않는다 (쓸데없는 재컴파일 방지).</summary>
        private static bool WriteIfChanged(string path, string content)
        {
            if (File.Exists(path) && File.ReadAllText(path) == content)
                return false;

            File.WriteAllText(path, content);
            return true;
        }

        private static Result Fail(Object context, string message)
        {
            Debug.LogError(LogPrefix + message, context);
            return Result.Failed;
        }
    }
}
