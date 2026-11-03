# 이니스랜드 화면 참조 번호

개발 대화·검수용 번호다. 게임 UI에 번호를 표시하지 않는다. 2026-11-03 실제 씬 전환과 SimpleHud 분기 코드를 기준으로 정리했다.

| 번호·이름 | 용도 | 실제 씬 / 구현 |
| --- | --- | --- |
| 01 설원 플레이 | 이동·전투·채집·마을 복구 HUD | SnowfieldScene / SimpleHud.OnGUI |
| 01-1 건설 | 일시정지 건물 선택과 배치 | SimpleHud.BuildMode / _buildPaused |
| 01-2 모험 메뉴 | 일시정지·설정·게임 메뉴 | SimpleHud.DrawPauseMenu |
| 01-3 특성 선택 | 레벨 상승 특성 선택 | SimpleHud.DrawRuneModal |
| 01-4 동료 영입 | 일러스트 대화와 영입 결정 | SimpleHud.RecruitConversation |
| 01-5 이야기·튜토리얼 | 시작 이야기와 플레이 안내 | SimpleHud.DrawTutorialOverlay |
| 01-6 연구 | 건물별 연구 목록과 실행 | SimpleHud.DrawResearchCatalog |
| 01-7 주변 작업 | 근처 시설 작업 선택 | SimpleHud.DrawNearbyWorkMenu |
| 01-8 자원 상세 | 보유 자원 상세 | SimpleHud.DrawResourceDetails |
| 01-9 동료 요약 | 영입 동료 정보 | SimpleHud._showCompanions |
| 01-10 사망 | 사망 결과와 재시작 | SimpleHud.DrawDeathOverlay |
| 01-11 아침 울타리 보수 | 손상·파괴 수와 목재 비용 확인 후 선택 수리 | SimpleHud.DrawMorningFenceRepair |
| 01-12 일몰 사전 안내 | 낮 종료15초 예고와5초 카운트다운·종소리, 비모달 HUD 안내 | SimpleHud.SunsetWarning / SunsetWarningState |

| 01-13 마을 귀환 전환 | 해가 졌습니다 알림 다음 귀환 일러스트, 밤으로 전환 | SimpleHud.VillageReturn / VillageReturnPresentation |

BootScene은 GameSession을 준비하고 OnboardingScene으로 전환한다. 현재 OnboardingController.Start는 SnowfieldScene으로 즉시 이동하므로 별도의 플레이어 선택 화면으로 번호를 부여하지 않았다. 위 목록은 코드 기준 화면 식별이며 모든 화면의 네이티브 검수를 의미하지 않는다. 이후 번호를 재배치하지 않고 새로운 화면은 새 번호로 추가한다.
