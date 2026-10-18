# MVVMGUI 설계

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
| `UIParam` 캐스팅 | 실행 중에 터짐 | `ValueElement<T>` — 타입 고정 |
| 이름 Form | "입력 양식" 처럼 읽힘 | **Element** |
| `UIManager : LightSingleton`, 화면끼리 `public static bool` | 싱글톤 · 전역 상태 | 씬마다 일반 컴포넌트 `ScreenManager` |
| `IMultiable` (같은 팝업 N 개가 모두 이벤트를 받고 상태 머신으로 조율) | 복잡 | `ScreenManager` 하나가 받고 인스턴스를 고른다 |

## 1. 층과 의존 방향 (확정)

| 층 | 무엇 | 종류 |
| --- | --- | --- |
| **Model** | 게임 쪽 (전투 · 설정 SO · 이벤트). 틀은 모른다 | 프로젝트 |
| **ViewModel** | 화면 하나의 상태 (`Bindable`) · 버튼 명령 (`Command`) | **순수 C#** (MonoBehaviour 아님), `IDisposable` |
| **View** | 프리팹 루트 MonoBehaviour. ViewModel 을 만들고 Element 와 연결. 특수 연출만 코드로 | `ScreenView<TViewModel>` (partial) |
| **Element** | 재사용 위젯. 한 타입의 값을 보여주거나 받는다 | MonoBehaviour |

```
View ──안다──▶ ViewModel ──▶ (프로젝트의 이벤트 시스템) ◀── Model
                    └──── 설정 SO · 읽기 전용 상태 (필요할 때만, 한 방향) ────┘
```

- 참조는 **V → VM → M 한 방향**. 위로 올라오는 것은 이벤트 (보내는 쪽이 받는 쪽을 모른다). M 이 VM 을 알아야 하는 경우는 없다 — UI 로 값을 바꾸는 설정창도 VM → M (1.2).
- ViewModel 은 View · Element 를 모른다. Element 는 `Bindable` · `Command` 만 안다.
- ViewModel 이 순수 C# 인 이유: `new` 로 만들어 EditMode 테스트가 바로 되고, 프리팹 컴포넌트가 View 하나뿐. `OnDestroy` 가 없으므로 View 가 대신 `Dispose` (2.6).

### 1.1 처음 값

이벤트는 바뀐 것만 알려준다. 화면이 만들어질 때의 값은 — 기본은 **시작 이벤트에 담아** 받고, 중간에 여는 화면만 **읽기 전용 상태나 SO 를 생성자로** 받아 읽는다.

### 1.2 UI 로 값을 바꾸는 화면 (설정창 예)

```csharp
public class SettingsViewModel : ViewModel
{
    public readonly Bindable<float> MasterVolume;

    public SettingsViewModel(SettingsSO settings)                                  // Model = 프로젝트의 설정 SO
    {
        MasterVolume = new Bindable<float>(settings.MasterVolume);                  // 처음 값
        MasterVolume.Subscribe(v => settings.SetMasterVolume(v), notifyNow: false);  // VM → M (자기 Bindable → 해제 불필요)
        Own(settings.MasterVolumeChanged.Subscribe(v => MasterVolume.Value = v));    // 다른 곳에서 바뀌면 따라감 (SO 는 오래 산다 → Own)
    }
}
```

적용 (AudioMixer · 해상도) 과 저장은 Model 쪽이 한다 — 설정창이 열린 적 없어도 게임 시작 때 적용된다.

## 2. 바인딩 (순수 C#)

### 2.1 `Bindable<T>` · `IReadOnlyBindable<T>` (확정)

```csharp
public interface IReadOnlyBindable<T>
{
    T Value { get; }
    IDisposable Subscribe(Action<T> onChanged, bool notifyNow = true);   // 기본: 구독 즉시 현재 값을 한 번 넘김
}

public class Bindable<T> : IReadOnlyBindable<T>
{
    public Bindable(T initial = default);
    public T Value { get; set; }          // 같은 값이면 무시 (EqualityComparer<T>.Default), 다르면 구독자 전부 호출
    public IDisposable Subscribe(Action<T> onChanged, bool notifyNow = true);
}
```

