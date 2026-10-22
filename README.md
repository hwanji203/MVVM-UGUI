# MVVM UGUI

UGUI 용 MVVM 틀. 화면 하나 만들 때 반복되는 연결 코드를 0 에 가깝게 줄이는 것이 목표다.

- `Bindable<T>` — 값이 바뀌면 연결된 화면이 따라간다. 구독은 `Bind` (연결하는 즉시 현재 값으로 한 번 부른다). 파생 값 (`Select` · `Combine`) 으로 "무엇을 갱신할지" 를 한 번만 선언. 버튼은 `Command`, 일회성 알림은 `Signal<T>`, 목록은 `BindableList<T>`.
- `AbstractViewModel` — 화면 상태를 가진 순수 C# 객체. 오래 사는 것의 구독은 `Own`, 열 때마다 되돌릴 값은 `ResetForOpen` (+ `ResetAll`), 한 번만 할 연결은 `IInitializable.Initialize`.
- `Element` — 재사용 위젯 41개 (Text · Gauge · Button · Slider …). 새 Element 는 바탕 클래스의 추상 메서드만 구현한다 — 한 방향 `VisualElement<T>.HandleValueChanged`, 트윈 `TweenValueElement<T>.ApplyImmediate` · `AppendTween`, 양방향 `FormElement<T>` (조작은 `Write`), 버튼 `CommandElement.HandleActiveChanged`, 알림 `SignalElement<T>.HandleSignal`.
- `ScreenView<TVM>` · `ScreenManager` — **모든 화면은 매니저로 연다** (`Open<TVM>(setup)`, 키는 ViewModel 타입). 닫힌 화면은 꺼 두었다가 다시 쓴다. 팝업은 `AbstractPopupVM` · `AbstractPopupView<T>` (닫기는 VM 이 요청), 확인 팝업 템플릿 `ConfirmPopup`.
- 이름으로 채우기 — View 의 `[SerializeField]` Element 필드를 같은 이름의 자식으로 채우는 에디터 버튼. 연결 (`Bind`) 은 View 에 손으로 쓴다. 비워 둬도 되는 필드는 `[Nullable]`.

> 개발 중 (0.x). 팀원용 사용 가이드 `Design Doc.html` (패키지 루트) · 설계 `Documentation~/DESIGN.md` · 진행 `Documentation~/ROADMAP.md` · AI 작업 지침 `AGENTS.md`.

## 설치

`Packages/manifest.json` 의 `dependencies` 에 한 줄:

```json
"com.banggae.mvvm-ugui": "https://github.com/hwanji203/MVVM-UGUI.git#v0.3.0"
```

`#` 뒤의 태그가 버전이다. 올릴 때는 태그만 바꾼다.

## 요구 사항

- Unity 6000.3 이상, UGUI (TextMeshPro 포함)
- **DOTween** — 패키지 의존성으로 선언할 수 없어 프로젝트에 직접 설치되어 있어야 한다.
- **UniTask** (2.5.11 이상, Unity 6000.2+ 에서는 2.5.10 이하가 에디터 코드에서 컴파일 오류) — 화면 열기 · 닫기 (`ScreenView.OpenAsync` · `CloseAsync`) 가 `UniTask` 를 돌려준다. Git 패키지라 의존성으로 선언할 수 없어 직접 설치한다:
  `"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.11"`

## 프로젝트와 연결하는 곳

MVVM UGUI 는 특정 이벤트 · 사운드 시스템을 모른다. 프로젝트마다 아래 두 곳만 잇는다.

| 연결점 | 패키지가 주는 것 | 프로젝트가 하는 일 |
| --- | --- | --- |
| 클릭 소리 | 추상 `UISoundSO` (+ 기본 `AudioSource` 구현) | 자기 사운드 시스템으로 `Play()` 를 구현한 SO 하나 (선택) |
| 화면 열기 · 닫기 | `ScreenManager.Open<TVM>(setup)` · `OpenAsync<TVM>(setup)` (키는 ViewModel 타입) · 닫기 | 자기 이벤트 시스템에서 받아 이 메서드를 부르는 컴포넌트 하나 (선택). 시작할 때 띄울 화면 (HUD 등) 은 시작 컴포넌트가 `Open<TVM>(vm => vm.Setup(model))` 로 연다 |

매니저는 화면을 일반 `Instantiate` 로 만들고 루트 Canvas 는 `DontDestroyOnLoad` 라, DI (Reflex 등) 의 자동 주입이 매니저가 만든 화면에 닿지 않는다. 화면에 넘길 것은 씬의 시작 컴포넌트가 주입받아 `setup` 으로 넘긴다.
