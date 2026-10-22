# Project Instructions

## Project Identity

`47_IL / InisLand`는 Unity 기반 생존 어드벤처다. 낯선 섬을 탐험하며 생존, 성장, 동료, 전투, 다음 목표를 쌓아가는 플레이를 우선한다.

## Authoritative Stack

- Unity 프로젝트가 권위다.
- `Assets/`, `Packages/`, `ProjectSettings/`, Unity 씬, 프리팹, C# 스크립트, Unity 빌드 파이프라인을 기준으로 작업한다.
- Vite/npm 경로는 기존 참고 자료나 임시 프로토타입으로만 취급한다. 사용자가 명시하지 않으면 런타임 권위로 보지 않는다.

## Build And Verification

- 공식 Unity 빌드는 `IL6.EditorBuild.BuildScript.BuildWindows`를 우선한다.
- 빌드 출력은 `Build/Windows/IL6.exe`와 루트 portable 실행파일 기준으로 확인한다.
- Unity가 열려 있어 batch build가 막히면 실행파일 최신화를 주장하지 말고 차단 사유를 보고한다.
- 웹 npm 명령은 Unity 기능 검증을 대체하지 않는다.
- 테스트는 Unity EditMode/PlayMode 테스트와 가장 가까운 씬 확인을 우선한다.

## Documentation Rules

- 현재 문서는 `docs/InisLand_기획서.md`와 `docs/InisLand_기획서.html`을 함께 유지한다.
- gameplay, UX, UI, 시스템, 리소스, 빌드 동작이 바뀌면 MD와 HTML을 같이 최신화한다.
- `docs/next_improvement_instruction.md`는 다음 작업의 작은 실행 단위와 검증 기준을 담는다.

## Resource And Preview Rules

### Game-view contract (2026-10-19)

- 건물은 지면을 위에서 내려다보는 게임 시점에 맞춘다. 정면 벽 위주의 입면도는 사용하지 않는다. 지붕의 앞뒤 깊이, 작업대·발판·통 등의 윗면이 명확히 보여야 한다.
- 카메라 기준은 수평선 없는 직교 약탑다운이며, 화면 아래로 향하는 입구와 수평 전면 축을 유지한다. 임의의 아이소메트릭 대각 회전이나 정면 카메라로 바꾸지 않는다. 약 45도 하향 시점은 생성 시작값이며 최종 판단은 게임 화면과의 비교로 한다.
- 캐릭터의 측면 표현과 건물의 하향 시점을 혼동하지 않는다. 캐릭터 요청을 건물 카메라 규칙으로 확대 적용하지 않는다.
- 생성 전에 존재하는 게임 화면/승인된 리소스를 직접 열어 시점·설경·픽셀 밀도를 확인한다. 아래 대표 이미지가 없으면 유효한 자료를 찾아 경로를 기록하고, 누락된 자료를 확인했다고 보고하지 않는다.
- 현재 시점 비교 자료는 `docs/design-references/building-camera-gameplay-reference.png`이다. 복구된 기존 게임 레퍼런스이며 현재 빌드의 스크린샷으로 소개하지 않는다.
- 여러 건물은 먼저 한 종류를 수정하여 지붕/바닥 깊이를 레퍼런스와 비교한 뒤 같은 카메라 조건으로 확장한다. 이미지 단독 비교와 실제 게임 화면 검증 여부를 구분해서 보고한다.
- 생성 프롬프트에 시점·축·윗면 노출·투명 배경·전체 외곽 여백을 명시한다. 최종 리소스의 알파·바닥 피벗·게임 배율 및 목록/설치 프리뷰/완성 건물 참조를 확인한다. 기존 울타리 크기는 별도 요청 없이 바꾸지 않는다.

- 대표 이미지는 `docs/InisLand_gameplay_preview.png`와 `_workspace_docs/project_previews/06_InisLand_InisLand.png` 계열을 우선한다.
- 외부 공유 문서에서는 Unity 내부 `Assets/art/reference` 직접 참조보다 `docs/` 또는 `_workspace_docs/project_previews` 복사본을 사용한다.
- 리소스 교체는 파일 추가가 아니라 Unity 런타임 참조 확인까지 완료해야 한다.

## AI-Assisted Workflow

1. Plan: Unity에서 보여줄 핵심 플레이 순간과 현재 목표 표시 문제를 먼저 정의한다.
2. Split: 씬/HUD, 플레이 피드백, 리소스, 문서 작업을 겹치지 않게 나눈다.
3. Build: Unity 권위 파일만 좁게 수정한다.
4. Verify: Unity 테스트, 씬 확인, GDD/HTML 동기화, 대표 이미지 링크를 확인한다.
5. Reflect: Unity 전용 예외는 이 파일에 남기고 전역 규칙을 불필요하게 늘리지 않는다.

## Do Not

- 사용자가 명시하지 않는 한 Vite/npm 앱을 InisLand의 권위 구현으로 확장하지 않는다.
- Unity `.meta`, 씬, 프리팹 참조를 확인 없이 이동하거나 삭제하지 않는다.
- 빌드가 막혔는데 실행파일이 최신이라고 보고하지 않는다.