- **구독하는 길은 `Subscribe` 하나.** `Changed` 이벤트를 밖에 열지 않는다 (처음 값 놓침 · 해제 잊음 방지). 내부 도우미도 `Subscribe` 를 쓴다.
- 구독 즉시 현재 값 → 화면을 처음 열 때 "전부 갱신" 이 필요 없다.
- 참조 타입의 내부만 바뀌면 알 수 없다 → 목록은 `BindableList<T>`, 그 밖에는 새 값을 넣는다.
- 이름 구분: `Subscribe` (Bindable 의 것) ≠ `Bind` (Element 의 것, 안에서 `Subscribe(Apply)`).

**노출 규칙 (확정)** — 값 하나에 private + public 두 줄을 쓰는 반복을 피한다:

| 종류 | 노출 타입 |
| --- | --- |
| 파생 값 (`Select` · `Combine` 결과) | `IReadOnlyBindable<T>` |
| 원본 상태 | `public readonly Bindable<T>` |
| 양방향 입력 (슬라이더 · 토글) | `Bindable<T>` |

### 2.2 파생 값 (확정)

```csharp
// 정적 클래스 Bindable. Combine 은 CombineBindable<TR, T1, T2> (IReadOnlyBindable<TR> + IDisposable) 를 돌려준다
HpRatio = Bindable.Combine(Hp, MaxHp, (h, m) => (float)h / m);   // 원본이 VM 자신의 값 → 끊지 않아도 된다
HpText  = Bindable.Combine(Hp, MaxHp, (h, m) => $"{h}/{m}");

var volumeText = Bindable.Combine(settings.Volume, settings.Muted, (v, m) => m ? "음소거" : $"{v * 100:0}%");
Own(volumeText);                                                  // 원본이 VM 보다 오래 산다 → 맡긴다
VolumeText = volumeText;

Dead    = Bindable.Select(Hp, h => h <= 0).bindable;               // Select 는 아직 (bindable, unSub) 튜플
```

"원본이 바뀌면 무엇을 갱신할지" 를 **만들 때 한 번** 선언한다. 원본을 바꾸는 이벤트가 늘어도 갱신 코드는 안 는다.
**해제 (사용자 2026-10-18):** 파생 값은 원본 구독을 끊을 수 있어야 한다. 원본이 VM 자신의 값이면 같이 버려지므로 끊지 않아도 되고, VM 보다 오래 사는 원본 (SO · 전역 객체) 이면 `Own` — 안 하면 화면을 닫아도 원본이 파생 값을 붙잡아 계속 다시 계산된다. `Combine` 은 값과 해제가 한 객체 (생성자가 원본을 구독, `Dispose` 로 끊음), `Select` 는 아직 튜플. `Own` 을 잊어도 컴파일은 되므로 오래 사는 원본일 때는 리뷰로 본다.
`[세부]` 인자 수 — 지금 `Select` 1 · `Combine` 2 (3 은 필요할 때). namespace `MVVMGUI.Binding` (ROADMAP "정한 세부").

### 2.3 `Command` (확정)

```csharp
public class Command
{
    public Command(Action command, bool active = true);
    public IReadOnlyBindable<bool> Active { get; }
    public void Execute();                // Active 가 false 면 무시
    public void SetEnabled(bool active);
}
```

인자 없는 입력 (버튼). 값이 있는 입력은 양방향 `Bindable`.

### 2.4 `BindableList<T>`

한 화면 안에서 개수가 바뀌는 목록 (손패 · 상점 칸). `[세부]` 알림 종류 — `Added(index, item)` · `Removed(index)` · `Replaced` · `Reset` 으로 시작.
같은 **화면**을 여러 개 띄우는 것 (5.3) 과는 다른 문제.

### 2.5 `ViewModel` 기반 (확정 — `Own` 만, `Listen` 같은 이벤트 구독 도우미는 넣지 않는다)

```csharp
public abstract class ViewModel : IDisposable
{
    protected void Own(IDisposable d);      // 오래 사는 것의 구독을 맡긴다
    public virtual void Dispose();          // 맡긴 것 전부 해제
}
```

틀은 이벤트 시스템을 모른다. 프로젝트의 이벤트 구독은 그 프로젝트의 ViewModel 이 직접 하고 `Dispose` 에서 푼다.

**해제가 필요한 것 — VM 보다 오래 사는 것을 구독할 때만:**

| 구독 대상 | 끊어야 하나 | 쓰는 것 |
| --- | --- | --- |
| VM 자신의 Bindable · 파생 값 | 아니요 (같이 버려진다) | 그냥 `Subscribe` |
| 프로젝트의 이벤트 채널 | **예** | 구독 + `Dispose` 에서 해제 (프로젝트 코드) |
| SO 의 변경 알림 · 전역 객체 | **예** | `Own(...Subscribe(...))` |

