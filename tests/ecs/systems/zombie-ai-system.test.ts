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
    Position.x[player] = 200; Position.y[player] = 0;
    Health.current[player] = 100; Health.max[player] = 100;

    const zombie = addEntity(w);
    addComponent(w, Position, zombie);
    addComponent(w, Velocity, zombie);
    addComponent(w, Health, zombie);
    addComponent(w, ZombieTag, zombie);
    Position.x[zombie] = 0; Position.y[zombie] = 0;
    Health.current[zombie] = 60; Health.max[zombie] = 60;

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
