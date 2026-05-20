import { describe, expect, it } from 'vitest';
import { applyKeysToState } from '@/input/input-adapter';

describe('InputAdapter', () => {
  it('returns zero vector when no keys pressed', () => {
    const state = applyKeysToState(new Set());
    expect(state.dx).toBe(0);
    expect(state.dy).toBe(0);
    expect(state.interact).toBe(false);
  });

  it('returns normalized diagonal movement', () => {
    const state = applyKeysToState(new Set(['W', 'A']));
    expect(state.dx).toBeCloseTo(-Math.SQRT1_2, 5);
    expect(state.dy).toBeCloseTo(-Math.SQRT1_2, 5);
  });

  it('maps W/A/S/D to directions', () => {
    expect(applyKeysToState(new Set(['W'])).dy).toBe(-1);
    expect(applyKeysToState(new Set(['S'])).dy).toBe(1);
    expect(applyKeysToState(new Set(['A'])).dx).toBe(-1);
    expect(applyKeysToState(new Set(['D'])).dx).toBe(1);
  });

  it('maps ArrowKeys as alternative', () => {
    expect(applyKeysToState(new Set(['ARROWUP'])).dy).toBe(-1);
    expect(applyKeysToState(new Set(['ARROWLEFT'])).dx).toBe(-1);
  });

  it('detects interact key (F)', () => {
    const state = applyKeysToState(new Set(['F']));
    expect(state.interact).toBe(true);
  });
});
