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
    w.deltaTime = 1 / 30;
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
