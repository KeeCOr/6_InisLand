import { defineQuery } from 'bitecs';
import { Position, Velocity, Health, DeerTag, PlayerTag } from '@/ecs/components';
import type { GameWorld } from '@/ecs/world';

const deerQuery = defineQuery([Position, Velocity, Health, DeerTag]);
const playerQuery = defineQuery([Position, PlayerTag]);

const FLEE_RADIUS = 128;
const DEER_SPEED = 70;

export function deerAiSystem(world: GameWorld): void {
  const deers = deerQuery(world);
  const players = playerQuery(world);

  for (let i = 0; i < deers.length; i++) {
    const did = deers[i]!;
    if (Health.current[did]! <= 0) {
      Velocity.vx[did] = 0; Velocity.vy[did] = 0;
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
