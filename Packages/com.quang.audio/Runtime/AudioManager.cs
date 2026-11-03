using System;
using System.Collections;
using System.Collections.Generic;
using Quang.Base;
using UnityEngine;

namespace Quang.Audio
{
    /// <summary>
    /// Default <see cref="IAudioService"/>: registers itself as <see cref="Locator.Audio"/>, plays pooled SFX,
    /// crossfades BGM between two sources and applies mute. It does not persist mute state; the caller does.
    /// Fades use unscaled time so they keep running while the game is paused.
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour, IAudioService
    {
        [SerializeField] private AudioLibrarySO library;
        [SerializeField, Min(1)] private int sfxPoolSize = 8;
        [SerializeField] private bool dontDestroyOnLoad = true;

        private readonly AudioSource[] bgmSources = new AudioSource[2];
        private readonly Dictionary<AudioSource, SFX_TYPE> sfxTypeBySource = new();
        private readonly Dictionary<SFX_TYPE, float> lastPlayTime = new();
        private readonly List<AudioSource> loopSources = new();
        private readonly Dictionary<AudioSource, Coroutine> fades = new();
        private SfxSourcePool<AudioSource> sfxPool;
        private int activeBgm;
        private BGM_TYPE currentBgm = BGM_TYPE.NONE;
        private bool isBgmMute;
        private bool isSfxMute;
        private bool initialized;
#if UNITY_EDITOR
        private readonly HashSet<SFX_TYPE> warnedMissing = new();
#endif

        internal Func<float> Clock = () => Time.unscaledTime;
        internal event Action<SFX_TYPE> SfxPlayed;

        public AudioLibrarySO Library
        {
            get => library;
            set => library = value;
        }
        public bool IsBgmMute => isBgmMute;
        public bool IsSfxMute => isSfxMute;
        public BGM_TYPE CurrentBgm => currentBgm;

        protected virtual void Awake()
        {
            if (!Initialize()) return;
            if (dontDestroyOnLoad && Application.isPlaying)
            {
                if (transform.parent == null) DontDestroyOnLoad(gameObject);
                else Debug.LogWarning("[AudioManager] dontDestroyOnLoad needs a root GameObject; skipped.", this);
            }
        }

        protected virtual void OnDestroy() => Shutdown();

        /// <summary>Registers with <see cref="Locator"/> and builds the sources. Returns false (and destroys this object) when another manager is registered.</summary>
        internal bool Initialize()
        {
            if (initialized) return true;

            if (Locator.Audio != null && !ReferenceEquals(Locator.Audio, this))
            {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
                return false;
            }

            Locator.Audio = this;

            for (int i = 0; i < bgmSources.Length; i++)
            {
                bgmSources[i] = CreateSource(true);
            }

            var pooled = new List<AudioSource>(sfxPoolSize);
            for (int i = 0; i < Mathf.Max(1, sfxPoolSize); i++)
            {
                pooled.Add(CreateSource(false));
            }
            sfxPool = new SfxSourcePool<AudioSource>(pooled, source => source.isPlaying);

            initialized = true;
            return true;
        }

        internal void Shutdown()
        {
            if (ReferenceEquals(Locator.Audio, this)) Locator.Audio = null;
        }

        #region SFX

        public void PlaySfx(SFX_TYPE type)
        {
            if (!initialized || isSfxMute || type == SFX_TYPE.NONE) return;
            if (!TryGetSfx(type, out AudioLibrarySO.SfxEntry entry)) return;

            AudioClip clip = entry.PickClip();
            if (clip == null) return;

            float now = Clock();
            if (lastPlayTime.TryGetValue(type, out float last) && now - last < entry.cooldown) return;
            lastPlayTime[type] = now;

            AudioSource source = sfxPool.Rent();
            source.Stop();
            source.clip = clip;
            source.volume = entry.volume;
            source.pitch = 1f + UnityEngine.Random.Range(-entry.pitchJitter, entry.pitchJitter);
            source.Play();
            sfxTypeBySource[source] = type;
            SfxPlayed?.Invoke(type);
        }

        public void PlayRandomSfx(List<SFX_TYPE> sfxTypes)
        {
            if (sfxTypes == null || sfxTypes.Count == 0) return;
            PlaySfx(sfxTypes[UnityEngine.Random.Range(0, sfxTypes.Count)]);
        }

        public AudioSource PlayLoopSfx(SFX_TYPE type, float fadeIn = 0.1f)
        {
            if (!initialized || !TryGetSfx(type, out AudioLibrarySO.SfxEntry entry)) return null;

            AudioClip clip = entry.PickClip();
            if (clip == null) return null;

            AudioSource source = loopSources.Find(s => !s.isPlaying && !fades.ContainsKey(s));
            if (source == null)
            {
                source = CreateSource(true);
                loopSources.Add(source);
            }

            source.clip = clip;
            source.mute = isSfxMute;
            source.volume = 0f;
            source.Play();
            Fade(source, entry.volume, fadeIn, null);
            return source;
        }

        public void StopLoopSfx(AudioSource source, float fadeOut = 0.1f)
        {
            if (source == null || !loopSources.Contains(source)) return;
            Fade(source, 0f, fadeOut, source.Stop);
        }

        public void StopSfx(SFX_TYPE type = SFX_TYPE.NONE)
        {
            if (!initialized) return;

            foreach (AudioSource source in sfxPool.Items)
            {
                if (type == SFX_TYPE.NONE || (sfxTypeBySource.TryGetValue(source, out SFX_TYPE playing) && playing == type))
                {
                    source.Stop();
                }
            }

            if (type != SFX_TYPE.NONE) return;
            foreach (AudioSource source in loopSources)
            {
                CancelFade(source);
                source.Stop();
            }
        }

        public void ToggleSfxVolume(bool isMute)
        {
            isSfxMute = isMute;
            if (!initialized) return;
            foreach (AudioSource source in sfxPool.Items)
            {
                source.mute = isMute;
            }
            foreach (AudioSource source in loopSources)
            {
                source.mute = isMute;
            }
        }

        #endregion

        #region BGM

        public void PlayBgm(BGM_TYPE type, float fadeOut = 0.3f)
        {
            if (!initialized) return;
            if (type == BGM_TYPE.NONE)
            {
                StopBgm();
                return;
            }
            if (library == null || !library.TryGetBgm(type, out AudioLibrarySO.BgmEntry entry) || entry.clip == null)
            {
                Debug.LogWarning($"[AudioManager] No BGM clip for {type}.", this);
                return;
            }
            if (currentBgm == type && bgmSources[activeBgm].isPlaying) return;

            AudioSource previous = bgmSources[activeBgm];
            activeBgm = 1 - activeBgm;
            AudioSource next = bgmSources[activeBgm];

            next.clip = entry.clip;
            next.volume = 0f;
            next.Play();
            Fade(next, entry.volume, fadeOut, null);
            if (previous.isPlaying) Fade(previous, 0f, fadeOut, previous.Stop);
            currentBgm = type;
        }

        public void PauseBgm()
        {
            if (initialized) bgmSources[activeBgm].Pause();
        }

        public void UnPauseBgm()
        {
            if (initialized) bgmSources[activeBgm].UnPause();
        }

        public void StopBgm()
        {
            if (!initialized) return;
            foreach (AudioSource source in bgmSources)
            {
                CancelFade(source);
                source.Stop();
            }
            currentBgm = BGM_TYPE.NONE;
        }

        public void ToggleBgmVolume(bool isMute)
        {
            isBgmMute = isMute;
            if (!initialized) return;
            foreach (AudioSource source in bgmSources)
            {
                source.mute = isMute;
            }
        }

        #endregion

        private bool TryGetSfx(SFX_TYPE type, out AudioLibrarySO.SfxEntry entry)
        {
            if (library != null && library.TryGetSfx(type, out entry)) return true;
            entry = null;
#if UNITY_EDITOR
            if (warnedMissing.Add(type))
            {
                Debug.LogWarning($"[AudioManager] No SFX entry for {type}.", this);
            }
#endif
            return false;
        }

        private AudioSource CreateSource(bool loop)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.mute = loop ? isBgmMute : isSfxMute;
            return source;
        }

        private void Fade(AudioSource source, float target, float duration, Action onDone)
        {
            CancelFade(source);
            if (duration <= 0f || !isActiveAndEnabled || !Application.isPlaying)
            {
                source.volume = target;
                onDone?.Invoke();
                return;
            }
            fades[source] = StartCoroutine(FadeRoutine(source, target, duration, onDone));
        }

        private void CancelFade(AudioSource source)
        {
            if (!fades.TryGetValue(source, out Coroutine routine)) return;
            if (routine != null) StopCoroutine(routine);
            fades.Remove(source);
        }

        private IEnumerator FadeRoutine(AudioSource source, float target, float duration, Action onDone)
        {
            float start = source.volume;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            source.volume = target;
            fades.Remove(source);
            onDone?.Invoke();
        }
    }
}
