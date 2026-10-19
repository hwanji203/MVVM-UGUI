# MVVMGUI 진행 순서

> 설계는 `DESIGN.md`. 단계마다 **컴파일 → 테스트 → 기록**. 단계 범위를 넘는 기능은 넣지 않는다.
> 2026-10-18 최신 결정 (10-15 결정을 바꿈): 사용자는 핵심 코드만 직접 작성하고, 에디터 도구와 핵심 설계 문제가 아닌 디버깅은 AI가 한다 (경계는 `AGENTS.md` "현재 작업 담당"). Project_B에서 먼저 검증한다. 공용 코드는 MVVMGUI 패키지 저장소에서 직접 개발한다. Apes에서 구현한 뒤 복사하는 절차는 사용하지 않는다.
> **같은 날 이어서 (사용자): 틀 (UI0 테스트 · UI3 · UI4) 은 여기서 만들고 Project_B 로 확인한다. Element 추가 (UI2) 와 실제 화면 (UI6) 은 Apes 에서 화면과 함께. UI5 (목록) 는 `[?]`. Apes 에는 Git URL + 태그로 적용한다.** 코드는 이 저장소 `Runtime/` (Project_B DevLib 사본에서 옮김).

## 현황

| 단계 | 상태 | 내용 | 끝났다고 보는 기준 |
| --- | --- | --- | --- |
| P0 | 완료 (10-15, Claude) | 저장소 · `package.json` · asmdef 2개 · README · 이 문서들 | Project_B 에서 패키지 인식, 기존 테스트 통과 |
| **UI0** | 다음 | 바인딩: `Bindable` · `IReadOnlyBindable` · `Select` · `Combine` · `Command` · `BindableList` · `ViewModel` (`Own`) + 테스트 asmdef | DESIGN 8절의 Bindable · Command · BindableList · ViewModel 테스트 통과 |
| UI2 | | 기본 Element: `Element` · `ValueElement` · Label · Gauge · Button (+ `UISoundSO` · `AudioSourceUISoundSO`) · Toggle · Slider · Switch · Visible, `Reset()` 채우기 | 빈 씬에서 각 Element 를 Bindable 에 묶어 값이 바뀌면 화면이 따라감 (수동 확인 방법을 기록) |
| UI3 | (사용자 확인 대기) | `ScreenView<TVM>` (사용자 10-18) + 생성기 (partial `.g.cs` Element 속성 + 숨은 배열 — 자동 연결 없음, Claude 10-18) · 프리팹 저장 때 생성 · 인스펙터 버튼 · 처음 생성은 컴파일 뒤 이어 채움 | 테스트 프리팹으로 생성 결과 확인, 생성기 테스트 통과 — 10-18 Project_B 임시 프리팹으로 확인 (버튼 경로 · 컴파일 뒤 채우기 · 프리팹 모드 저장 때 다시 생성), 생성기 테스트 14개 통과 |
| UI4 | | `ScreenManager` (`Open<T>` · `Close` · 스택 · 여러 개) · `PopupView` | 팝업 열기 · 닫기 · 배경 클릭 닫기 · 두 개 겹쳐 띄우기 |
| UI5 | | `BindableList` 를 쓰는 `ListElement` · `ItemView` | 항목 추가 · 삭제 · 교체가 화면에 반영 |
| UI6 | | 실제 화면 하나로 검증 | Apes: 사용자가 정한 화면 (10-18: Element 와 함께 Apes 에서). Project_B: 전투 HUD 는 틀이 갖춰진 뒤 `[?]` |

어디서 하나 (사용자 10-18): **UI0 테스트 · UI3 · UI4 = 이 저장소 (Project_B 로 확인)**, UI2 Element · UI6 화면 = Apes, UI5 = `[?]`.
UI0 테스트: `Tests/` (asmdef `MVVMGUI.Tests`, Editor 전용, `UNITY_INCLUDE_TESTS`) 25개 — Project_B 에서 통과 (10-18). 실행은 Project_B 의 `ProjectEditor.Testing.TestCli` (eval) 또는 Test Runner.

(UI1 은 "DOTween 가져오기" 였고 Project_B · Apes 모두 이미 있다.)

## 정한 세부

`DESIGN.md` 의 `[세부]` 를 정하면 여기에 한 줄씩 적는다 (날짜 · 누가 · 무엇 · 이유).

