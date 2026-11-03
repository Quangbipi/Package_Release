using System.Collections.Generic;
using NUnit.Framework;
using Quang.Base;
using UnityEngine;
using UnityEngine.TestTools;

namespace Quang.Audio.Tests
{
    public sealed class AudioLibrarySOTests
    {
        private AudioLibrarySO library;

        [SetUp]
        public void SetUp() => library = ScriptableObject.CreateInstance<AudioLibrarySO>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(library);

        [Test]
        public void TryGetSfx_WhenTypeMapped_ReturnsEntry()
        {
            var entry = new AudioLibrarySO.SfxEntry { type = SFX_TYPE.BLOCK_PLACE };
            library.SetEntries(new List<AudioLibrarySO.SfxEntry> { entry }, null);

            Assert.IsTrue(library.TryGetSfx(SFX_TYPE.BLOCK_PLACE, out AudioLibrarySO.SfxEntry found));
            Assert.AreSame(entry, found);
        }

        [Test]
        public void TryGetSfx_WhenTypeMissing_ReturnsFalse()
        {
            library.SetEntries(new List<AudioLibrarySO.SfxEntry>(), null);

            Assert.IsFalse(library.TryGetSfx(SFX_TYPE.LINE_CLEAR, out _));
        }

        [Test]
        public void TryGetSfx_WhenDuplicateType_ReturnsFirst()
        {
            var first = new AudioLibrarySO.SfxEntry { type = SFX_TYPE.LOSE };
            var second = new AudioLibrarySO.SfxEntry { type = SFX_TYPE.LOSE };
            library.SetEntries(new List<AudioLibrarySO.SfxEntry> { first, second }, null);

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("duplicate SFX entry LOSE"));
            Assert.IsTrue(library.TryGetSfx(SFX_TYPE.LOSE, out AudioLibrarySO.SfxEntry found));
            Assert.AreSame(first, found);
        }

        [Test]
        public void TryGetBgm_WhenTypeMapped_ReturnsEntry()
        {
            var entry = new AudioLibrarySO.BgmEntry { type = BGM_TYPE.BACK_GROUND_1 };
            library.SetEntries(null, new List<AudioLibrarySO.BgmEntry> { entry });

            Assert.IsTrue(library.TryGetBgm(BGM_TYPE.BACK_GROUND_1, out AudioLibrarySO.BgmEntry found));
            Assert.AreSame(entry, found);
        }
    }
}
