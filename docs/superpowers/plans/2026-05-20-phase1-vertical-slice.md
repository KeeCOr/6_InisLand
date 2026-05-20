# Phase 1 Vertical Slice — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a playable 1-day loop: explore snowfield by day, defend village by night, survive or die.

**Architecture:** Single `GameScene` manages both day/night phases with camera repositioning. ECS (bitecs) handles all combat entities (player, zombies, deer, projectiles) for performance. Phaser GameObjects handle buildings, resource nodes, and UI. A `DayNightCycle` state machine drives phase transitions. Procedural snowfield generated per session via seeded RNG.

**Tech Stack:** Phaser 3, TypeScript (strict), bitecs ECS, Vite, Vitest

---

## File Structure

```
src/
  ecs/
    components.ts              # All ECS components (Position, Velocity, Health, Combat, PlayerTag, ZombieTag, DeerTag)
    systems/
      movement-system.ts       # Applies velocity to position, syncs Phaser sprites
      combat-system.ts         # Auto-attack: find targets in range, deal damage on cooldown
      zombie-ai-system.ts      # Zombie pathfinding toward player/village
      deer-ai-system.ts        # Deer flee behavior
      death-system.ts          # Remove dead entities, emit events
    world.ts                   # (existing) - extend GameWorld with sprite registry
  scenes/
    boot.ts                    # (existing)
    preload.ts                 # Load placeholder assets (colored rectangles via generateTexture)
    game.ts                    # Main game scene - day/night loop, input, spawning, rendering
  gameplay/
    day-night-cycle.ts         # State machine: Day -> Evening -> Night -> Dawn -> Day
    resource-manager.ts        # Track resources, handle gathering
    village-grid.ts            # 24x24 grid, building placement, bonfire/barricade state
    wave-spawner.ts            # Zombie wave timing and spawning logic
  ui/
    hud.ts                     # Resource display, HP bar, day/time indicator, wave counter
    game-over.ts               # Death screen with stats
  input/
    input-adapter.ts           # Keyboard -> movement vector + action keys
  config/
    balance.ts                 # (existing) - extend with combat/wave/gathering values
    constants.ts               # (existing)
    weapons.ts                 # Weapon definitions (longsword only for Phase 1)
    enemies.ts                 # Enemy definitions (basic zombie only)
  events/
    event-bus.ts               # (existing)
    types.ts                   # (existing) - extend with new events
  util/
    rng.ts                     # (existing)
    sprite-registry.ts         # Map ECS entity IDs to Phaser sprites
```

---

### Task 1: ECS Components

**Files:**
- Create: `src/ecs/components.ts`
- Test: `tests/ecs/components.test.ts`

- [ ] **Step 1: Write the test**

```ts
// tests/ecs/components.test.ts
import { describe, expect, it } from 'vitest';
import { createGameWorld, addEntity, addComponent, hasComponent } from '@/ecs/world';
import {
  Position, Velocity, Health, Combat, PlayerTag, ZombieTag, DeerTag,
  ResourceNode, Warmth,
} from '@/ecs/components';

describe('ECS components', () => {
  it('attaches Position and Velocity to an entity', () => {
    const w = createGameWorld();
    const e = addEntity(w);
    addComponent(w, Position, e);
    addComponent(w, Velocity, e);
    Position.x[e] = 100;
    Position.y[e] = 200;
    Velocity.vx[e] = 5;
    Velocity.vy[e] = -3;
    expect(Position.x[e]).toBe(100);
    expect(Velocity.vx[e]).toBe(5);
  });

  it('attaches Health with current and max', () => {
    const w = createGameWorld();
    const e = addEntity(w);
    addComponent(w, Health, e);
    Health.current[e] = 100;
    Health.max[e] = 100;
    expect(Health.current[e]).toBe(100);
  });

  it('attaches Combat with weapon stats', () => {
    const w = createGameWorld();
    const e = addEntity(w);
    addComponent(w, Combat, e);
    Combat.damage[e] = 25;
    Combat.range[e] = 48;
    Combat.cooldown[e] = 500;
    Combat.lastAttackTime[e] = 0;
    expect(Combat.damage[e]).toBe(25);
    expect(Combat.range[e]).toBe(48);
  });

  it('attaches tag components', () => {
    const w = createGameWorld();
    const e = addEntity(w);
    addComponent(w, PlayerTag, e);
    expect(hasComponent(w, PlayerTag, e)).toBe(true);
    expect(hasComponent(w, ZombieTag, e)).toBe(false);
  });

  it('attaches ResourceNode with type and amount', () => {
    const w = createGameWorld();
    const e = addEntity(w);
    addComponent(w, ResourceNode, e);
    ResourceNode.kind[e] = 0; // 0=wood, 1=stone, 2=meat
    ResourceNode.amount[e] = 5;
    ResourceNode.gatherTime[e] = 3000;
    expect(ResourceNode.kind[e]).toBe(0);
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run tests/ecs/components.test.ts`
Expected: FAIL — module `@/ecs/components` not found

- [ ] **Step 3: Implement components**

```ts
// src/ecs/components.ts
import { defineComponent, Types } from 'bitecs';

// --- Spatial ---
export const Position = defineComponent({ x: Types.f32, y: Types.f32 });
export const Velocity = defineComponent({ vx: Types.f32, vy: Types.f32 });

// --- Combat ---
export const Health = defineComponent({ current: Types.f32, max: Types.f32 });
export const Combat = defineComponent({
  damage: Types.f32,
  range: Types.f32,       // pixels
  cooldown: Types.f32,    // ms between attacks
  lastAttackTime: Types.f32,
});

// --- Tags ---
export const PlayerTag = defineComponent();
export const ZombieTag = defineComponent();
export const DeerTag = defineComponent();

// --- Resource Nodes ---
/** kind: 0=wood, 1=stone, 2=meat */
export const ResourceNode = defineComponent({
  kind: Types.ui8,
  amount: Types.ui8,
  gatherTime: Types.f32,  // ms to gather
});

// --- Environment ---
export const Warmth = defineComponent({ radius: Types.f32 });
```

- [ ] **Step 4: Run test to verify it passes**

Run: `npx vitest run tests/ecs/components.test.ts`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/ecs/components.ts tests/ecs/components.test.ts
git commit -m "feat(ecs): add Phase 1 components — Position, Velocity, Health, Combat, tags, ResourceNode"
```

---

### Task 2: Sprite Registry + Extended GameWorld

**Files:**
- Create: `src/util/sprite-registry.ts`
- Test: `tests/util/sprite-registry.test.ts`
- Modify: `src/ecs/world.ts`

- [ ] **Step 1: Write the test**

```ts
// tests/util/sprite-registry.test.ts
import { describe, expect, it } from 'vitest';
import { SpriteRegistry } from '@/util/sprite-registry';

