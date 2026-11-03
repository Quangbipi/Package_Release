# Quang Audio

## Purpose

Default implementation of the `IAudioService` contract from `com.quang.base`. Consumers keep calling `Locator.Audio?.PlaySfx(...)` / `PlayBgm(...)`; nothing references this assembly directly.

## Layer + dependencies

- Layer: `4`
- Dependencies: `com.quang.base` 0.27.0, `com.quang.data` 0.12.6, `com.quang.designpattern` 0.4.5
- Prerequisites: DOTween, Sirenix Odin Inspector (serialization)

## Quick start

1. **Create > ScriptableObjects > System > AudioData** and fill the BGM / SFX dictionaries (`clip`, `multiplier`).
2. Build the prefab:
   ```
   AudioManager      AudioManager · ObjectContainer (MainObjects = [SFX], Amounts = [10])
   ├── BGM           AudioSource (loop, playOnAwake off)   → AudioManager.bgm.audioSource
   └── SFX           AudioSource (playOnAwake off)          → AudioManager.sfx.audioSource
   ```
   Assign `audioData` and `audioSourcePool`. At runtime `ObjectContainer` clones `SFX` to the pool size.
3. Put the prefab at the root of the first scene (it calls `DontDestroyOnLoad`). Extra instances in later scenes destroy themselves.
4. Play: `Locator.Audio?.PlaySfx(SFX_TYPE.CLICK)`, `Locator.Audio?.PlayBgm(BGM_TYPE.HOME)`.

## Public API index

- `AudioManager` — `Singleton<AudioManager>, IAudioService`; `AudioData`, `BgmVolume`, `SfxVolume`, `IsBgmMute()`, `IsSfxMute()`.
- `AudioSourceData` — one channel: `Source`, `currentAudio`, `BaseVolume` (0 = muted, 1 = on), `Volume` (= `BaseVolume * multiplier`), `SetAudio`, `Play`, `Pause`, `Stop`, `IsPlaying`, `IsMute`.
- `AudioData` — `BgmAudioDict`, `SfxAudioDict`; `Audio { clip, multiplier }`.

## Known limitations / sharp edges

- Mute flags are read from `GameData.setting` when the manager starts; `ToggleBgmVolume` / `ToggleSfxVolume` do not save (the settings UI saves).
- `PlayBgm` with the BGM that is already current does nothing; it fades the current BGM out, then switches (no crossfade).
- `PlaySfx` restarts the current SFX source if all pooled sources are busy; `PlayLoopSfx` returns `null` in that case.
- `ToggleSfxVolume` does not change sources already looping via `PlayLoopSfx`.

## Samples

None.
