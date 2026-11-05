# 6_IL BGM identity — 2026-11-04

눈보라 속 등불 — 설원 개척·첼로와 맑은 종, 따뜻한 난방 거점

- Replaced the exact runtime assets listed in `audio/BGM_IDENTITY_20261104.json`. Existing save data, gameplay, SFX and original source paths are preserved.
- Original MIDI hooks, game-specific lead rhythm, arrangement, instrumental colors and harmony; GeneralUser GS License v2.0 is included.
- Repeated source render + circular tail, -21 LUFS target, complete Vorbis decode, stereo 44100 Hz, peak and seam checks passed at media creation. Actual per-file measurements are in the manifest.
- Source integration only: existing executable has not been rebuilt. Native hardware listening and blind game identification remain unverified.
- Codex implemented this scope because the user directly requested the music changes. Existing unrelated dirty changes were retained.
