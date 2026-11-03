# Changelog

## [0.1.0] - 2026-11-03
- Initial release: `AudioManager` implements `IAudioService` (registers `Locator.Audio`, pooled SFX with per-type cooldown and pitch jitter, looping SFX, BGM crossfade on unscaled time, BGM/SFX mute), `AudioLibrarySO` maps `SFX_TYPE`/`BGM_TYPE` to clips, `SfxSourcePool<T>` picks a free or oldest source. EditMode tests cover the library, pool and manager.
