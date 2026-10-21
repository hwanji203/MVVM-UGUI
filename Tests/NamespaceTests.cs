using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MVVM.UGUI.Binding;
using NUnit.Framework;
using UnityEngine;

namespace MVVM.UGUI.Tests
{
    /// <summary>
    /// 패키지 규칙: namespace = 가장 가까운 asmdef 의 rootNamespace + 그 아래 폴더 경로 (Rider 가 기대하는 것과 같다).
    /// 파일을 다른 폴더로 옮기고 namespace 를 안 고치면 여기서 걸린다.
    /// </summary>
    public class NamespaceTests
    {
        [Serializable]
        private class AsmdefFields
        {
            public string rootNamespace;
        }

        [Test]
        public void EveryScript_NamespaceMatchesFolder()
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(Bindable<int>).Assembly);
            Assert.IsNotNull(package, "MVVM.UGUI 어셈블리가 패키지 안에 있지 않다");

            string root = Path.GetFullPath(package.resolvedPath);
            var wrong = new List<string>();
            int checkedCount = 0;
            foreach (string file in Scripts(root))
            {
                string expected = ExpectedNamespace(Path.GetDirectoryName(file), root);
                if (expected == null)
                    continue;

                checkedCount++;
                var match = Regex.Match(File.ReadAllText(file), @"^\s*namespace\s+([\w.]+)", RegexOptions.Multiline);
                string actual = match.Success ? match.Groups[1].Value : "(namespace 없음)";
                if (actual != expected)
                    wrong.Add($"{file.Substring(root.Length + 1)}: {actual} → {expected}");
            }

            Assert.Greater(checkedCount, 50, "패키지 스크립트를 거의 못 찾았다 — 경로 계산이 틀렸다");
            Assert.IsEmpty(wrong, "폴더 경로와 다른 namespace:\n" + string.Join("\n", wrong));
        }

        // .git 같은 숨은 폴더와 Unity 가 무시하는 '~' 폴더 (Documentation~) 는 뺀다
        private static IEnumerable<string> Scripts(string dir)
        {
            foreach (string file in Directory.GetFiles(dir, "*.cs"))
                yield return file;

            foreach (string sub in Directory.GetDirectories(dir))
            {
                string name = Path.GetFileName(sub);
                if (name.StartsWith(".") || name.EndsWith("~"))
                    continue;
                foreach (string file in Scripts(sub))
                    yield return file;
            }
        }

        // 위로 올라가며 asmdef 를 찾고, 그 rootNamespace 에 지나온 폴더 이름을 붙인다
        private static string ExpectedNamespace(string dir, string packageRoot)
        {
            var folders = new List<string>();
            while (dir != null && dir.Length >= packageRoot.Length)
            {
                string[] asmdefs = Directory.GetFiles(dir, "*.asmdef");
                if (asmdefs.Length > 0)
                {
                    var fields = JsonUtility.FromJson<AsmdefFields>(File.ReadAllText(asmdefs[0]));
                    folders.Reverse();
                    folders.Insert(0, fields.rootNamespace);
                    return string.Join(".", folders);
                }

                folders.Add(Path.GetFileName(dir));
                dir = Path.GetDirectoryName(dir);
            }

            return null;
        }
    }
}
