using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using MVVM.UGUI.Binding;
using MVVM.UGUI.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MVVM.UGUI.Tests
{
    public class OtherTestViewModel : AbstractViewModel
    {
        protected override void ResetForOpen() { }
    }

    [ExecuteAlways]
    public class OtherTestView : ScreenView<OtherTestViewModel>
    {
        protected override void Bind(OtherTestViewModel vm) { }
    }

    // 열 때마다 받는 값 (Title · Message · Confirm) 과 계속 사는 값 (Gold) 을 함께 든 VM
    public class ReopenTestViewModel : AbstractViewModel
    {
        public readonly Bindable<string> Title = new();
        public readonly Bindable<string> Message = new();
        public readonly Command Confirm = new();
        public readonly Bindable<int> Gold = new();   // Model 이 채우는 값 자리 — ResetForOpen 에서 안 건드린다
        public int ResetCount;

        public void Setup(string title, string message = null, Action onConfirm = null)
        {
            Title.Value = title;
            Message.Value = message;
            Confirm.Action = onConfirm;
        }

        protected override void ResetForOpen()
        {
            ResetCount++;
            ResetAll(Title, Message, Confirm);
        }
    }

    [ExecuteAlways]
    public class ReopenTestView : ScreenView<ReopenTestViewModel>
    {
        public ReopenTestViewModel BoundWith;

        protected override void Bind(ReopenTestViewModel vm) => BoundWith = vm;
    }

    public class ScreenManagerTests
    {
        private readonly List<GameObject> _spawned = new();
        private Canvas _canvas;
        private ScreenManager _manager;

        [SetUp]
        public void SetUp()
        {
            _canvas = Spawn("RootCanvas").AddComponent<Canvas>();
            _manager = Spawn("ScreenManager").AddComponent<ScreenManager>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _spawned)
                if (go != null) Object.DestroyImmediate(go);
            _spawned.Clear();
        }

        private GameObject Spawn(string name)
        {
            var go = new GameObject(name);
            _spawned.Add(go);
            return go;
        }

        // 프리팹 자리. Awake 가 끈 것을 다시 켜 둔다 — 꺼진 채 복제하면 복제본의 Awake 가 안 돈다
        private T Template<T>() where T : ScreenView
        {
            var view = Spawn(typeof(T).Name).AddComponent<T>();
            view.gameObject.SetActive(true);
            return view;
        }

        private void Use(params ScreenView[] prefabs)
        {
            var so = new SerializedObject(_manager);
            var list = so.FindProperty("screenViewPrefabs");
            list.arraySize = prefabs.Length;
            for (int i = 0; i < prefabs.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
            so.FindProperty("rootCanvas").objectReferenceValue = _canvas;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private T[] Instances<T>() where T : ScreenView => _canvas.GetComponentsInChildren<T>(true);

        private static bool IsDone(UniTask task) => task.Status == UniTaskStatus.Succeeded;

        [Test]
        public void Open_FirstTime_CreatesUnderCanvasAndShows()
        {
            Use(Template<TestView>());

            _manager.Open<TestViewModel>();

            var views = Instances<TestView>();
            Assert.AreEqual(1, views.Length);
            Assert.IsTrue(views[0].gameObject.activeSelf);
        }

        [Test]
        public void Open_SetupGetsThatInstanceVm_BeforeShowing()
        {
            Use(Template<TestView>());
            TestViewModel got = null;
            bool shownAtSetup = true;

            _manager.Open<TestViewModel>(vm =>
            {
                got = vm;
                shownAtSetup = Instances<TestView>()[0].gameObject.activeSelf;
            });

            Assert.AreSame(Instances<TestView>()[0].BoundWith, got, "그 인스턴스의 VM 으로 setup 해야 한다");
            Assert.IsFalse(shownAtSetup, "setup 은 켜기 전에 불려야 한다");
        }

        [Test]
        public void Open_WhileOpen_CreatesAnother()
        {
            Use(Template<TestView>());

            _manager.Open<TestViewModel>();
            _manager.Open<TestViewModel>();

            Assert.AreEqual(2, Instances<TestView>().Length);
        }

        // 재사용 (사용자 10-19): 닫힌 화면은 꺼 둔 채 다시 쓴다
        [Test]
        public void Open_AfterClose_ReusesSameInstance()
        {
            Use(Template<TestView>());
            _manager.Open<TestViewModel>();
            var first = Instances<TestView>()[0];

            _manager.Close<TestViewModel>();
            _manager.Open<TestViewModel>();

            var views = Instances<TestView>();
            Assert.AreEqual(1, views.Length);
            Assert.AreSame(first, views[0]);
            Assert.IsTrue(first.gameObject.activeSelf);
        }

        [Test]
        public void Open_Reused_DrawsOnTop()
        {
            Use(Template<TestView>(), Template<OtherTestView>());
            _manager.Open<TestViewModel>();
            var reused = Instances<TestView>()[0];
            _manager.Close<TestViewModel>();
            _manager.Open<OtherTestViewModel>();

            _manager.Open<TestViewModel>();

            Assert.AreEqual(_canvas.transform.childCount - 1, reused.transform.GetSiblingIndex());
        }

        // 열 때마다 setup 직전에 VM 의 ResetForOpen 이 지난번 값을 되돌린다 (사용자 10-21)
        private ReopenTestViewModel ReopenVm() => Instances<ReopenTestView>()[0].BoundWith;

        [Test]
        public void Open_FirstTime_ResetsBeforeSetup_SetupValuesSurvive()
        {
            Use(Template<ReopenTestView>());
            int resetsAtSetup = -1;

            _manager.Open<ReopenTestViewModel>(vm =>
            {
                resetsAtSetup = vm.ResetCount;
                vm.Setup("title", "message");
            });

            Assert.AreEqual(1, resetsAtSetup, "setup 보다 먼저 되돌려야 한다");
            Assert.AreEqual("title", ReopenVm().Title.Value, "되돌리기가 setup 값을 지우면 안 된다");
            Assert.AreEqual("message", ReopenVm().Message.Value);
        }

        [Test]
        public void Reopen_WithoutCallback_DoesNotRunLastCallback()
        {
            Use(Template<ReopenTestView>());
            int firstRuns = 0;
            _manager.Open<ReopenTestViewModel>(vm => vm.Setup("first", onConfirm: () => firstRuns++));
            _manager.Close<ReopenTestViewModel>();

            _manager.Open<ReopenTestViewModel>(vm => vm.Setup("second"));
            ReopenVm().Confirm.Execute();

            Assert.AreEqual(0, firstRuns);
        }

        [Test]
        public void Reopen_SetupSkipsValue_ShowsInitialNotLast()
        {
            Use(Template<ReopenTestView>());
            _manager.Open<ReopenTestViewModel>(vm => vm.Setup("first", "first message"));
            _manager.Close<ReopenTestViewModel>();

            _manager.Open<ReopenTestViewModel>(vm => vm.Title.Value = "second");

            Assert.AreEqual("second", ReopenVm().Title.Value);
            Assert.IsNull(ReopenVm().Message.Value);
        }

        [Test]
        public void Reopen_KeepsValuesNotReset()
        {
            Use(Template<ReopenTestView>());
            _manager.Open<ReopenTestViewModel>();
            ReopenVm().Gold.Value = 50;
            _manager.Close<ReopenTestViewModel>();

            _manager.Open<ReopenTestViewModel>();

            Assert.AreEqual(50, ReopenVm().Gold.Value);
        }

        [Test]
        public void Reopen_WithoutSetup_StillResets()
        {
            Use(Template<ReopenTestView>());
            _manager.Open<ReopenTestViewModel>(vm => vm.Setup("first", "message"));
            _manager.Close<ReopenTestViewModel>();

            _manager.Open<ReopenTestViewModel>();

            Assert.AreEqual(2, ReopenVm().ResetCount);
            Assert.IsNull(ReopenVm().Title.Value);
            Assert.IsNull(ReopenVm().Message.Value);
        }

        [Test]
        public void Close_ClosesTopmostOfThatType()
        {
            Use(Template<TestView>());
            _manager.Open<TestViewModel>();
            _manager.Open<TestViewModel>();
            var bottom = Instances<TestView>()[0];
            var top = Instances<TestView>()[1];

            _manager.Close<TestViewModel>();

            Assert.IsFalse(top.gameObject.activeSelf);
            Assert.IsTrue(bottom.gameObject.activeSelf);
        }

        // 열린 순서는 목록이 아니라 형제 순서로 본다 — 재사용 뒤 목록 순서와 화면 순서가 달라진 경우
        [Test]
        public void Close_AfterReuseInOtherOrder_FollowsSiblingOrder()
        {
            Use(Template<TestView>());
            _manager.Open<TestViewModel>();
            _manager.Open<TestViewModel>();
            _manager.Close<TestViewModel>();
            _manager.Close<TestViewModel>();

            _manager.Open<TestViewModel>();
            _manager.Open<TestViewModel>();
            var views = Instances<TestView>();   // 형제 순서대로
            var bottom = views[0];
            var top = views[1];

            _manager.Close<TestViewModel>();

            Assert.IsFalse(top.gameObject.activeSelf);
            Assert.IsTrue(bottom.gameObject.activeSelf);
        }

        [Test]
        public void Close_LeavesOtherTypesOpen()
        {
            Use(Template<TestView>(), Template<OtherTestView>());
            _manager.Open<TestViewModel>();
            _manager.Open<OtherTestViewModel>();

            _manager.Close<TestViewModel>();

            Assert.IsFalse(Instances<TestView>()[0].gameObject.activeSelf);
            Assert.IsTrue(Instances<OtherTestView>()[0].gameObject.activeSelf);
        }

        [Test]
        public void Close_WhenNothingOpen_DoesNothing()
        {
            Use(Template<TestView>());

            var neverOpened = _manager.CloseAsync<TestViewModel>();
            _manager.Open<TestViewModel>();
            _manager.Close<TestViewModel>();
            var allClosed = _manager.CloseAsync<TestViewModel>();

            Assert.IsTrue(IsDone(neverOpened));
            Assert.IsTrue(IsDone(allClosed));
        }

        // 화면이 자기 버튼으로 스스로 닫는 중이어도 매니저는 그 화면을 건너뛴다 (IsClosing)
        [Test]
        public void Close_WhileViewClosesItself_ClosesNextOne()
        {
            Use(Template<AnimatedTestView>());
            _manager.Open<TestViewModel>();
            _manager.Open<TestViewModel>();
            var bottom = Instances<AnimatedTestView>()[0];
            var top = Instances<AnimatedTestView>()[1];
            top.CloseGate = new UniTaskCompletionSource();

            top.CloseAsync().Forget();
            _manager.Close<TestViewModel>();

            Assert.IsFalse(bottom.gameObject.activeSelf);
            Assert.IsTrue(top.IsClosing);
        }

        // 닫기 연출 중인 화면은 다시 고르지 않는다 (사용자 10-20, IsClosing)
        [Test]
        public void Close_DuringCloseAnimation_ClosesNextOne()
        {
            Use(Template<AnimatedTestView>());
            _manager.Open<TestViewModel>();
            _manager.Open<TestViewModel>();
            var bottom = Instances<AnimatedTestView>()[0];
            var top = Instances<AnimatedTestView>()[1];
            top.CloseGate = new UniTaskCompletionSource();

            _manager.Close<TestViewModel>();
            _manager.Close<TestViewModel>();

            Assert.IsTrue(top.gameObject.activeSelf, "연출이 안 끝났으니 아직 켜져 있다");
            Assert.IsFalse(bottom.gameObject.activeSelf, "두 번째 닫기는 아래 화면을 닫아야 한다");

            top.CloseGate.TrySetResult();
            Assert.IsFalse(top.gameObject.activeSelf);
        }

        // Awake 는 DontDestroyOnLoad 때문에 EditMode 에서 부를 수 없어 경고 메서드만 부른다
        private void WarnDuplicatePrefabs() =>
            typeof(ScreenManager).GetMethod("WarnDuplicatePrefabs", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_manager, null);

        [Test]
        public void SamePrefabVmTwice_Warns()
        {
            Use(Template<TestView>(), Template<OtherTestView>(), Template<AnimatedTestView>());

            LogAssert.Expect(LogType.Warning, new Regex(nameof(TestViewModel)));
            WarnDuplicatePrefabs();
        }

        [Test]
        public void DifferentPrefabVms_DoesNotWarn()
        {
            Use(Template<TestView>(), Template<OtherTestView>());
            int warnings = 0;
            void Count(string _, string __, LogType type) { if (type == LogType.Warning) warnings++; }

            Application.logMessageReceived += Count;
            WarnDuplicatePrefabs();
            Application.logMessageReceived -= Count;

            Assert.AreEqual(0, warnings);
        }

        [Test]
        public void Open_WithoutPrefab_LogsError()
        {
            Use(Template<TestView>());

            LogAssert.Expect(LogType.Error, new Regex(nameof(OtherTestViewModel)));
            _manager.Open<OtherTestViewModel>();

            Assert.AreEqual(0, Instances<OtherTestView>().Length);
        }
    }
}
