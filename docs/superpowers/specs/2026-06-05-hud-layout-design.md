# HUD Layout And Style Refactor Design

Date: 2026-06-05
Project: Ini's Land / 6_IL
Target: Unity IMGUI HUD

## Goal

Clean up the overall HUD layout without replacing the current IMGUI system. The refactor should make the screen easier to read during combat and survival play, while reducing maintenance cost by moving visual constants out of scattered drawing code.

The chosen direction is option C: keep IMGUI, but centralize layout and style data.

## Current Problems

- `SimpleHud` owns too many responsibilities: player status, resources, village state, interaction buttons, build hotbar, modals, warnings, overlays, debug, and visual effects.
- Many visual values are embedded directly in draw methods: positions, sizes, spacing, font sizes, colors, button dimensions, and panel offsets.
- Contextual actions can compete for the same screen area.
- HUD text and images can be clipped when the viewport is small or when several systems are active at once.
- Some UI rendering code is difficult to change because layout, styling, data lookup, and gameplay actions are mixed together.

## Layout Model

The HUD should be organized into five stable screen regions.

### Top Left: Player Status

Shows only always-important player information:

- HP
- XP or level progress
- body temperature
- current weapon and mode

This area should stay compact and readable during combat.

### Top Right: Resources

Shows the resource inventory as a compact icon grid:

- wood
- stone
- meat
- food
- frostbloom

The resource area should use fixed cell dimensions and avoid resizing when values change. It should remain in the top right because the user requested resources there.

### Bottom Left: Village And Companions

Shows settlement-level information:

- companion stance
- population
- daily food consumption
- pregnancy and birth status
- current wave or settlement pressure
- medium-term settlement goal summary when relevant

This area should be readable at a glance but should not grow into a large quest log.

### Bottom Center: Context Actions

Shows actions the player can take now:

- recruit
- repair
- upgrade
- refuel
- gather wood
- mine stone
- farm actions
- fence repair or rebuild work

Only the highest priority available actions should be visible at the same time. The priority order is:

1. recruit
2. urgent repair or refuel
3. building upgrade
4. fence repair or rebuild
5. farm action
6. gather or mine

The default cap is three visible action buttons. Lower-priority actions can be hidden until higher-priority actions are no longer available.

### Center Overlay: Modals And Events

Center screen should be reserved for modal or high-attention UI:

- companion recruitment visual novel scene
- rune selection
- death overlay
- pause menu
- boss warning
- day/night phase banner
- autosave or achievement toast

These overlays should not share layout rules with the persistent HUD. They should have their own modal rectangles from the layout helper.

## Architecture

### `HudLayout`

New static helper responsible only for rectangle calculation.

Responsibilities:

- define screen margins
- define safe area values
- return top-left, top-right, bottom-left, bottom-center, and modal rectangles
- scale compactly for small screens
- provide common row, cell, button, icon, and panel dimensions

It should not read gameplay state and should not draw anything.

### `HudStyleConfig`

New serializable style container or static config that centralizes visual constants.

Initial version can be a static class to keep the change small. Later it can become a `ScriptableObject` if iteration in the Unity Inspector becomes important.

Responsibilities:

- font sizes
- padding
- row heights
- icon sizes
- button heights
- panel alpha values
- border thickness
- spacing between HUD groups

Existing `UiTheme` should keep drawing primitives and colors. `HudStyleConfig` should hold dimensions and typography values.

### `SimpleHud`

`SimpleHud` remains the entry point during the first pass.

Responsibilities after refactor:

- call the HUD sections in order
- own cached textures and GUI styles until a later split
- route user actions to gameplay systems
- keep modal state

It should gradually stop hard-coding layout numbers.

### Future Section Classes

After the first pass is stable, the following classes can be extracted:

- `StatusHud`
- `ResourceHud`
- `VillageHud`
- `InteractionHud`
- `ModalHud`

This extraction is intentionally second-phase work. The first phase should reduce risk by preserving the current `SimpleHud` entry point.

## Data Flow

1. `SimpleHud.OnGUI()` calls `EnsureStyles()`.
2. `SimpleHud` asks `HudLayout` for stable regions.
3. Each draw method receives or computes its own region from `HudLayout`.
4. Draw methods read game state as they do today.
5. Buttons still call existing gameplay methods.
6. Modal state remains in `SimpleHud`.

No gameplay behavior should change during the layout refactor except action button visibility priority.

## Interaction Button Rules

Contextual actions should be collected before drawing.

Each candidate action should have:

- label
- enabled flag
- priority
- optional icon key
- callback
- optional warning color

The renderer sorts by priority and draws at most three actions in the bottom-center region.

This avoids stacking many world buttons in different places.

## Responsive Rules

- Use fixed minimum sizes for icons and buttons.
- Use shorter labels on narrow screens.
- Avoid font scaling based on viewport width.
- Prevent dynamic values from resizing panels.
- Clamp panels inside screen margins.
- Modal overlays may cover the center, but persistent HUD regions should not overlap each other.

## Visual Direction

The HUD should feel like a quiet survival management interface, not a marketing-style or overly decorative layout.

Use:

- restrained dark panels
- small gold accents
- existing generated HUD icons
- compact rows
- stable icon cells
- clear active, disabled, and danger states

Avoid:

- large floating cards for every feature
- nested cards
- oversized text
- one-color monotone palettes
- decorative background shapes

## Migration Plan

1. Add `HudStyleConfig`.
2. Add `HudLayout`.
3. Move resource panel dimensions and top-right positioning to `HudLayout`.
4. Move player status panel dimensions and top-left positioning to `HudLayout`.
5. Build an interaction action list and render it in bottom-center.
6. Move village and companion status into a bottom-left region.
7. Move modal rectangles to `HudLayout`.
8. Remove obsolete scattered positioning constants from `SimpleHud`.

## Testing And Verification

Manual Unity checks:

- 16:9 desktop viewport
- narrow window or low resolution
- day phase with gather/farm/build actions
- night phase with wave and blizzard status
- companion recruitment panel
- companion recruitment cutscene
- rune selection modal
- death overlay
- build mode hotbar

Code checks:

- Unity editor compile
- no new C# warnings caused by missing references
- `git diff --check`

Reference checks:

- Ensure existing HUD icons under `Assets/Resources/UI/hud` still load.
- Ensure companion portrait resources still load.
- Ensure no generated executable or build output is committed as part of this refactor.

## Out Of Scope

- Full Unity Canvas migration
- New art generation
- New gameplay systems
- Rebalancing resources, combat, or village progression
- Replacing IMGUI entirely

## Open Decision

The first implementation should keep `SimpleHud` as the single `MonoBehaviour` entry point. Further class extraction should happen only after the layout and style constants are centralized and verified in play.
