# Changelog

## [0.2.0] - 2026-11-04
### Changed (breaking)
- Rebuilt after the PetVsMonster `AudioManager`: `AudioManager : Singleton<AudioManager>, IAudioService` with two serialized `AudioSourceData` channels (`bgm`, `sfx`) on child GameObjects `BGM` / `SFX`, and an `ObjectContainer` pool (`MainObjects = [SFX]`) for overlapping and looping SFX. Fades use DOTween on unscaled time.
- `AudioData` (Odin `SerializedScriptableObject`, `Dictionary<BGM_TYPE, Audio>` / `Dictionary<SFX_TYPE, Audio>`, `Audio { clip, multiplier }`) replaces `AudioLibrarySO`. `SfxSourcePool` removed. Per-SFX cooldown, pitch jitter, random clip lists and BGM crossfade are gone.
- The manager reads `GameData.setting.isBgmMute/isSfxMute` on start; new dependencies `com.quang.data` 0.12.6 and `com.quang.designpattern` 0.4.5.
### Fixed (compared with the reference implementation)
- No `NullReferenceException` when there is no `Locator.Data` (defaults to unmuted).
- A second `AudioManager` destroys itself instead of replacing `Locator.Audio`.
- `PlayBgm` switches immediately when nothing is playing instead of fading silence.
- No exception when every pooled SFX source is busy: the current source is reused; `PlayLoopSfx` returns `null`.

## [0.1.0] - 2026-11-03
- Initial release: `AudioManager` implements `IAudioService` (registers `Locator.Audio`, pooled SFX with per-type cooldown and pitch jitter, looping SFX, BGM crossfade on unscaled time, BGM/SFX mute), `AudioLibrarySO` maps `SFX_TYPE`/`BGM_TYPE` to clips, `SfxSourcePool<T>` picks a free or oldest source. EditMode tests cover the library, pool and manager.
