import { describe, expect, it } from 'vitest';
import { createGameWorld, addEntity, addComponent } from '@/ecs/world';
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
    Health.current[zombie] = 0; Health.max[zombie] = 60;
    Position.x[zombie] = 10; Position.y[zombie] = 20;

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
    Health.current[zombie] = 30; Health.max[zombie] = 60;

    const dead = deathSystem(w);
    expect(dead.length).toBe(0);
  });
});
