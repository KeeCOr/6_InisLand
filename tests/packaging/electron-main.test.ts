import { describe, expect, it } from 'vitest';
import { existsSync, readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import packageJson from '../../package.json';

describe('Electron packaging entrypoint', () => {
  it('uses a CommonJS main file when package type is module', () => {
    expect(packageJson.type).toBe('module');
    expect(packageJson.main).toMatch(/\.cjs$/);
    expect(existsSync(resolve(packageJson.main))).toBe(true);
  });

  it('includes the Electron main file in electron-builder files', () => {
    const files = packageJson.build.files;

    expect(files).toContain(packageJson.main);
  });

  it('does not leave the CommonJS Electron entrypoint as a .js module', () => {
    const mainJsPath = resolve('main.js');
    if (!existsSync(mainJsPath)) return;

    const mainJs = readFileSync(mainJsPath, 'utf8');
    expect(mainJs).not.toContain("require('electron')");
  });
});