### 2.6 수명 (확정)

- **View 에 bag 이 없다.** Element 가 자기 연결을 들고 있다가 자기 `OnDestroy` 에서 끊고, ViewModel 은 View 가 `OnDestroy` 에서 `Dispose`.
- 그래서 **Element 의 `Bind` 는 `void`**. 끊을 일이 있으면 `Unbind()`, 다시 묶으면 `Bind` 가 이전 연결을 끊는다.
- `Bindable.Subscribe` 는 `IDisposable` 을 돌려준다 (Element 내부 · `Combine` · `Own` 이 쓴다).
- **`Dispose` 는 View 가 파괴될 때 한 번** (씬이 바뀔 때 · 여러 개 뜨는 팝업을 닫을 때). **닫힌 (비활성) 화면은 Dispose 하지 않는다** — 계속 이벤트를 받아 값이 최신, 다시 열면 바로 맞다.

## 3. Element

### 3.1 기반 (확정)

```csharp
public abstract class ValueElement<T> : Element
{
    private IDisposable _binding;

    public void Bind(IReadOnlyBindable<T> source)
    {
        Unbind();                             // 다시 묶으면 이전 연결은 끊는다
        _binding = source.Subscribe(Apply);   // 구독 즉시 현재 값으로 Apply
    }

    public void Unbind() { _binding?.Dispose(); _binding = null; }

    protected abstract void Apply(T value);   // 각 Element 는 이것만 구현
    protected virtual void OnDestroy() => Unbind();
}
```

- `Bind` 는 `IReadOnlyBindable<T>` 를 받는다 → `Bindable` · 파생 값 둘 다 넘길 수 있다. 양방향 Element 는 `Bindable<T>` 를 받는 오버로드.
- **피할 이름**: `TextElement` · `BindableElement` — `UnityEngine.UIElements` 에 이미 있다.
- 필요한 컴포넌트는 `[SerializeField]` 로 받고 **`Reset()` 에서 같은 오브젝트의 것을 채운다** (확정).

### 3.2 양방향 (확정)

```csharp
private void OnSlider(float v) => _target.Value = v;                          // 조작 → ViewModel (알림이 나간다 — 의도)
protected override void Apply(float v) => slider.SetValueWithoutNotify(v);   // ViewModel → 화면
```

- 조작하면 알림이 나가 같은 값을 쓰는 다른 곳이 따라온다. 되먹임은 ① `SetValueWithoutNotify` ② Bindable 같은 값 무시 두 겹으로 끊긴다.
- 범위 제한 · 반올림은 **ViewModel 이 값을 넣을 때** (위젯이 고쳐 받으면 화면과 Bindable 이 어긋난다).

### 3.3 목록

| Element | 묶는 값 | 비고 |
| --- | --- | --- |
| `Element` (추상) | — | 값 없이 생성 속성으로 꺼내기만 하는 것도 이것 (특수 연출용) |
| `LabelElement` (TMP) | `string` | 원래 글에 `{0}` 이 있으면 포맷으로. `[세부]` `int` 편의 오버로드 |
| `GaugeElement` (Image fill) | `float` 0~1 | 트윈 |
| `CooldownElement` | `float` 0~1 | `[세부]` |
| `ButtonElement` | `Command` | `Active` false 면 잠김. 클릭 소리 (3.5) |
| `SliderElement` | `Bindable<float>` 양방향 | |
| `ToggleElement` | `Bindable<bool>` 양방향 | |
| `SwitchElement` | `int` | 자식 중 하나만 켠다 |
| `VisibleElement` | `bool` | 켜고 끄기 (+ 페이드) `[세부]` |
| `ListElement` | `BindableList<T>` | 항목 프리팹 반복, 항목마다 `ItemView<TItemVM>` |

### 3.4 꺼진 오브젝트 · 트윈 중 (확정)

- 꺼진 오브젝트에도 구독은 살아 있다. 트윈이 있는 Element 는 **꺼져 있으면 트윈 없이 즉시 적용**.
- 트윈 중 새 값 → 이전 트윈을 `Kill`, 지금 위치에서 새 목표로.

### 3.5 클릭 소리 (확정)

```csharp
public abstract class UISoundSO : ScriptableObject { public abstract void Play(); }
public class AudioSourceUISoundSO : UISoundSO { … }      // 패키지 기본 구현 (AudioClip 하나)
```

