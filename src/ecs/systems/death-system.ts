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
