# 이니스랜드 화면 참조 번호

개발 대화·검수용 번호다. 게임 UI에 번호를 표시하지 않는다. 2026-11-03 실제 씬 전환과 SimpleHud 분기 코드를 기준으로 정리했다.

| 번호·이름 | 용도 | 실제 씬 / 구현 |
| --- | --- | --- |
| 01 설원 플레이 | 이동·전투·채집·마을 복구 HUD | SnowfieldScene / SimpleHud.OnGUI |
| 01-1 건설 | 일시정지 건물 선택과 배치 | SimpleHud.BuildMode / _buildPaused |
| 01-2 모험 메뉴 | 일시정지·설정·게임 메뉴 | SimpleHud.DrawPauseMenu |
| 01-3 특성 선택 | 레벨 상승 특성 선택 | SimpleHud.DrawRuneModal |
| 01-4 동료 영입 | 일러스트 대화와 영입 결정 | SimpleHud.RecruitConversation |
| 01-5 이야기·튜토리얼 | 주인공 초상/독백과 실제 행동 안내 | SimpleHud.DrawTutorialOverlay / GuideTutorial |
| 01-6 연구 | 건물별 연구 목록과 실행 | SimpleHud.DrawResearchCatalog |
| 01-7 주변 작업 | 근처 시설 작업 선택 | SimpleHud.DrawNearbyWorkMenu |
| 01-8 자원 상세 | 보유 자원 상세 | SimpleHud.DrawResourceDetails |
| 01-9 동료 요약 | 영입 동료 정보 | SimpleHud._showCompanions |
| 01-10 사망 | 사망 결과와 재시작 | SimpleHud.DrawDeathOverlay |
| 01-11 아침 울타리 보수 | 손상·파괴 수와 목재 비용 확인 후 선택 수리 | SimpleHud.DrawMorningFenceRepair |
| 01-12 일몰 사전 안내 | 낮 종료15초 예고와5초 카운트다운·종소리, 비모달 HUD 안내 | SimpleHud.SunsetWarning / SunsetWarningState |

| 01-13 마을 귀환 전환 | 해가 졌습니다 알림 다음 귀환 일러스트, 밤으로 전환 | SimpleHud.VillageReturn / VillageReturnPresentation |

2026-11-06 BootScene은 아래 시작 로비로 변경했다. OnboardingScene은 선택 후 SnowfieldScene으로 전환하는 내부 경로다. 기존 번호는 유지한다.

| 번호·이름 | 용도 | 실제 씬 / 구현 |
| --- | --- | --- |
| 02 시작 로비 | 새로 하기·이어서 하기와 저장3슬롯 선택 | BootScene / LobbyController |
| 02-1 새 모험 확인 | 선택 슬롯 덮어쓰기 확인/취소 | LobbyController.PendingOverwriteSlot |
| 02-2 새 모험 이야기 로딩 | 실제 씬 로딩과4장 시작 이야기, 준비 후 입장 | OnboardingScene / NewAdventureLoading |


| 번호·이름 | 용도 | 실제 씬 / 구현 |
| --- | --- | --- |
| 01-14 방향·피해 안내 | 집/마을 방향과 화면 밖 시설·동료 피해 요약 | SimpleHud.HomeNavigation / DrawThreatCompass / ThreatNavigation |
