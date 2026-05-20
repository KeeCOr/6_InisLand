import Phaser from 'phaser';
import type { ResourceManager } from '@/gameplay/resource-manager';
import type { DayNightCycle } from '@/gameplay/day-night-cycle';
import { Health } from '@/ecs/components';

const PHASE_NAMES: Record<string, string> = {
  day: '낮',
  evening: '저녁',
  night: '밤',
  dawn: '새벽',
};

export class HUD {
  private readonly resText: Phaser.GameObjects.Text;
  private readonly dayText: Phaser.GameObjects.Text;
  private readonly waveText: Phaser.GameObjects.Text;
  private readonly hpBar: Phaser.GameObjects.Graphics;

  constructor(
    private readonly scene: Phaser.Scene,
    private readonly resources: ResourceManager,
    private readonly cycle: DayNightCycle,
    private readonly playerEid: number,
  ) {
    this.resText = scene.add
      .text(10, 10, '', {
        fontFamily: 'ui-monospace, monospace',
        fontSize: '14px',
        color: '#e0e0e0',
      })
      .setScrollFactor(0)
      .setDepth(1000);

    this.dayText = scene.add
      .text(scene.scale.width / 2, 10, '', {
        fontFamily: 'ui-monospace, monospace',
        fontSize: '16px',
        color: '#ffd700',
      })
      .setOrigin(0.5, 0)
      .setScrollFactor(0)
      .setDepth(1000);

    this.waveText = scene.add
      .text(scene.scale.width - 10, 10, '', {
        fontFamily: 'ui-monospace, monospace',
        fontSize: '14px',
        color: '#ff6666',
      })
      .setOrigin(1, 0)
      .setScrollFactor(0)
      .setDepth(1000);

    this.hpBar = scene.add.graphics().setScrollFactor(0).setDepth(1000);
  }

  setWaveText(text: string): void {
    this.waveText.setText(text);
  }

  update(): void {
    const res = this.resources.getAll();
    this.resText.setText(
      `나무:${res.wood} 돌:${res.stone} 고기:${res.meat} 식량:${res.food}`,
    );

    const phaseName = PHASE_NAMES[this.cycle.phase] ?? this.cycle.phase;
    const remaining = Math.ceil(this.cycle.remainingSec);
    this.dayText.setText(`Day ${this.cycle.day} — ${phaseName} (${remaining}s)`);

    // HP bar
    const hp = Health.current[this.playerEid] ?? 0;
    const maxHp = Health.max[this.playerEid] ?? 1;
    const ratio = Math.max(0, hp / maxHp);
    const barW = 120;
    const barH = 12;
    const barX = 10;
    const barY = this.scene.scale.height - 30;

    this.hpBar.clear();
    this.hpBar.fillStyle(0x333333);
    this.hpBar.fillRect(barX, barY, barW, barH);
    this.hpBar.fillStyle(ratio > 0.3 ? 0x44cc44 : 0xcc4444);
    this.hpBar.fillRect(barX, barY, barW * ratio, barH);
    this.hpBar.lineStyle(1, 0xffffff, 0.5);
    this.hpBar.strokeRect(barX, barY, barW, barH);
  }
}
