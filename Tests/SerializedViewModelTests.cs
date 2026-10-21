using System;
using MVVM.UGUI.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MVVM.UGUI.Tests
{
    public class TestChannelSO : ScriptableObject { }

    // VM 이 SO 를 직접 직렬화해 든다 (사용자 10-21 — View 가 생성자로 넘기던 것 대신)
    [Serializable]
    public class SerializedTestViewModel : AbstractViewModel, IInitializable
    {
        [SerializeField] private TestChannelSO channel;

        public TestChannelSO Channel => channel;
        public bool Initialized { get; private set; }

        public void Initialize() => Initialized = true;

        protected override void ResetForOpen() { }
    }

    [ExecuteAlways]
    public class SerializedTestView : ScreenView<SerializedTestViewModel>
    {
        [NonSerialized] public SerializedTestViewModel BoundWith;
        [NonSerialized] public bool InitializedWhenBound;

        protected override void Bind(SerializedTestViewModel vm)
        {
            BoundWith = vm;
            InitializedWhenBound = vm.Initialized;
        }
    }

    public class SerializedViewModelTests
    {
        [Test]
        public void ViewModelFields_SurviveInstantiateAndAwake_InitializeBeforeBind()
        {
            var channel = ScriptableObject.CreateInstance<TestChannelSO>();
            var template = new GameObject("Template");
            template.SetActive(false);   // 템플릿에서는 Awake 가 돌지 않게
            var view = template.AddComponent<SerializedTestView>();

            // 제네릭 바탕의 T 타입 필드가 하위 화면에서 직렬화되는가 — 안 되면 FindProperty 가 null
            var serialized = new SerializedObject(view);
            var channelField = serialized.FindProperty("viewModel.channel");
            Assert.IsNotNull(channelField, "ScreenView<T>.viewModel 이 직렬화되지 않는다 (VM 에 [Serializable] 이 있는지)");
            channelField.objectReferenceValue = channel;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // 프리팹을 여는 것처럼 직렬화된 값으로 복제한 뒤 켜서 Awake 를 돌린다
            var clone = Object.Instantiate(template);
            var cloneView = clone.GetComponent<SerializedTestView>();
            clone.SetActive(true);

            Assert.IsNotNull(cloneView.BoundWith, "Awake 에서 Bind 가 VM 을 받아야 한다");
            Assert.AreSame(channel, cloneView.BoundWith.Channel, "Awake 가 직렬화된 VM 을 새 VM 으로 덮으면 안 된다");
            Assert.IsTrue(cloneView.InitializedWhenBound, "IInitializable 은 Bind 전에 불려야 한다");

            Object.DestroyImmediate(clone);
            Object.DestroyImmediate(template);
            Object.DestroyImmediate(channel);
        }
    }
}
