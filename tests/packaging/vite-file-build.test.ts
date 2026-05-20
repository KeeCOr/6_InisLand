import { describe, expect, it } from 'vitest';
import { readFileSync } from 'node:fs';

describe('Vite file build', () => {
  it('uses relative asset paths so Electron file:// loads the renderer bundle', () => {
    const config = readFileSync('vite.config.ts', 'utf8');

    expect(config).toContain("base: './'");
  });
});
