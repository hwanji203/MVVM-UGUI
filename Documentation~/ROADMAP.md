# MVVMSystem 진행 순서

> 설계는 `DESIGN.md`. 단계마다 **컴파일 → 테스트 → 기록**. 단계 범위를 넘는 기능은 넣지 않는다.
> 지금 개발 장소: **Apes 프로젝트의 사용자 작업폴더** (`HANDOFF-APES.md`). 단계가 끝나면 이 저장소로 옮긴다 (아래 "옮기는 법").

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
| | | | |

## 옮기는 법 (Apes 작업폴더 → 이 저장소)

Apes 에서는 틀 코드를 **`Assets/_MemberWorkspace/JTH/Scripts/MVVMSystem/` 아래 `Runtime/` · `Editor/` · `Tests/` 에만** 둔다 (화면별 · 검증용 코드는 그 밖에). 그래야 통째로 옮길 수 있다.

1. Apes 의 `MVVMSystem/Runtime` · `Editor` · `Tests` 를 **`.meta` 와 함께** 이 저장소 루트의 같은 이름 폴더로 복사 (덮어쓰기).
   - `.meta` 를 같이 옮겨야 스크립트 GUID 가 유지된다 → 나중에 Apes 가 패키지로 바꿔도 프리팹의 스크립트 연결이 안 끊긴다.
   - asmdef 는 시작할 때 이 저장소의 것을 `.meta` 째 복사해 썼으므로 GUID 가 같다 (`HANDOFF-APES.md` 1단계).
2. 이 저장소에서 커밋 · 푸시. 쓸 만한 단계에서 태그 (`v0.1.0` …) — `package.json` 의 `version` 도 같이 올린다.
3. Project_B 는 로컬 참조라 따로 할 일이 없다 → Project_B 에서 컴파일 · 테스트 확인.
4. **Apes 를 패키지 참조로 바꿀 때** (나중, 팀 승인 필요): 작업폴더의 `MVVMSystem/` 를 먼저 지우고 manifest 에 git URL 을 넣는다. 둘이 같이 있으면 asmdef 이름 · GUID 가 겹친다.
