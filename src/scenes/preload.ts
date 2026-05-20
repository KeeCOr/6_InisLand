import Phaser from 'phaser';

export class PreloadScene extends Phaser.Scene {
  constructor() {
    super({ key: 'Preload' });
  }

  create(): void {
    const g = (color: number, w: number, h: number, key: string) => {
      const gfx = this.make.graphics({ x: 0, y: 0, add: false });
      gfx.fillStyle(color);
      gfx.fillRect(0, 0, w, h);
      gfx.generateTexture(key, w, h);
      gfx.destroy();
    };

    g(0x4488ff, 24, 24, 'player');
    g(0x44aa44, 20, 20, 'zombie');
    g(0xaa7744, 18, 18, 'deer');
    g(0x226622, 24, 32, 'tree');
    g(0x888888, 20, 16, 'rock');
    g(0xff6622, 48, 48, 'bonfire');
    g(0x8b5e3c, 28, 28, 'barricade');

    // Snow tile with subtle border
    const snowGfx = this.make.graphics({ x: 0, y: 0, add: false });
    snowGfx.fillStyle(0xe8eef4);
    snowGfx.fillRect(0, 0, 32, 32);
    snowGfx.lineStyle(1, 0xd0d8e0, 0.3);
    snowGfx.strokeRect(0, 0, 32, 32);
    snowGfx.generateTexture('snow_tile', 32, 32);
    snowGfx.destroy();

    // Village ground tile
    const villageGfx = this.make.graphics({ x: 0, y: 0, add: false });
    villageGfx.fillStyle(0x5a4a3a);
    villageGfx.fillRect(0, 0, 32, 32);
    villageGfx.lineStyle(1, 0x4a3a2a, 0.3);
    villageGfx.strokeRect(0, 0, 32, 32);
    villageGfx.generateTexture('village_tile', 32, 32);
    villageGfx.destroy();

    this.scene.start('Game');
  }
}
