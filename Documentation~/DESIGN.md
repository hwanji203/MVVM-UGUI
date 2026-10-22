# MVVM UGUI 설계

> UGUI 용 MVVM 틀. 2026-10-14 ~ 15 사용자 (hwanji203, 이니셜 JTH) 와 Claude 가 대화로 확정했다.
> 원본 설계: Project_B `Docs/design/unity-ui-architecture.md` (게임 내용이 섞여 있음). 이 문서는 그중 **틀에 해당하는 부분만** 프로젝트와 무관하게 옮긴 것이다.
> 진행 순서 · 현황 · 정한 세부는 `ROADMAP.md`.
>
> 표기: **(확정)** = 사용자가 정함 — 바꾸려면 사용자에게 먼저 묻는다. `[세부]` = 구현하는 쪽이 정해도 됨 — 정하면 `ROADMAP.md` 의 "정한 세부" 에 적는다.

## 0. 목표 (확정)

**화면 하나 만들 때 반복되는 연결 코드를 0 에 가깝게.**

- 자식 찾기 · `GetComponent` · 인스펙터 드래그 · 위젯 `AddListener` · "값이 바뀌었으니 이것 · 저것 갱신" 호출을 화면마다 쓰지 않는다.
- 화면마다 직접 쓰는 것 = **ViewModel 하나 + View 하나 (이름이 다른 것만 연결) + 프리팹.**
- **어떤 프로젝트에든 쉽게 붙인다.** 특정 이벤트 · 사운드 시스템을 모른다. 프로젝트와 잇는 곳은 2 군데뿐이고 둘 다 선택 (7.3).

### 0.1 예전 시도 (HwanLib MVP) 에서 고친 것

사용자의 예전 MVP (`2026-club-PPTeamProject/Assets/HwanLib/MVP`) 의 문제와 대응:

| 예전 | 문제 | 여기서는 |
| --- | --- | --- |
| Presenter 가 값을 바꾸고 `UpdateForm(A)` · `UpdateForm(B)` 를 손으로 | 반복, 빠뜨림 | `Bindable<T>` + 파생 값 |
| SO 에 메서드 이름 문자열 → 실행 시 리플렉션 | 이름이 바뀌면 조용히 끊김 | 연결은 C# 코드 (생성되거나 손으로) — 틀리면 컴파일 에러 |
| `F_` 이름 → 계층 순서 (childIndex) → enum | 계층 순서가 바뀌면 깨짐 | 참조 배열 + 생성된 타입 있는 속성 |
| `UIParam` 캐스팅 | 실행 중에 터짐 | `VisualElement<T>` · `FormElement<T>` — 타입 고정 |
| 이름 Form | "입력 양식" 처럼 읽힘 | **Element** |
| `UIManager : LightSingleton`, 화면끼리 `public static bool` | 싱글톤 · 전역 상태 | 씬마다 일반 컴포넌트 `ScreenManager` |
| `IMultiable` (같은 팝업 N 개가 모두 이벤트를 받고 상태 머신으로 조율) | 복잡 | `ScreenManager` 하나가 받고 인스턴스를 고른다 |

## 1. 층과 의존 방향 (확정)

| 층 | 무엇 | 종류 |
| --- | --- | --- |
| **Model** | 게임 쪽 (전투 · 설정 SO · 이벤트). 틀은 모른다 | 프로젝트 |
| **ViewModel** | 화면 하나의 상태 (`Bindable`) · 버튼 명령 (`Command`) | **순수 C#** (MonoBehaviour 아님), `IDisposable` |
| **View** | 프리팹 루트 MonoBehaviour. ViewModel 을 만들고 Element 와 연결. 특수 연출만 코드로 | `ScreenView<TViewModel>` |
| **Element** | 재사용 위젯. 한 타입의 값을 보여주거나 받는다 | MonoBehaviour |

```
View ──안다──▶ ViewModel ──▶ (프로젝트의 이벤트 시스템) ◀── Model
                    └──── 설정 SO · 읽기 전용 상태 (필요할 때만, 한 방향) ────┘
```

- 참조는 **V → VM → M 한 방향**. 위로 올라오는 것은 이벤트 (보내는 쪽이 받는 쪽을 모른다). M 이 VM 을 알아야 하는 경우는 없다 — UI 로 값을 바꾸는 설정창도 VM → M (1.2).
- ViewModel 은 View · Element 를 모른다. Element 는 `Bindable` · `Command` 만 안다.
- ViewModel 이 순수 C# 인 이유: `new` 로 만들어 EditMode 테스트가 바로 되고, 프리팹 컴포넌트가 View 하나뿐. `OnDestroy` 가 없으므로 View 가 대신 `Dispose` (2.6).

### 1.1 처음 값

이벤트는 바뀐 것만 알려준다. 화면이 만들어질 때의 값은 — 기본은 **시작 이벤트에 담아** 받고, 중간에 여는 화면만 **읽기 전용 상태나 SO 를 VM 의 직렬화 필드 (4.3) 나 setup (5.1) 으로** 받아 읽는다. (처음엔 생성자로 받았는데, VM 이 `new()` 로 만들어지는 직렬화 필드가 되면서 바뀌었다 — 4.3.)

### 1.2 UI 로 값을 바꾸는 화면 (설정창 예)

```csharp
[Serializable]
public class SettingsViewModel : AbstractViewModel, IInitializable
{
    [SerializeField] private SettingsSO settings;                                   // Model = 프로젝트의 설정 SO (VM 의 직렬화 필드, 4.3)
    public Bindable<float> MasterVolume { get; } = new();

    public void Initialize()                                                        // 역직렬화 뒤 한 번 — 생성자에서는 settings 가 아직 비어 있다
    {
        MasterVolume.Value = settings.MasterVolume;                                 // 처음 값
        MasterVolume.Bind(v => settings.SetMasterVolume(v));                        // VM → M (자기 Bindable → 해제 불필요. 구독 즉시 지금 값을 한 번 넘긴다 — 같은 값)
        Own(settings.MasterVolumeChanged.Subscribe(v => MasterVolume.Value = v));   // 다른 곳에서 바뀌면 따라감 (프로젝트 SO 의 구독 API. SO 는 오래 산다 → Own)
    }

    protected override void ResetForOpen() { }                                      // 저장되는 양방향 값은 되돌리지 않는다 (2.5)
}
```

적용 (AudioMixer · 해상도) 과 저장은 Model 쪽이 한다 — 설정창이 열린 적 없어도 게임 시작 때 적용된다.

## 2. 바인딩 (순수 C#)

### 2.1 `Bindable<T>` · `IReadOnlyBindable<T>` (확정)

```csharp
public interface IReadOnlyBindable<T>
{
    T Value { get; }
    IDisposable Bind(Action<T> onChanged);   // 구독 즉시 현재 값을 한 번 넘긴다
}

public class Bindable<T> : IReadOnlyBindable<T>, IResettable
{
    public Bindable(T initial = default);
    public T Value { get; set; }          // 같은 값이면 무시 (EqualityComparer<T>.Default), 다르면 구독자 전부 호출
    public IDisposable Bind(Action<T> onChanged);
    public void Reset();                  // 생성자 값으로 (IResettable, 2.5) — { Value = … } 객체 초기화는 초기값이 아니다
}
```

- **구독하는 길은 `Bind` 하나.** `Changed` 이벤트를 밖에 열지 않는다 (처음 값 놓침 · 해제 잊음 방지). 내부 도우미 (`Select` · `Combine` · Element) 도 `Bind` 를 쓴다.
- 구독 즉시 현재 값 → 화면을 처음 열 때 "전부 갱신" 이 필요 없다. 처음 설계의 `notifyNow` (구독 즉시 호출 끄기) 는 v0.3.0 에 없다 — 늘 부른다.
- 참조 타입의 내부만 바뀌면 알 수 없다 → 목록은 `BindableList<T>`, 그 밖에는 새 값을 넣는다.
- 이름: 구독 메서드도 `Bind` 라 Element 의 `Bind` 와 이름이 같다. 처음 설계는 `Subscribe` (Bindable) ≠ `Bind` (Element) 로 나눴지만, 사용자가 코드를 쓰며 `Bind` 로 정했고 이름 문제는 없다고 봐 그대로 둔다 (사용자 10-22).

**노출 규칙 (확정)** — 값 하나에 private + public 두 줄을 쓰는 반복을 피한다:

| 종류 | 노출 타입 |
| --- | --- |
| 파생 값 (`Select` · `Combine` 결과) | `IReadOnlyBindable<T>` |
| 원본 상태 | `public readonly Bindable<T>` |
| 양방향 입력 (슬라이더 · 토글) | `Bindable<T>` |

### 2.2 파생 값 (확정)

