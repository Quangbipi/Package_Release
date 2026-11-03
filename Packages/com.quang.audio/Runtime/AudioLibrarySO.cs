using System;
using System.Collections.Generic;
using Quang.Base;
using UnityEngine;

namespace Quang.Audio
{
    /// <summary>
    /// Maps <see cref="SFX_TYPE"/> and <see cref="BGM_TYPE"/> to clips and playback settings.
    /// A type listed twice resolves to its first entry.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "Quang/Audio/Audio Library")]
    public class AudioLibrarySO : ScriptableObject
    {
        [Serializable]
        public class SfxEntry
        {
            public SFX_TYPE type;
            [Tooltip("One clip is picked at random each play.")]
            public AudioClip[] clips;
            [Range(0f, 1f)] public float volume = 1f;
            [Tooltip("Random pitch offset in [-pitchJitter, +pitchJitter].")]
            [Range(0f, 0.5f)] public float pitchJitter;
            [Tooltip("Minimum seconds between two plays of this type.")]
            [Min(0f)] public float cooldown = 0.03f;

            public AudioClip PickClip()
            {
                if (clips == null || clips.Length == 0) return null;
                return clips.Length == 1 ? clips[0] : clips[UnityEngine.Random.Range(0, clips.Length)];
            }
        }

        [Serializable]
        public class BgmEntry
        {
            public BGM_TYPE type;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 0.6f;
        }

        [SerializeField] private List<SfxEntry> sfx = new();
        [SerializeField] private List<BgmEntry> bgm = new();

        private Dictionary<SFX_TYPE, SfxEntry> sfxByType;
        private Dictionary<BGM_TYPE, BgmEntry> bgmByType;

        public bool TryGetSfx(SFX_TYPE type, out SfxEntry entry)
        {
            EnsureCache();
            return sfxByType.TryGetValue(type, out entry);
        }

        public bool TryGetBgm(BGM_TYPE type, out BgmEntry entry)
        {
            EnsureCache();
            return bgmByType.TryGetValue(type, out entry);
        }

        internal void SetEntries(List<SfxEntry> sfxEntries, List<BgmEntry> bgmEntries)
        {
            sfx = sfxEntries ?? new List<SfxEntry>();
            bgm = bgmEntries ?? new List<BgmEntry>();
            ClearCache();
        }

        private void OnValidate() => ClearCache();

        private void ClearCache()
        {
            sfxByType = null;
            bgmByType = null;
        }

        private void EnsureCache()
        {
            if (sfxByType != null && bgmByType != null) return;

            sfxByType = new Dictionary<SFX_TYPE, SfxEntry>();
            foreach (SfxEntry entry in sfx)
            {
                if (entry == null) continue;
                if (!sfxByType.TryAdd(entry.type, entry))
                {
                    Debug.LogWarning($"[AudioLibrary] {name}: duplicate SFX entry {entry.type}, using the first one.", this);
                }
            }

            bgmByType = new Dictionary<BGM_TYPE, BgmEntry>();
            foreach (BgmEntry entry in bgm)
            {
                if (entry == null) continue;
                if (!bgmByType.TryAdd(entry.type, entry))
                {
                    Debug.LogWarning($"[AudioLibrary] {name}: duplicate BGM entry {entry.type}, using the first one.", this);
                }
            }
        }
    }
}
