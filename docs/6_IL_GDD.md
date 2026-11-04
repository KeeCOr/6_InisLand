# 6_IL — GDD

<!-- PORTFOLIO-GDD-20261104:START -->
## 2026-11-04 구현 대조 및 GDD 현행화

- **현재 제품 정의**: 설원생존전투·채집·건설과마을개척을결합한Unity게임.
- **핵심 경험 / 흐름**: 낮행동/채집·사냥 → 모닥불·집·울타리관리/장비·성장 → 밤적/보스대응 → 보수/새날또는새모험.
- **이번 확인 소스**: C:\work\project-resume\games\6_IL
- **소스 버전**: 0.1.0
- **확인 시 Git 기준**: backup/2026-10-02-local / 69b052aca334ec48e7d94a6796bab5061b8ba2e2
- **현재 작업 상태**: 커밋되지 않은 변경 있음. 완료·원격 반영과 구분한다.
- **이번 검증 범위**: 2026-11-04 문서·선택 소스·Git 메타데이터 읽기 확인. 제품 테스트·빌드 재실행 없음. 과거 PASS 날짜를 오늘로 갱신하지 않음.
- **구현 / 문서 근거 [existing_document_report]**: DayActionTime은낮에만실제이동/공격/벌목/채집행동시간을차감하고중복차감을막는다.밤/저녁/새벽과AI전체는기존실시간을유지한다는오늘기획서기록.
- **근거 파일**: C:/work/project-resume/games/6_IL/docs/InisLand_기획서.md
- **구현 / 문서 근거 [existing_document_report]**: 새모험은BootScene공통초기화경로를재사용하고시작모닥불1/집1/울타리를중복생성·스토리삭제로부터보존한다는기록.
- **근거 파일**: C:/work/project-resume/games/6_IL/docs/InisLand_기획서.md
- **구현 / 문서 근거 [source_read_confirmed]**: 실제ProceduralSpawner는Rabbit0.32/Deer0.20/Boar0.16,DeerChance0.035를정의한다.세종비율8:5:4이며화면개체수보장이아니다.
- **근거 파일**: C:/work/project-resume/games/6_IL/Assets/Scripts/World/ProceduralSpawner.cs
- **구현 / 문서 근거 [existing_document_report]**: 모닥불연료/생존에연결한눈녹은땅비트맵,액터를가리지않는바닥정렬및꺼짐후재적설전환을오늘기획서가보고한다.
- **근거 파일**: C:/work/project-resume/games/6_IL/docs/InisLand_기획서.md
- **이전 검증 기록 (이번 재실행 아님)**: 2026-11-04 행동낮/새모험변경:348EditMode+17PlayMode및Windows오류0/157파일SHA일치기록.
- **이전 검증 기록 (이번 재실행 아님)**: 2026-11-04 사냥감비중최신변경:350EditMode+3PlayMode=353통과,공식Windows오류0/157파일SHA일치기록. 각실행범위가달라검사수를합산하지않음.
- **미완료 / 제한**: UnitybundleVersion1.0과기존기획서0.2.0표기를동일릴리즈로추정하지않음.
- **미완료 / 제한**: 실제네이티브전체플레이/실사용자세이브는미검증. 격리IL6_TEST_SAVE_DIRECTORY만사용한보고.
- **다음 행동**: 오늘행동시간·시작마을·모닥불바닥·동물확률의실제구현을보충하고장시간자연플레이와세이브범위를구분한다.

<!-- PORTFOLIO-GDD-20261104:END -->
