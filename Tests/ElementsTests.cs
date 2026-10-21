using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using MVVM.UGUI.Binding;
using MVVM.UGUI.Core;
using MVVM.UGUI.Elements.Commands;
using MVVM.UGUI.Elements.Display;
using MVVM.UGUI.Elements.Display.Gauges;
using MVVM.UGUI.Elements.Display.Texts;
using MVVM.UGUI.Elements.Feedback;
using MVVM.UGUI.Elements.Inputs;
using MVVM.UGUI.Elements.Lists;
using MVVM.UGUI.Elements.Popups;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MVVM.UGUI.Tests
{
    // 에디트 모드에서도 Awake · OnEnable 이 돌도록 (리스너 연결 · 켜짐 판정)
    [ExecuteAlways] public class TestSliderElement : SliderElement { }
    [ExecuteAlways] public class TestToggleGroupElement : ToggleGroupElement { }
    [ExecuteAlways] public class TestButtonElement : ButtonElement { }
    [ExecuteAlways] public class TestLockButtonElement : LockButtonElement { }
    [ExecuteAlways] public class TestDropdownElement : DropdownElement { }
    [ExecuteAlways] public class TestOptionStepperElement : OptionStepperElement { }
    [ExecuteAlways] public class TestToastElement : ToastElement { }

    [ExecuteAlways]
    public class TestItemView : ItemView<int>
    {
        public readonly List<int> Bound = new();
        protected override void Bind(int item) => Bound.Add(item);
    }

    /// <summary>
    /// 패키지 Element 들의 에디트 모드 동작. 플레이 중이 아니면 트윈 Element 는 늘 바로 맞추므로 (TweenValueElement 규칙)
    /// 여기서는 값 반영 · 양방향 쓰기 · 연결 끊기를 본다. 트윈 자체는 플레이 모드에서 확인한다.
    /// </summary>
    public class ElementsTests
    {
        private readonly List<GameObject> _created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created)
                if (go != null) Object.DestroyImmediate(go);
            _created.Clear();
        }

        private GameObject Go(string name = "Test", Transform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null)
                go.transform.SetParent(parent, false);
            else
                _created.Add(go);
            return go;
        }

        private static void SetField(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetStrings(Object target, string field, params string[] values)
        {
            var so = new SerializedObject(target);
            var array = so.FindProperty(field);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                array.GetArrayElementAtIndex(i).stringValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- 보여주기 ----------

        [Test]
        public void TextView_OriginalTextWithPlaceholder_IsUsedAsFormat()
        {
            var go = Go();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = "HP {0}";
            var element = go.AddComponent<TextViewElement>();
            var hp = new Bindable<string> { Value = "30" };

            element.Bind(hp);
            hp.Value = "12";

            Assert.AreEqual("HP 12", tmp.text);
        }

        [Test]
        public void TextView_WithoutPlaceholder_ShowsValue()
        {
            var go = Go();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = "이전 글";
            var element = go.AddComponent<TextViewElement>();

            element.Bind(new Bindable<string> { Value = "새 글" });

            Assert.AreEqual("새 글", tmp.text);
        }

        [Test]
        public void NumberText_IntOverload_FormatsAndRebindDropsOld()
        {
            var go = Go();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            var element = go.AddComponent<NumberTextElement>();
            var so = new SerializedObject(element);
            so.FindProperty("format").stringValue = "{0:0} G";
            so.ApplyModifiedPropertiesWithoutUndo();
            var first = new Bindable<int> { Value = 1200 };

            element.Bind(first);
            Assert.AreEqual("1200 G", tmp.text);

            element.Bind(new Bindable<int> { Value = 5 });
            first.Value = 999;

            Assert.AreEqual("5 G", tmp.text, "다시 묶으면 이전 int 원본 (Select 포함) 은 끊긴다");
        }

        [Test]
        public void Timer_MinutesSeconds_RoundsUp()
        {
            var go = Go();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            var element = go.AddComponent<TimerTextElement>();

            element.Bind(new Bindable<float> { Value = 65.2f });

            Assert.AreEqual("01:06", tmp.text);
        }

        [Test]
        public void Gauge_EditMode_AppliesImmediatelyAndClamps()
        {
            var go = Go();
            var image = go.AddComponent<Image>();
            var element = go.AddComponent<GaugeElement>();
            var ratio = new Bindable<float> { Value = 0.3f };

            element.Bind(ratio);
            Assert.AreEqual(0.3f, image.fillAmount, 1e-5f);

            ratio.Value = 1.7f;
            Assert.AreEqual(1f, image.fillAmount, 1e-5f, "플레이 중이 아니면 트윈 없이 바로, 0~1 로 자른다");
        }

        [Test]
        public void DelayedGauge_EditMode_BothBarsFollowImmediately()
        {
            var go = Go();
            var front = go.AddComponent<Image>();
            var trail = Go("Trail", go.transform).AddComponent<Image>();
            var element = go.AddComponent<DelayedGaugeElement>();
            SetField(element, "trail", trail);
            var hp = new Bindable<float> { Value = 1f };

            element.Bind(hp);
            hp.Value = 0.4f;

            Assert.AreEqual(0.4f, front.fillAmount, 1e-5f);
            Assert.AreEqual(0.4f, trail.fillAmount, 1e-5f);
        }

        [Test]
        public void Visible_ActiveMode_TogglesTarget()
        {
            var go = Go();
            var target = Go("Target", go.transform);
            var element = go.AddComponent<VisibleElement>();
            var so = new SerializedObject(element);
            so.FindProperty("mode").enumValueIndex = (int)VisibleElement.Mode.Active;
            so.FindProperty("target").objectReferenceValue = target;
            so.ApplyModifiedPropertiesWithoutUndo();
            var visible = new Bindable<bool> { Value = false };

            element.Bind(visible);
            Assert.IsFalse(target.activeSelf);

            visible.Value = true;
            Assert.IsTrue(target.activeSelf);
        }

        [Test]
        public void Visible_FadeMode_SetsAlphaAndBlocksInputWhenHidden()
        {
            var go = Go();
            var group = go.AddComponent<CanvasGroup>();
            var element = go.AddComponent<VisibleElement>();

            element.Bind(new Bindable<bool> { Value = false });

            Assert.AreEqual(0f, group.alpha);
            Assert.IsFalse(group.blocksRaycasts);
        }

        [Test]
        public void Switch_OnlyIndexIsActive_OutOfRangeTurnsAllOff()
        {
            var go = Go();
            var a = Go("A", go.transform);
            var b = Go("B", go.transform);
            var element = go.AddComponent<SwitchElement>();
            var index = new Bindable<int> { Value = 1 };

            element.Bind(index);
            Assert.IsFalse(a.activeSelf);
            Assert.IsTrue(b.activeSelf);

            index.Value = 5;
            Assert.IsFalse(a.activeSelf || b.activeSelf);
        }

        [Test]
        public void CountIcons_TintsFirstK()
        {
            var go = Go();
            var icons = new Image[3];
            for (int i = 0; i < icons.Length; i++)
                icons[i] = Go("Icon" + i, go.transform).AddComponent<Image>();
            var element = go.AddComponent<CountIconsElement>();

            element.Bind(new Bindable<int> { Value = 2 });

            Assert.AreEqual(Color.white, icons[0].color);
            Assert.AreEqual(Color.white, icons[1].color);
            Assert.AreNotEqual(Color.white, icons[2].color);
        }

        [Test]
        public void Badge_HidesAtZero_CapsAtMax()
        {
            var go = Go();
            var badge = Go("Badge", go.transform);
            var tmp = Go("Count", badge.transform).AddComponent<TextMeshProUGUI>();
            var element = go.AddComponent<BadgeElement>();
            SetField(element, "badge", badge);
            var count = new Bindable<int> { Value = 0 };

            element.Bind(count);
            Assert.IsFalse(badge.activeSelf);

            count.Value = 150;
            Assert.IsTrue(badge.activeSelf);
            Assert.AreEqual("99+", tmp.text);
        }

        // ---------- 입력 ----------

        [Test]
        public void Slider_UserChange_WritesToBindable_ModelChange_DoesNotEcho()
        {
            var go = Go();
            go.SetActive(false);
            var element = go.AddComponent<TestSliderElement>();
            var slider = go.GetComponent<Slider>();
            go.SetActive(true);
            var volume = new Bindable<float> { Value = 0.2f };
            var notified = 0;
            volume.Bind(_ => notified++);
            notified = 0;

            element.Bind(volume);
            Assert.AreEqual(0.2f, slider.value, 1e-5f);

            slider.value = 0.7f;
            Assert.AreEqual(0.7f, volume.Value, 1e-5f);

            volume.Value = 0.1f;
            Assert.AreEqual(0.1f, slider.value, 1e-5f);
            Assert.AreEqual(2, notified, "조작 1번 + ViewModel 1번 — 되먹임이 없다");
        }

        [Test]
        public void FormElement_ReadOnlySource_Asserts()
        {
            var go = Go();
            go.SetActive(false);
            var element = go.AddComponent<TestSliderElement>();
            go.SetActive(true);
            var derived = Bindable.Select(new Bindable<int> { Value = 1 }, v => (float)v);

            LogAssert.Expect(LogType.Assert, new Regex("Bindable<T>"));
            element.Bind(derived);
            derived.Dispose();
        }

        [Test]
        public void ToggleGroup_PicksOne_ReclickingSelectedKeepsIt()
        {
            var go = Go();
            var toggles = new Toggle[3];
            for (int i = 0; i < toggles.Length; i++)
                toggles[i] = Go("T" + i, go.transform).AddComponent<Toggle>();
            go.SetActive(false);
            var element = go.AddComponent<TestToggleGroupElement>();
            go.SetActive(true);
            var choice = new Bindable<int> { Value = 0 };

            element.Bind(choice);
            toggles[2].isOn = true;

            Assert.AreEqual(2, choice.Value);
            Assert.IsFalse(toggles[0].isOn);

            toggles[2].isOn = false;
            Assert.IsTrue(toggles[2].isOn, "고른 것을 다시 눌러도 꺼지지 않는다");
            Assert.AreEqual(2, choice.Value);
        }

        [Test]
        public void Dropdown_OptionsAndSelected_BoundTogether()
        {
            var go = Go();
            go.SetActive(false);
            var element = go.AddComponent<TestDropdownElement>();
            var dropdown = go.GetComponent<TMP_Dropdown>();
            go.SetActive(true);
            var options = new BindableList<string>();
            var selected = new Bindable<int> { Value = 2 };

            element.Bind(options, selected);
            options.Add("A");
            options.Add("B");
            options.Add("C");

            Assert.AreEqual(3, dropdown.options.Count);
            Assert.AreEqual(2, dropdown.value, "항목이 오기 전에 잘린 번호를 다시 맞춘다");

            dropdown.value = 1;
            Assert.AreEqual(1, selected.Value);
        }

        [Test]
        public void OptionStepper_Wraps_AndShowsLabel()
        {
            var go = Go();
            var prev = Go("Prev", go.transform).AddComponent<Button>();
            var next = Go("Next", go.transform).AddComponent<Button>();
            var label = Go("Label", go.transform).AddComponent<TextMeshProUGUI>();
            go.SetActive(false);   // 필드를 채운 뒤 켜서 Awake 가 버튼에 리스너를 잇게 한다
            var element = go.AddComponent<TestOptionStepperElement>();
            SetField(element, "previousButton", prev);
            SetField(element, "nextButton", next);
            SetField(element, "label", label);
            SetStrings(element, "options", "쉬움", "보통", "어려움");
            go.SetActive(true);
            var difficulty = new Bindable<int> { Value = 2 };

            element.Bind(difficulty);
            Assert.AreEqual("어려움", label.text);

            next.onClick.Invoke();
            Assert.AreEqual(0, difficulty.Value, "끝에서 넘기면 처음으로");
            Assert.AreEqual("쉬움", label.text);
        }

        // ---------- 명령 ----------

        [Test]
        public void Button_ClickExecutes_InteractableFollowsActive()
        {
            var go = Go();
            go.SetActive(false);
            var element = go.AddComponent<TestButtonElement>();
            var button = go.GetComponent<Button>();
            go.SetActive(true);
            var runs = 0;
            var command = new Command(() => runs++);

            element.Bind(command);
            button.onClick.Invoke();
            command.SetEnabled(false);

            Assert.AreEqual(1, runs);
            Assert.IsFalse(button.interactable);
        }

        [Test]
        public void LockButton_Locked_ShowsMarkAndText_UnlockRestores()
        {
            var go = Go();
            var mark = Go("Lock", go.transform);
            var label = Go("Label", go.transform).AddComponent<TextMeshProUGUI>();
            label.text = "구매";
            go.SetActive(false);
            var element = go.AddComponent<TestLockButtonElement>();
            SetField(element, "lockedMark", mark);
            SetField(element, "label", label);
            go.SetActive(true);
            var command = new Command(() => { }, active: false);

            element.Bind(command);
            Assert.IsTrue(mark.activeSelf);
            Assert.AreEqual("잠김", label.text);

            command.SetEnabled(true);
            Assert.IsFalse(mark.activeSelf);
            Assert.AreEqual("구매", label.text);
        }

        // ---------- 목록 ----------

        private (ItemListElement list, GameObject container) MakeList()
        {
            var prefab = Go("ItemPrefab");
            prefab.AddComponent<TestItemView>();
            var go = Go("List");
            var list = go.AddComponent<ItemListElement>();
            SetField(list, "itemPrefab", prefab.GetComponent<TestItemView>());
            return (list, go);
        }

        private static List<TestItemView> ActiveViews(GameObject container)
        {
            var views = new List<TestItemView>();
            foreach (Transform child in container.transform)
            {
                var view = child.GetComponent<TestItemView>();
                if (view != null && child.gameObject.activeSelf)
                    views.Add(view);
            }
            return views;
        }

        [Test]
        public void ItemList_AddRemove_ReusesHiddenViews()
        {
            var (list, container) = MakeList();
            var items = new BindableList<int>();
            list.Bind(items);

            items.Add(10);
            items.Add(20);
            items.Add(30);
            Assert.AreEqual(3, ActiveViews(container).Count);

            items.RemoveAt(0);
            var views = ActiveViews(container);
            Assert.AreEqual(2, views.Count);
            Assert.AreEqual(20, views[0].Bound[^1]);
            Assert.AreEqual(3, container.transform.childCount, "뺀 항목 View 는 지우지 않고 꺼 둔다");

            items.Add(40);
            Assert.AreEqual(3, container.transform.childCount, "꺼 둔 View 를 다시 쓴다");
        }

        [Test]
        public void ItemList_ElementReplace_RebindsThatView()
        {
            var (list, container) = MakeList();
            var items = new BindableList<int>();
            items.Add(1);
            items.Add(2);
            list.Bind(items);

            items[1] = 99;

            Assert.AreEqual(99, ActiveViews(container)[1].Bound[^1]);
        }

        [Test]
        public void ItemList_WrongItemType_LogsError()
        {
            var (list, _) = MakeList();

            LogAssert.Expect(LogType.Error, new Regex("ItemView<String>"));
            list.Bind(new BindableList<string>());
        }

        // ---------- 팝업 · 알림 ----------

        [Test]
        public void Window_EditMode_OpenAndCloseFinishImmediately()
        {
            var go = Go();
            var window = go.AddComponent<WindowElement>();

            var open = window.PlayOpen(CancellationToken.None);
            Assert.AreEqual(UniTaskStatus.Succeeded, open.Status);
            Assert.AreEqual(Vector3.one, go.transform.localScale);

            var close = window.PlayClose(CancellationToken.None);
            Assert.AreEqual(UniTaskStatus.Succeeded, close.Status);
            Assert.AreEqual(Vector3.zero, go.transform.localScale);
        }

        [Test]
        public void BackgroundDim_ClickExecutesCommand_CloseBlocksInput()
        {
            var go = Go();
            go.AddComponent<Image>();
            var dim = go.AddComponent<BackgroundDimElement>();
            var group = go.GetComponent<CanvasGroup>();
            var closes = 0;
            dim.Bind(new Command(() => closes++));

            dim.PlayOpen(CancellationToken.None);
            dim.OnPointerClick(null);
            dim.PlayClose(CancellationToken.None);

            Assert.AreEqual(1, closes);
            Assert.AreEqual(0f, group.alpha);
            Assert.IsFalse(group.blocksRaycasts);
        }

        [Test]
        public void Toast_ActiveShowsSameMessageTwice_InactiveIgnores()
        {
            var go = Go();
            var tmp = Go("Text", go.transform).AddComponent<TextMeshProUGUI>();
            go.SetActive(false);
            var toast = go.AddComponent<TestToastElement>();
            go.SetActive(true);
            var signal = new Signal<string>();
            toast.Bind(signal);

            signal.Fire("골드 부족");
            Assert.AreEqual("골드 부족", tmp.text);

            tmp.text = "";
            signal.Fire("골드 부족");
            Assert.AreEqual("골드 부족", tmp.text, "같은 글도 다시 뜬다");

            go.SetActive(false);
            signal.Fire("숨은 동안");
            Assert.AreNotEqual("숨은 동안", tmp.text, "꺼져 있을 때 온 알림은 버린다");
        }
    }
}
