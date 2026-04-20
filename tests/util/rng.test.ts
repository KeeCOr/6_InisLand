import { describe, expect, it } from 'vitest';
import { createRng } from '@/util/rng';

describe('createRng', () => {
  it('returns deterministic sequence for same seed', () => {
    const a = createRng(42);
    const b = createRng(42);
    const seqA = [a.next(), a.next(), a.next()];
    const seqB = [b.next(), b.next(), b.next()];
    expect(seqA).toEqual(seqB);
  });

  it('different seeds produce different sequences', () => {
    const a = createRng(1);
    const b = createRng(2);
    expect(a.next()).not.toEqual(b.next());
  });

  it('next() returns values in [0, 1)', () => {
    const rng = createRng(123);
    for (let i = 0; i < 1000; i++) {
      const v = rng.next();
      expect(v).toBeGreaterThanOrEqual(0);
      expect(v).toBeLessThan(1);
    }
  });

  it('intRange(min, max) returns integer in [min, max]', () => {
    const rng = createRng(7);
    for (let i = 0; i < 200; i++) {
      const v = rng.intRange(5, 10);
      expect(Number.isInteger(v)).toBe(true);
      expect(v).toBeGreaterThanOrEqual(5);
      expect(v).toBeLessThanOrEqual(10);
    }
  });

  it('pick() selects from array', () => {
    const rng = createRng(99);
    const arr = ['a', 'b', 'c'];
    for (let i = 0; i < 50; i++) {
      expect(arr).toContain(rng.pick(arr));
    }
  });

  it('getState/setState allows save/restore', () => {
    const rng = createRng(55);
    rng.next();
    rng.next();
    const state = rng.getState();
    const snapshot = [rng.next(), rng.next(), rng.next()];

    rng.setState(state);
    const replay = [rng.next(), rng.next(), rng.next()];

    expect(replay).toEqual(snapshot);
  });
});
