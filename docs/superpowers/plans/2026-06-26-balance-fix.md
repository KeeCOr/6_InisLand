# Balance Fix: DayDuration + CampfireDPS Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** DayDurationSec 기본값을 540s→40s로, 모닥불 DPS를 6/5→2.5로 수정하고 CampfireAura가 BalanceConfig 단일 소스에서 DPS를 읽도록 통일한다.

**Architecture:** BalanceConfig.cs는 단일 소스로 값만 수정. CampfireAura.cs는 Awake()에서 BalanceConfig.Instance.BonfireDamagePerSec를 읽어 DamagePerSecond를 덮어써서 하드코딩 제거.

**Tech Stack:** Unity 2D, C#, ScriptableObject, MonoBehaviour

---

### Task 1: BalanceConfig 기본값 수정

**Files:**
- Modify: `Assets/Scripts/Config/BalanceConfig.cs:13` (DayDurationSec)
- Modify: `Assets/Scripts/Config/BalanceConfig.cs:67` (BonfireDamagePerSec)

- [ ] **Step 1: DayDurationSec 수정**

`BalanceConfig.cs` 13번 줄:
```csharp
public float DayDurationSec = 40f;   // 540f→40f: 기획 의도 1사이클 약 2분
```

- [ ] **Step 2: BonfireDamagePerSec 수정**

`BalanceConfig.cs` 67번 줄:
```csharp
public float BonfireDamagePerSec = 2.5f;  // 5f→2.5f: 밤 난이도 회복
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Config/BalanceConfig.cs
git commit -m "[Balance] DayDuration 540s→40s, BonfireDPS 5→2.5"
```

---

### Task 2: CampfireAura — BalanceConfig 단일 소스화

**Files:**
- Modify: `Assets/Scripts/Combat/CampfireAura.cs:12` (DamagePerSecond 하드코딩 제거)
- Modify: `Assets/Scripts/Combat/CampfireAura.cs:36-41` (Awake에서 읽기)

- [ ] **Step 1: Awake()에 BalanceConfig 읽기 추가**

`CampfireAura.cs` Awake() 메서드 (36~41번 줄):
```csharp
private void Awake()
{
    _sr = GetComponent<SpriteRenderer>();
    if (_sr != null) _baseColor = _sr.color;
    VisionRadius = BaseVisionRadius;
    DamagePerSecond = BalanceConfig.Instance.BonfireDamagePerSec;
}
```

- [ ] **Step 2: Inspector 기본값 주석 추가 (선택적 힌트)**

`CampfireAura.cs` 12번 줄 — Inspector에서 덮어쓰기 방지 안내:
```csharp
[Tooltip("런타임에 BalanceConfig.BonfireDamagePerSec으로 덮어씀")]
public float DamagePerSecond = 2.5f;
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Combat/CampfireAura.cs
git commit -m "[Feat] CampfireAura DPS를 BalanceConfig 단일 소스에서 읽도록 수정"
```

---

### Task 3: 빌드 및 수동 검증

- [ ] **Step 1: Unity 빌드**

```
빌드 명령: 프로젝트 CLAUDE.md 또는 package.json 빌드 스크립트 참조
```

- [ ] **Step 2: 수동 검증 체크리스트**
  - 낮 페이즈가 약 40초 후 저녁으로 전환되는지 확인
  - 모닥불 범위 내 좀비가 기존보다 느리게 죽는지 확인 (DPS 절반)
  - 밤이 충분히 긴장감 있게 느껴지는지 체감

- [ ] **Step 3: 버전 패치 업 및 배포 (완료 후)**

`package.json` patch 버전 올리고 빌드/배치.
