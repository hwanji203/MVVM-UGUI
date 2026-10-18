# MVVMSystem 진행 순서

> 설계는 `DESIGN.md`. 단계마다 **컴파일 → 테스트 → 기록**. 단계 범위를 넘는 기능은 넣지 않는다.
> 2026-10-18 최신 결정 (10-15 결정을 바꿈): 사용자는 핵심 코드만 직접 작성하고, 에디터 도구와 핵심 설계 문제가 아닌 디버깅은 AI가 한다 (경계는 `AGENTS.md` "현재 작업 담당"). Project_B에서 먼저 검증한다. 공용 코드는 MVVMSystem 패키지 저장소에서 직접 개발한다. Apes에서 구현한 뒤 복사하는 절차는 사용하지 않는다.

## 현황

| 단계 | 상태 | 내용 | 끝났다고 보는 기준 |
| --- | --- | --- | --- |
| P0 | 완료 (10-15, Claude) | 저장소 · `package.json` · asmdef 2개 · README · 이 문서들 | Project_B 에서 패키지 인식, 기존 테스트 통과 |
| **UI0** | 다음 | 바인딩: `Bindable` · `IReadOnlyBindable` · `Select` · `Combine` · `Command` · `BindableList` · `ViewModel` (`Own`) + 테스트 asmdef | DESIGN 8절의 Bindable · Command · BindableList · ViewModel 테스트 통과 |
| UI2 | | 기본 Element: `Element` · `ValueElement` · Label · Gauge · Button (+ `UISoundSO` · `AudioSourceUISoundSO`) · Toggle · Slider · Switch · Visible, `Reset()` 채우기 | 빈 씬에서 각 Element 를 Bindable 에 묶어 값이 바뀌면 화면이 따라감 (수동 확인 방법을 기록) |
| UI3 | | `ScreenView<TVM>` + 생성기 (partial `.g.cs` 속성 + `AutoBind`) · 프리팹 저장 · 컴파일 뒤 자동 생성 · 어긋남 검사 | 테스트 프리팹으로 생성 결과 확인, 생성기 테스트 통과 |
| UI4 | | `ScreenManager` (`Open<T>` · `Close` · 스택 · 여러 개) · `PopupView` | 팝업 열기 · 닫기 · 배경 클릭 닫기 · 두 개 겹쳐 띄우기 |
| UI5 | | `BindableList` 를 쓰는 `ListElement` · `ItemView` | 항목 추가 · 삭제 · 교체가 화면에 반영 |
| UI6 | | 실제 화면 하나로 검증 | Apes: 사용자가 정한 화면. Project_B: 전투 HUD (Project_B 쪽 작업) |

(UI1 은 "DOTween 가져오기" 였고 Project_B · Apes 모두 이미 있다.)

## 정한 세부

`DESIGN.md` 의 `[세부]` 를 정하면 여기에 한 줄씩 적는다 (날짜 · 누가 · 무엇 · 이유).

| 날짜 | 누가 | 무엇 | 이유 |
| --- | --- | --- | --- |
| 2026-10-18 | 사용자 | `Bindable.Combine` · `Bindable.Select` 는 `(IReadOnlyBindable<TR> bindable, IDisposable unSub)` 튜플을 돌려준다. 원본이 VM 자신의 값이면 `unSub` 은 버려도 되고, VM 보다 오래 사는 원본 (SO · 전역 객체) 이면 `Own(unSub)` (DESIGN 2.2) | 오래 사는 원본이 구독 람다로 파생 값을 붙잡아, 화면을 닫아도 남아서 계속 다시 계산된다 |
| 2026-10-18 | 사용자 | 바인딩 하위 namespace = `MVVMSystem.Runtime.Binding` (`Bindable<T>` · 정적 `Bindable` · `BindableList<T>` · `Command`) | 처음 이름 `…Runtime.Bindable` 은 정적 클래스 `Bindable` 과 같아서, 패키지 안 (`MVVMSystem.Runtime.*`) 코드의 `Bindable.Combine` 이 네임스페이스로 잡혀 CS0234 (Roslyn 으로 확인) |
| 2026-10-18 | 사용자 | `Command` 생성자의 사용 가능 기본값 = `true` (`new Command(action, active = true)`) | 기본값 `false` 면 `new Command(OnClick)` 이 꺼진 버튼으로 시작한다 |

## 개발 · 적용 방식

1. 사용자가 이 저장소의 Runtime/ · Editor/ · Tests/에 공용 코드 중 핵심을 직접 작성한다. 에디터 도구와 핵심 설계 문제가 아닌 디버깅은 AI가 한다 (`AGENTS.md` "현재 작업 담당").
2. Project_B의 기존 로컬 패키지 참조로 컴파일·테스트·전투 HUD를 검증한다. 파일 복사는 필요 없다.
3. AI는 설명·설계 검토·리뷰를 지원하고, 디버깅으로 고친 것은 무엇을 왜 고쳤는지 보고한다. 구현 완료나 단계 진행을 임의로 기록하지 않는다.
4. 공유할 버전이 준비되면 버전·태그를 정하고, Apes에는 팀 절차에 따라 Git URL + 태그로 적용한다.
5. Apes의 기존 시스템과 공용 파일은 이 작업 때문에 변경하지 않는다. 프로젝트별 연결 코드는 각 프로젝트에 둔다.
