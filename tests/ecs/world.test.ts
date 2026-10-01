import { describe, expect, it } from 'vitest';
import { defineComponent, Types } from 'bitecs';
import {
  createGameWorld,
  addEntity,
  addComponent,
  removeEntity,
  hasComponent,
} from '@/ecs/world';

const Position = defineComponent({ x: Types.f32, y: Types.f32 });

describe('ecs world', () => {
  it('creates a world', () => {
    const w = createGameWorld();
    expect(w).toBeDefined();
  });

  it('adds and removes entities', () => {
    const w = createGameWorld();
    const e = addEntity(w);
    expect(typeof e).toBe('number');
    removeEntity(w, e);
  });

  it('adds components and sets values', () => {
    const w = createGameWorld();
    const e = addEntity(w);
    addComponent(w, Position, e);
    Position.x[e] = 10;
    Position.y[e] = 20;

    expect(hasComponent(w, Position, e)).toBe(true);
    expect(Position.x[e]).toBe(10);
    expect(Position.y[e]).toBe(20);
  });

  it('tracks deltaTime across ticks', () => {
    const w = createGameWorld();
    w.deltaTime = 1 / 30;
    expect(w.deltaTime).toBeCloseTo(1 / 30);
  });
});