| 날짜 | 누가 | 무엇 | 이유 |
| --- | --- | --- | --- |
| 2026-10-18 | 사용자 | 파생 값은 원본 구독을 끊을 수 있어야 한다. 원본이 VM 자신의 값이면 끊지 않아도 되고, VM 보다 오래 사는 원본 (SO · 전역 객체) 이면 `Own` (DESIGN 2.2). 처음엔 `Combine` · `Select` 모두 `(bindable, unSub)` 튜플 → 같은 날 **`Combine` 은 `CombineBindable<TR, T1, T2>` 객체** (`IReadOnlyBindable` + `IDisposable`, 생성자가 구독) 로 바꿈. `Select` 는 아직 튜플 | 오래 사는 원본이 구독 람다로 파생 값을 붙잡아, 화면을 닫아도 남아서 계속 다시 계산된다. 튜플은 `unSub` 을 잊기 쉬워 값과 해제를 한 객체로 |
| 2026-10-18 | 사용자 | 바인딩 하위 namespace = `MVVMGUI.Binding` (`Bindable<T>` · 정적 `Bindable` · `BindableList<T>` · `Command`) | 처음 이름 `…Runtime.Bindable` 은 정적 클래스 `Bindable` 과 같아서, 패키지 안 (`MVVMGUI.Runtime.*`) 코드의 `Bindable.Combine` 이 네임스페이스로 잡혀 CS0234 (Roslyn 으로 확인) |
| 2026-10-18 | 사용자 | namespace 는 **asmdef 폴더 기준, `Runtime` 없이**: `MVVMGUI` (UnSubscriber · DisposableBag) · `MVVMGUI.Binding` · `MVVMGUI.Core` · `MVVMGUI.Elements` · `MVVMGUI.Elements.Core` · `MVVMGUI.Editor` · `MVVMGUI.Tests` | Rider 는 asmdef 가 있는 폴더 (`Runtime/`) 를 뿌리로 보고 네임스페이스를 기대한다 — `MVVMGUI.Runtime.*` 는 어긋남 경고. 설계 문서도 처음부터 `MVVMGUI.Binding` 꼴 |
| 2026-10-18 | 사용자 | `Command` 생성자의 사용 가능 기본값 = `true` (`new Command(action, active = true)`) | 기본값 `false` 면 `new Command(OnClick)` 이 꺼진 버튼으로 시작한다 |
| 2026-10-18 | 사용자 | **자동 연결 (`AutoBind`) 은 없앤다** — 생성기는 Element 속성만, 연결은 전부 View 의 `Bind` 에 손으로 (DESIGN 4.2). 생성 시점도 프리팹 저장 + 버튼만 (컴파일 뒤 다시 생성 뺌) | 자동으로 `A ↔ a` 가 묶인 뒤 손으로 `B ↔ a` 를 묶으면 A 의 연결이 안 끊겨 둘 다 `a` 를 본다. 막으려면 이름 바꾸기 · `Unbind` · 제외 표시가 필요하고 연결이 코드에 다 안 드러난다 |
| 2026-10-18 | 사용자 (구현은 Claude 에 맡김) | **Element 의 구독은 바탕 `Element` 의 `DisposableBag Bindings` 하나에 담는다.** 하위 Element 는 `Bind` 에서 먼저 `Unbind()` (= 바구니 비우기) 하고 담는다. `Element.OnDestroy` 는 `protected virtual` 이고 `Unbind()` 를 부른다. `CommandElement` 는 `Unbind` 를 덮어 `CallCommand` 도 비운다. `AbstractViewModel` 의 `Own` 도 같은 `DisposableBag` 을 쓴다 | Element 마다 해제 핸들 · `?.Dispose()` · `= null` 을 반복했고 빠뜨리기 쉬웠다 (`ReadOnlyListElement` 의 요소 구독 누락). 설계의 `Unbind()` 도 생긴다. 한 Element 가 서로 다른 원본 둘을 따로 묶게 되면 한쪽 재바인딩이 다른 쪽도 끊으니 그때 다시 본다 |
| 2026-10-18 | 사용자 | 바탕 View 이름 `ScreenView<T>` (`AbstractView` 에서 바꿈) | 설계 문서 이름과 맞춤 |
| 2026-10-18 | 사용자 (나누기는 Claude 에 맡김) | `ScreenView` 를 둘로: 제네릭 아닌 `ScreenView` (열기 · 닫기 · 연출 · 전환 취소) + `ScreenView<T> : ScreenView` (VM 생성 · Bind · Dispose). `ScreenManager` · 생성기는 제네릭 아닌 쪽만 본다 (DESIGN 5.0) | `ScreenView<A>` · `ScreenView<B>` 를 함께 다룰 공통 타입이 없어 `ScreenManager` 의 목록 · 스택 · `Open<T>() where T : ScreenView` 를 쓸 수 없었다. 생성기도 리플렉션으로 `ScreenView<>` 를 찾아야 했다 |
| 2026-10-18 | 사용자 (UniTask 코드는 Claude 에 맡김) | `ScreenView.Open` · `Close` 는 `UniTask` 를 돌려준다. 켜기 / 끄기와 `OnOpen` · `OnClose` 사이에 `PlayOpen` · `PlayClose (CancellationToken)` 연출 자리. 새 열기 · 닫기 · 파괴가 오면 진행 중인 것은 취소 (알림 없음). 패키지는 **UniTask 2.5.11 이상**에 의존 (asmdef 참조, 설치는 프로젝트가 — README) | 팝업 연출이 끝난 뒤에 알리거나 끄려면 기다려야 한다. Apes 가 이미 UniTask 를 쓴다. 2.5.10 은 Unity 6000.2+ 에서 TreeView API 변경으로 에디터 코드가 컴파일되지 않아 2.5.11 로 (Project_B 에서 확인). 대안 Unity `Awaitable` 은 의존성이 없지만 사용자가 UniTask 를 골랐다 |
| 2026-10-18 | 사용자 (구현은 Claude 에 맡김) | `Command` 는 `Active` (읽기 전용 Bindable) · `Execute()` (꺼져 있으면 무시) · `SetEnabled` 를 내준다. `Bind` 가 돌려주던 `(실행 함수, 해제)` 튜플은 없앰 — `CommandElement` 는 `Active` 만 구독하고 눌리면 `Execute()`. 설계의 "밖의 Bindable 을 `canExecute` 로 받기" 는 지금은 안 함 (`SetEnabled` 로 충분, 필요해지면) | 튜플 때문에 Element 가 실행 함수 (`CallCommand`) 를 따로 들고 `Unbind` 에서 비워야 했다 |
| 2026-10-18 | 사용자 (구현은 Claude 에 맡김) | 해제 핸들은 **`System.IDisposable`** — 직접 만든 `MVVMGUI.IDisposable` 삭제. `AbstractViewModel` 도 `IDisposable` (DESIGN 2.5) | 쓰는 쪽 파일에 `using System;` · `using MVVMGUI;` 가 같이 있으면 `IDisposable` 이 모호해 CS0104. `using (...)` 문 · UniTask 같은 다른 라이브러리와도 안 맞았다 |
| 2026-10-18 | 사용자 (구현은 Claude 에 맡김) | Element 의 컴포넌트 참조는 `Reset()` 에서 채운다 (`OnValidate` 의 "비어 있으면 GetComponent" 대신, DESIGN 3.3) | 설계대로 — 붙이는 순간 · 인스펙터 Reset 때 잡는다. 매 검증마다 돌 필요가 없다 |
| 2026-10-18 | 사용자 (구현은 Claude 에 맡김) | `CombineBindable` · `SelectBindable` 은 안에 `Bindable<T>` 를 두고 `Value` · `Bind` 를 맡긴다 (원본 구독은 `DisposableBag` · 필드 하나). 같은 값 비교는 `EqualityComparer<T>.Default` (`Bindable` · `BindableList` 인덱서). `BindableList.Remove` 는 지웠을 때만 알리고 `bool` 을 돌려준다 | 값 저장 · 같은 값 무시 · 알림이 세 클래스에 복사돼 있었다. `Equals(object)` 는 값 타입에서 값을 넣을 때마다 박싱. 없는 항목을 `Remove` 해도 "바뀌었다" 고 알렸다 |
| 2026-10-19 | 사용자 (처음 안: VM 을 Registry 에 넣고 VM 이 프리팹을 듦 → Claude 가 대안 제시 → 사용자 선택) | 화면 목록은 **프로젝트의 Registry 항목 `ScreenEntry` (프리팹 + SO + `CreateVM`)**, Registry 가 만든 enum 으로 연다. 패키지는 Registry 를 모르고 **VM 을 받아 여는 길만** 연다. View 의 `CreateVM` · VM 용 `[SerializeField]` 는 빠지는 방향. 메서드 모양 · VM 넣는 때 등은 `[?]` (DESIGN 5.1) | 처음 안의 이점 (enum 키 · View 에서 직렬화 필드 · `CreateVM` 제거) 은 살리고, VM 이 View 를 모르는 것 · 열 때마다 새 VM · 생성자로 SO 받기 · 패키지가 DevLib 을 모르는 것을 지킨다. 단점: SO 를 프리팹이 아닌 Registry 에셋에 넣으니 프리팹만 보고는 무엇이 필요한지 안 보인다, 프리팹과 VM 타입 짝이 따로 놀 수 있다 |
| 2026-10-18 | Claude (디버깅) | `AbstractView` 의 `new()` 제약 삭제 · 파괴될 때 VM `Dispose` (`protected virtual OnDestroy` — 하위가 쓰면 `base.OnDestroy()`) · `AbstractViewModel.Dispose` 뒤 목록 비움 | `CreateVM` 이 있어 `new()` 는 생성자에 SO 를 받는 VM 만 막았다 · `Own` 한 구독이 풀리지 않았다 · 두 번 `Dispose` 하면 두 번 풀렸다 |

