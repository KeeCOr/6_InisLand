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
      Health.current[nearest]! -= dmg;
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
        Health.current[pid]! -= dmg;
        Combat.lastAttackTime[zid] = now;
        break;
      }
    }
  }
}
