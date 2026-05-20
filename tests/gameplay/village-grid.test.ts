import { describe, expect, it } from 'vitest';
import { VillageGrid, BuildingType } from '@/gameplay/village-grid';

describe('VillageGrid', () => {
  it('creates a 24x24 grid', () => {
    const grid = new VillageGrid();
    expect(grid.size).toBe(24);
  });

  it('places a bonfire (2x2) at center', () => {
    const grid = new VillageGrid();
    expect(grid.place(BuildingType.Bonfire, 11, 11)).toBe(true);
    expect(grid.getAt(11, 11)).toBe(BuildingType.Bonfire);
    expect(grid.getAt(12, 12)).toBe(BuildingType.Bonfire);
  });

  it('rejects overlapping placement', () => {
    const grid = new VillageGrid();
    grid.place(BuildingType.Bonfire, 11, 11);
    expect(grid.place(BuildingType.Bonfire, 12, 12)).toBe(false);
  });

  it('places a barricade (1x1)', () => {
    const grid = new VillageGrid();
    expect(grid.place(BuildingType.Barricade, 5, 5)).toBe(true);
    expect(grid.getAt(5, 5)).toBe(BuildingType.Barricade);
  });

  it('rejects out-of-bounds placement', () => {
    const grid = new VillageGrid();
    expect(grid.place(BuildingType.Barricade, 24, 24)).toBe(false);
    expect(grid.place(BuildingType.Barricade, -1, 0)).toBe(false);
  });

  it('gets building HP', () => {
    const grid = new VillageGrid();
    grid.place(BuildingType.Bonfire, 11, 11);
    const b = grid.getBuilding(11, 11);
    expect(b).toBeDefined();
    expect(b!.hp).toBe(400);
  });

  it('damages a building', () => {
    const grid = new VillageGrid();
    grid.place(BuildingType.Barricade, 5, 5);
    grid.damageAt(5, 5, 50);
    expect(grid.getBuilding(5, 5)!.hp).toBe(150);
  });

  it('removes building when HP reaches 0', () => {
    const grid = new VillageGrid();
    grid.place(BuildingType.Barricade, 5, 5);
    grid.damageAt(5, 5, 200);
    expect(grid.getAt(5, 5)).toBeNull();
  });

  it('converts grid coords to pixel coords', () => {
    const grid = new VillageGrid();
    const px = grid.toPixel(12, 12);
    expect(px.x).toBe(12 * 32);
    expect(px.y).toBe(12 * 32);
  });
});
