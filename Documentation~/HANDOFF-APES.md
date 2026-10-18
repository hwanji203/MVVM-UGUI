# 이전 Apes 인수인계 — 현재 절차 아님

> 2026-10-18 최신 결정 (10-15 결정을 바꿈): 사용자는 핵심 코드만 직접 작성하고, 에디터 도구와 핵심 설계 문제가 아닌 디버깅은 AI가 한다 (경계는 `AGENTS.md` "현재 작업 담당"). Project_B에서 먼저 검증한다. 공용 코드는 MVVMSystem 패키지 저장소에서 직접 개발한다. Apes에서 구현한 뒤 복사하는 절차는 사용하지 않는다.
> 현재 작업은 DESIGN.md와 ROADMAP.md를 따른다. 아래는 이전 계획의 기록이며 실행 지침이 아니다.

# Apes 에서 이어 만들기 (GPT · Codex 용 인수인계)

> 2026-10-15 Claude 작성. 사용자 결정: **MVVMSystem 의 나머지 구현은 팀 프로젝트 Apes 에서 하고, 끝나면 이 저장소 (그리고 Project_B) 로 옮긴다.** 작업은 GPT 로 한다.

## 0. 먼저 읽을 것 (순서대로)

1. Apes 의 팀 규칙 — `Teckdeck/AGENTS.md` → `Teckdeck/.codex/AI_COLLABORATION_GUIDE.md`. **이 규칙이 이 문서보다 우선한다.**
2. 사용자 개인 규칙 — `2026-graduation-Apes_Project/CLAUDE.local.md` (Claude 용으로 쓴 것이지만 같은 사용자: git 은 스테이지까지 · 코드를 맡길지 직접 쓸지 판정표).
3. 사용자 진행 기록 — `Teckdeck/Assets/_MemberWorkspace/JTH/_AgentLog/PROGRESS.md` (기존 Registry 작업 기록이 있다. 새 요청으로 이어서 적는다).
4. 이 저장소의 `Documentation~/DESIGN.md` (설계) · `ROADMAP.md` (단계 · 기준 · 옮기는 법).

## 1. Apes 쪽 사실 (2026-10-15 확인)

- 경로: `D:\GameDev_C\Projects\Unity\2026-graduation-Apes_Project\Teckdeck` · Unity **6000.3.10f1** · UGUI 2.0 (TMP 포함) · **DOTween 있음** (`Assets/Plugins/Demigiant`) · UniTask · Reflex.
- 사용자 이니셜 **JTH**. 실제 폴더 이름은 `Assets/_MemberWorkspace/` (팀 가이드 본문은 `_MemeberWorkspace` 로 오타 — 실제 폴더를 쓴다).
- 작업 위치: **`Assets/_MemberWorkspace/JTH/Scripts/MVVMSystem/`** — 아래에 `Runtime/` · `Editor/` · `Tests/`. 검증용 씬 · 프리팹 · 화면 코드는 `JTH/` 안의 다른 곳.
- Apes 의 DevLib (`Assets/_Shared/Systems`) 은 쓰지 않는다 — 틀은 어떤 이벤트 · 사운드 시스템도 모른다 (DESIGN 0 · 7.3).
- `Packages/manifest.json` · ProjectSettings 는 건드리지 않는다 (팀 가이드: 별도 승인).

## 2. 시작 순서

1. **뼈대 복사**: 이 저장소의 `Runtime/MVVMSystem.asmdef` · `Editor/MVVMSystem.Editor.asmdef` 를 **`.meta` 와 함께** `JTH/Scripts/MVVMSystem/Runtime/` · `Editor/` 로 복사한다 (asmdef GUID 를 패키지와 같게 — 나중에 옮길 때 그대로 맞는다).
   - 저장소: https://github.com/hwanji203/MVVMSystem (공개). 사용자 PC 에는 `D:\GameDev_C\Projects\Unity\MVVMSystem`.
   - 패키지 폴더가 아니라 Assets 안이므로 `package.json` 은 복사하지 않는다.
2. `ROADMAP.md` 의 **UI0** 부터 한 단계씩. 팀 가이드대로 Phase 계획 → 사용자 승인 → 구현 → `JTH/_AgentLog/phases/PHASE-NNN.md` 기록.
3. `Tests/` 에 EditMode 테스트 asmdef (`MVVMSystem.Tests` — `MVVMSystem` 참조, `UNITY_INCLUDE_TESTS`, Editor 전용) 를 만든다.
4. `[세부]` 를 정하면 Phase 기록에 적는다 (옮길 때 `ROADMAP.md` "정한 세부" 로 옮긴다).

## 3. 지킬 것

- `DESIGN.md` 의 **(확정)** 항목은 바꾸지 않는다. 바꿀 이유가 생기면 구현을 멈추고 사용자에게 묻는다.
- Unity 6000.3 에서 되는 API 만. `UnityEngine.UIElements` 와 겹치는 이름 (`TextElement` · `BindableElement`) 금지.
- 싱글톤 금지 (`ScreenManager` 는 씬의 일반 컴포넌트).
- 틀 코드 (`MVVMSystem/` 아래) 에 Apes 전용 타입 · DevLib 참조를 넣지 않는다 — 다른 프로젝트로 옮겨야 한다.

## 4. 단계가 끝나면

사용자가 Apes 에서 커밋한 뒤, 이 저장소로 옮기는 것은 `ROADMAP.md` "옮기는 법". 사용자는 이 일을 Claude (Project_B 세션) 에 맡길 수 있다.
