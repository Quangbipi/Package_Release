using System;
using System.Collections.Generic;
using DG.Tweening;
using DesignPattern;
using Quang.Base;
using Quang.DesignPattern;
using UnityEngine;

namespace Quang.Audio
{
    [Serializable]
    public struct AudioSourceData
    {
        [SerializeField]
        private AudioSource audioSource;

        /// <summary>Set by <see cref="AudioManager"/> for the SFX channel: returns a free pooled source when the current one is busy.</summary>
        [NonSerialized]
        public Func<AudioSource, AudioSource> rentFreeSource;

        public Audio currentAudio;

        [SerializeField] private float baseVolume;

        public AudioSource Source
        {
            get => audioSource;
            set => audioSource = value;
        }

        public void SetLoop(bool loop)
        {
            audioSource.loop = loop;
        }

        public void SetAudio(Audio audio)
        {
            if (audio is null) return;
            if (rentFreeSource != null)
            {
                audioSource = rentFreeSource(audioSource);
            }
            if (audioSource == null) return;

            currentAudio = audio;
            audioSource.clip = audio.clip;
            Volume = audio.multiplier;
        }

        public void Play()
        {
            if (audioSource != null) audioSource.Play();
        }

        public void Pause()
        {
            if (audioSource != null) audioSource.Pause();
        }

        public void Stop()
        {
            if (audioSource != null) audioSource.Stop();
        }

        public bool IsPlaying => audioSource != null && audioSource.isPlaying;

        public float BaseVolume
        {
            get => baseVolume;
            set
            {
                baseVolume = value;
                Volume = currentAudio?.multiplier ?? 1;
            }
        }

        public float Volume
        {
            get => audioSource.volume;
            set => audioSource.volume = baseVolume * value;
        }

        public bool IsMute => audioSource.volume < 0.01 || audioSource.mute;
    }

