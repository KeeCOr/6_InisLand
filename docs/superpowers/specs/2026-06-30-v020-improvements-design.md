# InisLand v0.2.0 → v0.3.0 개선 설계

> 작성: 2026-06-30 | 기반: 페르소나 피드백 보고서 + GDD 검증 결과

---

## 범위

페르소나 피드백 보고서에서 도출된 P0/P1/P2 이슈 8개를 한 번에 처리한다.

---

## P0-1: 디버그 UI 분리

**문제**: `DrawDebugCorner()`가 프로덕션에서 개발 전용 버튼(페이즈 조작, 좀비 강제 소환)을 노출.  
**해결**:
- `DrawSfxCorner()` — 볼륨 슬라이더 전용 소형 패널 (항상 표시)
- `DrawDebugButtons()` — 개발 버튼 전용 (`#if UNITY_EDITOR || DEVELOPMENT_BUILD` 조건)
- `OnGUI()`에서 두 메서드를 분리 호출

---

## P0-2: 건물 배치 범위 미리보기

**문제**: 배치 모드 중 건물의 영향 반경이 표시되지 않아 전략적 배치 불가.  
**해결**: `PlacementController`에 `LineRenderer` 기반 원형 링 추가
- 모닥불(Campfire): `CampfireAura.Radius = 2.5f` 반경 (황금색)
- 망루(Watchtower): `Watchtower.Range = 8f` 반경 (하늘색)
- 울타리/바리케이드: 링 없음 (footprint만)
- `Begin()` 시 링 활성화, `Cancel()` 시 비활성화
- 쉐이더: `Hidden/Internal-Colored` (Unity 내장, 항상 포함됨)

---

## P1-3: 룬 원소 태그

**문제**: 어떤 룬이 원소 시너지 그룹인지 UI에 표시 없음.  
**해결**: `DrawRuneModal()` 각 카드에 원소 배지 추가
- `PoisonBlade` → `☠ 독 (원소)` (초록)
- `IceArrow` → `❄ 얼음 (원소)` (하늘)
- `LightningStrike` → `⚡ 번개 (원소)` (노랑)
- 현재 마스터된 원소 룬 수 표시 → 시너지 조건 피드백

---

## P1-4: 마스터 시너지 발동 피드백

**문제**: 원소 2종 마스터 시 50% 시너지 발동이 무소음.  
**해결**:
- `PlayerProgression`에 `event Action OnElementalSynergyAchieved` 추가
- `ApplyRune()` 내 시너지 달성 감지 후 이벤트 발행
- `SimpleHud`가 이벤트 구독 → 3초 토스트 "⚡ 원소 마스터 시너지 +50%!" 표시

---

## P2-6: 밤 결산 화면

**문제**: 밤 생존 후 어떤 결과였는지 요약 없음.  
**해결**:
- `VillageController` 씬 전환을 Coroutine으로 변경 (4s 지연)
- `SimpleHud`가 `DawnStartedPayload` 수신 시 4초 결산 패널 표시
- 표시 항목: Day N 생존, 처치 수(NightController.KillsThisNight), 총 점수
- 4초 후 자동 소멸 (플레이 흐름 유지)

---

## P2-7: 동료 개별 HP 표시

**문제**: 동료 상태가 총 스탠스만 표시, 개별 HP 불투명.  
**해결**: `DrawWaveStanceBar()` 패널 확장 (H 118→200)
- 스탠스 버튼 아래에 동료 1인당 HP 바 추가
- 이름(없으면 "동료"), 현재HP/최대HP, 컬러 바 표시
- 최대 6명까지 표시 (이상은 "+N명" 축약)

---

## P2-8: 데드 코드 제거

- `DrawRightPanel()` (SimpleHud:1305) — 호출 없음, 삭제
- `DrawInfoCard_DEPRECATED()` (SimpleHud:933) — 호출 없음, 삭제

---

## 미결 사항

- **DayDurationSec 180s vs GDD 40s**: 방향성 미결정. 현행 180s 유지.
- GDD 수정 또는 BalanceConfig 조정은 별도 세션에서 결정.
