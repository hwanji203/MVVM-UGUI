# MVVM UGUI 아키텍처

화면마다 **ViewModel + View + 프리팹**만 작성하도록 반복되는 UI 연결을 공용화한다.
이 문서는 구조 지도다(v0.4.0 기준). 세부 설계와 결정 이유는 [DESIGN.md](DESIGN.md), 진행·결정 기록은 [ROADMAP.md](ROADMAP.md), 팀원용 사용 가이드는 패키지 루트의 `Design Doc.html`에 있다.

## 1. 구성과 책임

- **Model**: 프로젝트의 데이터와 게임 규칙. UI를 모른다.
- **ViewModel**: 화면의 상태·계산·입력 명령을 가진 순수 C# 객체(`AbstractViewModel`). View와 Element를 모른다.
- **View**: 프리팹 루트의 MonoBehaviour(`ScreenView<TVM>`). ViewModel을 직렬화 필드로 들고 Element와 연결하며 화면별 특수 연출을 맡는다.
- **Element**: 텍스트·게이지·버튼·슬라이더 같은 재사용 위젯. 특정 ViewModel 대신 공통 바인딩 타입(`Bindable`·`Command`·`Signal`)에 연결된다.

참조 방향은 **View → ViewModel → Model 쪽 데이터·이벤트**다. Model은 ViewModel을 참조하지 않는다.

## 2. 상태와 데이터 흐름

- `Bindable<T>`: 원본 상태와 변경 알림. 같은 값은 무시하고, `Bind`하는 즉시 현재 값을 한 번 전달한다.
- `IReadOnlyBindable<T>`: 읽기·구독만 허용한다. `Select`·`Combine`으로 만든 파생 값은 이 형태로 노출한다.
- `Command`: 버튼의 동작(`Action` 속성)과 실행 가능 여부. 슬라이더·토글 입력은 쓰기 가능한 Bindable에 연결한다.
- `Signal<T>`: 저장하지 않는 일회성 알림(토스트·데미지 숫자). `Bind`할 때 부르지 않는다.
- `BindableList<T>`: 항목 수와 내용이 바뀌는 목록. `ItemListElement`가 항목 프리팹의 `ItemView<TItem>`과 연결한다.

**표시**: Model의 이벤트·SO → ViewModel 상태 → Bindable 알림 → Element.
**입력**: Element → Command 또는 Bindable → ViewModel → Model 쪽 요청.

ViewModel이 쓰는 SO·이벤트 채널은 ViewModel의 직렬화 필드로 받고 `Initialize`에서 구독한다. 여는 쪽이 그때마다 넣을 값(팝업 문구·콜백·씬의 Model)은 `Open`의 `setup`으로 넘긴다. 값의 보정은 ViewModel, 실제 적용·저장은 Model이 맡는다. 양방향 위젯은 값을 표시할 때 입력 알림을 다시 발생시키지 않는다.

## 3. 소유권과 수명

- View가 ViewModel을 소유한다. `Awake`에서 View `Initialize` → ViewModel `Initialize`(`IInitializable`이면, 한 번) → `Bind`(손으로 쓴 연결) → 꺼 두기 순서로 준비하고, 파괴될 때 ViewModel을 `Dispose`한다.
- Element가 자기 바인딩을 소유한다. 다시 묶으면 이전 연결을 끊고, 파괴될 때 해제한다. View에 별도 구독 목록을 두지 않는다.
- ViewModel은 자신보다 오래 사는 SO·이벤트의 구독을 `Own`에 맡긴다. 공용 기반은 구독 보관·해제만 맡고 특정 이벤트 시스템을 모른다.
- 닫힌 화면은 파괴하지 않고 꺼 둔 채 다시 쓴다. 그래서 열 때마다 `ResetForOpen`이 지난번 setup의 값·콜백을 되돌린다(Model 값은 되돌리지 않는다). 꺼진 화면도 상태와 구독을 유지한다.

## 4. View와 Element 연결

- View는 Element를 `[SerializeField]` 필드로 든다. 에디터의 **이름으로 채우기**가 비어 있는 필드를 같은 이름의 자식으로 채운다(대소문자·띄어쓰기·`_` 무시, 안쪽 View 아래는 찾지 않음). 비워 둬도 되는 필드는 `[Nullable]`.
- 연결은 전부 View의 `Bind(T vm)`에 손으로 쓴다. 자동 연결과 코드 생성기는 없다(처음 설계의 생성기·자동 연결은 10-18·10-21에 뺐다 — DESIGN 4.1·4.2).
- 같은 Element를 다시 묶으면 이전 연결이 끊긴다. 원본이 둘인 Element(드롭다운 항목 + 고른 번호)는 한 번에 받는다.

## 5. 화면 관리와 연출

- `ScreenManager`: 루트 Canvas 오브젝트에 붙는 일반 컴포넌트(싱글톤 아님, `DontDestroyOnLoad`). 화면 프리팹 목록을 들고 VM 타입을 키로 열고 닫는다. 화면끼리는 서로 참조하지 않는다.
- **모든 화면은 매니저로 연다**(사용자 10-22). 시작할 때 띄울 화면(HUD 등)은 시작 컴포넌트가 `Open<TVM>(setup)`으로 연다. 매니저는 VM을 돌려주지 않는다.
- 열린 순서는 형제 순서가 든다. 열 때 맨 위로 올리고, 닫기는 그 타입에서 켜진 것 중 맨 위부터. 같은 화면을 여러 개 띄울 수 있다(쉬는 인스턴스가 없으면 새로 만든다).
- 팝업: `AbstractPopupView<T>`가 창(`TransitionElement`)·배경·닫기 버튼과 열기·닫기 연출을 맡는다. 닫기는 `AbstractPopupVM`이 `RequestClose`로 요청하고 View가 닫는다. 확인 팝업 템플릿은 `ConfirmPopup`.
- 트윈은 Element·View에서 DOTween을 직접 쓴다. 꺼진 위젯은 즉시 값을 반영하고, 일시정지 중에도 UI 연출은 동작한다. 열기·닫기 대기는 UniTask.

## 6. 패키지 경계와 구현 순서

공용 코드는 `Runtime/`, 에디터 도구(이름으로 채우기·`[Nullable]` 표시)는 `Editor/`, EditMode 테스트는 `Tests/`로 분리한다. Unity 6000.3을 기준으로 UGUI·TMP·DOTween·UniTask를 사용한다. 패키지는 DevLib 등 특정 프로젝트 시스템을 참조하지 않는다.

프로젝트 연결점은 두 가지다. **클릭 소리**는 `UISoundSO`의 기본 구현 또는 프로젝트 구현을 끼우고, **화면 요청**은 프로젝트의 연결 컴포넌트가 받아 ScreenManager를 호출한다. 게임별 Model·ViewModel·View와 연결 코드는 프로젝트에 둔다. DI(Reflex 등)를 쓰는 프로젝트는 매니저가 만든 화면에 자동 주입이 닿지 않으므로, 씬의 시작 컴포넌트가 주입받아 `setup`으로 넘긴다.

구현 순서: **바인딩·ViewModel 기반 → Element → View·이름으로 채우기 → 화면·팝업 관리 → 목록 UI → 실제 화면**.
공용 코드는 MVVM UGUI 저장소에서 작성하고 Project_B의 로컬 패키지 참조로 컴파일·테스트를 확인한다. 실제 화면 검증(UI6)은 Apes에서 했다(종료 확인 팝업·설정·HUD, 2026-10-22).
