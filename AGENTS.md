# MVVM UGUI 작업 지침 (AI 용)

UGUI MVVM 틀 패키지 (`com.banggae.mvvm-ugui`). 사람용 설명은 `README.md`.

1. 먼저 `Documentation~/DESIGN.md` (설계) 와 `Documentation~/ROADMAP.md` (단계 · 현황 · 옮기는 법) 를 읽는다.
2. 다른 프로젝트 (Apes) 의 작업폴더에서 이어 만드는 중이면 `Documentation~/HANDOFF-APES.md` 도 읽는다 — 그 프로젝트의 규칙이 우선한다.
3. DESIGN 의 **(확정)** 항목은 사용자 확인 없이 바꾸지 않는다. `[세부]` 는 정해도 되고, 정하면 ROADMAP "정한 세부" 에 적는다.
4. Unity **6000.3** 에서 되는 API 만. 틀은 특정 이벤트 · 사운드 시스템 (DevLib 등) 을 참조하지 않는다.
5. 스크립트를 옮기거나 복사할 때는 `.meta` 와 함께 (GUID 유지).
6. 설명 · 문서 · 커밋 메시지는 한국어.


## 현재 작업 담당 (2026-10-18 — 10-15 결정을 바꿈)

- **사용자는 핵심 코드만 직접 작성한다.** 무엇이 핵심인지는 단계마다 AI가 후보를 제시하고 사용자가 정한다. 사용자가 작성 중인 핵심 부분은 덮어쓰지 않는다.
- **AI가 직접 하는 것:** 설명·설계 검토·코드 리뷰, 에디터 도구(이름으로 채우기 등), 그리고 **핵심 설계 문제가 아닌 디버깅**. 컴파일 오류·null 처리·해제 누락·잘못된 기본값·이름 충돌 같은 것은 AI가 고치고 무엇을 왜 고쳤는지 보고한다.
- **사용자에게 넘기는 것:** 구조·수명 흐름·공개 API 모양을 바꿔야 하거나, DESIGN 의 (확정) 항목과 어긋나는 문제. 고치지 않고 원인과 선택지를 알린다.
- 공용 코드는 이 저장소에 둔다 (2026-10-18 Project_B DevLib 사본에서 `.meta` 째 옮김). **틀 (UI0 테스트 · UI3 ScreenView · 에디터 도구 · UI4 ScreenManager · PopupView) 은 이 저장소에서 만들고 Project_B 로 컴파일 · 테스트를 확인한다. Element 추가 (UI2) 와 실제 화면 (UI6) 은 Apes 에서 화면과 함께 만든다 (사용자 10-18). UI5 (목록) 는 `[?]`.** → **10-20 사용자: Element (UI2) · 목록 (UI5) 도 이 저장소에서 (Project_B 로 확인). 화면 (UI6) 만 Apes.** Apes 에는 Git URL + 태그로 적용하고 (manifest 변경은 Apes 팀 승인), Project_B 는 `file:` 로컬 참조로 컴파일을 확인한다.
- Apes 에서 일할 때는 Apes 팀 규칙이 우선한다 (`HANDOFF-APES.md` 1 · 3절의 사실과 지킬 것은 그대로 유효: Apes 저장소 안의 작업은 JTH 폴더 · Unity 6000.3 · git 은 스테이지까지).
  사용자 PC 의 Apes `Packages/com.banggae.mvvm-ugui/` 는 **이 저장소의 clone** (Apes git 이 무시 — `.git/info/exclude`) 이라, 그 안의 commit · push 는 이 저장소 일이다 (ROADMAP "Apes 에서 고치는 흐름", 사용자 10-18).