```csharp
// 정적 클래스 Bindable. Select 는 SelectBindable<TR, T1>, Combine 은 CombineBindable<TR, T1, T2> 를 돌려준다 (둘 다 IReadOnlyBindable<TR> + IDisposable)
HpRatio = Bindable.Combine(Hp, MaxHp, (h, m) => (float)h / m);   // 원본이 VM 자신의 값 → 끊지 않아도 된다
HpText  = Bindable.Combine(Hp, MaxHp, (h, m) => $"{h}/{m}");

var volumeText = Bindable.Combine(settings.Volume, settings.Muted, (v, m) => m ? "음소거" : $"{v * 100:0}%");
Own(volumeText);                                                  // 원본이 VM 보다 오래 산다 → 맡긴다
VolumeText = volumeText;

Dead    = Bindable.Select(Hp, h => h <= 0);                        // 원본이 VM 자신의 값 → 끊지 않아도 된다
```

"원본이 바뀌면 무엇을 갱신할지" 를 **만들 때 한 번** 선언한다. 원본을 바꾸는 이벤트가 늘어도 갱신 코드는 안 는다.
**해제 (사용자 2026-10-18):** 파생 값은 원본 구독을 끊을 수 있어야 한다. 원본이 VM 자신의 값이면 같이 버려지므로 끊지 않아도 되고, VM 보다 오래 사는 원본 (SO · 전역 객체) 이면 `Own` — 안 하면 화면을 닫아도 원본이 파생 값을 붙잡아 계속 다시 계산된다. `Select` · `Combine` 모두 값과 해제가 한 객체 (생성자가 원본을 구독, `Dispose` 로 끊음 — `Select` 도 처음의 `(bindable, unSub)` 튜플에서 이 모양으로 바뀌었다). `Own` 을 잊어도 컴파일은 되므로 오래 사는 원본일 때는 리뷰로 본다.
`[세부]` 인자 수 — 지금 `Select` 1 · `Combine` 2 (3 은 필요할 때). namespace `MVVM.UGUI.Binding` (ROADMAP "정한 세부").

### 2.3 `Command` (확정)

```csharp
public class Command
{
    public Command(Action command = null, bool active = true);
    public IReadOnlyBindable<bool> Active { get; }
    public Action Action { get; set; }    // 동작 갈아 끼우기 (null 이면 비움)
    public void Execute();                // Active 가 false 거나 동작이 없으면 무시
    public void SetEnabled(bool active);
    public void Reset();                  // 생성자의 동작 · 켜짐으로 (IResettable, 2.5)
}
```

인자 없는 입력 (버튼). 값이 있는 입력은 양방향 `Bindable`.
**동작 교체 (사용자 10-21):** 화면은 다시 쓰이고 `Bind` 는 `Awake` 에서 한 번이라, Command **객체**는 VM 에서 한 번 만들고 (get-only 속성 초기화 · 생성자) 열 때마다 바뀌는 동작은 `Action` 속성으로 넣는다 — 처음엔 `SetAction(Action)` 메서드였고 같은 날 사용자가 속성으로 바꿨다 (`Command<T>` 도 Claude 가 맞춤). 지난 동작 · 잠금은 `ResetForOpen` 의 `Reset` 이 생성자 상태로 되돌린다 (2.5). 처음 모양 (`public event Action` + 생성자로만 동작) 은 VM 에서 `=` 를 못 해 (CS0070, `event` 는 밖에서 `+=` · `-=` 만) `+=` 로 콜백이 쌓이고, 새 Command 를 만들면 Element 는 옛 객체를 들고 있었다. 안 고른 안 (Claude 추천이었음): Command 는 불변, VM 이 자기 메서드를 넣고 그 메서드가 콜백 필드를 부름 — 콜백마다 VM 필드가 는다. 단점: Command 를 든 쪽 (View · Element) 도 동작을 바꾸거나 지울 수 있다.
**`Command<T>` (사용자 10-20)** — 같은 모양에 `Execute(T argument)`. 실행할 때 인자가 필요한 입력 (끌어다 놓은 곳 번호 — `DragElement`). Element 쪽 바탕은 `CommandElement<T>`.

### 2.4 `BindableList<T>`

한 화면 안에서 개수가 바뀌는 목록 (손패 · 상점 칸). `[세부]` 알림 종류 — v0.3.0 은 두 가지: **목록 전체** (`Add` · `Remove` · `RemoveAt` · `Clear` → `Bind(Action<IReadOnlyList<T>>)`, 구독 즉시 부름) · **한 칸** (인덱서 대입 → `Bind(Action<int, T>)`, 구독 때는 안 부름). 처음 계획은 `Added(index, item)` · `Removed(index)` · `Replaced` · `Reset` 이었다.
`Reset()` (IResettable, 2.5) 은 빈 목록으로 — 이미 비어 있으면 알리지 않는다 (`Clear()` 는 늘 알린다).
같은 **화면**을 여러 개 띄우는 것 (5.3) 과는 다른 문제.

### 2.5 `ViewModel` 기반 (확정 — `Own` 만, `Listen` 같은 이벤트 구독 도우미는 넣지 않는다)

```csharp
public abstract class AbstractViewModel : IDisposable
{
    protected void Own(IDisposable d);       // 오래 사는 것의 구독을 맡긴다
    public virtual void Dispose();           // 맡긴 것 전부 해제
    protected abstract void ResetForOpen();  // 열 때마다 setup 직전 — 지난번 setup 값 되돌리기 (사용자 10-21)
    protected static void ResetAll(params IResettable[] items);   // ResetForOpen 에서 한 줄로 (사용자 10-21 요청, Claude)
}
```

틀은 이벤트 시스템을 모른다. 프로젝트의 이벤트 구독은 그 프로젝트의 ViewModel 이 직접 하고 `Dispose` 에서 푼다.

**해제가 필요한 것 — VM 보다 오래 사는 것을 구독할 때만:**

| 구독 대상 | 끊어야 하나 | 쓰는 것 |
| --- | --- | --- |
| VM 자신의 Bindable · 파생 값 | 아니요 (같이 버려진다) | 그냥 `Bind` |
| 프로젝트의 이벤트 채널 | **예** | 구독 + `Dispose` 에서 해제 (프로젝트 코드) |
| SO 의 변경 알림 · 전역 객체 | **예** | `Own(...Bind(...))` (Bindable 이 아니면 그 구독의 해제 핸들) |
| setup 으로 받은 대상 (씬의 Model 등, 5.1) | **예** — 열 때마다 대상이 바뀔 수 있다 | `Setup` 에서 구독, `ResetForOpen` · `Dispose` 에서 해제 |

**열 때마다 되돌리기 — `ResetForOpen` (사용자 10-21):** 닫힌 화면은 파괴되지 않고 다시 쓰이며 (5.1) `Bind` 는 `Awake` 에서 한 번이라, 지난번 `setup` 이 넣은 값 · 콜백이 다음 열기에 남는다. `ScreenManager` 가 열 때마다 `setup` 직전에 (setup 이 없어도) 부른다. 매니저는 `internal PrepareOpen()` 을 거쳐 부르므로 하위 VM 은 어느 어셈블리에 있든 `protected override`.

```csharp
public class ShopVM : AbstractViewModel
{
    public Bindable<string> Title { get; } = new();   // get-only — 아래에서 새로 대입하면 컴파일 오류 (CS0200)
    public Command BuyCmd { get; } = new();
    public readonly Bindable<int> Gold = new();       // Model 이 채우는 값 — 되돌리지 않는다

    public void Setup(string title, Action onBuy) { Title.Value = title; BuyCmd.Action = onBuy; }

    protected override void ResetForOpen() => ResetAll(Title, BuyCmd);   // 값 · 동작 · 잠금을 만든 직후로
}
```