describe('SpriteRegistry', () => {
  it('registers and retrieves a sprite-like object', () => {
    const reg = new SpriteRegistry();
    const fakeSprite = { x: 0, y: 0, destroy: () => {} } as any;
    reg.set(1, fakeSprite);
    expect(reg.get(1)).toBe(fakeSprite);
  });

  it('returns undefined for unknown entity', () => {
    const reg = new SpriteRegistry();
    expect(reg.get(999)).toBeUndefined();
  });

  it('removes a sprite', () => {
    const reg = new SpriteRegistry();
    const fakeSprite = { x: 0, y: 0, destroy: () => {} } as any;
    reg.set(1, fakeSprite);
    reg.delete(1);
    expect(reg.get(1)).toBeUndefined();
  });

  it('clear removes all entries', () => {
    const reg = new SpriteRegistry();
    reg.set(1, { x: 0 } as any);
    reg.set(2, { x: 0 } as any);
    reg.clear();
    expect(reg.get(1)).toBeUndefined();
    expect(reg.get(2)).toBeUndefined();
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run tests/util/sprite-registry.test.ts`
Expected: FAIL — module not found

- [ ] **Step 3: Implement SpriteRegistry**

```ts
// src/util/sprite-registry.ts
import type Phaser from 'phaser';

type SpriteRef = Phaser.GameObjects.Sprite | Phaser.GameObjects.Rectangle;

export class SpriteRegistry {
  private readonly map = new Map<number, SpriteRef>();

  set(eid: number, sprite: SpriteRef): void {
    this.map.set(eid, sprite);
  }

  get(eid: number): SpriteRef | undefined {
    return this.map.get(eid);
  }

  delete(eid: number): void {
    this.map.delete(eid);
  }

  clear(): void {
    this.map.clear();
  }

  forEach(fn: (sprite: SpriteRef, eid: number) => void): void {
    this.map.forEach(fn);
  }
}
```

- [ ] **Step 4: Extend GameWorld**

```ts
// src/ecs/world.ts — replace full file
import {
  addComponent as bitAddComponent,
  addEntity as bitAddEntity,
  createWorld as bitCreateWorld,
  hasComponent as bitHasComponent,
  removeComponent as bitRemoveComponent,
  removeEntity as bitRemoveEntity,
  type Component,
  type IWorld,
} from 'bitecs';
import { SpriteRegistry } from '@/util/sprite-registry';

export interface GameWorld extends IWorld {
  deltaTime: number;
  elapsed: number;
  sprites: SpriteRegistry;
}

export function createGameWorld(): GameWorld {
  const w = bitCreateWorld() as GameWorld;
  w.deltaTime = 0;
  w.elapsed = 0;
  w.sprites = new SpriteRegistry();
  return w;
}

export function addEntity(world: GameWorld): number {
  return bitAddEntity(world);
}

export function removeEntity(world: GameWorld, eid: number): void {
  bitRemoveEntity(world, eid);
}

export function addComponent(world: GameWorld, component: Component, eid: number): void {
  bitAddComponent(world, component, eid);
}

export function removeComponent(world: GameWorld, component: Component, eid: number): void {
  bitRemoveComponent(world, component, eid);
}

export function hasComponent(world: GameWorld, component: Component, eid: number): boolean {
  return bitHasComponent(world, component, eid);
}
```

- [ ] **Step 5: Run all tests**

Run: `npx vitest run`
Expected: ALL PASS (existing world tests still pass since interface is superset)

- [ ] **Step 6: Commit**

```bash
git add src/util/sprite-registry.ts tests/util/sprite-registry.test.ts src/ecs/world.ts
git commit -m "feat: add SpriteRegistry, extend GameWorld with sprite registry"
```

---

### Task 3: Input Adapter

**Files:**
- Create: `src/input/input-adapter.ts`
- Test: `tests/input/input-adapter.test.ts`

- [ ] **Step 1: Write the test**

```ts
// tests/input/input-adapter.test.ts
import { describe, expect, it } from 'vitest';
import { InputState, applyKeysToState } from '@/input/input-adapter';

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
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run tests/input/input-adapter.test.ts`
Expected: FAIL

- [ ] **Step 3: Implement InputAdapter**

```ts
// src/input/input-adapter.ts

export interface InputState {
  dx: number; // -1..1
  dy: number; // -1..1
  interact: boolean;
}

export function applyKeysToState(pressed: Set<string>): InputState {
  let dx = 0;
  let dy = 0;

  if (pressed.has('W') || pressed.has('ARROWUP')) dy -= 1;
  if (pressed.has('S') || pressed.has('ARROWDOWN')) dy += 1;
  if (pressed.has('A') || pressed.has('ARROWLEFT')) dx -= 1;
  if (pressed.has('D') || pressed.has('ARROWRIGHT')) dx += 1;

  // Normalize diagonal
  const len = Math.sqrt(dx * dx + dy * dy);
  if (len > 1) {
    dx /= len;
    dy /= len;
  }

  return {
    dx,
    dy,
    interact: pressed.has('F'),
  };
}

/**
 * Phaser keyboard listener adapter. Call pollInput(scene) each frame.
 */
export class InputAdapter {
  private readonly pressed = new Set<string>();

  register(scene: Phaser.Scene): void {
    scene.input.keyboard!.on('keydown', (e: KeyboardEvent) => {
      this.pressed.add(e.key.toUpperCase());
    });
    scene.input.keyboard!.on('keyup', (e: KeyboardEvent) => {
      this.pressed.delete(e.key.toUpperCase());
    });
  }

  poll(): InputState {
    return applyKeysToState(this.pressed);
  }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `npx vitest run tests/input/input-adapter.test.ts`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/input/input-adapter.ts tests/input/input-adapter.test.ts
git commit -m "feat: add InputAdapter with WASD/arrow key support"
```

---

### Task 4: Day/Night Cycle State Machine

**Files:**
- Create: `src/gameplay/day-night-cycle.ts`
- Test: `tests/gameplay/day-night-cycle.test.ts`
- Modify: `src/events/types.ts` — add cycle events

- [ ] **Step 1: Write the test**

```ts
// tests/gameplay/day-night-cycle.test.ts
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
    cycle.update(DAY_CYCLE.dayDurationSec * 500); // half day
    expect(cycle.phaseProgress).toBeCloseTo(0.5, 1);
  });

  it('reports remaining seconds in current phase', () => {
    const { cycle } = makeCycle();
    expect(cycle.remainingSec).toBeCloseTo(DAY_CYCLE.dayDurationSec, 0);
    cycle.update(10_000);
    expect(cycle.remainingSec).toBeCloseTo(DAY_CYCLE.dayDurationSec - 10, 0);
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run tests/gameplay/day-night-cycle.test.ts`
Expected: FAIL

- [ ] **Step 3: Update event types**

```ts
// src/events/types.ts — replace full file
export type GameEvents = {
  'day:started': { day: number };
  'day:ended': { day: number };
  'evening:started': { day: number };
  'night:started': { day: number };
  'night:ended': { day: number };
  'dawn:started': { day: number };
  'player:damaged': { amount: number; remaining: number };
  'player:died': { day: number };
  'zombie:died': { id: number; position: { x: number; y: number } };
  'resource:changed': { kind: string; delta: number; total: number };
  'resource:gathered': { kind: string; amount: number };
  'wave:started': { waveNumber: number; count: number };
  'wave:cleared': { waveNumber: number };
};
```

- [ ] **Step 4: Implement DayNightCycle**

```ts
// src/gameplay/day-night-cycle.ts
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

    // Emit end events
    if (this.phase === Phase.Day) this.bus.emit('day:ended', { day: this.day });
    if (this.phase === Phase.Night) this.bus.emit('night:ended', { day: this.day });

    // Advance
    this.phase = next;

    // Day increments when returning to Day
    if (this.phase === Phase.Day) this.day++;

    // Emit start events
    if (this.phase === Phase.Day) this.bus.emit('day:started', { day: this.day });
    if (this.phase === Phase.Evening) this.bus.emit('evening:started', { day: this.day });
    if (this.phase === Phase.Night) this.bus.emit('night:started', { day: this.day });
    if (this.phase === Phase.Dawn) this.bus.emit('dawn:started', { day: this.day });
  }
}
```

- [ ] **Step 5: Run tests**

Run: `npx vitest run tests/gameplay/day-night-cycle.test.ts`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add src/gameplay/day-night-cycle.ts tests/gameplay/day-night-cycle.test.ts src/events/types.ts
git commit -m "feat: add DayNightCycle state machine with phase events"
```

---

### Task 5: Resource Manager

**Files:**
- Create: `src/gameplay/resource-manager.ts`
- Test: `tests/gameplay/resource-manager.test.ts`

- [ ] **Step 1: Write the test**

```ts
// tests/gameplay/resource-manager.test.ts
import { describe, expect, it } from 'vitest';
import { ResourceManager, ResourceKind } from '@/gameplay/resource-manager';
import { EventBus } from '@/events/event-bus';
import type { GameEvents } from '@/events/types';

describe('ResourceManager', () => {
  function make() {
    const bus = new EventBus<GameEvents>();
    return { rm: new ResourceManager(bus), bus };
  }

  it('initializes with starting resources from balance config', () => {
    const { rm } = make();
    expect(rm.get(ResourceKind.Wood)).toBe(15);
    expect(rm.get(ResourceKind.Stone)).toBe(5);
    expect(rm.get(ResourceKind.Food)).toBe(5);
  });

  it('adds resources', () => {
    const { rm } = make();
    rm.add(ResourceKind.Wood, 10);
    expect(rm.get(ResourceKind.Wood)).toBe(25);
  });

  it('spends resources and returns true if sufficient', () => {
    const { rm } = make();
    expect(rm.spend(ResourceKind.Wood, 10)).toBe(true);
    expect(rm.get(ResourceKind.Wood)).toBe(5);
  });

  it('returns false and does not deduct if insufficient', () => {
    const { rm } = make();
    expect(rm.spend(ResourceKind.Wood, 100)).toBe(false);
    expect(rm.get(ResourceKind.Wood)).toBe(15);
  });

  it('emits resource:changed on add', () => {
    const { rm, bus } = make();
    let event: any;
    bus.on('resource:changed', (e) => { event = e; });
    rm.add(ResourceKind.Stone, 3);
    expect(event).toEqual({ kind: 'stone', delta: 3, total: 8 });
  });

  it('canAfford checks without spending', () => {
    const { rm } = make();
    expect(rm.canAfford(ResourceKind.Wood, 15)).toBe(true);
    expect(rm.canAfford(ResourceKind.Wood, 16)).toBe(false);
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run tests/gameplay/resource-manager.test.ts`
Expected: FAIL

- [ ] **Step 3: Implement ResourceManager**

```ts
// src/gameplay/resource-manager.ts
import { RESOURCES } from '@/config/balance';
import type { EventBus } from '@/events/event-bus';
import type { GameEvents } from '@/events/types';

export enum ResourceKind {
  Wood = 'wood',
  Stone = 'stone',
  Iron = 'iron',
  Meat = 'meat',
  Food = 'food',
  Frostbloom = 'frostbloom',
}

const STARTING: Record<ResourceKind, number> = {
  [ResourceKind.Wood]: RESOURCES.startingWood,
  [ResourceKind.Stone]: RESOURCES.startingStone,
  [ResourceKind.Iron]: RESOURCES.startingIron,
  [ResourceKind.Meat]: RESOURCES.startingMeat,
  [ResourceKind.Food]: RESOURCES.startingFood,
  [ResourceKind.Frostbloom]: RESOURCES.startingFrostbloom,
};

export class ResourceManager {
  private readonly amounts = new Map<ResourceKind, number>();

  constructor(private readonly bus: EventBus<GameEvents>) {
    for (const kind of Object.values(ResourceKind)) {
      this.amounts.set(kind, STARTING[kind]);
    }
  }

  get(kind: ResourceKind): number {
    return this.amounts.get(kind) ?? 0;
  }

  add(kind: ResourceKind, amount: number): void {
    const next = this.get(kind) + amount;
    this.amounts.set(kind, next);
    this.bus.emit('resource:changed', { kind, delta: amount, total: next });
  }

  spend(kind: ResourceKind, amount: number): boolean {
    const cur = this.get(kind);
    if (cur < amount) return false;
    const next = cur - amount;
    this.amounts.set(kind, next);
    this.bus.emit('resource:changed', { kind, delta: -amount, total: next });
    return true;
  }

  canAfford(kind: ResourceKind, amount: number): boolean {
    return this.get(kind) >= amount;
  }

  getAll(): Record<ResourceKind, number> {
    const result = {} as Record<ResourceKind, number>;
    for (const kind of Object.values(ResourceKind)) {
      result[kind] = this.get(kind);
    }
    return result;
  }
}
```

- [ ] **Step 4: Run test**

Run: `npx vitest run tests/gameplay/resource-manager.test.ts`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/gameplay/resource-manager.ts tests/gameplay/resource-manager.test.ts
git commit -m "feat: add ResourceManager with starting resources and events"
```

---

### Task 6: Weapon & Enemy Data + Combat Logic

**Files:**
- Create: `src/config/weapons.ts`
- Create: `src/config/enemies.ts`
- Create: `src/gameplay/combat.ts`
- Test: `tests/gameplay/combat.test.ts`

- [ ] **Step 1: Write the test**

```ts
// tests/gameplay/combat.test.ts
import { describe, expect, it } from 'vitest';
import { calculateDamage } from '@/gameplay/combat';
import { LONGSWORD } from '@/config/weapons';
import { BASIC_ZOMBIE } from '@/config/enemies';

describe('combat', () => {
  it('calculates base damage = weapon damage - enemy defense', () => {
    const dmg = calculateDamage(LONGSWORD.damage, 0, BASIC_ZOMBIE.defense);
    expect(dmg).toBe(LONGSWORD.damage - BASIC_ZOMBIE.defense);
  });

  it('never goes below 1', () => {
    const dmg = calculateDamage(1, 0, 999);
    expect(dmg).toBe(1);
  });

  it('applies critical multiplier', () => {
    const base = LONGSWORD.damage - BASIC_ZOMBIE.defense;
    const dmg = calculateDamage(LONGSWORD.damage, 0, BASIC_ZOMBIE.defense, 2.0);
    expect(dmg).toBe(Math.max(1, Math.floor(base * 2.0)));
  });

  it('applies rune bonus as additive multiplier', () => {
    const runeBonus = 0.5; // +50%
    const base = LONGSWORD.damage - BASIC_ZOMBIE.defense;
    const dmg = calculateDamage(LONGSWORD.damage, runeBonus, BASIC_ZOMBIE.defense);
    expect(dmg).toBe(Math.max(1, Math.floor(base * (1 + runeBonus))));
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run tests/gameplay/combat.test.ts`
Expected: FAIL

- [ ] **Step 3: Implement weapon/enemy data and combat function**

```ts
// src/config/weapons.ts
export interface WeaponDef {
  id: string;
  name: string;
  damage: number;
  range: number;       // pixels
  cooldownMs: number;
  pattern: 'melee_sweep';
}

export const LONGSWORD: WeaponDef = {
  id: 'longsword',
  name: '롱소드',
  damage: 25,
  range: 48,
  cooldownMs: 600,
  pattern: 'melee_sweep',
};
```

```ts
// src/config/enemies.ts
export interface EnemyDef {
  id: string;
  name: string;
  hp: number;
  damage: number;
  defense: number;
  speed: number;        // pixels/sec
  attackRange: number;  // pixels
  attackCooldownMs: number;
}

export const BASIC_ZOMBIE: EnemyDef = {
  id: 'zombie_basic',
  name: '좀비',
  hp: 60,
  damage: 10,
  defense: 2,
  speed: 40,
  attackRange: 32,
  attackCooldownMs: 1200,
};
```

```ts
// src/gameplay/combat.ts
/**
 * Damage formula from spec section 4.6:
 * finalDamage = (baseDamage - defense) * (1 + runeBonus) * critMultiplier
 * Minimum 1.
 */
export function calculateDamage(
  baseDamage: number,
  runeBonus: number,
  defense: number,
  critMultiplier = 1.0,
): number {
  const raw = (baseDamage - defense) * (1 + runeBonus) * critMultiplier;
  return Math.max(1, Math.floor(raw));
}
```

- [ ] **Step 4: Run test**

Run: `npx vitest run tests/gameplay/combat.test.ts`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/config/weapons.ts src/config/enemies.ts src/gameplay/combat.ts tests/gameplay/combat.test.ts
git commit -m "feat: add weapon/enemy defs and damage calculation"
```

---

### Task 7: ECS Systems — Movement, CombatSystem, ZombieAI, DeerAI, DeathSystem

**Files:**
- Create: `src/ecs/systems/movement-system.ts`
- Create: `src/ecs/systems/combat-system.ts`
- Create: `src/ecs/systems/zombie-ai-system.ts`
- Create: `src/ecs/systems/deer-ai-system.ts`
- Create: `src/ecs/systems/death-system.ts`
- Test: `tests/ecs/systems/movement-system.test.ts`
- Test: `tests/ecs/systems/combat-system.test.ts`
- Test: `tests/ecs/systems/zombie-ai-system.test.ts`
- Test: `tests/ecs/systems/death-system.test.ts`

- [ ] **Step 1: Write movement system test**

```ts
// tests/ecs/systems/movement-system.test.ts
import { describe, expect, it } from 'vitest';
import { createGameWorld, addEntity, addComponent } from '@/ecs/world';
import { Position, Velocity } from '@/ecs/components';
import { movementSystem } from '@/ecs/systems/movement-system';

describe('movementSystem', () => {
  it('applies velocity * dt to position', () => {
    const w = createGameWorld();
    const e = addEntity(w);
    addComponent(w, Position, e);
    addComponent(w, Velocity, e);
    Position.x[e] = 0;
    Position.y[e] = 0;
    Velocity.vx[e] = 100;
    Velocity.vy[e] = 50;
    w.deltaTime = 1 / 30; // one tick

    movementSystem(w);

    expect(Position.x[e]).toBeCloseTo(100 / 30, 2);
    expect(Position.y[e]).toBeCloseTo(50 / 30, 2);
  });

  it('handles multiple entities', () => {
    const w = createGameWorld();
    const e1 = addEntity(w);
    const e2 = addEntity(w);
    for (const e of [e1, e2]) {
      addComponent(w, Position, e);
      addComponent(w, Velocity, e);
    }
    Velocity.vx[e1] = 10;
    Velocity.vx[e2] = -10;
    w.deltaTime = 1;

    movementSystem(w);

    expect(Position.x[e1]).toBeCloseTo(10);
    expect(Position.x[e2]).toBeCloseTo(-10);
  });
});
```

- [ ] **Step 2: Implement movement system**

```ts
// src/ecs/systems/movement-system.ts
import { defineQuery } from 'bitecs';
import { Position, Velocity } from '@/ecs/components';
import type { GameWorld } from '@/ecs/world';

const movingQuery = defineQuery([Position, Velocity]);

export function movementSystem(world: GameWorld): void {
  const dt = world.deltaTime;
  const entities = movingQuery(world);
  for (let i = 0; i < entities.length; i++) {
    const eid = entities[i]!;
    Position.x[eid] += Velocity.vx[eid]! * dt;
    Position.y[eid] += Velocity.vy[eid]! * dt;
  }
}
```

- [ ] **Step 3: Run movement test**

Run: `npx vitest run tests/ecs/systems/movement-system.test.ts`
Expected: PASS

- [ ] **Step 4: Write combat system test**

```ts
// tests/ecs/systems/combat-system.test.ts
import { describe, expect, it } from 'vitest';
import { createGameWorld, addEntity, addComponent } from '@/ecs/world';
import { Position, Health, Combat, PlayerTag, ZombieTag } from '@/ecs/components';
import { combatSystem } from '@/ecs/systems/combat-system';

describe('combatSystem', () => {
  function setup(playerX: number, zombieX: number, range: number) {
    const w = createGameWorld();
    const player = addEntity(w);
    addComponent(w, Position, player);
    addComponent(w, Health, player);
    addComponent(w, Combat, player);
    addComponent(w, PlayerTag, player);
    Position.x[player] = playerX;
    Position.y[player] = 0;
    Health.current[player] = 100;
    Health.max[player] = 100;
    Combat.damage[player] = 25;
    Combat.range[player] = range;
    Combat.cooldown[player] = 500;
    Combat.lastAttackTime[player] = 0;

    const zombie = addEntity(w);
    addComponent(w, Position, zombie);
    addComponent(w, Health, zombie);
    addComponent(w, Combat, zombie);
    addComponent(w, ZombieTag, zombie);
    Position.x[zombie] = zombieX;
    Position.y[zombie] = 0;
    Health.current[zombie] = 60;
    Health.max[zombie] = 60;
    Combat.damage[zombie] = 10;
    Combat.range[zombie] = 32;
    Combat.cooldown[zombie] = 1200;
    Combat.lastAttackTime[zombie] = 0;

    return { w, player, zombie };
  }

  it('player attacks zombie in range when cooldown elapsed', () => {
    const { w, zombie } = setup(0, 30, 48);
    w.elapsed = 600; // past cooldown
    combatSystem(w);
    expect(Health.current[zombie]).toBeLessThan(60);
  });

  it('does not attack if target out of range', () => {
    const { w, zombie } = setup(0, 200, 48);
    w.elapsed = 600;
    combatSystem(w);
    expect(Health.current[zombie]).toBe(60);
  });

  it('does not attack if cooldown not elapsed', () => {
    const { w, zombie } = setup(0, 30, 48);
    w.elapsed = 100; // cooldown 500, not reached
    combatSystem(w);
    expect(Health.current[zombie]).toBe(60);
  });

  it('zombie attacks player in range', () => {
    const { w, player } = setup(0, 20, 48);
    w.elapsed = 1300; // past zombie cooldown 1200
    combatSystem(w);
    expect(Health.current[player]).toBeLessThan(100);
  });
});
```

- [ ] **Step 5: Implement combat system**

```ts
// src/ecs/systems/combat-system.ts
import { defineQuery } from 'bitecs';
import { Position, Health, Combat, PlayerTag, ZombieTag } from '@/ecs/components';
import type { GameWorld } from '@/ecs/world';
import { calculateDamage } from '@/gameplay/combat';

const playerQuery = defineQuery([Position, Health, Combat, PlayerTag]);
const zombieQuery = defineQuery([Position, Health, Combat, ZombieTag]);

function distSq(ax: number, ay: number, bx: number, by: number): number {
  const dx = ax - bx;
  const dy = ay - by;
  return dx * dx + dy * dy;
}

export function combatSystem(world: GameWorld): void {
  const players = playerQuery(world);
  const zombies = zombieQuery(world);
  const now = world.elapsed;

  // Player attacks nearest zombie in range
  for (let i = 0; i < players.length; i++) {
    const pid = players[i]!;
    const range = Combat.range[pid]!;
    const rangeSq = range * range;
    const cd = Combat.cooldown[pid]!;
    if (now - Combat.lastAttackTime[pid]! < cd) continue;

    let nearest = -1;
    let nearestDistSq = Infinity;
    for (let j = 0; j < zombies.length; j++) {
      const zid = zombies[j]!;
      if (Health.current[zid]! <= 0) continue;
      const d = distSq(Position.x[pid]!, Position.y[pid]!, Position.x[zid]!, Position.y[zid]!);
      if (d <= rangeSq && d < nearestDistSq) {
        nearest = zid;
        nearestDistSq = d;
      }
    }
    if (nearest >= 0) {
      const dmg = calculateDamage(Combat.damage[pid]!, 0, 0);
      Health.current[nearest] -= dmg;
      Combat.lastAttackTime[pid] = now;
    }
  }

  // Zombies attack nearest player in range
  for (let i = 0; i < zombies.length; i++) {
    const zid = zombies[i]!;
    if (Health.current[zid]! <= 0) continue;
    const range = Combat.range[zid]!;
    const rangeSq = range * range;
    const cd = Combat.cooldown[zid]!;
    if (now - Combat.lastAttackTime[zid]! < cd) continue;

    for (let j = 0; j < players.length; j++) {
      const pid = players[j]!;
      const d = distSq(Position.x[zid]!, Position.y[zid]!, Position.x[pid]!, Position.y[pid]!);
      if (d <= rangeSq) {
        const dmg = calculateDamage(Combat.damage[zid]!, 0, 0);
        Health.current[pid] -= dmg;
        Combat.lastAttackTime[zid] = now;
        break;
      }
    }
  }
}
```

- [ ] **Step 6: Run combat test**

Run: `npx vitest run tests/ecs/systems/combat-system.test.ts`
Expected: PASS

- [ ] **Step 7: Write zombie AI test**

```ts
// tests/ecs/systems/zombie-ai-system.test.ts
import { describe, expect, it } from 'vitest';
import { createGameWorld, addEntity, addComponent } from '@/ecs/world';
import { Position, Velocity, Health, ZombieTag, PlayerTag } from '@/ecs/components';
import { zombieAiSystem } from '@/ecs/systems/zombie-ai-system';
import { BASIC_ZOMBIE } from '@/config/enemies';

describe('zombieAiSystem', () => {
  it('sets zombie velocity toward player', () => {
    const w = createGameWorld();
    const player = addEntity(w);
    addComponent(w, Position, player);
    addComponent(w, PlayerTag, player);
    addComponent(w, Health, player);
    Position.x[player] = 200;
    Position.y[player] = 0;
    Health.current[player] = 100;
    Health.max[player] = 100;

    const zombie = addEntity(w);
    addComponent(w, Position, zombie);
    addComponent(w, Velocity, zombie);
    addComponent(w, Health, zombie);
    addComponent(w, ZombieTag, zombie);
    Position.x[zombie] = 0;
    Position.y[zombie] = 0;
    Health.current[zombie] = 60;
    Health.max[zombie] = 60;
    Velocity.vx[zombie] = 0;
    Velocity.vy[zombie] = 0;

    zombieAiSystem(w);

    expect(Velocity.vx[zombie]).toBeCloseTo(BASIC_ZOMBIE.speed, 0);
    expect(Velocity.vy[zombie]).toBeCloseTo(0, 0);
  });

  it('stops dead zombies', () => {
    const w = createGameWorld();
    const zombie = addEntity(w);
    addComponent(w, Position, zombie);
    addComponent(w, Velocity, zombie);
    addComponent(w, Health, zombie);
    addComponent(w, ZombieTag, zombie);
    Health.current[zombie] = 0;
    Velocity.vx[zombie] = 50;

    zombieAiSystem(w);

    expect(Velocity.vx[zombie]).toBe(0);
    expect(Velocity.vy[zombie]).toBe(0);
  });
});
```

- [ ] **Step 8: Implement zombie AI system**

```ts
// src/ecs/systems/zombie-ai-system.ts
import { defineQuery } from 'bitecs';
import { Position, Velocity, Health, ZombieTag, PlayerTag } from '@/ecs/components';
import type { GameWorld } from '@/ecs/world';
import { BASIC_ZOMBIE } from '@/config/enemies';

const zombieQuery = defineQuery([Position, Velocity, Health, ZombieTag]);
const playerQuery = defineQuery([Position, Health, PlayerTag]);

export function zombieAiSystem(world: GameWorld): void {
  const zombies = zombieQuery(world);
  const players = playerQuery(world);

  for (let i = 0; i < zombies.length; i++) {
    const zid = zombies[i]!;

    if (Health.current[zid]! <= 0) {
      Velocity.vx[zid] = 0;
      Velocity.vy[zid] = 0;
      continue;
    }

    // Find nearest living player
    let nearestPid = -1;
    let nearestDistSq = Infinity;
    for (let j = 0; j < players.length; j++) {
      const pid = players[j]!;
      if (Health.current[pid]! <= 0) continue;
      const dx = Position.x[pid]! - Position.x[zid]!;
      const dy = Position.y[pid]! - Position.y[zid]!;
      const dsq = dx * dx + dy * dy;
      if (dsq < nearestDistSq) {
        nearestDistSq = dsq;
        nearestPid = pid;
      }
    }

    if (nearestPid < 0) {
      Velocity.vx[zid] = 0;
      Velocity.vy[zid] = 0;
      continue;
    }

    const dx = Position.x[nearestPid]! - Position.x[zid]!;
    const dy = Position.y[nearestPid]! - Position.y[zid]!;
    const dist = Math.sqrt(dx * dx + dy * dy);

    if (dist < 1) {
      Velocity.vx[zid] = 0;
      Velocity.vy[zid] = 0;
    } else {
      Velocity.vx[zid] = (dx / dist) * BASIC_ZOMBIE.speed;
      Velocity.vy[zid] = (dy / dist) * BASIC_ZOMBIE.speed;
    }
  }
}
```

- [ ] **Step 9: Write death system test**

```ts
// tests/ecs/systems/death-system.test.ts
import { describe, expect, it } from 'vitest';
import { createGameWorld, addEntity, addComponent, hasComponent } from '@/ecs/world';
import { Position, Health, ZombieTag } from '@/ecs/components';
import { createDeathSystem } from '@/ecs/systems/death-system';
import { EventBus } from '@/events/event-bus';
import type { GameEvents } from '@/events/types';

describe('deathSystem', () => {
  it('marks dead entities for removal and emits events', () => {
    const w = createGameWorld();
    const bus = new EventBus<GameEvents>();
    const deathSystem = createDeathSystem(bus);

    const zombie = addEntity(w);
    addComponent(w, Position, zombie);
    addComponent(w, Health, zombie);
    addComponent(w, ZombieTag, zombie);
    Health.current[zombie] = 0;
    Health.max[zombie] = 60;
    Position.x[zombie] = 10;
    Position.y[zombie] = 20;

    const events: any[] = [];
    bus.on('zombie:died', (e) => events.push(e));

    const dead = deathSystem(w);

    expect(dead).toContain(zombie);
    expect(events.length).toBe(1);
    expect(events[0].id).toBe(zombie);
  });

  it('does not mark alive entities', () => {
    const w = createGameWorld();
    const bus = new EventBus<GameEvents>();
    const deathSystem = createDeathSystem(bus);

    const zombie = addEntity(w);
    addComponent(w, Position, zombie);
    addComponent(w, Health, zombie);
    addComponent(w, ZombieTag, zombie);
    Health.current[zombie] = 30;
    Health.max[zombie] = 60;

    const dead = deathSystem(w);
    expect(dead.length).toBe(0);
  });
});
```

- [ ] **Step 10: Implement death system**

```ts
// src/ecs/systems/death-system.ts
import { defineQuery } from 'bitecs';
import { Position, Health, ZombieTag, PlayerTag } from '@/ecs/components';
import type { GameWorld } from '@/ecs/world';
import type { EventBus } from '@/events/event-bus';
import type { GameEvents } from '@/events/types';

const healthQuery = defineQuery([Position, Health]);
const zombieCheck = defineQuery([ZombieTag]);
const playerCheck = defineQuery([PlayerTag]);

export function createDeathSystem(bus: EventBus<GameEvents>) {
  return function deathSystem(world: GameWorld): number[] {
    const entities = healthQuery(world);
    const dead: number[] = [];

    for (let i = 0; i < entities.length; i++) {
      const eid = entities[i]!;
      if (Health.current[eid]! > 0) continue;

      dead.push(eid);

      if (zombieCheck(world).includes(eid)) {
        bus.emit('zombie:died', {
          id: eid,
          position: { x: Position.x[eid]!, y: Position.y[eid]! },
        });
      }

      if (playerCheck(world).includes(eid)) {
        bus.emit('player:died', { day: 0 });
      }
    }

    return dead;
  };
}
```

- [ ] **Step 11: Implement deer AI system**

```ts
// src/ecs/systems/deer-ai-system.ts
import { defineQuery } from 'bitecs';
import { Position, Velocity, Health, DeerTag, PlayerTag } from '@/ecs/components';
import type { GameWorld } from '@/ecs/world';

const deerQuery = defineQuery([Position, Velocity, Health, DeerTag]);
const playerQuery = defineQuery([Position, PlayerTag]);

const FLEE_RADIUS = 128; // pixels
const DEER_SPEED = 70;   // pixels/sec

export function deerAiSystem(world: GameWorld): void {
  const deers = deerQuery(world);
  const players = playerQuery(world);

  for (let i = 0; i < deers.length; i++) {
    const did = deers[i]!;
    if (Health.current[did]! <= 0) {
      Velocity.vx[did] = 0;
      Velocity.vy[did] = 0;
      continue;
    }

    let flee = false;
    let fleeX = 0;
    let fleeY = 0;

    for (let j = 0; j < players.length; j++) {
      const pid = players[j]!;
      const dx = Position.x[did]! - Position.x[pid]!;
      const dy = Position.y[did]! - Position.y[pid]!;
      const distSq = dx * dx + dy * dy;

      if (distSq < FLEE_RADIUS * FLEE_RADIUS && distSq > 0) {
        const dist = Math.sqrt(distSq);
        fleeX = dx / dist;
        fleeY = dy / dist;
        flee = true;
        break;
      }
    }

    if (flee) {
      Velocity.vx[did] = fleeX * DEER_SPEED;
      Velocity.vy[did] = fleeY * DEER_SPEED;
    } else {
      Velocity.vx[did] = 0;
      Velocity.vy[did] = 0;
    }
  }
}
```

- [ ] **Step 12: Run all system tests**

Run: `npx vitest run tests/ecs/systems/`
Expected: ALL PASS

- [ ] **Step 13: Commit**

```bash
git add src/ecs/systems/ tests/ecs/systems/ src/gameplay/combat.ts
git commit -m "feat(ecs): add movement, combat, zombie AI, deer AI, death systems"
```

---

### Task 8: Wave Spawner

**Files:**
- Create: `src/gameplay/wave-spawner.ts`
- Test: `tests/gameplay/wave-spawner.test.ts`

- [ ] **Step 1: Write the test**

```ts
// tests/gameplay/wave-spawner.test.ts
import { describe, expect, it } from 'vitest';
import { WaveSpawner } from '@/gameplay/wave-spawner';

describe('WaveSpawner', () => {
  it('generates wave 1 with base zombie count', () => {
    const spawner = new WaveSpawner();
    const wave = spawner.getWave(1, 1);
    expect(wave.count).toBeGreaterThanOrEqual(5);
    expect(wave.count).toBeLessThanOrEqual(10);
  });

  it('increases zombie count on later days', () => {
    const spawner = new WaveSpawner();
    const w1 = spawner.getWave(1, 1);
    const w5 = spawner.getWave(5, 1);
    expect(w5.count).toBeGreaterThan(w1.count);
  });

  it('generates 3 waves per night', () => {
    const spawner = new WaveSpawner();
    expect(spawner.totalWaves).toBe(3);
  });

  it('returns spawn positions around village edge', () => {
    const spawner = new WaveSpawner();
    const wave = spawner.getWave(1, 1);
    for (const pos of wave.positions) {
      // Positions should be outside village center (24*32 = 768 pixel grid)
      const dist = Math.sqrt(pos.x * pos.x + pos.y * pos.y);
      expect(dist).toBeGreaterThan(300);
    }
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run tests/gameplay/wave-spawner.test.ts`
Expected: FAIL

- [ ] **Step 3: Implement WaveSpawner**

```ts
// src/gameplay/wave-spawner.ts
import { VILLAGE_GRID_SIZE, TILE_SIZE } from '@/config/constants';

export interface WaveData {
  waveNumber: number;
  count: number;
  positions: { x: number; y: number }[];
}

const BASE_COUNT = 8;
const COUNT_PER_DAY = 4;
const SPAWN_DISTANCE = (VILLAGE_GRID_SIZE * TILE_SIZE) / 2 + 64; // just outside village

export class WaveSpawner {
  readonly totalWaves = 3;

  getWave(day: number, waveNumber: number): WaveData {
    const count = BASE_COUNT + (day - 1) * COUNT_PER_DAY + (waveNumber - 1) * 2;
    const positions = this.generatePositions(count);
    return { waveNumber, count, positions };
  }

  private generatePositions(count: number): { x: number; y: number }[] {
    const positions: { x: number; y: number }[] = [];
    for (let i = 0; i < count; i++) {
      const angle = (Math.PI * 2 * i) / count + (Math.random() * 0.3 - 0.15);
      const dist = SPAWN_DISTANCE + Math.random() * 40;
      positions.push({
        x: Math.cos(angle) * dist,
        y: Math.sin(angle) * dist,
      });
    }
    return positions;
  }
}
```

- [ ] **Step 4: Run test**

Run: `npx vitest run tests/gameplay/wave-spawner.test.ts`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/gameplay/wave-spawner.ts tests/gameplay/wave-spawner.test.ts
git commit -m "feat: add WaveSpawner with scaling zombie counts"
```

---

### Task 9: Village Grid (Bonfire + Barricade)

**Files:**
- Create: `src/gameplay/village-grid.ts`
- Test: `tests/gameplay/village-grid.test.ts`

- [ ] **Step 1: Write the test**

```ts
// tests/gameplay/village-grid.test.ts
import { describe, expect, it } from 'vitest';
import { VillageGrid, BuildingType } from '@/gameplay/village-grid';

describe('VillageGrid', () => {
  it('creates a 24x24 grid', () => {
    const grid = new VillageGrid();
    expect(grid.size).toBe(24);
  });

  it('places a bonfire (2x2) at center', () => {
    const grid = new VillageGrid();
    expect(grid.place(BuildingType.Bonfire, 11, 11)).toBe(true);
    expect(grid.getAt(11, 11)).toBe(BuildingType.Bonfire);
    expect(grid.getAt(12, 12)).toBe(BuildingType.Bonfire);
  });

  it('rejects overlapping placement', () => {
    const grid = new VillageGrid();
    grid.place(BuildingType.Bonfire, 11, 11);
    expect(grid.place(BuildingType.Bonfire, 12, 12)).toBe(false);
  });

  it('places a barricade (1x1)', () => {
    const grid = new VillageGrid();
    expect(grid.place(BuildingType.Barricade, 5, 5)).toBe(true);
    expect(grid.getAt(5, 5)).toBe(BuildingType.Barricade);
  });

  it('rejects out-of-bounds placement', () => {
    const grid = new VillageGrid();
    expect(grid.place(BuildingType.Barricade, 24, 24)).toBe(false);
    expect(grid.place(BuildingType.Barricade, -1, 0)).toBe(false);
  });

  it('gets building HP', () => {
    const grid = new VillageGrid();
    grid.place(BuildingType.Bonfire, 11, 11);
    const b = grid.getBuilding(11, 11);
    expect(b).toBeDefined();
    expect(b!.hp).toBe(400);
  });

  it('damages a building', () => {
    const grid = new VillageGrid();
    grid.place(BuildingType.Barricade, 5, 5);
    grid.damageAt(5, 5, 50);
    expect(grid.getBuilding(5, 5)!.hp).toBe(150);
  });

  it('removes building when HP reaches 0', () => {
    const grid = new VillageGrid();
    grid.place(BuildingType.Barricade, 5, 5);
    grid.damageAt(5, 5, 200);
    expect(grid.getAt(5, 5)).toBeNull();
  });

  it('converts grid coords to pixel coords', () => {
    const grid = new VillageGrid();
    const px = grid.toPixel(12, 12);
    expect(px.x).toBe(12 * 32);
    expect(px.y).toBe(12 * 32);
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run tests/gameplay/village-grid.test.ts`
Expected: FAIL

- [ ] **Step 3: Implement VillageGrid**

```ts
// src/gameplay/village-grid.ts
import { VILLAGE_GRID_SIZE, TILE_SIZE } from '@/config/constants';

export enum BuildingType {
  Bonfire = 'bonfire',
  Barricade = 'barricade',
}

interface BuildingDef {
  width: number;
  height: number;
  maxHp: number;
}

const BUILDING_DEFS: Record<BuildingType, BuildingDef> = {
  [BuildingType.Bonfire]: { width: 2, height: 2, maxHp: 400 },
  [BuildingType.Barricade]: { width: 1, height: 1, maxHp: 200 },
};

export interface Building {
  type: BuildingType;
  gridX: number;
  gridY: number;
  hp: number;
  maxHp: number;
}

export class VillageGrid {
  readonly size = VILLAGE_GRID_SIZE;
  private readonly cells: (Building | null)[][] = [];
  private readonly buildings: Building[] = [];

  constructor() {
    for (let y = 0; y < this.size; y++) {
      this.cells[y] = [];
      for (let x = 0; x < this.size; x++) {
        this.cells[y]![x] = null;
      }
    }
  }

  place(type: BuildingType, gx: number, gy: number): boolean {
    const def = BUILDING_DEFS[type];
    if (!this.canPlace(gx, gy, def.width, def.height)) return false;

    const building: Building = {
      type,
      gridX: gx,
      gridY: gy,
      hp: def.maxHp,
      maxHp: def.maxHp,
    };

    for (let dy = 0; dy < def.height; dy++) {
      for (let dx = 0; dx < def.width; dx++) {
        this.cells[gy + dy]![gx + dx] = building;
      }
    }
    this.buildings.push(building);
    return true;
  }

  private canPlace(gx: number, gy: number, w: number, h: number): boolean {
    if (gx < 0 || gy < 0 || gx + w > this.size || gy + h > this.size) return false;
    for (let dy = 0; dy < h; dy++) {
      for (let dx = 0; dx < w; dx++) {
        if (this.cells[gy + dy]![gx + dx] !== null) return false;
      }
    }
    return true;
  }

  getAt(gx: number, gy: number): BuildingType | null {
    if (gx < 0 || gy < 0 || gx >= this.size || gy >= this.size) return null;
    return this.cells[gy]![gx]?.type ?? null;
  }

  getBuilding(gx: number, gy: number): Building | null {
    if (gx < 0 || gy < 0 || gx >= this.size || gy >= this.size) return null;
    return this.cells[gy]![gx] ?? null;
  }

  damageAt(gx: number, gy: number, amount: number): void {
    const building = this.getBuilding(gx, gy);
    if (!building) return;
    building.hp -= amount;
    if (building.hp <= 0) {
      this.removeBuilding(building);
    }
  }

  private removeBuilding(building: Building): void {
    const def = BUILDING_DEFS[building.type];
    for (let dy = 0; dy < def.height; dy++) {
      for (let dx = 0; dx < def.width; dx++) {
        this.cells[building.gridY + dy]![building.gridX + dx] = null;
      }
    }
    const idx = this.buildings.indexOf(building);
    if (idx >= 0) this.buildings.splice(idx, 1);
  }

  getBuildings(): readonly Building[] {
    return this.buildings;
  }

  toPixel(gx: number, gy: number): { x: number; y: number } {
    return { x: gx * TILE_SIZE, y: gy * TILE_SIZE };
  }
}
```

- [ ] **Step 4: Run test**

Run: `npx vitest run tests/gameplay/village-grid.test.ts`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/gameplay/village-grid.ts tests/gameplay/village-grid.test.ts
git commit -m "feat: add VillageGrid with bonfire and barricade placement"
```

---

### Task 10: Preload Scene (Placeholder Assets)

**Files:**
- Create: `src/scenes/preload.ts`
- Modify: `src/scenes/boot.ts` — transition to Preload
- Modify: `src/main.ts` — register scenes

- [ ] **Step 1: Implement PreloadScene**

No test needed — pure Phaser rendering. Uses `generateTexture` for colored rectangles as placeholders.

```ts
// src/scenes/preload.ts
import Phaser from 'phaser';

export class PreloadScene extends Phaser.Scene {
  constructor() {
    super({ key: 'Preload' });
  }

  create(): void {
    // Player — blue square 24x24
    const playerGfx = this.make.graphics({ x: 0, y: 0, add: false });
    playerGfx.fillStyle(0x4488ff);
    playerGfx.fillRect(0, 0, 24, 24);
    playerGfx.generateTexture('player', 24, 24);
    playerGfx.destroy();

    // Zombie — green square 20x20
    const zombieGfx = this.make.graphics({ x: 0, y: 0, add: false });
    zombieGfx.fillStyle(0x44aa44);
    zombieGfx.fillRect(0, 0, 20, 20);
    zombieGfx.generateTexture('zombie', 20, 20);
    zombieGfx.destroy();

    // Deer — brown square 18x18
    const deerGfx = this.make.graphics({ x: 0, y: 0, add: false });
    deerGfx.fillStyle(0xaa7744);
    deerGfx.fillRect(0, 0, 18, 18);
    deerGfx.generateTexture('deer', 18, 18);
    deerGfx.destroy();

    // Tree — dark green tall rect 24x32
    const treeGfx = this.make.graphics({ x: 0, y: 0, add: false });
    treeGfx.fillStyle(0x226622);
    treeGfx.fillRect(0, 0, 24, 32);
    treeGfx.generateTexture('tree', 24, 32);
    treeGfx.destroy();

    // Rock — gray square 20x16
    const rockGfx = this.make.graphics({ x: 0, y: 0, add: false });
    rockGfx.fillStyle(0x888888);
    rockGfx.fillRect(0, 0, 20, 16);
    rockGfx.generateTexture('rock', 20, 16);
    rockGfx.destroy();

    // Bonfire — orange square 48x48 (2x2 tiles, somewhat smaller)
    const bonfireGfx = this.make.graphics({ x: 0, y: 0, add: false });
    bonfireGfx.fillStyle(0xff6622);
    bonfireGfx.fillRect(0, 0, 48, 48);
    bonfireGfx.generateTexture('bonfire', 48, 48);
    bonfireGfx.destroy();

    // Barricade — brown rect 28x28
    const barricadeGfx = this.make.graphics({ x: 0, y: 0, add: false });
    barricadeGfx.fillStyle(0x8b5e3c);
    barricadeGfx.fillRect(0, 0, 28, 28);
    barricadeGfx.generateTexture('barricade', 28, 28);
    barricadeGfx.destroy();

    // Sword slash — white arc
    const slashGfx = this.make.graphics({ x: 0, y: 0, add: false });
    slashGfx.fillStyle(0xffffff, 0.7);
    slashGfx.slice(16, 16, 16, Phaser.Math.DegToRad(-45), Phaser.Math.DegToRad(45), false);
    slashGfx.fillPath();
    slashGfx.generateTexture('slash', 32, 32);
    slashGfx.destroy();

    // Snow tile — white with slight blue tint
    const snowGfx = this.make.graphics({ x: 0, y: 0, add: false });
    snowGfx.fillStyle(0xe8eef4);
    snowGfx.fillRect(0, 0, 32, 32);
    snowGfx.lineStyle(1, 0xd0d8e0, 0.3);
    snowGfx.strokeRect(0, 0, 32, 32);
    snowGfx.generateTexture('snow_tile', 32, 32);
    snowGfx.destroy();

    // Village ground tile — darker brown
    const villageGfx = this.make.graphics({ x: 0, y: 0, add: false });
    villageGfx.fillStyle(0x5a4a3a);
    villageGfx.fillRect(0, 0, 32, 32);
    villageGfx.lineStyle(1, 0x4a3a2a, 0.3);
    villageGfx.strokeRect(0, 0, 32, 32);
    villageGfx.generateTexture('village_tile', 32, 32);
    villageGfx.destroy();

    this.scene.start('Game');
  }
}
```

- [ ] **Step 2: Update BootScene to go to Preload**

```ts
// src/scenes/boot.ts — replace full file
import Phaser from 'phaser';

export class BootScene extends Phaser.Scene {
  constructor() {
    super({ key: 'Boot' });
  }

  create(): void {
    const { width, height } = this.scale;
    this.cameras.main.setBackgroundColor('#0c1626');

    this.add
      .text(width / 2, height / 2, '눈보라 마을...', {
        fontFamily: 'ui-monospace, monospace',
        fontSize: '20px',
        color: '#d0d8e0',
      })
      .setOrigin(0.5);

    this.time.delayedCall(500, () => {
      this.scene.start('Preload');
    });
  }
}
```

- [ ] **Step 3: Update main.ts (scene registration will happen in Task 11)**

(Deferred to Task 11 to avoid registering GameScene before it exists)

- [ ] **Step 4: Commit**

```bash
git add src/scenes/preload.ts src/scenes/boot.ts
git commit -m "feat: add PreloadScene with placeholder textures"
```

---

### Task 11: GameScene — Main Game Loop (Integration)

**Files:**
- Create: `src/scenes/game.ts`
- Create: `src/ui/hud.ts`
- Create: `src/ui/game-over.ts`
- Modify: `src/main.ts`
- Modify: `src/config/balance.ts` — add player/gathering balance values

This is the integration task. All prior systems come together here.

- [ ] **Step 1: Extend balance config**

```ts
// src/config/balance.ts — replace full file
export const DAY_CYCLE = {
  dayDurationSec: 540,
  nightDurationSec: 360,
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

export const PLAYER = {
  maxHp: 100,
  speed: 120, // pixels/sec
} as const;

export const GATHERING = {
  treeWood: 3,
  treeGatherMs: 4000,
  rockStone: 2,
  rockGatherMs: 6000,
  deerMeat: 2,
} as const;

export const BONFIRE = {
  damagePerSec: 5,
  radius: 128, // pixels
  buffAttack: 0.15, // +15% attack
} as const;

export type BalanceConfig = {
  dayCycle: typeof DAY_CYCLE;
  vision: typeof VISION;
  resources: typeof RESOURCES;
  player: typeof PLAYER;
  gathering: typeof GATHERING;
  bonfire: typeof BONFIRE;
};

export const BALANCE: BalanceConfig = {
  dayCycle: DAY_CYCLE,
  vision: VISION,
  resources: RESOURCES,
  player: PLAYER,
  gathering: GATHERING,
  bonfire: BONFIRE,
};
```

- [ ] **Step 2: Implement HUD**

```ts
// src/ui/hud.ts
import Phaser from 'phaser';
import type { ResourceManager, ResourceKind } from '@/gameplay/resource-manager';
import type { DayNightCycle } from '@/gameplay/day-night-cycle';
import { Health } from '@/ecs/components';

export class HUD {
  private readonly texts: Map<string, Phaser.GameObjects.Text> = new Map();
  private readonly hpBar: Phaser.GameObjects.Graphics;
  private readonly container: Phaser.GameObjects.Container;

  constructor(
    private readonly scene: Phaser.Scene,
    private readonly resources: ResourceManager,
    private readonly cycle: DayNightCycle,
    private readonly playerEid: number,
  ) {
    this.container = scene.add.container(0, 0).setScrollFactor(0).setDepth(1000);

    // Resource text (top-left)
    const resText = scene.add.text(10, 10, '', {
      fontFamily: 'ui-monospace, monospace',
      fontSize: '14px',
      color: '#e0e0e0',
    });
    this.texts.set('resources', resText);
    this.container.add(resText);

    // Day/phase text (top-center)
    const dayText = scene.add.text(scene.scale.width / 2, 10, '', {
      fontFamily: 'ui-monospace, monospace',
      fontSize: '16px',
      color: '#ffd700',
    }).setOrigin(0.5, 0);
    this.texts.set('day', dayText);
    this.container.add(dayText);

    // HP bar (bottom-left)
    this.hpBar = scene.add.graphics().setScrollFactor(0);
    this.container.add(this.hpBar);

    // Wave info (top-right)
    const waveText = scene.add.text(scene.scale.width - 10, 10, '', {
      fontFamily: 'ui-monospace, monospace',
      fontSize: '14px',
      color: '#ff6666',
    }).setOrigin(1, 0);
    this.texts.set('wave', waveText);
    this.container.add(waveText);
  }

  setWaveText(text: string): void {
    this.texts.get('wave')?.setText(text);
  }

  update(): void {
    const res = this.resources.getAll();
    this.texts.get('resources')?.setText(
      `나무:${res.wood} 돌:${res.stone} 고기:${res.meat} 식량:${res.food}`
    );

    const phaseNames: Record<string, string> = {
      day: '낮', evening: '저녁', night: '밤', dawn: '새벽',
    };
    const phaseName = phaseNames[this.cycle.phase] ?? this.cycle.phase;
    const remaining = Math.ceil(this.cycle.remainingSec);
    this.texts.get('day')?.setText(`Day ${this.cycle.day} — ${phaseName} (${remaining}s)`);

    // HP bar
    const hp = Health.current[this.playerEid] ?? 0;
    const maxHp = Health.max[this.playerEid] ?? 1;
    const ratio = Math.max(0, hp / maxHp);
    const barWidth = 120;
    const barHeight = 12;
    const barX = 10;
    const barY = this.scene.scale.height - 30;

    this.hpBar.clear();
    this.hpBar.fillStyle(0x333333);
    this.hpBar.fillRect(barX, barY, barWidth, barHeight);
    this.hpBar.fillStyle(ratio > 0.3 ? 0x44cc44 : 0xcc4444);
    this.hpBar.fillRect(barX, barY, barWidth * ratio, barHeight);
    this.hpBar.lineStyle(1, 0xffffff, 0.5);
    this.hpBar.strokeRect(barX, barY, barWidth, barHeight);
  }
}
```

- [ ] **Step 3: Implement GameOver screen**

```ts
// src/ui/game-over.ts
import Phaser from 'phaser';

export class GameOverScreen {
  private container: Phaser.GameObjects.Container | null = null;

  show(scene: Phaser.Scene, day: number, kills: number): void {
    const { width, height } = scene.scale;
    this.container = scene.add.container(0, 0).setScrollFactor(0).setDepth(2000);

    const bg = scene.add.rectangle(width / 2, height / 2, width, height, 0x000000, 0.7);
    this.container.add(bg);

    const title = scene.add.text(width / 2, height / 2 - 60, '마을이 무너졌다...', {
      fontFamily: 'ui-monospace, monospace',
      fontSize: '28px',
      color: '#ff4444',
    }).setOrigin(0.5);
    this.container.add(title);

    const stats = scene.add.text(width / 2, height / 2, `생존 일수: ${day}\n처치 수: ${kills}`, {
      fontFamily: 'ui-monospace, monospace',
      fontSize: '18px',
      color: '#e0e0e0',
      align: 'center',
    }).setOrigin(0.5);
    this.container.add(stats);

    const restart = scene.add.text(width / 2, height / 2 + 80, '[R] 다시 시작', {
      fontFamily: 'ui-monospace, monospace',
      fontSize: '16px',
      color: '#ffd700',
    }).setOrigin(0.5);
    this.container.add(restart);

    scene.input.keyboard!.once('keydown-R', () => {
      scene.scene.restart();
    });
  }
}
```

- [ ] **Step 4: Implement GameScene**

```ts
// src/scenes/game.ts
import Phaser from 'phaser';
import { createGameWorld, addEntity, addComponent, removeEntity } from '@/ecs/world';
import type { GameWorld } from '@/ecs/world';
import {
  Position, Velocity, Health, Combat, PlayerTag, ZombieTag, DeerTag, ResourceNode,
} from '@/ecs/components';
import { movementSystem } from '@/ecs/systems/movement-system';
import { combatSystem } from '@/ecs/systems/combat-system';
import { zombieAiSystem } from '@/ecs/systems/zombie-ai-system';
import { deerAiSystem } from '@/ecs/systems/deer-ai-system';
import { createDeathSystem } from '@/ecs/systems/death-system';
import { InputAdapter } from '@/input/input-adapter';
import { DayNightCycle, Phase } from '@/gameplay/day-night-cycle';
import { ResourceManager, ResourceKind } from '@/gameplay/resource-manager';
import { VillageGrid, BuildingType } from '@/gameplay/village-grid';
import { WaveSpawner } from '@/gameplay/wave-spawner';
import { EventBus } from '@/events/event-bus';
import type { GameEvents } from '@/events/types';
import { LONGSWORD } from '@/config/weapons';
import { BASIC_ZOMBIE } from '@/config/enemies';
import { PLAYER, GATHERING, BONFIRE, VISION } from '@/config/balance';
import { TILE_SIZE, VILLAGE_GRID_SIZE, GAME_WIDTH, GAME_HEIGHT } from '@/config/constants';
import { HUD } from '@/ui/hud';
import { GameOverScreen } from '@/ui/game-over';
import { createRng } from '@/util/rng';

const SNOWFIELD_SIZE = 2400; // total world size in pixels
const VILLAGE_OFFSET_X = (SNOWFIELD_SIZE - VILLAGE_GRID_SIZE * TILE_SIZE) / 2;
const VILLAGE_OFFSET_Y = (SNOWFIELD_SIZE - VILLAGE_GRID_SIZE * TILE_SIZE) / 2;

export class GameScene extends Phaser.Scene {
  private world!: GameWorld;
  private bus!: EventBus<GameEvents>;
  private input!: InputAdapter;
  private cycle!: DayNightCycle;
  private resources!: ResourceManager;
  private village!: VillageGrid;
  private waveSpawner!: WaveSpawner;
  private hud!: HUD;
  private gameOver!: GameOverScreen;
  private deathSystem!: (w: GameWorld) => number[];
  private rng!: ReturnType<typeof createRng>;

  private playerEid = -1;
  private kills = 0;
  private isDead = false;

  // Night wave state
  private currentWave = 0;
  private waveTimer = 0;
  private wavePending = false;
  private zombiesAlive = 0;

  // Gathering state
  private gatherTarget = -1;
  private gatherProgress = 0;
  private gatherBar: Phaser.GameObjects.Graphics | null = null;

  // Vision mask
  private visionMask!: Phaser.GameObjects.Graphics;

  // Snowfield objects (resource nodes, deer) — tracked for cleanup
  private snowfieldEntities: number[] = [];

  // Village display objects
  private villageSprites: Phaser.GameObjects.Sprite[] = [];

  constructor() {
    super({ key: 'Game' });
  }

  create(): void {
    this.bus = new EventBus<GameEvents>();
    this.world = createGameWorld();
    this.input = new InputAdapter();
    this.input.register(this);
    this.cycle = new DayNightCycle(this.bus);
    this.resources = new ResourceManager(this.bus);
    this.village = new VillageGrid();
    this.waveSpawner = new WaveSpawner();
    this.deathSystem = createDeathSystem(this.bus);
    this.gameOver = new GameOverScreen();
    this.rng = createRng(Date.now());
    this.kills = 0;
    this.isDead = false;
    this.currentWave = 0;
    this.waveTimer = 0;
    this.wavePending = false;
    this.zombiesAlive = 0;
    this.gatherTarget = -1;
    this.gatherProgress = 0;
    this.snowfieldEntities = [];
    this.villageSprites = [];

    // Draw snow background
    this.drawBackground();

    // Place starting village buildings
    this.village.place(BuildingType.Bonfire, 11, 11);
    this.placeStartingBarricades();
    this.drawVillage();

    // Create player
    this.playerEid = this.spawnPlayer();
    this.hud = new HUD(this, this.resources, this.cycle, this.playerEid);

    // Spawn snowfield resources + deer
    this.spawnSnowfieldContent();

    // Camera follows player
    const playerSprite = this.world.sprites.get(this.playerEid);
    if (playerSprite) {
      this.cameras.main.startFollow(playerSprite, true, 0.1, 0.1);
      this.cameras.main.setBounds(0, 0, SNOWFIELD_SIZE, SNOWFIELD_SIZE);
    }

    // Vision mask
    this.visionMask = this.add.graphics().setDepth(900);

    // Gather progress bar
    this.gatherBar = this.add.graphics().setDepth(950);

    // Event listeners
    this.bus.on('night:started', () => this.onNightStart());
    this.bus.on('dawn:started', () => this.onDawnStart());
    this.bus.on('zombie:died', () => {
      this.kills++;
      this.zombiesAlive--;
    });
    this.bus.on('player:died', () => {
      if (!this.isDead) {
        this.isDead = true;
        this.gameOver.show(this, this.cycle.day, this.kills);
      }
    });
  }

  update(_time: number, delta: number): void {
    if (this.isDead) return;

    const dtSec = delta / 1000;
    this.world.deltaTime = dtSec;
    this.world.elapsed += delta;

    // Day/night cycle
    this.cycle.update(delta);

    // Player input -> velocity
    const inp = this.input.poll();
    Velocity.vx[this.playerEid] = inp.dx * PLAYER.speed;
    Velocity.vy[this.playerEid] = inp.dy * PLAYER.speed;

    // Gathering
    this.updateGathering(inp.interact, delta);

    // ECS systems
    if (this.cycle.phase === Phase.Night) {
      this.updateNightWaves(delta);
      zombieAiSystem(this.world);
      combatSystem(this.world);
    }
    if (this.cycle.phase === Phase.Day) {
      deerAiSystem(this.world);
      combatSystem(this.world); // player can attack deer
    }
    movementSystem(this.world);

    // Death system
    const dead = this.deathSystem(this.world);
    for (const eid of dead) {
      // Drop meat from deer
      if (this.world.sprites.get(eid) && DeerTag.eid?.[eid] !== undefined) {
        // Check if deer tag component exists on this entity - handled by death event
      }
      const sprite = this.world.sprites.get(eid);
      if (sprite) {
        sprite.destroy();
        this.world.sprites.delete(eid);
      }
      removeEntity(this.world, eid);
    }

    // Sync ECS positions to Phaser sprites
    this.syncSprites();

    // Bonfire damage to zombies at night
    if (this.cycle.phase === Phase.Night) {
      this.applyBonfireDamage(dtSec);
    }

    // Vision
    this.drawVision();

    // HUD
    this.hud.update();
  }

  // --- Spawning ---

  private spawnPlayer(): number {
    const eid = addEntity(this.world);
    addComponent(this.world, Position, eid);
    addComponent(this.world, Velocity, eid);
    addComponent(this.world, Health, eid);
    addComponent(this.world, Combat, eid);
    addComponent(this.world, PlayerTag, eid);

    const cx = SNOWFIELD_SIZE / 2;
    const cy = SNOWFIELD_SIZE / 2;
    Position.x[eid] = cx;
    Position.y[eid] = cy;
    Velocity.vx[eid] = 0;
    Velocity.vy[eid] = 0;
    Health.current[eid] = PLAYER.maxHp;
    Health.max[eid] = PLAYER.maxHp;
    Combat.damage[eid] = LONGSWORD.damage;
    Combat.range[eid] = LONGSWORD.range;
    Combat.cooldown[eid] = LONGSWORD.cooldownMs;
    Combat.lastAttackTime[eid] = 0;

    const sprite = this.add.sprite(cx, cy, 'player').setDepth(100);
    this.world.sprites.set(eid, sprite);
    return eid;
  }

  private spawnSnowfieldContent(): void {
    const cx = SNOWFIELD_SIZE / 2;
    const cy = SNOWFIELD_SIZE / 2;

    // Trees (near zone)
    for (let i = 0; i < 30; i++) {
      const angle = this.rng.next() * Math.PI * 2;
      const dist = 200 + this.rng.next() * 600;
      const x = cx + Math.cos(angle) * dist;
      const y = cy + Math.sin(angle) * dist;
      this.spawnResourceNode(x, y, 0, GATHERING.treeWood, GATHERING.treeGatherMs, 'tree');
    }

    // Rocks
    for (let i = 0; i < 15; i++) {
      const angle = this.rng.next() * Math.PI * 2;
      const dist = 250 + this.rng.next() * 500;
      const x = cx + Math.cos(angle) * dist;
      const y = cy + Math.sin(angle) * dist;
      this.spawnResourceNode(x, y, 1, GATHERING.rockStone, GATHERING.rockGatherMs, 'rock');
    }

    // Deer
    for (let i = 0; i < 8; i++) {
      const angle = this.rng.next() * Math.PI * 2;
      const dist = 300 + this.rng.next() * 500;
      const x = cx + Math.cos(angle) * dist;
      const y = cy + Math.sin(angle) * dist;
      this.spawnDeer(x, y);
    }
  }

  private spawnResourceNode(
    x: number, y: number, kind: number, amount: number, gatherTime: number, texture: string,
  ): number {
    const eid = addEntity(this.world);
    addComponent(this.world, Position, eid);
    addComponent(this.world, ResourceNode, eid);
    Position.x[eid] = x;
    Position.y[eid] = y;
    ResourceNode.kind[eid] = kind;
    ResourceNode.amount[eid] = amount;
    ResourceNode.gatherTime[eid] = gatherTime;

    const sprite = this.add.sprite(x, y, texture).setDepth(50);
    this.world.sprites.set(eid, sprite);
    this.snowfieldEntities.push(eid);
    return eid;
  }

  private spawnDeer(x: number, y: number): number {
    const eid = addEntity(this.world);
    addComponent(this.world, Position, eid);
    addComponent(this.world, Velocity, eid);
    addComponent(this.world, Health, eid);
    addComponent(this.world, DeerTag, eid);
    Position.x[eid] = x;
    Position.y[eid] = y;
    Velocity.vx[eid] = 0;
    Velocity.vy[eid] = 0;
    Health.current[eid] = 30;
    Health.max[eid] = 30;

    const sprite = this.add.sprite(x, y, 'deer').setDepth(60);
    this.world.sprites.set(eid, sprite);
    this.snowfieldEntities.push(eid);
    return eid;
  }

  private spawnZombie(x: number, y: number): number {
    const eid = addEntity(this.world);
    addComponent(this.world, Position, eid);
    addComponent(this.world, Velocity, eid);
    addComponent(this.world, Health, eid);
    addComponent(this.world, Combat, eid);
    addComponent(this.world, ZombieTag, eid);
    Position.x[eid] = x;
    Position.y[eid] = y;
    Velocity.vx[eid] = 0;
    Velocity.vy[eid] = 0;
    Health.current[eid] = BASIC_ZOMBIE.hp;
    Health.max[eid] = BASIC_ZOMBIE.hp;
    Combat.damage[eid] = BASIC_ZOMBIE.damage;
    Combat.range[eid] = BASIC_ZOMBIE.attackRange;
    Combat.cooldown[eid] = BASIC_ZOMBIE.attackCooldownMs;
    Combat.lastAttackTime[eid] = 0;

    const sprite = this.add.sprite(x, y, 'zombie').setDepth(80);
    this.world.sprites.set(eid, sprite);
    return eid;
  }

  // --- Village ---

  private placeStartingBarricades(): void {
    // Place barricades around the bonfire in cardinal directions
    const positions = [
      [10, 10], [11, 10], [12, 10], [13, 10],
      [10, 13], [11, 13], [12, 13], [13, 13],
      [10, 11], [10, 12],
      [13, 11], [13, 12],
    ];
    for (const [gx, gy] of positions) {
      this.village.place(BuildingType.Barricade, gx!, gy!);
    }
  }

  private drawBackground(): void {
    // Simple tiled background
    const tilesX = Math.ceil(SNOWFIELD_SIZE / TILE_SIZE);
    const tilesY = Math.ceil(SNOWFIELD_SIZE / TILE_SIZE);
    for (let y = 0; y < tilesY; y++) {
      for (let x = 0; x < tilesX; x++) {
        const px = x * TILE_SIZE + TILE_SIZE / 2;
        const py = y * TILE_SIZE + TILE_SIZE / 2;
        const inVillage =
          x >= VILLAGE_OFFSET_X / TILE_SIZE &&
          x < VILLAGE_OFFSET_X / TILE_SIZE + VILLAGE_GRID_SIZE &&
          y >= VILLAGE_OFFSET_Y / TILE_SIZE &&
          y < VILLAGE_OFFSET_Y / TILE_SIZE + VILLAGE_GRID_SIZE;
        this.add.sprite(px, py, inVillage ? 'village_tile' : 'snow_tile').setDepth(0);
      }
    }
  }

  private drawVillage(): void {
    for (const s of this.villageSprites) s.destroy();
    this.villageSprites = [];

    for (const b of this.village.getBuildings()) {
      const px = VILLAGE_OFFSET_X + b.gridX * TILE_SIZE + TILE_SIZE;
      const py = VILLAGE_OFFSET_Y + b.gridY * TILE_SIZE + TILE_SIZE;
      const texture = b.type === BuildingType.Bonfire ? 'bonfire' : 'barricade';
      const sprite = this.add.sprite(px, py, texture).setDepth(40);
      this.villageSprites.push(sprite);
    }
  }

  // --- Night Waves ---

  private onNightStart(): void {
    this.currentWave = 0;
    this.waveTimer = 0;
    this.wavePending = true;
    this.hud.setWaveText('밤 시작!');
  }

  private onDawnStart(): void {
    this.hud.setWaveText('');
    // Respawn snowfield content for next day
    this.spawnSnowfieldContent();
  }

  private updateNightWaves(dtMs: number): void {
    if (!this.wavePending) return;

    this.waveTimer += dtMs;

    // Spawn next wave every 60 seconds, or immediately if all zombies dead
    const waveInterval = 60_000; // 60 sec between waves
    if (
      this.currentWave === 0 ||
      (this.zombiesAlive <= 0 && this.currentWave < this.waveSpawner.totalWaves) ||
      this.waveTimer >= waveInterval
    ) {
      this.currentWave++;
      if (this.currentWave > this.waveSpawner.totalWaves) {
        this.wavePending = false;
        return;
      }

      const wave = this.waveSpawner.getWave(this.cycle.day, this.currentWave);
      const cx = SNOWFIELD_SIZE / 2;
      const cy = SNOWFIELD_SIZE / 2;

      for (const pos of wave.positions) {
        this.spawnZombie(cx + pos.x, cy + pos.y);
      }
      this.zombiesAlive += wave.count;
      this.waveTimer = 0;

      this.hud.setWaveText(`Wave ${this.currentWave}/${this.waveSpawner.totalWaves} (${wave.count})`);
      this.bus.emit('wave:started', { waveNumber: this.currentWave, count: wave.count });
    }
  }

  // --- Gathering ---

  private updateGathering(interactPressed: boolean, dtMs: number): void {
    if (!interactPressed || this.cycle.phase !== Phase.Day) {
      this.gatherTarget = -1;
      this.gatherProgress = 0;
      this.gatherBar?.clear();
      return;
    }

    const px = Position.x[this.playerEid]!;
    const py = Position.y[this.playerEid]!;

    // Find nearest resource node within gather range
    if (this.gatherTarget < 0) {
      let nearest = -1;
      let nearestDist = Infinity;
      for (const eid of this.snowfieldEntities) {
        if (!ResourceNode.kind[eid] && ResourceNode.kind[eid] === undefined) continue;
        const dx = Position.x[eid]! - px;
        const dy = Position.y[eid]! - py;
        const dist = Math.sqrt(dx * dx + dy * dy);
        if (dist < 48 && dist < nearestDist) {
          nearestDist = dist;
          nearest = eid;
        }
      }
      this.gatherTarget = nearest;
    }

    if (this.gatherTarget < 0) {
      this.gatherBar?.clear();
      return;
    }

    const target = this.gatherTarget;
    const gatherTime = ResourceNode.gatherTime[target]!;
    this.gatherProgress += dtMs;

    // Draw progress bar above node
    const nx = Position.x[target]!;
    const ny = Position.y[target]! - 20;
    const progress = Math.min(this.gatherProgress / gatherTime, 1);
    this.gatherBar?.clear();
    this.gatherBar?.fillStyle(0x333333);
    this.gatherBar?.fillRect(nx - 16, ny, 32, 4);
    this.gatherBar?.fillStyle(0x44cc44);
    this.gatherBar?.fillRect(nx - 16, ny, 32 * progress, 4);

    if (this.gatherProgress >= gatherTime) {
      // Gather complete
      const kind = ResourceNode.kind[target]!;
      const amount = ResourceNode.amount[target]!;
      const resourceKind = kind === 0 ? ResourceKind.Wood : kind === 1 ? ResourceKind.Stone : ResourceKind.Meat;
      this.resources.add(resourceKind, amount);
      this.bus.emit('resource:gathered', { kind: resourceKind, amount });

      // Remove node
      const sprite = this.world.sprites.get(target);
      if (sprite) {
        sprite.destroy();
        this.world.sprites.delete(target);
      }
      removeEntity(this.world, target);
      this.snowfieldEntities = this.snowfieldEntities.filter((e) => e !== target);

      this.gatherTarget = -1;
      this.gatherProgress = 0;
      this.gatherBar?.clear();
    }
  }

  // --- Bonfire ---

  private applyBonfireDamage(dtSec: number): void {
    const bonfires = this.village.getBuildings().filter((b) => b.type === BuildingType.Bonfire);
    if (bonfires.length === 0) return;

    const cx = SNOWFIELD_SIZE / 2;
    const cy = SNOWFIELD_SIZE / 2;

    for (const bonfire of bonfires) {
      const bx = VILLAGE_OFFSET_X + bonfire.gridX * TILE_SIZE + TILE_SIZE;
      const by = VILLAGE_OFFSET_Y + bonfire.gridY * TILE_SIZE + TILE_SIZE;
      const radiusSq = BONFIRE.radius * BONFIRE.radius;

      // Iterate zombie entities via the ECS query is handled in combatSystem
      // Here we do simple proximity check for bonfire AOE
      this.world.sprites.forEach((sprite, eid) => {
        if (!Health.current[eid] || Health.current[eid]! <= 0) return;
        const ex = Position.x[eid]!;
        const ey = Position.y[eid]!;
        const dx = ex - bx;
        const dy = ey - by;
        if (dx * dx + dy * dy < radiusSq) {
          // Only damage zombies
          if (ZombieTag.eid && ZombieTag.eid[eid]) {
            Health.current[eid] -= BONFIRE.damagePerSec * dtSec;
          }
        }
      });
    }
  }

  // --- Rendering ---

  private syncSprites(): void {
    this.world.sprites.forEach((sprite, eid) => {
      sprite.x = Position.x[eid]!;
      sprite.y = Position.y[eid]!;
    });
  }

  private drawVision(): void {
    const px = Position.x[this.playerEid]!;
    const py = Position.y[this.playerEid]!;
    const isNight = this.cycle.phase === Phase.Night || this.cycle.phase === Phase.Evening;
    const radiusTiles = isNight ? VISION.nightRadiusTiles : VISION.dayRadiusTiles;
    const radius = radiusTiles * TILE_SIZE;

    this.visionMask.clear();
    this.visionMask.fillStyle(0x0c1626, isNight ? 0.85 : 0.4);

    // Full screen dark overlay
    const cam = this.cameras.main;
    const left = cam.scrollX;
    const top = cam.scrollY;
    const w = cam.width;
    const h = cam.height;

    // Draw dark rect, then "cut" a circle by drawing lighter center
    // Using Phaser graphics: fill entire viewport, then clear circle with blend
    this.visionMask.fillRect(left, top, w, h);

    // Clear circle around player (draw with alpha 0 via erase)
    this.visionMask.fillStyle(0x0c1626, 0);
    // Use a radial gradient approach: concentric circles with decreasing alpha
    const steps = 8;
    for (let i = steps; i >= 0; i--) {
      const r = radius * (i / steps);
      const alpha = isNight ? 0.85 * (i / steps) : 0.4 * (i / steps);
      this.visionMask.fillStyle(0x0c1626, alpha);
      this.visionMask.fillCircle(px, py, r);
    }
  }
}
```

- [ ] **Step 5: Update main.ts**

```ts
// src/main.ts — replace full file
import Phaser from 'phaser';
import { GAME_WIDTH, GAME_HEIGHT } from '@/config/constants';
import { BootScene } from '@/scenes/boot';
import { PreloadScene } from '@/scenes/preload';
import { GameScene } from '@/scenes/game';

const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  parent: 'game',
  width: GAME_WIDTH,
  height: GAME_HEIGHT,
  backgroundColor: '#0a0a14',
  pixelArt: true,
  scale: {
    mode: Phaser.Scale.FIT,
    autoCenter: Phaser.Scale.CENTER_BOTH,
  },
  scene: [BootScene, PreloadScene, GameScene],
};

new Phaser.Game(config);
```

- [ ] **Step 6: Run all tests to ensure nothing broke**

Run: `npx vitest run`
Expected: ALL PASS (game scene itself is not unit-testable, but all logic modules should pass)

- [ ] **Step 7: Run dev server to verify visual output**

Run: `npx vite --open`
Expected: Browser opens, boot screen → preload → game with player on snow field, WASD movement works

- [ ] **Step 8: Commit**

```bash
git add src/scenes/game.ts src/scenes/preload.ts src/scenes/boot.ts src/main.ts src/ui/hud.ts src/ui/game-over.ts src/config/balance.ts
git commit -m "feat: integrate GameScene with day/night cycle, combat, gathering, waves, HUD"
```

---

### Task 12: Play-Test Pass + Bug Fixes

This task is for manual play-testing and fixing issues found.

- [ ] **Step 1: Run the game**

Run: `npx vite`
Open browser at http://localhost:5173

- [ ] **Step 2: Test checklist**

1. Player moves with WASD
2. Trees/rocks visible on snowfield, F key gathers them
3. Resources update in HUD
4. Deer flee when player approaches
5. Day counter ticks down in HUD
6. Evening transition happens, then Night
7. Zombies spawn in waves during night
8. Player auto-attacks nearby zombies
9. Zombies walk toward player
10. Bonfire deals AOE damage
11. Death screen shows on player death
12. R key restarts game

- [ ] **Step 3: Fix any issues found**

(Address specific bugs encountered during play-test)

- [ ] **Step 4: Run full test suite**

Run: `npx vitest run`
Expected: ALL PASS

- [ ] **Step 5: Commit fixes**

```bash
git add -A
git commit -m "fix: play-test bug fixes for Phase 1 vertical slice"
```

---

### Task 13: Build Verification + Version Bump

- [ ] **Step 1: Typecheck**

Run: `npx tsc --noEmit`
Expected: No errors

- [ ] **Step 2: Build**

Run: `npm run build`
Expected: Successful build in `dist/`

- [ ] **Step 3: Commit final state**

```bash
git add -A
git commit -m "chore: Phase 1 vertical slice complete — v0.2.0"
```