## 개발 · 적용 방식

1. 사용자가 이 저장소의 Runtime/ · Editor/ · Tests/에 공용 코드 중 핵심을 직접 작성한다. 에디터 도구와 핵심 설계 문제가 아닌 디버깅은 AI가 한다 (`AGENTS.md` "현재 작업 담당").
2. Project_B는 로컬 패키지 참조(`file:`)로 컴파일·테스트를 확인한다. 틀(UI0·UI3·UI4)은 여기서, Element와 화면은 Apes에서 (10-18). Apes에서 부품을 고치는 흐름은 아래 "Apes 에서 고치는 흐름".
3. AI는 설명·설계 검토·리뷰를 지원하고, 디버깅으로 고친 것은 무엇을 왜 고쳤는지 보고한다. 구현 완료나 단계 진행을 임의로 기록하지 않는다.
4. 공유할 버전이 준비되면 버전·태그를 정하고, Apes에는 팀 절차에 따라 Git URL + 태그로 적용한다.
5. Apes의 기존 시스템과 공용 파일은 이 작업 때문에 변경하지 않는다. 프로젝트별 연결 코드는 각 프로젝트에 둔다.

## Apes 에서 고치는 흐름 (2026-10-18 정함 — Claude 추천안, 사용자 채택)

Git URL 패키지는 Apes 안에서 고칠 수 없다 (`Library/PackageCache` 에 읽기 전용으로 받는다). 그래서 사용자 PC 의 Apes 에는 이 저장소의 clone 을 **내장 패키지**로 둔다 — Unity 는 `Packages/` 안의 같은 이름 패키지를 manifest 의 Git URL 보다 먼저 쓴다.

