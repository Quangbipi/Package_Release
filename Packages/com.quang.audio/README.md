# Quang Audio

## Purpose

Default implementation of the `IAudioService` contract from `com.quang.base`. Consumers keep calling `Locator.Audio?.PlaySfx(...)` / `PlayBgm(...)`; nothing references this assembly directly.

## Layer + dependencies

- Layer: `4`
- Dependencies: `com.quang.base` 0.27.0 (no DOTween / Odin)

## Quick start

1. **Create > Quang > Audio > Audio Library** and map each `SFX_TYPE` / `BGM_TYPE` you use to clips.
2. Add an `AudioManager` component to a root GameObject in the first scene and assign the library. It registers `Locator.Audio` in `Awake` and survives scene loads (`dontDestroyOnLoad`).
3. Play: `Locator.Audio?.PlaySfx(SFX_TYPE.CLICK)`, `Locator.Audio?.PlayBgm(BGM_TYPE.HOME)`.
4. Mute: call `ToggleSfxVolume` / `ToggleBgmVolume` at startup with the saved settings (e.g. `GameData.setting.isSfxMute`) and whenever the player changes them.

## Public API index

- `AudioManager` — `MonoBehaviour, IAudioService`. Inspector: `library`, `sfxPoolSize` (default 8), `dontDestroyOnLoad`. Exposes `Library`, `IsBgmMute`, `IsSfxMute`, `CurrentBgm`.
- `AudioLibrarySO` — `SfxEntry` (`clips[]` picked at random, `volume`, `pitchJitter`, `cooldown`), `BgmEntry` (`clip`, `volume`); `TryGetSfx`, `TryGetBgm`.
- `SfxSourcePool<T>` — fixed pool; `Rent()` returns a free source or the one rented longest ago.

## Known limitations / sharp edges

- The manager does not save mute state; the caller persists it (it lives in `GameData` behind `com.quang.data`, which this package does not reference).
- A second `AudioManager` destroys its own GameObject; only the first one stays registered.
- An `SFX_TYPE` with no entry is silent and logs one warning per type in the Editor.
- `PlayBgm` with the type already playing does nothing; fades run on `Time.unscaledDeltaTime`.

## Samples

None.