- `ButtonElement` 는 `[SerializeField] UISoundSO clickSound` 를 들고 눌리면 `Play()` 만. 화면 코드에 소리가 없다.
- 프로젝트가 자기 사운드 시스템으로 구현한 SO 를 만들어 쓰면 된다 (연결점 1, 7.3).
- `[세부]` `Reset()` 이 기본 SO 를 채우는 방법, 잠긴 버튼 소리, Toggle · Slider 소리.

## 4. View — 생성된 속성 + 손 연결

### 4.1 Element 속성 생성 — partial (확정)

```csharp
// BattleHudView.Elements.g.cs — 생성됨, 손대지 않음
partial class BattleHudView
{
    [SerializeField, HideInInspector]
    private Element[] generatedElements;          // 프리팹에 참조가 저장되는 곳 (사용자 10-18: ScreenView 가 아니라 생성된 파일에)

    private TextViewElement HpText => (TextViewElement)generatedElements[0];
    private ButtonElement PauseButton => (ButtonElement)generatedElements[1];
}
```

- 대상: View 오브젝트의 **자식 (꺼진 것 포함) 에 붙은 `Element` 컴포넌트**, 계층 순서. View 자신에 붙은 Element 는 빼고, 안쪽에 다른 `ScreenView` 가 붙은 오브젝트는 그 아래까지 그 View 의 것이라 뺀다.
- **배열은 생성된 파일에 있다** (사용자 10-18 — ScreenView 에 두면 헷갈린다). 그래서 **처음 한 번은 두 단계**: 파일 쓰기 → 컴파일 → 배열 채우기. 생성기가 기다릴 대상을 `SessionState` 에 적어 두고 `[DidReloadScripts]` 에서 이어 채운다 (프리팹은 디스크의 것을 `LoadPrefabContents` → `SaveAsPrefabAsset`). 그 뒤에는 필드가 있으니 한 번에 끝난다.
  배열 이름 `generatedElements` 를 바꾸면 프리팹의 참조가 끊긴다.
- 속성 이름 = 오브젝트 이름에서 C# 에 못 쓰는 글자를 뺀 것 (숫자로 시작하면 `_`, 키워드면 `@`). 이름이 비거나 겹치면 **파일을 건드리지 않고 멈추고** 어느 오브젝트인지 알린다.
- View 클래스에는 `partial` 만 붙인다 (없으면 멈추고 알림). 생성 파일은 View 스크립트 옆 `<View>.Elements.g.cs`, 타입은 `global::` 전체 이름이라 `using` 이 필요 없다. 제네릭 · 중첩 클래스 View 는 생성하지 않는다.
- **언제 생성 (확정)**: 프리팹 모드에서 저장할 때 (`PrefabStage.prefabSaving`) · GameObject 인스펙터 머리의 **"MVVMGUI · Element 속성 생성"** 버튼 · `Tools/MVVMGUI/선택한 View 의 Element 속성 생성`. 내용이 같으면 파일을 다시 쓰지 않는다. (자동 연결이 없어져 "스크립트 컴파일이 끝날 때" 다시 생성은 뺐다 — 사용자 10-18.)
- 어긋남: 저장할 때마다 다시 만들므로 배열과 속성은 맞춰진다. 프리팹을 저장하지 않은 채 Element 종류만 바꾸면 실행 중 형변환 예외가 난다 — 그때는 버튼으로 다시 생성. (처음 설계의 `OnValidate` · `Awake` 검사는 생성된 파일이 하위 View 의 같은 이름 메서드와 부딪혀 넣지 않았다 `[세부]`.)

### 4.2 자동 연결은 없다 (사용자 2026-10-18 — 처음 설계의 `AutoBind` 를 뺐다)

- 처음엔 Element 이름 == ViewModel 멤버 이름이면 생성기가 `AutoBind` 에 연결을 써 주는 안이었다.
- 뺀 이유: 자동으로 `A ↔ a` 가 묶인 뒤 손으로 `B ↔ a` 를 묶으면, 다른 Element 라 A 의 연결이 끊기지 않아 **A · B 가 둘 다 `a` 를 본다**. 막으려면 이름을 바꾸거나 `Unbind` · 제외 표시가 필요해지고, 연결이 코드에 다 드러나지 않는다.
- 그래서 **연결은 전부 View 의 `Bind` 에 손으로 쓴다.** 생성기는 Element 를 꺼내는 속성만 만든다 (4.1).
- ViewModel 쪽 선언도 생성하지 않는다 (그대로).

