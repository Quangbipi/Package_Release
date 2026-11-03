using System.Collections.Generic;
using DesignPattern;
using NUnit.Framework;
using Quang.Base;
using UnityEngine;

namespace Quang.Audio.Tests
{
    public sealed class AudioManagerTests
    {
        private readonly List<Object> created = new();
        private AudioClip clip;
        private AudioData data;

        [SetUp]
        public void SetUp()
        {
            Locator.Audio = null;
            Locator.ResetDataForTests();
            clip = AudioClip.Create("test", 441, 1, 44100, false);
            data = ScriptableObject.CreateInstance<AudioData>();
            data.SetEntries(
                new Dictionary<BGM_TYPE, Audio> { { BGM_TYPE.BACK_GROUND_1, new Audio { clip = clip, multiplier = 0.5f } } },
                new Dictionary<SFX_TYPE, Audio> { { SFX_TYPE.BLOCK_PLACE, new Audio { clip = clip, multiplier = 1f } } });
            created.Add(clip);
            created.Add(data);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in created)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }
            created.Clear();
            Locator.Audio = null;
        }

        private AudioManager CreateManager(int poolSize = 3)
        {
            var root = new GameObject("AudioManager");
            created.Add(root);
            var bgmGo = new GameObject("BGM");
            bgmGo.transform.SetParent(root.transform);
            var sfxGo = new GameObject("SFX");
            sfxGo.transform.SetParent(root.transform);

            var pool = root.AddComponent<ObjectContainer>();
            pool.MainObjects = new[] { sfxGo };
            pool.Amounts = new[] { poolSize };

            var manager = root.AddComponent<AudioManager>();
            manager.AudioData = data;
            manager.AudioSourcePool = pool;
            manager.SetSources(bgmGo.AddComponent<AudioSource>(), sfxGo.AddComponent<AudioSource>());
            return manager;
        }

        [Test]
        public void Init_RegistersLocatorAudio()
        {
            AudioManager manager = CreateManager();

            Assert.IsTrue(manager.Init());
            Assert.AreSame(manager, Locator.Audio);
        }

        [Test]
        public void Init_WhenAnotherRegistered_DestroysSelf()
        {
            AudioManager first = CreateManager();
            first.Init();
            AudioManager second = CreateManager();

            Assert.IsFalse(second.Init());
            Assert.AreSame(first, Locator.Audio);
            Assert.IsTrue(second == null);
        }

        [Test]
        public void Init_WithoutDataService_IsUnmuted()
        {
            AudioManager manager = CreateManager();

            manager.Init();

            Assert.AreEqual(1f, manager.BgmVolume);
            Assert.AreEqual(1f, manager.SfxVolume);
        }

        [Test]
        public void ToggleSfxVolume_True_SetsVolumeZero()
        {
            AudioManager manager = CreateManager();
            manager.Init();
            manager.PlaySfx(SFX_TYPE.BLOCK_PLACE);

            manager.ToggleSfxVolume(true);

            Assert.AreEqual(0f, manager.Sfx.Source.volume);
            Assert.IsTrue(manager.IsSfxMute());
        }

        [Test]
        public void PlaySfx_WhenSfxBusy_UsesAnotherPooledSource()
        {
            AudioManager manager = CreateManager();
            manager.Init();
            manager.IsSourceBusy = _ => true;
            AudioSource before = manager.Sfx.Source;

            manager.PlaySfx(SFX_TYPE.BLOCK_PLACE);

            Assert.AreNotSame(before, manager.Sfx.Source);
            Assert.AreSame(clip, manager.Sfx.Source.clip);
        }

        [Test]
        public void PlaySfx_WhenPoolExhausted_DoesNotThrow()
        {
            AudioManager manager = CreateManager();
            manager.Init();
            manager.IsSourceBusy = _ => true;
            Assert.IsNotNull(manager.PlayLoopSfx(SFX_TYPE.BLOCK_PLACE, 0f));
            Assert.IsNotNull(manager.PlayLoopSfx(SFX_TYPE.BLOCK_PLACE, 0f));

            Assert.IsNull(manager.PlayLoopSfx(SFX_TYPE.BLOCK_PLACE, 0f));
            Assert.DoesNotThrow(() => manager.PlaySfx(SFX_TYPE.BLOCK_PLACE));
            Assert.IsNotNull(manager.Sfx.Source);
        }

        [Test]
        public void PlayBgm_WhenNothingPlaying_SwitchesImmediately()
        {
            AudioManager manager = CreateManager();
            manager.Init();

            manager.PlayBgm(BGM_TYPE.BACK_GROUND_1);

            Assert.AreSame(clip, manager.Bgm.Source.clip);
            Assert.AreEqual(0.5f, manager.Bgm.Source.volume, 1e-4f);
            Assert.IsTrue(manager.Bgm.Source.loop);
        }

        [Test]
        public void PlaySfx_WhenTypeMissing_DoesNothing()
        {
            AudioManager manager = CreateManager();
            manager.Init();
            AudioSource before = manager.Sfx.Source;

            Assert.DoesNotThrow(() => manager.PlaySfx(SFX_TYPE.LINE_CLEAR));
            Assert.AreSame(before, manager.Sfx.Source);
            Assert.IsNull(before.clip);
        }
    }
}
