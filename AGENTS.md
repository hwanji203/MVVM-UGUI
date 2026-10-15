# MVVMSystem 작업 지침 (AI 용)

UGUI MVVM 틀 패키지 (`com.hwan.mvvmsystem`). 사람용 설명은 `README.md`.

1. 먼저 `Documentation~/DESIGN.md` (설계) 와 `Documentation~/ROADMAP.md` (단계 · 현황 · 옮기는 법) 를 읽는다.
2. 다른 프로젝트 (Apes) 의 작업폴더에서 이어 만드는 중이면 `Documentation~/HANDOFF-APES.md` 도 읽는다 — 그 프로젝트의 규칙이 우선한다.
3. DESIGN 의 **(확정)** 항목은 사용자 확인 없이 바꾸지 않는다. `[세부]` 는 정해도 되고, 정하면 ROADMAP "정한 세부" 에 적는다.
4. Unity **6000.3** 에서 되는 API 만. 틀은 특정 이벤트 · 사운드 시스템 (DevLib 등) 을 참조하지 않는다.
5. 스크립트를 옮기거나 복사할 때는 `.meta` 와 함께 (GUID 유지).
6. 설명 · 문서 · 커밋 메시지는 한국어.
