# project-values.md — `responsive-ui` section template

Paste into `.claude/rules/project-values.md` and fill every value from the project. Read the scaler values from the
prefab (`CanvasScaler.referenceResolution`, `matchWidthOrHeight`), not from memory.

```markdown
## responsive-ui

UI_SCALER_PREFAB = Assets/.../UIManager.prefab
CANVAS_REFERENCE = 390x844            # CanvasScaler reference resolution
CANVAS_MATCH = 0                      # 0 width, 0.5 blend, 1 height
MOCKUP_DIR = Assets/.../screens
MOCKUP_TO_CANVAS = 1/3                # canvas units per mockup pixel (1170 px mockup on a 390 canvas)
UI_SPRITES_DIR = Assets/.../ui
TITLE_FONT = <Font SDF>
BODY_FONT = <Font SDF>
CANVAS_DIR = Assets/.../Resources/UI
CANVAS_ASSEMBLY = <Game>.UI          # System.Type.GetType("UI.CanvasHome, <Game>.UI")
PREVIEW_DEVICES = default             # PreviewDevice.Defaults, or list name WxH [safeTop safeBottom]
```
```

Examples:

| Key | Conveyor Unjam | BlockDrag |
|---|---|---|
| `UI_SCALER_PREFAB` | `Assets/_Game/_UI/UIManager.prefab` | `Assets/_Game/Resources/UIManager.prefab` |
| `CANVAS_REFERENCE` / `CANVAS_MATCH` | 390x844 / 0 | 1080x1920 / 0.5 |
| `MOCKUP_DIR` | `Assets/_Game/_ConveyorUnjam/Resource/ConveyorUnjam_StyleA/screens` | `Assets/_Game/_BlockDrag/Resource/ComboBlock_UI_assets/UI Preview` |
| `MOCKUP_TO_CANVAS` | 1/3 | 3 |
| `TITLE_FONT` / `BODY_FONT` | `Baloo2 SDF` / `Nunito SDF` | `Fredoka-Bold Title SDF` / — |
| `CANVAS_DIR` / `CANVAS_ASSEMBLY` | `Assets/_Game/_UI/Resources/UI` / `ConveyorUnjam.UI` | `Assets/_Game/Resources/UI` / `BlockDrag.UI` |
