import Phaser from 'phaser';

export class GameOverScreen {
  show(scene: Phaser.Scene, day: number, kills: number): void {
    const { width, height } = scene.scale;

    const bg = scene.add
      .rectangle(width / 2, height / 2, width, height, 0x000000, 0.7)
      .setScrollFactor(0)
      .setDepth(2000);

    const title = scene.add
      .text(width / 2, height / 2 - 60, '마을이 무너졌다...', {
        fontFamily: 'ui-monospace, monospace',
        fontSize: '28px',
        color: '#ff4444',
      })
      .setOrigin(0.5)
      .setScrollFactor(0)
      .setDepth(2001);

    const stats = scene.add
      .text(width / 2, height / 2, `생존 일수: ${day}\n처치 수: ${kills}`, {
        fontFamily: 'ui-monospace, monospace',
        fontSize: '18px',
        color: '#e0e0e0',
        align: 'center',
      })
      .setOrigin(0.5)
      .setScrollFactor(0)
      .setDepth(2001);

    const restart = scene.add
      .text(width / 2, height / 2 + 80, '[R] 다시 시작', {
        fontFamily: 'ui-monospace, monospace',
        fontSize: '16px',
        color: '#ffd700',
      })
      .setOrigin(0.5)
      .setScrollFactor(0)
      .setDepth(2001);

    // Suppress unused warnings — objects stay alive in scene
    void bg;
    void title;
    void stats;
    void restart;

    scene.input.keyboard!.once('keydown-R', () => {
      scene.scene.restart();
    });
  }
}
