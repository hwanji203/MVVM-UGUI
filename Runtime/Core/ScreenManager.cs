using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MVVM.UGUI.Core
{
    public class ScreenManager : MonoBehaviour
    {
        private class ViewSlot
        {
            public List<ScreenView> ViewPool { get; set; }
            public ScreenView ViewPrefab { get; set; }
        }
        
        [SerializeField] private List<ScreenView> screenViewPrefabs;
        [SerializeField] private Canvas rootCanvas;

        private readonly Dictionary<Type, ViewSlot> _viewPool = new();

        private void Awake()
        {
            rootCanvas = GetComponent<Canvas>();
            
            WarnDuplicatePrefabs();
            DontDestroyOnLoad(gameObject);
        }

        //VM 타입 하나에 프리팹 하나 — 둘 이상이면 목록에서 먼저 나온 것만 쓰이므로 알린다
        private void WarnDuplicatePrefabs()
        {
            var firstByVM = new Dictionary<Type, ScreenView>();
            foreach (var prefab in screenViewPrefabs)
            {
                if (prefab == null)
                    continue;

                Type vmType = FindVMType(prefab.GetType());
                if (vmType == null)
                    continue;

                if (firstByVM.TryGetValue(vmType, out var first))
                    Debug.LogWarning($"{vmType} 타입을 쓰는 프리팹이 둘 이상입니다: {first.name}, {prefab.name} — {first.name} 만 쓰입니다.", this);
                else
                    firstByVM.Add(vmType, prefab);
            }
        }

        //ScreenView<TVM> 까지 올라가 TVM 을 꺼낸다. 시작할 때 한 번만 쓰므로 리플렉션
        private static Type FindVMType(Type viewType)
        {
            for (Type t = viewType; t != null; t = t.BaseType)
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ScreenView<>))
                    return t.GetGenericArguments()[0];
            }

            return null;
        }

        public void Open<TVM>(Action<TVM> setup = null) where TVM : AbstractViewModel
            => OpenAsync(setup).Forget();
                    
        public async UniTask OpenAsync<TVM>(Action<TVM> setup = null) where TVM : AbstractViewModel
        {
            Type vmType = typeof(TVM);
            //Pool에서 생성했던 해당 타입의 UI를 찾기, 만약 없다면 풀을 생성하기, 근데 해당 타입의 프리팹이 없으면 Error
            if (_viewPool.TryGetValue(vmType, out var viewSlot) || TryCreateViewPool<TVM>(vmType, out viewSlot))
                await OpenScreenAsync(vmType, viewSlot, setup);
            else
                Debug.LogError($"{vmType}타입을 가진 UI 프리팹이 존재하지 않습니다.");
        }

        public void Close<TVM>() where TVM : AbstractViewModel
            => CloseAsync<TVM>().Forget();

        public async UniTask CloseAsync<TVM>() where TVM : AbstractViewModel
        {
            ScreenView topView = FindTopOpenView(typeof(TVM));
            
            if (topView != null)
                await topView.CloseAsync();
        }

        //그 타입에서 켜져 있는 것 중 형제 순서가 가장 뒤 (= 화면 맨 위) 인 것. 없으면 null
        private ScreenView FindTopOpenView(Type vmType)
        {
            if (!_viewPool.TryGetValue(vmType, out var viewSlot))
                return null;

            ScreenView topView = null;
            foreach (var view in viewSlot.ViewPool)
            {
                if (!view.gameObject.activeSelf)
                    continue;
                if (view.IsClosing)
                    continue;
                if (topView == null || view.transform.GetSiblingIndex() > topView.transform.GetSiblingIndex())
                    topView = view;
            }

            return topView;
        }

        private async UniTask OpenScreenAsync<TVM>(Type type, ViewSlot viewSlot, Action<TVM> setup) where TVM : AbstractViewModel
        {
            List<ScreenView> viewPool = viewSlot.ViewPool;
            
            ScreenView<TVM> targetView = viewPool
                .LastOrDefault(v => !v.gameObject.activeSelf) as ScreenView<TVM>;
            if (targetView == null)
            {
                targetView = (ScreenView<TVM>)Instantiate(viewSlot.ViewPrefab, rootCanvas.transform);
                Debug.Assert(viewSlot.ViewPrefab.gameObject.activeSelf, $"{viewSlot.ViewPrefab.name} 프리팹의 active가 꺼져있습니다.");
                viewPool.Add(targetView);
            }

            //맨 아래로 내려서 보이게 만들기
            targetView.transform.SetAsLastSibling();
            
            //setup 시키고 Open
            if (setup != null)
                setup(targetView.vm);
            await targetView.OpenAsync();
        }

        private bool TryCreateViewPool<TVM>(Type vmType, out ViewSlot viewSlot) where TVM : AbstractViewModel
        {
            viewSlot = null;
            foreach (var prefab in screenViewPrefabs)
            {
                if (prefab is ScreenView<TVM> screenView)
                {
                    viewSlot = new ViewSlot
                    {
                        ViewPool = new List<ScreenView>(),
                        ViewPrefab = prefab
                    };
                    _viewPool.Add(vmType, viewSlot);

                    return true;
                }
            }

            return false;
        }
    }
}