### 4.3 손 연결

```csharp
public partial class BattleHudView : ScreenView<BattleHudViewModel>
{
    [SerializeField] private SomeChannelSO battleChannel;

    protected override BattleHudViewModel CreateViewModel() => new(battleChannel);

    protected override void Bind(BattleHudViewModel vm)
    {
        HpText.Bind(vm.HpText);
        HpGauge.Bind(vm.HpRatio);
        PauseButton.Bind(vm.PauseButton);
    }
}
```

- `ScreenView` 는 `Awake` 에서 `CreateViewModel` → `Bind`, `OnDestroy` 에서 ViewModel `Dispose`. (지금 코드 이름은 `AbstractView<T>` · `CreateVM` — 이름과 모양은 사용자가 정하는 중 `[?]`.)
- 같은 Element 를 다시 묶으면 이전 연결은 끊긴다 (Element 의 `Bind`).
- ViewModel 이 필요한 SO 는 View 가 `[SerializeField]` 로 들고 생성자로 넘긴다.

## 5. 화면 열기 · 닫기 (확정)

### 5.0 `ScreenView` 의 열기 · 닫기 (사용자 10-18, 비동기는 UniTask)

**두 층 (사용자 10-18, 나누기는 Claude 에 맡김):**
- `ScreenView` (제네릭 아님) — 열기 · 닫기 · `OnOpen` / `OnClose` · `PlayOpen` / `PlayClose` · 전환 취소. **`ScreenManager` · 생성기는 이것만 본다** (목록 · 스택 · `GetComponent<ScreenView>()`).
- `ScreenView<T> : ScreenView` (T = ViewModel) — `Awake` 에서 `CreateVM` → `Initialize` → `Bind(T)` → 꺼 두기, 파괴될 때 VM `Dispose`. 화면은 이걸 상속해 형변환 없이 자기 VM 을 받는다.
- 나눈 이유: `ScreenView<A>` 와 `ScreenView<B>` 는 C# 에서 관계없는 타입이라 "어떤 화면이든" 을 가리킬 공통 타입이 없었다.

```csharp
public async UniTask Open();    // SetActive(true) → await PlayOpen(token) → OnOpen
public async UniTask Close();   // await PlayClose(token) → SetActive(false) → OnClose (이미 꺼져 있으면 아무것도 안 함)
protected virtual UniTask PlayOpen(CancellationToken token);    // 하위 View (PopupView) 가 트윈 · 연출을 넣는 자리, 기본은 바로 끝
protected virtual UniTask PlayClose(CancellationToken token);
```

- `Awake` 끝에서 꺼 둔다 — 열기 전까지 숨어 있다.
- 연출 중에 다른 `Open` · `Close` 가 오면 이전 것은 **취소** (`OnOpen` · `OnClose` 없이 끝난다). 파괴되면 (`destroyCancellationToken`) 도 취소.
  하위 View 는 받은 `token` 을 트윈 대기에 넘겨야 취소가 먹힌다.
- 기다릴 필요가 없으면 `view.Open().Forget();`. `ScreenManager` 는 `await` 로 순서를 맞출 수 있다 (닫힌 뒤 다음 열기 등).

### 5.1 `ScreenManager`

- 씬에 두는 **일반 컴포넌트** (싱글톤 아님). 자기 씬의 화면 프리팹 목록을 `[SerializeField]` 로 들고 시작할 때 만든다.
- **메서드만** — `Open<T>()` · `Close()`. 이벤트로 여는 것은 프로젝트가 잇는다 (연결점 2, 7.3).
- 팝업은 **스택**. "이 화면이 열려 있으면 저 요청 무시" 같은 판단은 여기서 — 화면끼리는 서로 모른다.

### 5.2 `PopupView`

```csharp
public partial class SettingView : PopupView<SettingViewModel>
{
    protected override Element Window => PopupWindow;          // 생성된 속성
    protected override Element Background => BackgroundDim;
    protected override Element[] CloseButtons => new Element[] { CloseBtn, BackgroundDim };
}
```

창 크기 · 배경 페이드 · `CanvasGroup` 입력 막기 · 닫힘 완료 알림. 일시정지 중에도 돌아야 하므로 트윈은 `SetUpdate(true)`.

### 5.3 같은 화면 여러 개