    /// <summary>
    /// Default <see cref="IAudioService"/>. Prefab: this component + <see cref="ObjectContainer"/> (MainObjects = [SFX]),
    /// children <c>BGM</c> and <c>SFX</c> with one <see cref="AudioSource"/> each. Reads saved mute flags from
    /// <see cref="GameData"/> on start; <see cref="ToggleBgmVolume"/>/<see cref="ToggleSfxVolume"/> do not save.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>, IAudioService
    {
        [SerializeField] private AudioData audioData;
        [SerializeField] private ObjectContainer audioSourcePool;

        [SerializeField] private AudioSourceData bgm;
        [SerializeField] private AudioSourceData sfx;

        private List<AudioSource> loopSfxAudioSource = new();
        private readonly HashSet<AudioSource> poppedSources = new();
        private int poolSize;
        private Tween bgmTween;
        private bool initialized;

        internal Func<AudioSource, bool> IsSourceBusy = source => source.isPlaying;

        public AudioData AudioData
        {
            get => audioData;
            set => audioData = value;
        }
        internal ObjectContainer AudioSourcePool
        {
            get => audioSourcePool;
            set => audioSourcePool = value;
        }
        internal AudioSourceData Bgm => bgm;
        internal AudioSourceData Sfx => sfx;

        internal void SetSources(AudioSource bgmSource, AudioSource sfxSource)
        {
            bgm.Source = bgmSource;
            sfx.Source = sfxSource;
        }

        protected void Awake()
        {
            if (!Init()) return;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }

        protected void OnDestroy()
        {
            bgmTween?.Kill();
            if (ReferenceEquals(Locator.Audio, this)) Locator.Audio = null;
        }

        /// <summary>Returns false (and destroys this object) when another manager is already registered.</summary>
        internal bool Init()
        {
            if (initialized) return true;

            if (Locator.Audio != null && !ReferenceEquals(Locator.Audio, this))
            {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
                return false;
            }

            audioSourcePool.OnInit();
            poolSize = audioSourcePool.Amounts.Length > 0 ? audioSourcePool.Amounts[0] : 0;
            sfx.rentFreeSource = RentFreeSfxSource;
            AudioSource first = TryPop();
            if (first != null) sfx.Source = first;
            loopSfxAudioSource = new List<AudioSource>();

            GameData data = Locator.Data?.GetData<GameData>();
            bgm.BaseVolume = data != null && data.setting.isBgmMute ? 0 : 1;
            sfx.BaseVolume = data != null && data.setting.isSfxMute ? 0 : 1;

            Locator.Audio = this;
            initialized = true;
            return true;
        }

        public float BgmVolume => bgm.BaseVolume;
        public float SfxVolume => sfx.BaseVolume;

        private Audio GetBgmAudio(BGM_TYPE type)
        {
            return audioData != null ? GetAudio(audioData.BgmAudioDict, type) : null;
        }

        private Audio GetSfxAudio(SFX_TYPE type)
        {
            return audioData != null ? GetAudio(audioData.SfxAudioDict, type) : null;
        }

        private static Audio GetAudio<T>(IReadOnlyDictionary<T, Audio> audioDictionary, T type)
        {
            return audioDictionary.GetValueOrDefault(type);
        }

        public void PlayBgm(BGM_TYPE type, float fadeOut = 0.3f)
        {
            Audio audioIn = GetBgmAudio(type);
            if (audioIn is null) return;
            if (audioIn == bgm.currentAudio) return;
            bgm.SetLoop(true);
            bgmTween?.Kill();
            if (fadeOut <= 0f || bgm.IsMute || !bgm.IsPlaying)
            {
                bgm.SetAudio(audioIn);
                bgm.Play();
            }
            else
            {
                bgmTween = DOVirtual.Float(bgm.currentAudio.multiplier, 0, fadeOut, value => bgm.Volume = value)
                    .SetEase(Ease.Linear)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        bgm.SetAudio(audioIn);
                        bgm.Play();
                    });
            }
        }

        public void PlaySfx(SFX_TYPE type)
        {
            Audio audioIn = GetSfxAudio(type);
            if (audioIn is null) return;
            sfx.SetAudio(audioIn);
            sfx.Play();
        }

        public AudioSource PlayLoopSfx(SFX_TYPE type, float fadeIn = 0.3f)
        {
            Audio audioIn = GetSfxAudio(type);
            if (audioIn is null) return null;

            AudioSource audioSource = TryPop();
            if (audioSource == null) return null;

            float volume = audioIn.multiplier * sfx.BaseVolume;
            audioSource.clip = audioIn.clip;
            audioSource.loop = true;
            audioSource.volume = 0;
            DOVirtual.Float(0, volume, fadeIn, x => audioSource.volume = x).SetEase(Ease.Linear).SetUpdate(true);
            loopSfxAudioSource.Add(audioSource);
            audioSource.Play();
            return audioSource;
        }

        public void StopLoopSfx(AudioSource source, float fadeOut = 0.3f)
        {
            if (source == null || !loopSfxAudioSource.Contains(source)) return;
            DOVirtual.Float(source.volume, 0, fadeOut, x => source.volume = x)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    source.loop = false;
                    source.Stop();
                    loopSfxAudioSource.Remove(source);
                    PushBack(source);
                });
        }

        public void PlayRandomSfx(List<SFX_TYPE> sfxTypes)
        {
            if (sfxTypes == null || sfxTypes.Count == 0) return;
            PlaySfx(sfxTypes[UnityEngine.Random.Range(0, sfxTypes.Count)]);
        }

        public void PauseBgm()
        {
            bgm.Pause();
        }

        public void UnPauseBgm()
        {
            bgm.Play();
        }

        public void StopBgm()
        {
            bgm.Stop();
        }

        public void StopSfx(SFX_TYPE type = SFX_TYPE.NONE)
        {
            if (type == SFX_TYPE.NONE)
            {
                sfx.Stop();
                return;
            }
            if (sfx.currentAudio != GetSfxAudio(type)) return;
            sfx.Stop();
        }

        public bool IsBgmMute()
        {
            return bgm.IsMute;
        }

        public bool IsSfxMute()
        {
            return sfx.IsMute;
        }

        public void ToggleBgmVolume(bool isMute)
        {
            bgm.BaseVolume = isMute ? 0 : 1;
        }

        public void ToggleSfxVolume(bool isMute)
        {
            sfx.BaseVolume = isMute ? 0 : 1;
        }

        // SFX channel: keep the current source while it is free; otherwise return it to the pool and take another.
        // When every pooled source is busy, the current one is reused (restarted) instead of throwing.
        private AudioSource RentFreeSfxSource(AudioSource current)
        {
            if (current != null && !IsSourceBusy(current)) return current;
            if (current != null) PushBack(current);
            return TryPop() ?? current;
        }

        private AudioSource TryPop()
        {
            if (poppedSources.Count >= poolSize) return null;
            AudioSource source = audioSourcePool.Pop(0).AudioSource;
            poppedSources.Add(source);
            return source;
        }

        private void PushBack(AudioSource source)
        {
            if (poppedSources.Remove(source)) audioSourcePool.Push(0, source.transform);
        }
    }
}
