using System;
using System.Collections.Generic;
using Quang.Base;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quang.Audio
{
    [CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/System/AudioData", order = 1)]
    public class AudioData : SerializedScriptableObject
    {
        [Title("BGM")]
        [SerializeField] private Dictionary<BGM_TYPE, Audio> _bgmAudioDict = new();

        [Title("SFX")]
        [SerializeField] private Dictionary<SFX_TYPE, Audio> _sfxAudioDict = new();

        public IReadOnlyDictionary<BGM_TYPE, Audio> BgmAudioDict => _bgmAudioDict;

        public IReadOnlyDictionary<SFX_TYPE, Audio> SfxAudioDict => _sfxAudioDict;

        internal void SetEntries(Dictionary<BGM_TYPE, Audio> bgm, Dictionary<SFX_TYPE, Audio> sfx)
        {
            _bgmAudioDict = bgm ?? new Dictionary<BGM_TYPE, Audio>();
            _sfxAudioDict = sfx ?? new Dictionary<SFX_TYPE, Audio>();
        }
    }

    [Serializable]
    public class Audio
    {
        public AudioClip clip;
        [Range(0, 1)]
        public float multiplier = 1;
    }
}
