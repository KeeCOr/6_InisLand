import { defineQuery } from 'bitecs';
import { Position, Velocity } from '@/ecs/components';
import type { GameWorld } from '@/ecs/world';

const movingQuery = defineQuery([Position, Velocity]);

export function movementSystem(world: GameWorld): void {
  const dt = world.deltaTime;
  const entities = movingQuery(world);
  for (let i = 0; i < entities.length; i++) {
    const eid = entities[i]!;
    Position.x[eid]! += Velocity.vx[eid]! * dt;
    Position.y[eid]! += Velocity.vy[eid]! * dt;
  }
}
