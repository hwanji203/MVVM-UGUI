# MVVMGUI

UGUI 용 MVVM 틀. 화면 하나 만들 때 반복되는 연결 코드를 0 에 가깝게 줄이는 것이 목표다.

- `Bindable<T>` — 값이 바뀌면 연결된 화면이 따라간다. 파생 값 (`Select` · `Combine`) 으로 "무엇을 갱신할지" 를 한 번만 선언.
- `Element` — 재사용 위젯 (Label · Gauge · Button · Slider …). 각 Element 는 `Apply` 만 구현.
- 생성기 — 프리팹의 Element 를 꺼내는 partial 속성과, 이름이 같은 ViewModel 멤버와의 연결 (`AutoBind`) 을 만든다.

> 개발 중 (0.x). 설계 `Documentation~/DESIGN.md` · 진행 `Documentation~/ROADMAP.md` · AI 작업 지침 `AGENTS.md`.

## 설치

`Packages/manifest.json` 의 `dependencies` 에 한 줄:

```json
"com.banggae.mvvmgui": "https://github.com/hwanji203/MVVMGUI.git#v0.1.0"
```

`#` 뒤의 태그가 버전이다. 올릴 때는 태그만 바꾼다.

## 요구 사항

- Unity 6000.3 이상, UGUI (TextMeshPro 포함)
- **DOTween** — 패키지 의존성으로 선언할 수 없어 프로젝트에 직접 설치되어 있어야 한다.

## 프로젝트와 연결하는 곳

MVVMGUI 은 특정 이벤트 · 사운드 시스템을 모른다. 프로젝트마다 아래 두 곳만 잇는다.

| 연결점 | 패키지가 주는 것 | 프로젝트가 하는 일 |
| --- | --- | --- |
| 클릭 소리 | 추상 `UISoundSO` (+ 기본 `AudioSource` 구현) | 자기 사운드 시스템으로 `Play()` 를 구현한 SO 하나 (선택) |
| 화면 열기 · 닫기 | `ScreenManager.Open<T>()` · `Close()` | 자기 이벤트 시스템에서 받아 이 메서드를 부르는 컴포넌트 하나 (선택) |
