import { VILLAGE_GRID_SIZE, TILE_SIZE } from '@/config/constants';

export enum BuildingType {
  Bonfire = 'bonfire',
  Barricade = 'barricade',
}

interface BuildingDef {
  width: number;
  height: number;
  maxHp: number;
}

const BUILDING_DEFS: Record<BuildingType, BuildingDef> = {
  [BuildingType.Bonfire]: { width: 2, height: 2, maxHp: 400 },
  [BuildingType.Barricade]: { width: 1, height: 1, maxHp: 200 },
};

export interface Building {
  type: BuildingType;
  gridX: number;
  gridY: number;
  hp: number;
  maxHp: number;
}

export class VillageGrid {
  readonly size = VILLAGE_GRID_SIZE;
  private readonly cells: (Building | null)[][] = [];
  private readonly buildings: Building[] = [];

  constructor() {
    for (let y = 0; y < this.size; y++) {
      this.cells[y] = [];
      for (let x = 0; x < this.size; x++) {
        this.cells[y]![x] = null;
      }
    }
  }

  place(type: BuildingType, gx: number, gy: number): boolean {
    const def = BUILDING_DEFS[type];
    if (!this.canPlace(gx, gy, def.width, def.height)) return false;

    const building: Building = {
      type, gridX: gx, gridY: gy,
      hp: def.maxHp, maxHp: def.maxHp,
    };

    for (let dy = 0; dy < def.height; dy++) {
      for (let dx = 0; dx < def.width; dx++) {
        this.cells[gy + dy]![gx + dx] = building;
      }
    }
    this.buildings.push(building);
    return true;
  }

  private canPlace(gx: number, gy: number, w: number, h: number): boolean {
    if (gx < 0 || gy < 0 || gx + w > this.size || gy + h > this.size) return false;
    for (let dy = 0; dy < h; dy++) {
      for (let dx = 0; dx < w; dx++) {
        if (this.cells[gy + dy]![gx + dx] !== null) return false;
      }
    }
    return true;
  }

  getAt(gx: number, gy: number): BuildingType | null {
    if (gx < 0 || gy < 0 || gx >= this.size || gy >= this.size) return null;
    return this.cells[gy]![gx]?.type ?? null;
  }

  getBuilding(gx: number, gy: number): Building | null {
    if (gx < 0 || gy < 0 || gx >= this.size || gy >= this.size) return null;
    return this.cells[gy]![gx] ?? null;
  }

  damageAt(gx: number, gy: number, amount: number): void {
    const building = this.getBuilding(gx, gy);
    if (!building) return;
    building.hp -= amount;
    if (building.hp <= 0) {
      this.removeBuilding(building);
    }
  }

  private removeBuilding(building: Building): void {
    const def = BUILDING_DEFS[building.type];
    for (let dy = 0; dy < def.height; dy++) {
      for (let dx = 0; dx < def.width; dx++) {
        this.cells[building.gridY + dy]![building.gridX + dx] = null;
      }
    }
    const idx = this.buildings.indexOf(building);
    if (idx >= 0) this.buildings.splice(idx, 1);
  }

  getBuildings(): readonly Building[] {
    return this.buildings;
  }

  toPixel(gx: number, gy: number): { x: number; y: number } {
    return { x: gx * TILE_SIZE, y: gy * TILE_SIZE };
  }
}
