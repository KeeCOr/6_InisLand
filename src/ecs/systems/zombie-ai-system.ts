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
      Velocity.vx[zid] = 0; Velocity.vy[zid] = 0;
      continue;
    }

    let nearestPid = -1;
    let nearestDistSq = Infinity;
    for (let j = 0; j < players.length; j++) {
      const pid = players[j]!;
      if (Health.current[pid]! <= 0) continue;
      const dx = Position.x[pid]! - Position.x[zid]!;
      const dy = Position.y[pid]! - Position.y[zid]!;
      const dsq = dx * dx + dy * dy;
      if (dsq < nearestDistSq) { nearestDistSq = dsq; nearestPid = pid; }
    }

    if (nearestPid < 0) {
      Velocity.vx[zid] = 0; Velocity.vy[zid] = 0;
      continue;
    }

    const dx = Position.x[nearestPid]! - Position.x[zid]!;
    const dy = Position.y[nearestPid]! - Position.y[zid]!;
    const dist = Math.sqrt(dx * dx + dy * dy);
    if (dist < 1) {
      Velocity.vx[zid] = 0; Velocity.vy[zid] = 0;
    } else {
      Velocity.vx[zid] = (dx / dist) * BASIC_ZOMBIE.speed;
      Velocity.vy[zid] = (dy / dist) * BASIC_ZOMBIE.speed;
    }
  }
}
