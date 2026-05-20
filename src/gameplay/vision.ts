import { VISION } from '@/config/balance';
import { Phase } from '@/gameplay/day-night-cycle';

export type VisionFadeStep = {
  radius: number;
  alpha: number;
};

export type VisionProfile = {
  clearRadius: number;
  fadeRadius: number;
  darkness: number;
  tint: number;
  fadeSteps: VisionFadeStep[];
};

export function createVisionProfile(phase: Phase, tileSize: number): VisionProfile {
  const isNight = phase === Phase.Night || phase === Phase.Evening;
  const radiusTiles = isNight ? VISION.nightRadiusTiles : VISION.dayRadiusTiles;
  const clearRadius = radiusTiles * tileSize;
  const fadeRadius = clearRadius + (isNight ? tileSize * 4 : tileSize * 3);
  const darkness = isNight ? 0.9 : 0.28;
  const tint = isNight ? 0x071225 : 0x6f8fac;
  const stepCount = isNight ? 8 : 5;
  const fadeSteps: VisionFadeStep[] = [];

  if (!isNight) {
    return {
      clearRadius,
      fadeRadius,
      darkness: 0,
      tint,
      fadeSteps,
    };
  }

  for (let i = 1; i <= stepCount; i++) {
    const t = i / stepCount;
    fadeSteps.push({
      radius: clearRadius + (fadeRadius - clearRadius) * t,
      alpha: darkness * t * t,
    });
  }

  return {
    clearRadius,
    fadeRadius,
    darkness,
    tint,
    fadeSteps,
  };
}
