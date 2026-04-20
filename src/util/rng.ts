/**
 * mulberry32 — 32bit 시드 기반 결정적 PRNG.
 * 도메인 로직(대미지, 날씨, 스폰)에서 Math.random 대신 반드시 이 RNG를 사용.
 */
export interface Rng {
  next(): number;
  intRange(min: number, max: number): number;
  pick<T>(arr: readonly T[]): T;
  getState(): number;
  setState(state: number): void;
}

export function createRng(seed: number): Rng {
  let state = seed >>> 0;

  const next = (): number => {
    state = (state + 0x6d2b79f5) >>> 0;
    let t = state;
    t = Math.imul(t ^ (t >>> 15), t | 1);
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };

  return {
    next,
    intRange(min, max) {
      if (max < min) throw new Error(`intRange: max(${max}) < min(${min})`);
      return Math.floor(next() * (max - min + 1)) + min;
    },
    pick(arr) {
      if (arr.length === 0) throw new Error('pick: empty array');
      const idx = Math.floor(next() * arr.length);
      const v = arr[idx];
      if (v === undefined) throw new Error('pick: undefined element (noUncheckedIndexedAccess)');
      return v;
    },
    getState() {
      return state;
    },
    setState(s) {
      state = s >>> 0;
    },
  };
}