- **값만 되돌린다** — 바인딩은 `IResettable.Reset()` (Bindable = 생성자 값, Command = 생성자의 동작 · 켜짐, BindableList = 빈 목록) 을 `ResetAll(...)` 로 한 번에, 일반 필드는 직접. Bindable · Command 를 새로 만들면 View 는 옛 객체를 들고 있다. 바인딩을 get-only 속성으로 두면 컴파일러가 막아 준다. 파생 값 (`Select` · `Combine`) 은 `IResettable` 이 아니라 넘기면 컴파일 오류 — 원본을 되돌리면 따라온다.
- **`Reset` 의 기준은 "만든 직후"** (사용자 10-21 요청 "동작 비우기 · 다시 켜기를 매번 쓰지 않게", 구현 Claude). 그래서 생성자에 넣은 값 · 동작이 열 때마다의 기본이 된다 — 예: `new Command(RequestClose)` 는 되돌려도 닫기는 남아 setup 없이 열어도 팝업이 갇히지 않는다. 단점: `new Bindable<int> { Value = 5 }` (객체 초기화) 는 생성 뒤 대입이라 `Reset` 이 0 으로 간다 — 초기값은 생성자로 (`new(5)`).
- **한 번만 할 일은 여기가 아니다** — 직렬화된 SO 구독 · `Own` · 파생 값은 `IInitializable.Initialize` (Awake, Bind 전, 한 번, 4.3). 여기에 두면 열 때마다 구독이 쌓인다.
- **Model 이 채우는 값 · 양방향으로 저장되는 값은 되돌리지 않는다** — 다시 열면 Model 이 바뀔 때까지 기본값이 보이거나, 되돌린 값이 저장으로 흘러간다.
- 단점: abstract 라 되돌릴 게 없는 VM 도 빈 override 를 쓴다. 목록은 손으로 쓰므로 빠뜨린 필드는 지난 값이 남는다 — 컴파일러로 못 막는다 (Unity 는 C# 9 라 `required` 도 없다).
- 안 고른 안: View 가 Bind 한 원본을 모아 닫힐 때 Clear (사용자) — Element 는 원본이 어디서 왔는지 몰라 Model 값 · 양방향 저장값 · View 가 만든 닫기 Command 까지 비우고, 읽기 전용 인터페이스에 Clear 를 열어야 한다. VM 선언에서 `PerOpen(...)` 으로 표시 (Claude) — 바인딩 타입마다 지원이 필요하고 일반 필드는 따로다. `Initialize` 를 abstract 로 열 때마다 (사용자) — 한 번만 할 일의 자리가 없어져 둘로 나눔. 경과는 ROADMAP 10-21.

### 2.6 수명 (확정)

- **View 에 bag 이 없다.** Element 가 자기 연결을 들고 있다가 자기 `OnDestroy` 에서 끊고, ViewModel 은 View 가 `OnDestroy` 에서 `Dispose`.
- 그래서 **Element 의 `Bind` 는 `void`**. 끊을 일이 있으면 `Unbind()`, 다시 묶으면 `Bind` 가 이전 연결을 끊는다.
- `Bindable.Bind` 는 `IDisposable` 을 돌려준다 (Element 내부 · `Select` · `Combine` · `Own` 이 쓴다).
- **`Dispose` 는 View 가 파괴될 때 한 번** (씬이 바뀔 때 · 여러 개 뜨는 팝업을 닫을 때). **닫힌 (비활성) 화면은 Dispose 하지 않는다** — 계속 이벤트를 받아 값이 최신, 다시 열면 바로 맞다.

### 2.7 `Signal<T>` — 일회성 알림 (사용자 10-20)

```csharp
public interface IReadOnlySignal<T> { IDisposable Bind(Action<T> onFired); }
public class Signal<T> : IReadOnlySignal<T> { public void Fire(T value); … }
```

- `Bindable` 은 **상태** (같은 값 무시 · 구독하는 순간 현재 값을 부름). 토스트 · 데미지 숫자 같은 **사건**에 쓰면 같은 값 두 번째가 사라지고, 다시 Bind 할 때 지난 사건이 또 뜬다.
- `Signal` 은 값을 저장하지 않는다 — `Fire` 마다 알리고, `Bind` 할 때 부르지 않는다. 구독 전에 `Fire` 된 것은 사라진다 (의도).
- VM 의 C# `event` 대신 쓰는 이유: Element 는 `Bindable` · `Command` · `Signal` 만 알고, `Bind` 가 `IDisposable` 을 돌려줘 Element 의 연결 바구니에 그대로 들어간다.
- 받는 Element 의 바탕은 `SignalElement<T>` — **꺼져 있을 때 온 알림은 버린다** (닫혀 숨은 화면에 토스트 · 데미지 숫자가 쌓이지 않게).

## 3. Element

### 3.1 기반 (확정)

```csharp
public class Element : MonoBehaviour
{
    protected readonly DisposableBag Bindings = new();   // 이 Element 의 연결 — 다시 묶거나 파괴될 때 한 번에 푼다
    public virtual void Unbind() => Bindings.Clear();
    protected virtual void OnDestroy() { Unbind(); /* + 트윈 Kill */ }
}

public abstract class VisualElement<T> : Element      // 한 방향 (값 → 화면)
{
    public virtual void Bind(IReadOnlyBindable<T> source)
    {
        Unbind();                                        // 다시 묶으면 이전 연결은 끊는다
        Bindings.Add(source.Bind(HandleValueChanged));   // 구독 즉시 현재 값으로 HandleValueChanged
    }

    protected abstract void HandleValueChanged(T value); // 각 Element 는 이것만 구현
}
```

- 처음 설계의 이름은 `ValueElement<T>` · `Apply` 였다. v0.3.0 은 위처럼 연결 바구니가 `Element` 에 있고, 값 Element 의 바탕은 `VisualElement<T>` (한 방향) · `FormElement<T>` (양방향, 3.2) · `TweenValueElement<T>` (트윈, 3.4) 다. 다른 바탕은 3.3.
- `Bind` 는 `IReadOnlyBindable<T>` 를 받는다 → `Bindable` · 파생 값 둘 다 넘길 수 있다. 양방향 Element (`FormElement<T>`) 는 같은 `Bind` 를 쓰고 안에서 `Bindable<T>` 인지 확인한다 — 파생 값이면 `Debug.Assert` 가 뜨고 조작이 VM 에 쓰이지 않는다.
- **피할 이름**: `TextElement` · `BindableElement` — `UnityEngine.UIElements` 에 이미 있다.
- 필요한 컴포넌트는 `[SerializeField]` 로 받고 **`Reset()` 에서 같은 오브젝트의 것을 채운다** (확정).

### 3.2 양방향 (확정)

```csharp
slider.onValueChanged.AddListener(Write);                                                 // 조작 → ViewModel (FormElement.Write, 알림이 나간다 — 의도)
protected override void HandleValueChanged(float v) => slider.SetValueWithoutNotify(v);   // ViewModel → 화면
```

- 조작하면 알림이 나가 같은 값을 쓰는 다른 곳이 따라온다. 되먹임은 ① `SetValueWithoutNotify` ② Bindable 같은 값 무시 두 겹으로 끊긴다.
- 범위 제한 · 반올림은 **ViewModel 이 값을 넣을 때** (위젯이 고쳐 받으면 화면과 Bindable 이 어긋난다).

### 3.3 목록 (사용자 10-20 — 후보 59개에서 고름, 구현 Claude)

바탕 (`Elements/Core`): `Element` · `VisualElement<T>` (한 방향) · `FormElement<T>` (양방향, `Write`) · `CommandElement` · `CommandElement<T>` · `SignalElement<T>` · **`TweenValueElement<T>`** (3.4) · `ReadOnlyListElement<T>` · `ListElement<T>` · `ITransitionElement` (창 · 배경의 `PlayOpen` · `PlayClose`) · **`TransitionElement`** (그 인터페이스의 추상 바탕 — 인터페이스는 인스펙터 필드로 못 받아서 틀은 이 타입으로 받는다, 사용자 10-21).
값 없이 필드로 들고 쓰기만 하는 것 (특수 연출용) 은 `Element` 를 그대로 붙인다.

**폴더 = namespace (사용자 10-21)** — 표의 갈래대로 `Runtime/Elements/` 아래에 나누고, namespace 는 폴더 경로를 따른다 (`NamespaceTests` 가 검사):

```
Elements/
 ├ Core/              MVVM.UGUI.Elements.Core            바탕
 ├ Display/           MVVM.UGUI.Elements.Display         Image · Color · GradientColor · Alpha · Visible · Interactable · Switch · Badge · Drawer · LoadingSpinner · WorldFollow · Tooltip
 │  ├ Texts/          MVVM.UGUI.Elements.Display.Texts   TextView · NumberText · CountUpText · TimerText · TypewriterText
 │  └ Gauges/         MVVM.UGUI.Elements.Display.Gauges  Gauge · DelayedGauge · Cooldown · CountIcons
 ├ Inputs/            MVVM.UGUI.Elements.Inputs          Slider · IntSlider · Toggle · ToggleGroup · Tab · Dropdown · InputField · OptionStepper · Hover
 ├ Commands/          MVVM.UGUI.Elements.Commands        Button · LockButton · HoldButton · ClickArea · Drag
 ├ Lists/             MVVM.UGUI.Elements.Lists           ItemList
 ├ Popups/            MVVM.UGUI.Elements.Popups          Window · BackgroundDim
 └ Feedback/          MVVM.UGUI.Elements.Feedback        Toast · FloatingText · Punch
```

- 경계에 걸리면 **View 에서 `Bind` 할 때 넘기는 것**으로 정한다 (Tooltip 은 `string` 이라 Display, Punch 는 한 번 하는 연출이라 Feedback). 예외: BackgroundDim 은 Command 를 받지만 쓰임이 팝업 연출이라 Popups.
- **폴더 이름은 복수형** (`Commands` · `Inputs` · `Texts` …, .NET 의 `System.Collections` 꼴). 단수 `Command` 면 `MVVM.UGUI.Elements.*` 안의 코드에서 `Command` 가 `Binding.Command` 클래스가 아니라 namespace 로 잡혀 컴파일이 깨진다 (C# 은 바깥 namespace 의 이름을 `using` 보다 먼저 찾는다). `Input` · `Text` 도 Unity 의 `Input` · `UI.Text` 와 같은 함정.
- 쓰는 쪽 영향: View 가 Element 필드를 들므로 쓰는 갈래마다 `using` 이 는다 (`using MVVM.UGUI.Elements.Inputs;` 등).

| 갈래 | Element | 묶는 값 | 하는 일 |
| --- | --- | --- | --- |
| 보여주기 | `TextViewElement` | `string` | TMP. 원래 글에 `{0}` 이 있으면 포맷으로 |
| | `NumberTextElement` | `float` (+ `int` 오버로드) | `{0:N0} G` 같은 형식 |
| | `CountUpTextElement` | `float` (+ `int`) | 숫자가 굴러가며 따라감 (트윈) |
| | `TimerTextElement` | `float` 초 | `mm:ss` · `h:mm:ss` · 초 · 0.1초, 남은 시간은 올림, 경고 색 |
| | `TypewriterTextElement` | `string` | 한 글자씩. `Skip()` |
| | `GaugeElement` | `float` 0~1 | Image fill · 가로 / 세로 크기 (트윈) |
| | `DelayedGaugeElement` | `float` 0~1 | 잔상 바가 늦게 따라옴 (트윈) |
| | `CooldownElement` | `float` 0~1 | 덮개 fill, 0 이면 숨김. 쿨다운 버튼 = 버튼 + 자식에 이것 |
| | `ImageElement` | `Sprite` | 그림 교체, null 이면 숨김 |
| | `ColorElement` · `GradientColorElement` | `Color` · `float` | Graphic 색 (트윈) |
| | `AlphaElement` | `float` | CanvasGroup 투명도 (트윈) |
| | `VisibleElement` | `bool` | Active (SetActive) · Fade · Scale. Fade · Scale 은 오브젝트를 끄지 않는다 |
| | `InteractableElement` | `bool` | CanvasGroup 통째로 잠금 + 흐리게 |
| | `SwitchElement` | `int` | 하나만 켬, 범위 밖이면 전부 끔 |
| | `CountIconsElement` | `int` | N 개 중 k 개 (하트 · 별점 · 칸 게이지). 꺼진 칸은 Hide · Tint · Swap |
| | `BadgeElement` | `int` | 0 이면 숨김, `99+` |
| | `DrawerElement` | `bool` | 한 방향으로 미끄러져 들어오고 나감 (트윈) |
| | `LoadingSpinnerElement` | `bool` | 보이고 돈다 (칸 단위 선택) |
| | `WorldFollowElement` | `Vector3` | 월드 좌표를 화면에서 따라감 (매 프레임) |
| 입력 | `SliderElement` · `IntSliderElement` | `Bindable<float>` · `Bindable<int>` | |
| | `ToggleElement` | `Bindable<bool>` | 클릭 소리 |
| | `ToggleGroupElement` | `Bindable<int>` | 하나만 고름, 고른 것을 다시 눌러도 안 꺼짐 |
| | `TabElement` | `Bindable<int>` | 토글 그룹 + 페이지 |
| | `DropdownElement` | `Bindable<int>` (+ 항목 목록) | 3.6 |
| | `InputFieldElement` | `Bindable<string>` | 칠 때마다 / 입력을 마칠 때 |
| | `OptionStepperElement` | `Bindable<int>` (+ 항목 목록) | `◀ 보통 ▶`, 끝에서 처음으로 (선택) |
| | `HoverElement` | `Bindable<bool>` | 포인터가 올라와 있나 (쓰기만) |
| 명령 | `ButtonElement` | `Command` | 잠김 = `Active` false. 클릭 소리 |
| | `LockButtonElement` | `Command` | 잠기면 자물쇠 · "잠김" 글, 잠긴 채 누르면 막힌 소리 |
| | `HoldButtonElement` | `Command` | 길게 눌러야 실행, 누르는 동안 fill |
| | `ClickAreaElement` | `Command` | 아무 Graphic 이나 누르면 |
| | `DragElement` | `Command<int>` | 놓은 `DropZone` 번호 (없으면 -1) 로 실행, 늘 제자리로 돌아감 |
| 목록 | `ItemListElement` | `IReadOnlyBindableList<TItem>` | 3.7 |
| 팝업 · 알림 | `WindowElement` | — (`ITransitionElement`) | 크기 (+ 투명도) 열기 · 닫기, 닫다가 다시 열면 이어서 |
| | `BackgroundDimElement` | `Command` (`ITransitionElement`) | 페이드 + 누르면 Command (닫기) |
| | `ToastElement` | `Signal<string>` | 차례로 잠깐 떴다 사라짐 |
| | `FloatingTextElement` | `Signal<string>` | 떠오르며 사라지는 글 (풀링) |
| | `PunchElement` | 어떤 `Bindable` · `Signal` 이든 | 바뀌면 통통 (첫 값은 안 튐) |
| | `TooltipElement` | `string` | delay 초 머무르면 띄움 — 터치는 길게 누르면 (누를 때 들어오고 뗄 때 나가서 같은 규칙) |

**AssistComponent (`Runtime/AssistComponent`, namespace `MVVM.UGUI.AssistComponent`)** — 값을 묶지 않아 Element 가 아닌 것 (사용자 10-20): `ButtonFeedback` (올림 · 누름 때 크기 · 투명도) · `DropZone` (놓을 곳 번호) · `SafeArea` (모바일 노치).

뺀 것 (사용자 10-20): Localized 글 · 키 다시 배정 · 단축키 (의존성 — 프로젝트 쪽), SpriteSwitch · Swap · Rotation · AnchoredPosition · ProgressText (VM `Combine` 으로) · ScrollPosition · SelectableList (`Command<int>` + 항목 VM 의 `IsSelected` 로) · RecycleList · PageIndicator · CooldownButton (버튼 + Cooldown 조합). 칸 게이지 (SegmentGauge) 는 `CountIconsElement` 와 같은 일이라 합쳤다 (Claude).

### 3.4 꺼진 오브젝트 · 트윈 중 (확정)

- 꺼진 오브젝트에도 구독은 살아 있다. 트윈이 있는 Element 는 **꺼져 있으면 트윈 없이 즉시 적용**.
- 트윈 중 새 값 → 이전 트윈을 `Kill`, 지금 위치에서 새 목표로.
- **이 규칙은 `TweenValueElement<T>` 한 곳에 있다 (사용자 10-20, 구현 Claude).** 하위는 `ApplyImmediate(value)` 와 `AppendTween(seq, value)` 만 쓴다.
  바로 맞추는 때: 첫 값 (Bind 직후 — 화면을 열 때마다 0 에서 차오르지 않게) · 꺼져 있음 · 플레이 중이 아님 · `duration` 0. 트윈 중 꺼지면 (`OnDisable`) 마지막 값으로 바로 맞춘다 — DOTween 은 꺼진 오브젝트의 트윈도 돌린다. `Kill` 이라 `Complete` 처럼 이전 목표로 튀지 않는다.
  `Sequence` 를 받는 이유: 잔상 게이지처럼 트윈이 둘인 Element 가 `Append` · `Insert` 로 그대로 된다. 단점: 단일 상속이라 트윈 있는 양방향 Element 는 이 바탕을 못 쓴다 (지금은 없음). 값이 아니라 열기 · 닫기로 움직이는 창 · 배경은 이것이 아니라 `ITransitionElement`.
- **Element 는 `Awake` 를 믿지 않는다** — 꺼진 채 만들어진 Element 는 `Awake` 보다 `Bind` (View 의 `Awake`) 가 먼저 온다. 원래 글 · 원래 색 · 원래 위치는 처음 값을 받을 때 읽는다. 리스너 연결만 `Awake` (눌리는 건 켜진 뒤라서).
- DOTween 은 **본체 API 만** (`DOTween.To` · `DOScale` · `DOPunchScale` · `DOLocalMoveY`). Modules 함수 (`DOFade` · `DOFillAmount`) 는 Project_B 에선 asmdef 없이 Assembly-CSharp 안, Apes 에선 따로 asmdef 라 패키지가 참조할 길이 없다 (Claude 10-20). 시작할 때 `SetLink(gameObject)`, 기본 `SetUpdate(true)`.

### 3.5 클릭 소리 (확정)

```csharp
public abstract class UISoundSO : ScriptableObject { public abstract void Play(); }
public class AudioSourceUISoundSO : UISoundSO { … }      // 패키지 기본 구현 (AudioClip 하나)
```

- `ButtonElement` 는 `[SerializeField] UISoundSO clickSound` 를 들고 눌리면 `Play()` 만. 화면 코드에 소리가 없다.
- 프로젝트가 자기 사운드 시스템으로 구현한 SO 를 만들어 쓰면 된다 (연결점 1, 7.3).
- `[세부]` `Reset()` 이 기본 SO 를 채우는 방법. 정한 것 (Claude 10-20): 버튼 · 토글 · 토글 그룹 · 스테퍼 · 클릭 영역 · 배경 = 클릭 소리, 길게 누르기 = 다 찼을 때, 잠긴 버튼 = `LockButtonElement` 의 막힌 소리, 슬라이더 = 없음. `AudioSourceUISoundSO` 는 처음 소리를 낼 때 씬 전환에도 남는 2D AudioSource 하나를 만든다.

### 3.6 원본이 둘인 Element (사용자 10-20)

Element 의 연결 바구니는 하나라 `Bind` 를 두 번 부르면 두 번째가 첫 번째를 끊는다. 그래서 원본 둘은 **한 번에** 받는다 — `DropdownElement.Bind(options, selected)` · `OptionStepperElement.Bind(options, selected)`.
항목이 인스펙터에 적혀 있으면 `Bind(selected)` 하나. 둘로 나눌 수 있는 것 (쿨다운 버튼) 은 Element 두 개를 조합한다.

### 3.7 목록 — `ItemListElement` + `ItemView<TItem>` (사용자 10-20, 구현 Claude)

```csharp
public class CardItemView : ItemView<CardViewModel>
{
    [SerializeField] private TextViewElement nameText;
    [SerializeField] private ButtonElement playButton;

    protected override void Bind(CardViewModel card) { nameText.Bind(card.Name); playButton.Bind(card.Play); }
}
handList.Bind(vm.Hand);   // ItemListElement — 제네릭이 아니고 Bind<TItem> 가 제네릭
```

- 제네릭 MonoBehaviour 는 컴포넌트로 붙일 수 없어서 **목록 Element 하나** (`ItemListElement`) 에 제네릭 `Bind<TItem>` 를 두고, 항목 프리팹의 `ItemView<TItem>` 타입은 Bind 할 때 확인한다 (다르면 오류 로그). 안 고른 것: 목록마다 한 줄 하위 클래스 (`CardListElement : ListElement<CardVM>`) — 타입은 컴파일에 잡히지만 목록마다 파일이 는다.
- 항목 View 는 VM 을 만들지 않고 받는다 — 항목은 목록을 가진 VM 의 것이라 **Dispose 하지 않는다**. 항목은 VM 이 아닌 값 (`int` · struct) 이어도 된다 — 목록이 `ItemView<TItem>` 로 형변환해 넘겨 박싱되지 않는다 (Claude 10-21, 처음엔 `SetItem(object)` 를 거쳐 struct 항목이 넘길 때마다 박싱됐다).
- 만든 항목 View 는 지우지 않고 꺼 두었다가 다시 쓴다 (화면 재사용 5.3 과 같은 이유). 뺄 때 안의 Element 연결을 끊는다. 목록이 바뀌면 보이는 항목을 전부 다시 Bind 한다 — 수백 개면 다시 본다.
- "이름으로 채우기" (4.1) 는 `ItemView` 에도 된다. 화면 프리팹 안에 놓인 항목 템플릿은 경계 — 화면 쪽에서는 그 아래를 찾지 않는다.
- 기존 `ReadOnlyListElement<T>` · `ListElement<T>` 는 ItemView 없이 직접 그리는 목록용으로 남긴다.

## 4. View — Element 는 `[SerializeField]` 필드, 연결은 손으로

### 4.1 Element 잡기 — 필드 + "이름으로 채우기" (사용자 10-21)

```csharp
public class BattleHudView : ScreenView<BattleHudViewModel>
{
    [SerializeField] private TextViewElement hpText;
    [SerializeField] private GaugeElement hpGauge;
    [SerializeField] private ButtonElement pauseButton;
}
```

- View 는 Element 를 보통 `[SerializeField]` 필드로 든다. **코드를 먼저 써도 된다.**
- **"이름으로 채우기"** (에디터 도구 `ElementAutoFill`, Claude 10-21): GameObject 인스펙터 머리의 **"MVVM UGUI · Element 이름으로 채우기"** 버튼 · `Tools/MVVM UGUI/선택한 View 의 Element 이름으로 채우기`.
  - **비어 있는** Element 필드만, 같은 이름의 자식 (꺼진 것 · 깊은 것 포함) 으로 채운다. 손으로 넣은 것은 그대로.
  - 이름은 대소문자 · 띄어쓰기 · `_` · 앞의 `m_` 를 무시하고 비교한다 (`hpText` ↔ `HP Text`).
  - 그 자식에 필드 타입의 컴포넌트가 있어야 한다. 같은 이름이 둘 이상이면 비워 두고 경고, 못 찾은 것도 경고 (끌어다 놓으면 된다).
  - 안쪽에 다른 View (ScreenView · ItemView) 가 붙은 오브젝트는 그 아래까지 그 View 의 것이라 찾지 않는다. 부모 클래스의 private 필드 (PopupView 의 `window` 등) 도 채운다. Undo 된다.
- 빠진 참조 검사 (`OnValidate` 경고) 는 넣지 않는다 (사용자 10-21).
- **비워 둬도 되는 필드는 `[Nullable]`** (`MVVM.UGUI.NullableAttribute` + `Editor/NullableDrawer`, 사용자 10-21): 인스펙터에서 이름이 기울임꼴, 툴팁에 "비워 둬도 된다". 어느 직렬화 필드에나 붙고 목록이면 각 요소에 적용. 표시만 한다 — 쓰는 쪽에서 null 을 확인한다 (`closeButton?.Bind(cmd)`).
- **안 고른 것 — 생성된 partial 속성 (10-14 에 정해 10-18 에 만들었다가 10-21 에 뺌)**: 생성기가 프리팹 저장 때 View 옆에 `<View>.Elements.g.cs` (숨은 참조 배열 + 타입 있는 속성) 를 만들던 방식. 뺀 이유 (사용자): 프리팹이 있어야 속성이 생겨 **코드를 먼저 쓰는 사람은 쓸 수 없고**, 언제 생기는지 (프리팹 모드 저장 · 버튼 · 첫 생성은 컴파일 뒤) 가 애매했다.
  필드 방식의 단점: Element 마다 필드 한 줄, 이름이 안 맞으면 끌어다 놓기, 빠지면 실행 중 `NullReferenceException` 으로 안다. 그보다 앞의 HwanLib 방식 (자식 순서 · 문자열) 은 조용히 깨져서 뺐다 (0.1).

### 4.2 자동 연결은 없다 (사용자 2026-10-18 — 처음 설계의 `AutoBind` 를 뺐다)

- 처음엔 Element 이름 == ViewModel 멤버 이름이면 생성기가 `AutoBind` 에 연결을 써 주는 안이었다.
- 뺀 이유: 자동으로 `A ↔ a` 가 묶인 뒤 손으로 `B ↔ a` 를 묶으면, 다른 Element 라 A 의 연결이 끊기지 않아 **A · B 가 둘 다 `a` 를 본다**. 막으려면 이름을 바꾸거나 `Unbind` · 제외 표시가 필요해지고, 연결이 코드에 다 드러나지 않는다.
- 그래서 **연결은 전부 View 의 `Bind` 에 손으로 쓴다.** ViewModel 쪽 선언도 만들어 주지 않는다.

### 4.3 손 연결 · VM 만들기 (사용자 10-21)

```csharp
[Serializable]                                   // VM 이 SO 를 직접 직렬화해 든다 — 인스펙터의 View 아래에 보인다
public class BattleHudViewModel : AbstractViewModel, IInitializable
{
    [SerializeField] private BattleChannelSO battleChannel;
    public readonly Bindable<string> HpText = new();

    public void Initialize() { /* 여기서 battleChannel 구독 — 생성자에서는 아직 null */ }

    protected override void ResetForOpen() { }   // 열 때마다 받는 값이 없다 (2.5)
}

public class BattleHudView : ScreenView<BattleHudViewModel>
{
    [SerializeField] private TextViewElement hpText;
    [SerializeField] private ButtonElement pauseButton;

    protected override void Bind(BattleHudViewModel vm)
    {
        hpText.Bind(vm.HpText);
        pauseButton.Bind(vm.PauseButton);
    }
}
```

- **VM 은 `ScreenView<T>` 의 직렬화 필드** `[SerializeField] internal T viewModel = new T();` (`T : AbstractViewModel, new()`). `CreateViewModel` 은 없앴다 (사용자 10-21).
  VM 에 `[Serializable]` 을 붙이면 Unity 가 프리팹에 저장된 값으로 만들고 (필드 초기화 `new T()` 는 역직렬화 전에 돌아 덮지 않는다), 안 붙이면 `new T()` 그대로. 복제 (`Instantiate`) 마다 VM 도 따로 생긴다 — `SerializedViewModelTests` 로 확인.
- **VM 이 쓰는 SO · 이벤트 채널은 VM 의 `[SerializeField]`** (사용자 10-21 — View 가 들고 생성자로 넘기던 것 대신). 지킬 것:
  - VM 에 **`[Serializable]`** 이 없으면 SO 칸이 안 생기고 오류도 안 난다.
  - **생성자에서 구독하지 않는다** — 역직렬화는 생성자 다음에 필드를 채우고, 에디터도 직렬화하며 생성자를 부른다. 구독은 **`IInitializable.Initialize()`** (선택, `Core/IInitializable`) 에서.
  - `Bindable` 은 `[Serializable]` 이 아니라 직렬화에서 빠진다 — VM 상태가 프리팹에 섞이지 않는다.
- `Awake` 순서: View `Initialize()` → VM 이 `IInitializable` 이면 `Initialize()` → `Bind(viewModel)` (**abstract**, 사용자 10-21) → 끄기. `OnDestroy` 에서 VM `Dispose`.
- 하위 View 는 VM 을 **`Bind(T vm)` 의 인자로만** 받는다 (잠깐 `Bind()` 로 바꿨다가 되돌림). 필드는 `internal` 이라 매니저만 쓴다.
- 같은 Element 를 다시 묶으면 이전 연결은 끊긴다 (Element 의 `Bind`).
- 여는 쪽이 넣을 값은 `ScreenManager.Open` 의 `setup` 으로 VM 에 넣는다 (10-19). `Open` · `OpenAsync` 의 `TVM` 에도 `new()` 제약.

## 5. 화면 열기 · 닫기 (확정)

### 5.0 `ScreenView` 의 열기 · 닫기 (사용자 10-18, 비동기는 UniTask)

**두 층 (사용자 10-18, 나누기는 Claude 에 맡김):**
- `ScreenView` (제네릭 아님) — 열기 · 닫기 · `PlayOpen` / `PlayClose` · 전환 취소 (`OnOpen` · `OnClose` 알림은 없앰, 사용자 10-19). **`ScreenManager` · 생성기는 이것만 본다** (목록 · 스택 · `GetComponent<ScreenView>()`).
- `ScreenView<T> : ScreenView` (T = ViewModel) — VM 은 직렬화 필드로 이미 있고 (4.3, 처음의 `CreateVM` 은 없앴다), `Awake` 에서 View `Initialize` → VM `Initialize` → `Bind(T)` → 꺼 두기, 파괴될 때 VM `Dispose`. 화면은 이걸 상속해 형변환 없이 자기 VM 을 받는다.
- 나눈 이유: `ScreenView<A>` 와 `ScreenView<B>` 는 C# 에서 관계없는 타입이라 "어떤 화면이든" 을 가리킬 공통 타입이 없었다.

```csharp
public async UniTask OpenAsync();    // SetActive(true) → await PlayOpen(token)
public async UniTask CloseAsync();   // await PlayClose(token) → SetActive(false) (이미 꺼져 있으면 아무것도 안 함)
protected virtual UniTask PlayOpen(CancellationToken token);    // 하위 View (PopupView) 가 트윈 · 연출을 넣는 자리, 기본은 바로 끝
protected virtual UniTask PlayClose(CancellationToken token);
```

- `Awake` 끝에서 꺼 둔다 — 열기 전까지 숨어 있다.
- 연출 중에 다른 `OpenAsync` · `CloseAsync` 가 오면 이전 것은 **취소** (취소된 쪽을 기다리던 `await` 도 풀린다). 파괴되면 (`destroyCancellationToken`) 도 취소.
  하위 View 는 받은 `token` 을 트윈 대기에 넘겨야 취소가 먹힌다.
- 기다릴 필요가 없으면 `view.OpenAsync().Forget();`. `ScreenManager` 는 `await` 로 순서를 맞출 수 있다 (닫힌 뒤 다음 열기 등).

### 5.1 `ScreenManager`

- **일반 컴포넌트** (싱글톤 아님). 화면 프리팹 목록을 `[SerializeField]` 로 든다.
- **2026-10-20 사용자 (위의 "씬에 두는 · 자기 씬의 목록" 을 바꾼다):** 매니저는 `DontDestroyOnLoad` 로 씬을 넘어 살고, **루트 Canvas 를 매니저가 든다** (매니저가 Canvas 오브젝트에 붙어 `Awake` 에서 `GetComponent<Canvas>()`). 그래서 화면 인스턴스도 씬을 넘어 다시 쓰인다 — 씬마다 `Instantiate` · `Awake` 를 다시 안 한다. 목록은 모든 씬의 화면을 담는다.
  지킬 것: 매니저 오브젝트는 씬의 최상위 (`DontDestroyOnLoad` 는 루트에만), 그 매니저가 있는 씬을 다시 불러도 둘이 되지 않게, Canvas 는 카메라가 필요 없는 Screen Space - Overlay (Camera 모드면 씬마다 카메라가 바뀐다), `EventSystem` 도 씬을 넘어 하나가 있어야 클릭을 받는다. 단점: 연 화면은 앱이 끝날 때까지 메모리에 남고 VM 도 계속 이벤트를 받는다 — 씬마다 정리할 게 생기면 VM 쪽에서.
- **메서드만**. 이벤트로 여는 것은 프로젝트가 잇는다 (연결점 2, 7.3).
- 열린 순서를 안다 (팝업과 다른 화면을 나누지 않는 목록 하나, 사용자 10-19). **게임 판단은 두지 않는다** — "게임이 끝나면 설정창을 못 연다" 같은 막기는 요청하는 쪽이 게임 상태로 한다 (버튼 VM 의 `Command.SetEnabled` · 프로젝트의 연결 컴포넌트). 패키지는 게임 화면을 모르고, 화면끼리 서로 알면 참조가 엉킨다 (사용자 10-19 지적).
- **모든 화면은 매니저로 연다 (사용자 10-22 — 처음의 "HUD 처럼 계속 떠 있는 화면은 씬에 두고 매니저 밖에서 다룬다" 를 바꾼다).** HUD 도 프리팹을 목록에 넣고, 시작을 맡은 컴포넌트가 `Open<HudVM>(vm => vm.Setup(model))` 로 연다.
  - 프리팹은 씬 오브젝트를 참조할 수 없어 씬의 Model 은 시작 컴포넌트가 받아 (인스펙터 · DI) setup 으로 넘긴다. VM 은 `Setup` 에서 구독하고 `ResetForOpen` · `Dispose` 에서 푼다 (2.5 표).
  - 대상이 바뀌면 (씬 전환) `CloseAsync` 뒤 `OpenAsync` — 켜진 채 또 `Open` 하면 쉬는 인스턴스가 없어 두 번째 HUD 가 생긴다.
  - 열 때 `SetAsLastSibling` 이라 HUD 를 먼저 열면 나중에 연 팝업이 위에 그려진다.
  - 씬에 직접 두면 `ScreenView<T>` 가 `Awake` 끝에서 꺼 버리고 (꺼 둔 채 저장하면 켜는 순간 `Awake` 가 돌아 다시 꺼진다) `ResetForOpen` 도 불리지 않는다 — Apes UI6 (PHASE-033 · 034) 에서 확인.

**열기 · 닫기 모양 (사용자 2026-10-19) — 키는 ViewModel 타입:**

```csharp
void    Open<TVM>(Action<TVM> setup = null)      where TVM : AbstractViewModel;  // = OpenAsync(setup).Forget()
UniTask OpenAsync<TVM>(Action<TVM> setup = null) where TVM : AbstractViewModel;
void    Close<TVM>()      where TVM : AbstractViewModel;  // 그 VM 타입의 맨 위 인스턴스, = CloseAsync<TVM>().Forget()
UniTask CloseAsync<TVM>() where TVM : AbstractViewModel;

ScreenManager.Open<ConfirmPopupVM>(vm => vm.Setup("저장할까요?", "되돌릴 수 없어요", OnYes));
```

- 여는 쪽은 **View 를 모르고 VM 타입만** 안다 (V → VM → M 한 방향, 1절). 그때마다 넣을 값 (팝업 제목 · 메시지 · 콜백) 은 `setup` 에서 VM 에 넣는다. VM 은 쓰기 가능한 `Bindable` 을 열기보다 `Setup(...)` 메서드 하나로 받는 것을 권장 — 무엇을 넣어야 열리는지가 시그니처에 드러난다.
- 순서: 쉬는 (꺼진) 인스턴스를 고르거나 없으면 `Instantiate` (→ `Awake` 가 `Initialize` → `Bind` → 끄기) → 열린 순서 맨 뒤로 · `SetAsLastSibling()` → **VM `ResetForOpen`** (setup 이 없어도, 사용자 10-21) → **`setup(vm)`** → `ScreenView.OpenAsync`. 열기 전에 값이 들어가므로 `PlayOpen` 도 새 값을 본다. 되돌리기 · setup 은 화면이 꺼져 있을 때라 값이 바뀌는 게 보이지 않는다.
- **VM 을 돌려주지 않는다.** 여는 쪽이 VM 을 들고 있다가 닫힌 뒤 (재사용이면 다음에 열린 다른 용도의) VM 에 값을 넣는 버그를 막는다. 키 (`int` 핸들) 로 특정 인스턴스를 닫는 안도 봤지만, 화면은 보통 자기 버튼으로 닫혀 필요 없다고 봄 (사용자 10-19). 밖에서 특정 인스턴스를 닫아야 하는 화면이 생기면 그때 핸들을 돌려준다 (`void` → 값 반환은 기존 호출을 깨지 않는다). 열린 동안 바뀌는 값은 여는 쪽이 아니라 VM 이 이벤트 · Model 을 구독한다.
- `void` 와 `UniTask` 를 반환 타입만 달리 오버로딩할 수 없어 (CS0111) 이름을 나눈다 — C# 관례대로 `Async`.
- VM 타입 하나에 프리팹 하나. 같은 VM 을 쓰는 프리팹이 둘이면 구분할 수 없다 (시작할 때 경고).
- **닫힌 화면은 파괴하지 않고 꺼 둔 채 다시 쓴다 (사용자 10-19, 5.3).** 그래서:
  - 지난번 setup 의 값 · 콜백은 **VM 의 `ResetForOpen` 이 열 때마다 되돌린다** (2.5, 사용자 10-21 — 10-19 의 "Setup 이 매번 모든 값을 다시 넣는다" 규칙을 바꿈). `Setup` 은 이번에 넣을 것만 넣으면 된다.
  - 다시 켠 화면은 Canvas 의 마지막 자식이 아니므로 열 때 `SetAsLastSibling()` — 안 하면 나중에 처음 만든 팝업 아래에 그려진다.
  - View 에 남은 상태 (스크롤 · 토글 · 트윈) 는 `Initialize` · `Bind` 가 `Awake` 에서 한 번뿐이라, 되돌릴 것은 그 화면의 `PlayOpen` 에서.
  - 숨은 동안에도 VM 이 살아 이벤트를 받는다. VM `Dispose` 는 화면이 파괴될 때 (씬 끝).
  - 열림 / 쉼은 `activeSelf` 로 본다 — 그래서 매니저에 닫힘 알림 (`OnClose`) 이 필요 없다. 닫기 연출 중인 화면은 켜져 있으니 "열림" 으로 보고 새 열기에 고르지 않는다.
- 닫기는 **VM 타입의 맨 위 인스턴스** (사용자 10-19). 같은 화면이 여러 개 떠 있을 때 그중 특정 하나를 밖에서 닫는 길은 아직 없다 — 화면은 보통 자기 버튼으로 닫힌다.
- `ScreenView` 의 열기 · 닫기도 같은 규칙으로 `OpenAsync` · `CloseAsync` (사용자 10-19, 5.0).
- 매니저가 프리팹의 VM 타입을 얻는 길: 처음 열 때 프리팹 목록에서 `prefab is ScreenView<TVM>` 인 것을 찾는다 (사용자 10-20 — 10-19 의 `GetVMType()` 안 대신). `setup` 에는 그 인스턴스의 `vm` (`internal`) 을 넘긴다.
- **자료구조 (사용자 10-20):** VM 타입 → (프리팹 · 인스턴스 목록). **목록은 보관만** (순서 없음), **열린 순서는 계층 (형제 순서)** 이 든다 — 열 때 `SetAsLastSibling`, 닫기는 그 타입에서 켜진 것 중 `GetSiblingIndex()` 가 가장 큰 것. 닫기 연출 중인 화면 (`ScreenView.IsClosing`, 사용자 10-20 — 매니저의 목록 `_closingViewBuffer` 대신) 은 다음 닫기에서 고르지 않는다. `IsClosing` 은 `CloseAsync` 가 켜고, 끝나서 끄거나 새 열기 · 닫기의 `BeginTransition` 이 푼다 — 화면이 자기 버튼으로 닫는 중이어도 매니저가 안다.
  안 고른 것: 목록에서 `Remove` → `Add` 로 순서를 맞추기 — 순서가 목록과 계층 두 곳이라 매니저 밖에서 형제 순서를 바꾸면 어긋나고, 타입별 목록으론 타입 상관없는 "맨 위 닫기" 를 못 한다. 단점: 모든 화면이 같은 부모 (`rootCanvas`) 바로 아래여야 한다 — 부모가 갈라지면 (레이어 Canvas 를 나누면) 열린 화면 전체 스택이 따로 필요하다.- **Canvas (사용자 10-20):** 씬에 루트 Canvas 하나 (`CanvasScaler` · 렌더 모드), 각 화면 프리팹 루트에 `Canvas` (중첩, Override Sorting 끔) — 한 화면의 변경이 다른 화면의 다시 그리기를 부르지 않게. 클릭을 받는 화면은 루트에 `GraphicRaycaster` 도 (Graphic 은 가장 가까운 Canvas 에 등록돼 루트의 Raycaster 가 못 본다). 숨기기는 **`SetActive`** 그대로 (사용자 10-20) — Element 가 `OnEnable` 에서 스크롤 같은 상태를 되돌릴 수 있고, 숨은 동안 스크립트가 멈춘다. `Canvas.enabled` 는 켜고 끄기가 싸지만 숨은 동안 `Update` 가 돌고 열림 판단 (`activeSelf`) 을 다 바꿔야 해서 안 씀 — 초당 여러 번 켜고 끄는 화면 (툴팁 등) 이 생기면 그때 다시 본다.
- 같은 VM 타입의 프리팹이 둘 이상이면 매니저 `Awake` 에서 경고 (먼저 나온 것만 쓰인다, Claude 10-20).
- 같은 날 거친 안 (되돌림): **DevLib `RegistrySystem` 항목 `ScreenEntry` (프리팹 + SO + `CreateVM`) + 생성된 enum 으로 열기.** enum 키는 인스펙터 · 이벤트 데이터에 담기 쉽지만, 패키지 밖에 Registry 연결 코드가 생기고 `Instantiate` 뒤 VM 넣기 · 프리팹과 VM 타입 짝 확인이 새로 필요했다. VM 타입 키는 패키지만으로 끝나고 이름 바꾸기 · 사용처 찾기가 IDE 로 된다. enum 으로 여는 것이 필요해지면 프로젝트에서 enum → `Open<TVM>()` 매핑만 얹는다. 그보다 앞의 처음 안 (VM 자체를 Registry 에 넣고 VM 이 프리팹을 든다) 은 VM → View 의존 · 항목마다 VM 하나 · 생성자 매개변수 불가 · 패키지의 DevLib 의존 때문에 뺐다.

### 5.2 `PopupView`

```csharp
public class SettingView : AbstractPopupView<SettingVM> { … }   // window · closeButton · backgroundDim 은 바탕의 [SerializeField]
public class SettingVM : AbstractPopupVM { … }                   // 팝업 VM 은 AbstractPopupVM 을 상속 (제약)
```

- **사용자 10-21 구현 (이름은 `AbstractPopupView<T>`, `Templates/Popup/`):** `TransitionElement window` (10-21: `WindowElement` → 바탕 타입, 옆 패널 등도 들어간다) · `ButtonElement closeButton` · `BackgroundDimElement backgroundDim` 을 `[SerializeField]` 로 든다 ("이름으로 채우기" 로 채워짐). 닫기 버튼 · 배경은 `[Nullable]` — 없어도 된다 (`ConfirmPopupView` 의 `cancelBtn` 도). `Initialize` 에서 루트 `CanvasGroup`, `Bind` 에서 VM 의 닫기 요청을 구독하고 버튼 · 배경을 VM 의 `CloseCmd` 에 묶는다.
  `PlayOpen` · `PlayClose` 는 창 · 배경 연출을 `UniTask.WhenAll` 로 함께 기다리고 (배경이 없으면 창만), 열고 닫는 동안 `SetClickable(false)` (`interactable` · `blocksRaycasts` 둘 다 — 포인터를 직접 받는 Element 까지 막으려고). 열기가 취소되면 다시 켜지 않는다.
- **닫기는 VM 이 요청하고 View 가 닫는다 (사용자 10-21, B — 바탕 `AbstractPopupVM` 의 이름 · `CloseCmd` 는 사용자, 나머지 구현은 Claude 에 맡김):**

  ```csharp
  public abstract class AbstractPopupVM : AbstractViewModel
  {
      public IReadOnlySignal<bool> CloseRequested { get; }  // AbstractPopupView 가 받아 CloseAsync (값은 안 씀 — 인자 없는 Signal 이 없다)
      public Command CloseCmd { get; }                      // 닫기 버튼 · 배경 → Dismiss
      protected void RequestClose();
      protected virtual void Dismiss();                     // 확인 없이 닫힐 때. 기본 = RequestClose, 취소 콜백이 있으면 덮어씀
      protected override void ResetForOpen();               // CloseCmd.Reset — 덮어쓰면 base 를 부른다
  }
  ```

  - VM 이 `RequestClose()` 를 부를 때만 닫힌다 — **눌린 뒤 VM 이 결과를 보고** 닫을지 정한다 (검증 실패면 안 부름 · 저장이 끝난 뒤 부름). 누르기 전에 막을 것은 `SetEnabled(false)`.
  - `ConfirmPopupVM`: 확인 = `onConfirm` → `RequestClose`, 취소 · 닫기 버튼 · 배경 = `Dismiss` (`onCancel` → 닫기). 콜백이 먼저 — 콜백이 같은 팝업을 또 열면 지금 것이 아직 켜져 있어 매니저가 새 인스턴스를 만든다. Command 는 생성자에서 VM 메서드로 만들고 콜백은 필드 — 닫기 버튼 (`CloseCmd`) 도 `onCancel` 을 불러야 해서 콜백을 Command 의 `Action` 에 넣지 않는다.
  - 구독은 풀지 않는다 — VM 과 View 는 같이 버려진다. 이미 닫힌 팝업의 요청은 `CloseAsync` 가 무시한다.
  - 단점: 모든 팝업 VM 이 `AbstractPopupVM` 을 상속해야 한다 (제약). 닫기는 VM 이 직접 불러야 해서 빠뜨리면 안 닫힌다 (눌러 보면 바로 보인다).
  - 안 고른 안: **Element 를 N:N 으로** (사용자 — `Bind(params Command[])` 에 Active 는 AND, `confirmBtn.Bind(vm.ConfirmCmd, CloseCmd)`) — 팝업 밖에서도 쓸 수 있지만 닫을지를 View 가 정해 "눌러 보고 결과에 따라 닫기" 를 못 하고, Element 6개의 바탕 규칙 (다시 묶으면 바뀜 — 목록 재사용이 기댄다) 을 바꾼다. **A. View 가 Command 를 감싸 실행 뒤 닫기** — 같은 한계에 `Active` 를 따로 이어야 한다. C. VM 이 매니저를 들고 닫기 · D. 여는 쪽이 콜백에서 닫기 · E. 버튼에 "부모 팝업 닫기" 컴포넌트 — 의존 방향 · 잊기 쉬움 · 프리팹에 숨은 연결.
- 하위 팝업이 `Initialize` · `Bind(vm)` 를 덮으면 **`base` 를 불러야 한다** (안 부르면 닫기 연결 · CanvasGroup 이 빠진다). 빼먹을 수 없게 하는 안 (`private protected` 틀 단계) 은 Claude 가 냈고 사용자는 지금 방식을 골랐다.
- 스케치 (10-14) 의 "닫힘 완료 알림" 은 `await CloseAsync()` 로 대신 (10-19). `SetUpdate(true)` 는 Element 가 한다.

### 5.3 같은 화면 여러 개

`ScreenManager` 하나가 열기 요청을 받아 쉬는 인스턴스를 고르고, 없으면 만든다. **닫힌 인스턴스는 꺼 둔 채 다시 쓴다** (사용자 10-19 — 처음의 "열 때 `Instantiate`, 닫을 때 `Destroy`" 를 바꿈. 다시 열 때 `Instantiate` · `Awake` 가 다시 돌지 않고, 스스로 닫힌 화면이 쌓이지 않아 닫힘 알림 없이도 정리된다. 대신 지킬 것은 5.1). 따로 풀 (`ObjectPool<T>`) 은 두지 않는다.

## 6. 트윈 (확정)

**DOTween 을 그대로** 쓴다 (`DOFade` · `DOFillAmount` · `DOScale` · `Sequence` · `DOPunchScale`). 시작할 때 `SetLink(gameObject)`, 일시정지 중에 돌 것은 `SetUpdate(true)`. 감싸는 층은 만들지 않는다.

## 7. 패키지

### 7.1 구성

```
MVVM-UGUI/                     (패키지 루트 = 저장소 루트, com.banggae.mvvm-ugui)
 ├ package.json · README.md · AGENTS.md
 ├ Documentation~/  DESIGN.md (이 문서) · ROADMAP.md · HANDOFF-APES.md   (~ 로 끝나는 폴더는 Unity 가 무시)
 ├ Runtime/  (asmdef MVVM.UGUI)          Binding · Core (ViewModel · ScreenView · ItemView · ScreenManager) · Elements · AssistComponent · Sound (UISoundSO) · Templates (PopupView)
 ├ Editor/   (asmdef MVVM.UGUI.Editor)   Element 이름으로 채우기
 └ Tests/    (asmdef MVVM.UGUI.Tests)    EditMode 테스트
```

- namespace `MVVM.UGUI` — 하위는 asmdef 폴더 기준, **폴더마다 namespace** (`NamespaceTests` 가 검사 — Element 갈래 폴더는 3.3) (`MVVM.UGUI.Binding` · `MVVM.UGUI.Core` · `MVVM.UGUI.Elements(.Core)` · `MVVM.UGUI.Editor` · `MVVM.UGUI.Tests`, `Runtime` 은 안 붙인다 — ROADMAP "정한 세부").
- **Unity 6000.3 기준** — 더 새 버전에만 있는 API 는 쓰지 않는다.
- 의존: UGUI (TMP 포함) — `package.json`. **DOTween** 은 선언할 수 없어 프로젝트에 설치되어 있어야 한다.

### 7.2 쓰는 프로젝트

| 프로젝트 | 참조 |
| --- | --- |
| Project_B (사용자 혼자, Unity 6000.6) | `file:../../../MVVM-UGUI` (로컬) + `testables` |
| Apes (팀, Unity 6000.3) | git URL + 태그 (사용자 10-18). 화면은 Apes 에서 만들고, 틀과 Element 는 이 저장소에서 (Element 는 10-20 에 바꿈) — 사용자 PC 에서는 `Packages/` 에 이 저장소 clone 을 내장 패키지로 두고 고친다 (ROADMAP "Apes 에서 고치는 흐름") |

### 7.3 프로젝트와 잇는 곳 (둘 다 선택)

| 연결점 | 패키지가 주는 것 | 프로젝트가 하는 일 | 안 이으면 |
| --- | --- | --- | --- |
| 1. 클릭 소리 | 추상 `UISoundSO` + `AudioSourceUISoundSO` | 자기 사운드 시스템으로 `Play()` 구현한 SO 하나 | 기본 구현 |
| 2. 화면 열기 · 닫기 | `ScreenManager.Open<TVM>(setup)` · `OpenAsync` · 닫기 (5.1) | 자기 이벤트에서 받아 이 메서드를 부르는 컴포넌트 하나 | 직접 호출 |

## 8. 테스트 (EditMode)

- `Bindable`: 같은 값 무시 · 구독 즉시 현재 값 · 새 구독자가 기존 구독자에게 다시 알리지 않음 · 해제 뒤 알림 없음 · `Select` · `Combine` 재계산과 해제.
- `Command`: `Active` false 면 무시, `Action` 으로 갈아 끼움 · 동작 없으면 무시. `BindableList`: 알림 순서 · 인덱스.
- `Reset`: Bindable 은 생성자 값 (객체 초기화 값 아님) · 같으면 안 알림, Command 는 생성자의 동작 · 켜짐, BindableList 는 비울 때만 알림.
- `ScreenManager`: 다시 열 때 setup 전에 `ResetForOpen` (setup 없어도), 지난 콜백 · 값이 남지 않고 되돌리지 않는 값은 남는다.
- 팝업: VM 이 요청하면 닫힘 · 안 하면 열린 채 · 나중에 요청하면 그때 닫힘 · 닫기 버튼은 `Dismiss` 를 거침 · 닫힌 뒤 요청은 무시. `ConfirmPopupVM`: 버튼마다 콜백 뒤 닫기 요청, setup 없이 열어도 버튼이 닫는다.
- `ViewModel`: `Own` 한 것이 `Dispose` 에서 풀린다.
- 이름으로 채우기 (`ElementAutoFillTests`): 이름 비교 · 꺼진 · 깊은 자식 · 채워진 것은 그대로 · 안쪽 View 경계 · 같은 이름 둘 · 타입 틀림 · 부모 클래스 private 필드.
- `Signal`: 같은 값도 매번 · Bind 때 안 부름 · 해제. Element (`ElementsTests`): 값 반영 · 양방향 쓰기와 되먹임 없음 · 다시 묶으면 이전 원본 끊김 · 원본 둘 · 목록 재사용 · 창 · 배경 · 꺼진 Signal 무시.
  트윈 경로는 플레이 중에만 돌아 EditMode 테스트에선 늘 "바로" 경로다 — 트윈은 플레이 모드에서 확인 (ROADMAP UI2).


## 현재 작업 담당 (2026-10-18)

2026-10-18 최신 결정 (10-15 결정을 바꿈): 사용자는 핵심 코드만 직접 작성하고, 에디터 도구와 핵심 설계 문제가 아닌 디버깅은 AI가 한다 (경계는 `AGENTS.md` "현재 작업 담당"). Project_B에서 먼저 검증한다. 공용 코드는 MVVM UGUI 패키지 저장소에서 직접 개발한다. Apes에서 구현한 뒤 복사하는 절차는 사용하지 않는다.