| 곳 | 역할 |
| --- | --- |
| GitHub `hwanji203/MVVMGUI` | 원본. 모든 수정은 결국 여기로 push |
| Apes `Packages/com.banggae.mvvmgui/` (사용자 PC 에만) | **주 작업 장소** — 이 저장소의 clone. 고치면 Apes 에 바로 반영. Apes git 은 무시 |
| `D:/GameDev_C/Projects/Unity/MVVMGUI` | Project_B 가 `file:` 로 쓰는 사본 — 여기서 고치고 push 해도 된다 (두 사본은 같은 원본의 clone 으로 동등) |

1. **처음 한 번** (Apes 세션, Apes 팀 규칙대로): 위 폴더에 clone → Apes 의 `.git/info/exclude` 에 `Packages/com.banggae.mvvmgui/` (내 PC 전용 — 팀 파일을 안 건드림) → manifest 에 `"com.banggae.mvvmgui": "https://github.com/hwanji203/MVVMGUI.git#v0.1.0"` (manifest 변경 — 팀 승인 1회).
2. **평소**: Apes 에서 화면을 만들다 부품이 필요하면 clone 안의 코드를 고치고, 그 폴더 (= 이 저장소) 에서 commit · push. Apes 저장소의 커밋과는 별개.
   Project_B 쪽 사본에서 고쳐도 된다 (사용자 10-18 확인). **어느 사본이든 고치기 전에 `git pull`, 끝나면 push** — 두 곳에서 따로 커밋하면 갈라진다.
   Project_B (6000.6) 에서 고칠 때는 6000.3 에 있는 API 만 쓴다 — 아니면 Apes 에서 컴파일이 깨진다.
3. **팀에 내보낼 때만**: `package.json` 의 `version` 을 올리고 같은 이름의 태그 (`v0.2.0` …) 를 push → Apes manifest 의 `#태그` 를 올린다 (manifest 변경 — 팀 규칙대로).
4. **단점**: 사본이 둘 (Apes clone · Project_B 용) — 다른 쪽에서 고친 뒤 `git pull` 을 잊으면 옛 코드로 컴파일되거나, 그 위에 따로 커밋해 갈라진다. 내 PC 에서는 내장 패키지가 태그보다 우선이라, 팀원이 받는 버전과 내가 보는 코드가 다를 수 있다 — 내보내기 전에 태그 버전으로 확인하려면 clone 폴더를 잠시 치운다.

안 고른 것: JTH 폴더에 만들고 나중에 옮기기 (옮기는 작업이 매번) · 원본에서만 고치고 태그로 왕복 (한 줄마다 push · 태그 · manifest) · junction 으로 두 사본을 한 벌로 (Unity 가 연결된 폴더를 불안정하게 다룰 때가 있다).
