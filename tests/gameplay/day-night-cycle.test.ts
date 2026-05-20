import { describe, expect, it } from 'vitest';
import { DayNightCycle, Phase } from '@/gameplay/day-night-cycle';
import { EventBus } from '@/events/event-bus';
import type { GameEvents } from '@/events/types';
import { DAY_CYCLE } from '@/config/balance';

describe('DayNightCycle', () => {
  function makeCycle() {
    const bus = new EventBus<GameEvents>();
    return { cycle: new DayNightCycle(bus), bus };
  }

  it('starts at Day phase, day 1', () => {
    const { cycle } = makeCycle();
    expect(cycle.phase).toBe(Phase.Day);
    expect(cycle.day).toBe(1);
  });

  it('transitions Day -> Evening after dayDurationSec', () => {
    const { cycle } = makeCycle();
    cycle.update(DAY_CYCLE.dayDurationSec * 1000);
    expect(cycle.phase).toBe(Phase.Evening);
  });

  it('transitions Evening -> Night after eveningTransitionSec', () => {
    const { cycle } = makeCycle();
    cycle.update(DAY_CYCLE.dayDurationSec * 1000);
    cycle.update(DAY_CYCLE.eveningTransitionSec * 1000);
    expect(cycle.phase).toBe(Phase.Night);
  });

  it('transitions Night -> Dawn -> Day, incrementing day', () => {
    const { cycle } = makeCycle();
    cycle.update(DAY_CYCLE.dayDurationSec * 1000);
    cycle.update(DAY_CYCLE.eveningTransitionSec * 1000);
    cycle.update(DAY_CYCLE.nightDurationSec * 1000);
    expect(cycle.phase).toBe(Phase.Dawn);
    cycle.update(DAY_CYCLE.dawnTransitionSec * 1000);
    expect(cycle.phase).toBe(Phase.Day);
    expect(cycle.day).toBe(2);
  });

  it('emits phase events', () => {
    const { cycle, bus } = makeCycle();
    const events: string[] = [];
    bus.on('night:started', () => events.push('night'));
    bus.on('day:started', () => events.push('day'));
    cycle.update(DAY_CYCLE.dayDurationSec * 1000);
    cycle.update(DAY_CYCLE.eveningTransitionSec * 1000);
    expect(events).toContain('night');
  });

  it('reports progress 0..1 within current phase', () => {
    const { cycle } = makeCycle();
    cycle.update(DAY_CYCLE.dayDurationSec * 500);
    expect(cycle.phaseProgress).toBeCloseTo(0.5, 1);
  });

  it('reports remaining seconds in current phase', () => {
    const { cycle } = makeCycle();
    expect(cycle.remainingSec).toBeCloseTo(DAY_CYCLE.dayDurationSec, 0);
    cycle.update(10_000);
    expect(cycle.remainingSec).toBeCloseTo(DAY_CYCLE.dayDurationSec - 10, 0);
  });
});
