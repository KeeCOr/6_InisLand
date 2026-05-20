import { DAY_CYCLE } from '@/config/balance';
import type { EventBus } from '@/events/event-bus';
import type { GameEvents } from '@/events/types';

export enum Phase {
  Day = 'day',
  Evening = 'evening',
  Night = 'night',
  Dawn = 'dawn',
}

const PHASE_DURATIONS: Record<Phase, number> = {
  [Phase.Day]: DAY_CYCLE.dayDurationSec * 1000,
  [Phase.Evening]: DAY_CYCLE.eveningTransitionSec * 1000,
  [Phase.Night]: DAY_CYCLE.nightDurationSec * 1000,
  [Phase.Dawn]: DAY_CYCLE.dawnTransitionSec * 1000,
};

const PHASE_ORDER: Phase[] = [Phase.Day, Phase.Evening, Phase.Night, Phase.Dawn];

export class DayNightCycle {
  phase: Phase = Phase.Day;
  day = 1;
  private elapsed = 0;

  constructor(private readonly bus: EventBus<GameEvents>) {}

  get phaseDuration(): number {
    return PHASE_DURATIONS[this.phase];
  }

  get phaseProgress(): number {
    return Math.min(this.elapsed / this.phaseDuration, 1);
  }

  get remainingSec(): number {
    return Math.max((this.phaseDuration - this.elapsed) / 1000, 0);
  }

  update(dtMs: number): void {
    this.elapsed += dtMs;
    while (this.elapsed >= this.phaseDuration) {
      this.elapsed -= this.phaseDuration;
      this.advancePhase();
    }
  }

  private advancePhase(): void {
    const idx = PHASE_ORDER.indexOf(this.phase);
    const nextIdx = (idx + 1) % PHASE_ORDER.length;
    const next = PHASE_ORDER[nextIdx]!;

    if (this.phase === Phase.Day) this.bus.emit('day:ended', { day: this.day });
    if (this.phase === Phase.Night) this.bus.emit('night:ended', { day: this.day });

    this.phase = next;

    if (this.phase === Phase.Day) this.day++;

    if (this.phase === Phase.Day) this.bus.emit('day:started', { day: this.day });
    if (this.phase === Phase.Evening) this.bus.emit('evening:started', { day: this.day });
    if (this.phase === Phase.Night) this.bus.emit('night:started', { day: this.day });
    if (this.phase === Phase.Dawn) this.bus.emit('dawn:started', { day: this.day });
  }
}
