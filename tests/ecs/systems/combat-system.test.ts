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
    Position.x[player] = playerX; Position.y[player] = 0;
    Health.current[player] = 100; Health.max[player] = 100;
    Combat.damage[player] = 25; Combat.range[player] = range;
    Combat.cooldown[player] = 500; Combat.lastAttackTime[player] = 0;

    const zombie = addEntity(w);
    addComponent(w, Position, zombie);
    addComponent(w, Health, zombie);
    addComponent(w, Combat, zombie);
    addComponent(w, ZombieTag, zombie);
    Position.x[zombie] = zombieX; Position.y[zombie] = 0;
    Health.current[zombie] = 60; Health.max[zombie] = 60;
    Combat.damage[zombie] = 10; Combat.range[zombie] = 32;
    Combat.cooldown[zombie] = 1200; Combat.lastAttackTime[zombie] = 0;

    return { w, player, zombie };
  }

  it('player attacks zombie in range when cooldown elapsed', () => {
    const { w, zombie } = setup(0, 30, 48);
    w.elapsed = 600;
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
    w.elapsed = 100;
    combatSystem(w);
    expect(Health.current[zombie]).toBe(60);
  });

  it('zombie attacks player in range', () => {
    const { w, player } = setup(0, 20, 48);
    w.elapsed = 1300;
    combatSystem(w);
    expect(Health.current[player]).toBeLessThan(100);
  });
});
