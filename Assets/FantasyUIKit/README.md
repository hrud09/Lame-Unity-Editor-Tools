# Complete Fantasy UI Kit (`com.lame.fantasyuikit`)

A modular, dynamic, zero-third-party-dependency Dark-Fantasy UI system & game-ready prefab library for Unity 6 (`com.unity.ugui` + TextMeshPro).

## Quick Start
1. Open **Unity Editor** — on first import, the automated builder imports all 103 sprites with exact 9-slice borders, builds all 35+ prefabs, creates 3 `UIThemeSO` presets, reconstructs `Assets/Scenes/Gameplayscene.unity`, and builds `Assets/FantasyUIKit/Demo/Scenes/UILibraryShowcase.unity`.
2. You can manually trigger or rebuild anytime via:
   - `Fantasy UI -> 1. Build Complete UI Library & Scenes`
   - `Window -> Fantasy UI -> UI Library & Builder` (Visual Prefab Browser, 1-click Canvas Spawner, Live Theme Switcher, and `.unitypackage` Exporter).
3. Press **Play** in `Assets/Scenes/Gameplayscene.unity` and use the top **Demo Sandbox Toolbar** (`SCROLL`, `COMPLETE`, `BAG`, `LOOT`, `SHIP`, `CARDS`, `VICTORY!`, `DEFEAT!`, `THEME`) or click any HUD element directly (Joystick, Weapon Slots, Quest Ribbons, Quick Action Tiles, Currency `+` Tags).

## Package Highlights
- **4 Cohesive Dark-Fantasy Surface Materials**: Carved Dungeon Stone & Riveted Iron, Aged Hanging Parchment Scroll, Planked Tavern Wood Drawer, and Faceted Stitched Plum Leather Bag.
- **Zero-Coupling Architecture**: `UIEventChannels` (ScriptableObject bus), `UIObservable<T>`, `UIManager` modal stack (Escape handling, dim backdrop dismiss, unscaled-time animations), `UIThemeSO` + `UIThemeBinder` live theming, and `UITweener` (no DOTween required).
- **11-Canvas GameplayScene Hierarchy**: Minimap, Floating Healthbars, Virtual Helm Joystick, Primary & Secondary Radial Cooldown Weapon Slots, Top HUD + Dual-Player Crowned-Skull Versus Dock, Hanging Quest Rail + Quick Action Tiles, Offscreen Enemy Edge Marks, Powerup Card Selection, Full Popups Stack (Stitched Leather Inventory Bag, Hanging Pause Scroll, Cracked-Stone Level Complete + Sliding Wooden Leaderboard Drawer, New Discovery, Ship Building Upgrade), Celebration Banners (`VICTORY!`, `DEFEATED!`, `AMAZING!`, `AWESOME!`), and FPS Counter.
