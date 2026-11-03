using System.Collections.Generic;
using NUnit.Framework;
using Quang.Base;
using UnityEngine;
using UnityEngine.TestTools;

namespace Quang.Audio.Tests
{
    public sealed class AudioManagerTests
    {
        private readonly List<Object> created = new();
        private AudioClip clip;
        private AudioLibrarySO library;

        [SetUp]
        public void SetUp()
        {
            Locator.Audio = null;
            clip = AudioClip.Create("test", 441, 1, 44100, false);
            library = ScriptableObject.CreateInstance<AudioLibrarySO>();
            library.SetEntries(
                new List<AudioLibrarySO.SfxEntry>
                {
                    new() { type = SFX_TYPE.BLOCK_PLACE, clips = new[] { clip }, cooldown = 0.1f },
                },
                null);
            created.Add(clip);
            created.Add(library);
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

        private AudioManager CreateManager()
        {
            var go = new GameObject("AudioManager");
            created.Add(go);
            AudioManager manager = go.AddComponent<AudioManager>();
            manager.Library = library;
            return manager;
        }

        [Test]
        public void Initialize_RegistersLocatorAudio()
        {
            AudioManager manager = CreateManager();

            Assert.IsTrue(manager.Initialize());
            Assert.AreSame(manager, Locator.Audio);
        }

        [Test]
        public void Shutdown_WhenStillRegistered_ClearsLocator()
        {
            AudioManager manager = CreateManager();
            manager.Initialize();

            manager.Shutdown();

            Assert.IsNull(Locator.Audio);
        }

        [Test]
        public void Initialize_WhenAnotherRegistered_KeepsFirst()
        {
            AudioManager first = CreateManager();
            first.Initialize();
            AudioManager second = CreateManager();

            Assert.IsFalse(second.Initialize());
            Assert.AreSame(first, Locator.Audio);
            Assert.IsTrue(second == null);
        }

        [Test]
        public void PlaySfx_WhenMuted_DoesNotPlay()
        {
            AudioManager manager = CreateManager();
            manager.Initialize();
            int plays = 0;
            manager.SfxPlayed += _ => plays++;

            manager.ToggleSfxVolume(true);
            manager.PlaySfx(SFX_TYPE.BLOCK_PLACE);

            Assert.AreEqual(0, plays);
        }

        [Test]
        public void PlaySfx_WithinCooldown_PlaysOnce()
        {
            AudioManager manager = CreateManager();
            manager.Initialize();
            float now = 10f;
            manager.Clock = () => now;
            int plays = 0;
            manager.SfxPlayed += _ => plays++;

            manager.PlaySfx(SFX_TYPE.BLOCK_PLACE);
            manager.PlaySfx(SFX_TYPE.BLOCK_PLACE);
            now += 0.2f;
            manager.PlaySfx(SFX_TYPE.BLOCK_PLACE);

            Assert.AreEqual(2, plays);
        }

        [Test]
        public void PlaySfx_WhenTypeMissing_DoesNotThrow()
        {
            AudioManager manager = CreateManager();
            manager.Initialize();

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("No SFX entry for LINE_CLEAR"));
            Assert.DoesNotThrow(() => manager.PlaySfx(SFX_TYPE.LINE_CLEAR));
        }
    }
}
