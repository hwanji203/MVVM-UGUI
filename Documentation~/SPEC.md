# MVVMGUI 아키텍처

화면마다 **ViewModel + View + 프리팹**만 작성하도록 반복되는 UI 연결을 공용화한다.
이 문서는 직접 구현할 때 참고할 구조 지도다. 메서드 형태와 내부 구현은 직접 정한다. 기존 세부 설계는 [DESIGN.md](DESIGN.md), 진행·결정 기록은 [ROADMAP.md](ROADMAP.md)에 있다.

## 1. 구성과 책임

- **Model**: 프로젝트의 데이터와 게임 규칙. UI를 모른다.
- **ViewModel**: 화면의 상태·계산·입력 명령을 가진 순수 C# 객체. View와 Element를 모른다.
- **View**: 프리팹 루트의 MonoBehaviour. ViewModel을 만들고 Element와 연결하며 화면별 특수 연출을 맡는다.
- **Element**: 라벨·게이지·버튼·슬라이더 같은 재사용 위젯. 특정 ViewModel 대신 공통 바인딩 타입에 연결된다.

참조 방향은 **View → ViewModel → Model 쪽 데이터·이벤트**다. Model은 ViewModel을 참조하지 않는다.

## 2. 상태와 데이터 흐름

- `Bindable<T>`: 원본 상태와 변경 알림. 같은 값은 무시하고 구독 시 현재 값을 전달한다.
- `IReadOnlyBindable<T>`: 읽기·구독만 허용한다. `Select`·`Combine`으로 만든 파생 값은 이 형태로 노출한다.
- `Command`: 버튼의 동작과 실행 가능 여부. 슬라이더·토글 입력은 쓰기 가능한 Bindable에 연결한다.
- `BindableList<T>`: 항목 수와 내용이 바뀌는 목록. `ListElement`가 항목별 `ItemView`와 연결한다.

**표시**: Model의 이벤트 → ViewModel 상태 → Bindable 알림 → Element.
**입력**: Element → Command 또는 Bindable → ViewModel → Model 쪽 요청.

첫 값은 시작 이벤트로 받고, 중간에 여는 화면은 읽기 전용 상태나 설정 SO를 받아 초기화한다. 값의 보정은 ViewModel, 실제 적용·저장은 Model이 맡는다. 양방향 위젯은 값을 표시할 때 입력 알림을 다시 발생시키지 않는다.

## 3. 소유권과 수명

- View가 ViewModel을 소유한다. 생성 → 자동 연결 → 수동 연결 순서로 준비하고, 파괴될 때 ViewModel을 정리한다.
- Element가 자기 바인딩을 소유한다. 재연결 전에 이전 연결을 끊고, 파괴될 때 해제한다. View에 별도 구독 목록을 두지 않는다.
- ViewModel은 자신보다 오래 사는 이벤트 채널·SO의 구독을 정리한다. 공용 기반은 구독 보관·해제만 맡고 특정 이벤트 시스템을 모른다.
- 비활성화만 한 화면은 상태와 구독을 유지한다. 파괴되는 화면·항목은 소유한 자원까지 정리한다.

## 4. View 자동 연결

- 에디터 생성기가 View 아래의 Element 참조를 배열에 저장하고, `partial` View에 타입 있는 속성과 연결 코드를 생성한다.
- Element와 ViewModel 멤버의 **이름이 완전히 같고 타입이 맞으면** 자동 연결한다. 나머지는 View에서 수동 연결하고, 수동 연결이 우선한다.
- 중첩 View·ItemView 아래는 각각의 연결 범위다. 생성기는 ViewModel 선언을 만들지 않으며, 실행 중 연결은 생성된 C# 코드로 처리한다.
- 프리팹 저장·컴파일 완료·인스펙터 버튼에서 생성한다. 같은 결과는 다시 쓰지 않고, 이름 충돌·타입 불일치·참조 어긋남을 알린다.

## 5. 화면 관리와 연출

- `ScreenManager`: 씬에 두는 일반 컴포넌트. 화면 인스턴스, 열기·닫기, 팝업 스택을 관리한다. 화면끼리는 서로 참조하지 않는다.
- `PopupView`: 창·배경·닫기 입력과 공통 열기·닫기 연출을 맡고, 닫힘 완료를 관리자에게 알린다.
- 재사용 화면은 숨겨 두고, 같은 종류를 여러 개 띄우는 화면은 우선 열 때 생성·닫을 때 파괴한다. 한 화면 안의 목록은 `ListElement`가 따로 관리한다.
- 트윈은 Element·View에서 DOTween을 직접 쓴다. 비활성 위젯은 즉시 값을 반영하고, 일시정지 중에도 필요한 UI 연출은 동작한다.

## 6. 패키지 경계와 구현 순서

공용 코드는 `Runtime/`, 생성기는 `Editor/`, 검증 코드는 `Tests/`로 분리한다. Unity 6000.3을 기준으로 UGUI·TMP·DOTween을 사용한다. 패키지는 DevLib 등 특정 프로젝트 시스템을 참조하지 않는다.

프로젝트 연결점은 두 가지다. **클릭 소리**는 `UISoundSO`의 기본 구현 또는 프로젝트 구현을 끼우고, **화면 요청**은 프로젝트의 연결 컴포넌트가 받아 ScreenManager를 호출한다. 게임별 Model·ViewModel·View와 연결 코드는 프로젝트에 둔다.

구현 순서: **바인딩·ViewModel 기반 → 기본 Element → View·생성기 → 화면·팝업 관리 → 목록 UI → 실제 화면**.
공용 코드는 MVVMGUI 저장소에서 직접 작성하고, Project_B의 로컬 패키지 참조로 전투 HUD까지 연결해 검증한다.