`ScreenManager` 하나가 열기 요청을 받아 쉬는 인스턴스를 고르고, 없으면 만든다. 처음엔 **열 때 `Instantiate`, 닫을 때 `Destroy`**. 자주 뜨는 것이 생기면 Unity 내장 `UnityEngine.Pool.ObjectPool<T>` (부모 = 그 씬 Canvas).

## 6. 트윈 (확정)

**DOTween 을 그대로** 쓴다 (`DOFade` · `DOFillAmount` · `DOScale` · `Sequence` · `DOPunchScale`). 시작할 때 `SetLink(gameObject)`, 일시정지 중에 돌 것은 `SetUpdate(true)`. 감싸는 층은 만들지 않는다.

## 7. 패키지

### 7.1 구성

```
MVVMGUI/                     (패키지 루트 = 저장소 루트, com.banggae.mvvmgui)
 ├ package.json · README.md · AGENTS.md
 ├ Documentation~/  DESIGN.md (이 문서) · ROADMAP.md · HANDOFF-APES.md   (~ 로 끝나는 폴더는 Unity 가 무시)
 ├ Runtime/  (asmdef MVVMGUI)          Binding · ViewModel · Elements · ScreenView · PopupView · ScreenManager · UISoundSO
 ├ Editor/   (asmdef MVVMGUI.Editor)   Element 속성 생성기
 └ Tests/    (asmdef MVVMGUI.Tests)    EditMode 테스트
```

- namespace `MVVMGUI` — 하위는 asmdef 폴더 기준 (`MVVMGUI.Binding` · `MVVMGUI.Core` · `MVVMGUI.Elements(.Core)` · `MVVMGUI.Editor` · `MVVMGUI.Tests`, `Runtime` 은 안 붙인다 — ROADMAP "정한 세부").
- **Unity 6000.3 기준** — 더 새 버전에만 있는 API 는 쓰지 않는다.
- 의존: UGUI (TMP 포함) — `package.json`. **DOTween** 은 선언할 수 없어 프로젝트에 설치되어 있어야 한다.

### 7.2 쓰는 프로젝트

| 프로젝트 | 참조 |
| --- | --- |
| Project_B (사용자 혼자, Unity 6000.6) | `file:../../../MVVMGUI` (로컬) + `testables` |
| Apes (팀, Unity 6000.3) | git URL + 태그 (사용자 10-18). Element 와 화면은 Apes 에서 함께 만들고, 틀 (ScreenView · 생성기 · ScreenManager · PopupView) 은 이 저장소에서 — 사용자 PC 에서는 `Packages/` 에 이 저장소 clone 을 내장 패키지로 두고 고친다 (ROADMAP "Apes 에서 고치는 흐름") |

### 7.3 프로젝트와 잇는 곳 (둘 다 선택)

| 연결점 | 패키지가 주는 것 | 프로젝트가 하는 일 | 안 이으면 |
| --- | --- | --- | --- |
| 1. 클릭 소리 | 추상 `UISoundSO` + `AudioSourceUISoundSO` | 자기 사운드 시스템으로 `Play()` 구현한 SO 하나 | 기본 구현 |
| 2. 화면 열기 · 닫기 | `ScreenManager.Open<T>()` · `Close()` | 자기 이벤트에서 받아 이 메서드를 부르는 컴포넌트 하나 | 직접 호출 |

## 8. 테스트 (EditMode)

- `Bindable`: 같은 값 무시 · 구독 즉시 현재 값 · `notifyNow: false` · 해제 뒤 알림 없음 · `Combine` 재계산.
- `Command`: `Active` false 면 무시. `BindableList`: 알림 순서 · 인덱스.
- `ViewModel`: `Own` 한 것이 `Dispose` 에서 풀린다.
- 생성기: 같은 이름 · 중첩 View 경계 · 생성 코드의 타입 = 실제 컴포넌트 · 내용이 같으면 다시 쓰지 않음.


## 현재 작업 담당 (2026-10-18)

2026-10-18 최신 결정 (10-15 결정을 바꿈): 사용자는 핵심 코드만 직접 작성하고, 에디터 도구와 핵심 설계 문제가 아닌 디버깅은 AI가 한다 (경계는 `AGENTS.md` "현재 작업 담당"). Project_B에서 먼저 검증한다. 공용 코드는 MVVMGUI 패키지 저장소에서 직접 개발한다. Apes에서 구현한 뒤 복사하는 절차는 사용하지 않는다.
