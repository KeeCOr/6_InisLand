/**
 * 게임 밸런스 단일 소스. 런타임 변경 시 여기만 수정.
 * Phase 1에서 실제 값들(무기 대미지, 자원 양, 웨이브 크기 등)이 채워짐.
 */

export const DAY_CYCLE = {
  dayDurationSec: 540, // 9분
  nightDurationSec: 360, // 6분
  eveningTransitionSec: 30,
  dawnTransitionSec: 30,
} as const;

export const VISION = {
  dayRadiusTiles: 10,
  nightRadiusTiles: 6,
  megaBlizzardRadiusTiles: 3,
} as const;

export const RESOURCES = {
  startingWood: 15,
  startingStone: 5,
  startingIron: 0,
  startingMeat: 0,
  startingFood: 5,
  startingFrostbloom: 0,
} as const;

export type BalanceConfig = {
  dayCycle: typeof DAY_CYCLE;
  vision: typeof VISION;
  resources: typeof RESOURCES;
};

export const BALANCE: BalanceConfig = {
  dayCycle: DAY_CYCLE,
  vision: VISION,
  resources: RESOURCES,
};
