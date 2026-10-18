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
| UI3 | | `ScreenView<TVM>` + 생성기 (partial `.g.cs` 속성 + `AutoBind`) · 프리팹 저장 · 컴파일 뒤 자동 생성 · 어긋남 검사 | 테스트 프리팹으로 생성 결과 확인, 생성기 테스트 통과 |
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
| 2026-10-18 | 사용자 | namespace 는 **asmdef 폴더 기준, `Runtime` 없이**: `MVVMGUI` (IDisposable · UnSubscriber) · `MVVMGUI.Binding` · `MVVMGUI.Core` · `MVVMGUI.Elements` · `MVVMGUI.Elements.Core` · `MVVMGUI.Editor` · `MVVMGUI.Tests` | Rider 는 asmdef 가 있는 폴더 (`Runtime/`) 를 뿌리로 보고 네임스페이스를 기대한다 — `MVVMGUI.Runtime.*` 는 어긋남 경고. 설계 문서도 처음부터 `MVVMGUI.Binding` 꼴 |
| 2026-10-18 | 사용자 | `Command` 생성자의 사용 가능 기본값 = `true` (`new Command(action, active = true)`) | 기본값 `false` 면 `new Command(OnClick)` 이 꺼진 버튼으로 시작한다 |
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
