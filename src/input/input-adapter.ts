export interface InputState {
  dx: number;
  dy: number;
  interact: boolean;
}

export function applyKeysToState(pressed: Set<string>): InputState {
  let dx = 0;
  let dy = 0;

  if (pressed.has('W') || pressed.has('ARROWUP')) dy -= 1;
  if (pressed.has('S') || pressed.has('ARROWDOWN')) dy += 1;
  if (pressed.has('A') || pressed.has('ARROWLEFT')) dx -= 1;
  if (pressed.has('D') || pressed.has('ARROWRIGHT')) dx += 1;

  const len = Math.sqrt(dx * dx + dy * dy);
  if (len > 1) {
    dx /= len;
    dy /= len;
  }

  return {
    dx,
    dy,
    interact: pressed.has('F'),
  };
}

export class InputAdapter {
  private readonly pressed = new Set<string>();

  register(scene: Phaser.Scene): void {
    scene.input.keyboard!.on('keydown', (e: KeyboardEvent) => {
      this.pressed.add(e.key.toUpperCase());
    });
    scene.input.keyboard!.on('keyup', (e: KeyboardEvent) => {
      this.pressed.delete(e.key.toUpperCase());
    });
  }

  poll(): InputState {
    return applyKeysToState(this.pressed);
  }
}
