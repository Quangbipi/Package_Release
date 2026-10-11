# Changelog

## [0.11.0] - 2026-09-29
### Removed
- `SDKToolkitWindow` / `SDKToolkitSettingsWindow` (`Tools/Universal/Maintenance/SDK Toolkit`): redundant tool.

## [0.10.1] - 2026-09-26
- Republished current source changes in the public snapshot.

## [0.10.0] - 2026-09-24
### Fixed
- `SpineSpriteSheetBakeSetup` and `SpineSpriteSheetParticleBakeSetup` moved from the editor-only
  `Quang.Tool.Editor.Spine` assembly into a new runtime assembly `Quang.Tool.Spine`
  (`SpineBake/`). As `MonoBehaviour`s inside an Editor-platform assembly, Unity could not load
  them on scene GameObjects ("The associated script can not be loaded"). Script `.meta` GUIDs
  are unchanged, so existing scene references reconnect without edits.
- `Quang.Tool.Editor.Spine` now references `Quang.Tool.Spine`.

## [0.8.1] - 2026-08-31
### Fixed
- Republished so the folder `.meta` files for `Editor/GrayscaleTextureConverter/` and its
  `Tests/` subfolder ship with the package. Both are committed in source but had never been
  carried into a public snapshot, so Unity logged "has no meta file, but it's in an immutable
  folder. The asset will be ignored." and skipped the Grayscale Texture Converter tool entirely.

## [0.7.0] - 2026-08-26
### Removed
- **Dependencies dropped:** `com.quang.base` and `com.quang.ui` removed from `package.json`, and the matching `Quang.UI`/`Quang.Base` entries removed from `Quang.Tool.asmdef` references. No source file in this package used any `Quang.*` type or namespace from either dependency; the declarations were stale. Real references retained: `DOTween.Modules`, `Unity.TextMeshPro` (root asmdef) and `spine-unity`/`spine-csharp` (Spine editor asmdef).
- **Migration:** the package is now self-contained and installable without pulling `com.quang.ui`, which lets consumers that keep their own local `Quang.UI` assembly adopt this package without an assembly-name collision.

## [0.6.0] - 2026-08-26
- Absorbed the latest PetVsMonster implementations of the duplicate asset resolver, asmdef reference finder, Meta GUID unifier, prefab particle scaler, Spine sprite-sheet baker, and Spine particle setup window.
- Added the generic VFX Color Tone Shifter editor tool with live preview and non-destructive tone adjustment.

## [0.5.4] - 2026-08-15
- Dependency alignment: com.quang.base 0.19.3 -> 0.19.4.

## [0.5.3] - 2026-08-11
- Dependency-only patch: align Base 0.19.2 and UI 0.5.3 for the F4-IE editor prerequisite.

## [0.5.2] - 2026-08-09
- Dependency-only patch: align exact package constraints for the approved F3B propagation; no runtime or API behavior changed.
- Dependency alignment: com.quang.base 0.19.0 -> 0.19.1; com.quang.ui 0.5.1 -> 0.5.2.

## [0.5.1] - 2026-07-21
- Dependency-only patch aligning package declarations with the ItemId migration release; no tool behavior changed.

## [0.5.0] - 2026-07-11
- **Migration (B1 Pass 6):** `Tool`->`Quang.Tool` (12 files in this package + 4 more physically in `Assets/_Game/_Scripts/Tools` sharing the same bare namespace: `UIFlowCodeEmitter`/`UIFlowPrefabBuilder`/`UIFlowReverseImporter`/`UIFlowGeneratorWindow`). `rootNamespace` corrected `Tools`/`Tools.Editor` (plural, never matched real code)->`Quang.Tool`/`Quang.Tool.Editor`. `DuplicateAssetResolver.EditorTool` namespace and `MetaGuidUnifierWindow.cs`'s already-`Quang.Base` squat left untouched (out of scope, no plan requirement).

## [0.4.0] - 2026-07-11
### Changed
- **Migration (B1 Pass 3 - base):** `MetaGuidUnifierWindow.cs` (`Editor/AssetTools/`), which squatted the bare `Base` namespace despite living in this package, renamed `Base`->`Quang.Base` alongside `com.quang.base`'s own Pass 3 rename.

## [0.3.0] - 2026-07-11
### Removed
- `GameLog` diagnostics (GameLog/GameLogConfig/GameLogRecord/GameLogScope/JsonFileLogSink/GameDiagnosticsBootstrap + Editor viewer window) re-homed to new `com.quang.diagnostics` 0.1.0 package (Ph3 Task 3.5). GUID-preserved move, zero behavior change. **Migration:** consumers referencing GameLog types need `com.quang.diagnostics` added as a dependency; no code changes required (namespace unchanged).

## [0.2.0] - 2026-07-09
### Changed
- Spine bake tools split into own `Quang.Tool.Editor.Spine` asmdef (delete-folder opt-out for Spine-less clones; `versionDefines` unusable since Spine is Assets-based, not a UPM package).
- `PrefabParticleScalerWindow` (no Spine dependency) moved from `Editor/SpineBake/` to `Editor/AssetTools/`.

## [0.1.0] - 2026-07-07
- Union of Package_Repo _Tools (GameLog, UI flow, DOTween preview) and PetVsMonster editor tools (Spine bakers, MetaGuidUnifier, AsmdefReferenceFinder, DuplicateAssetResolver).
