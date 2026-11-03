using NUnit.Framework;
using UnityEngine;

namespace Quang.Audio.Tests
{
    public sealed class AudioSourceDataTests
    {
        private GameObject go;
        private AudioSourceData data;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("Source");
            data = new AudioSourceData { Source = go.AddComponent<AudioSource>() };
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(go);

        [Test]
        public void Volume_IsBaseVolumeTimesMultiplier()
        {
            data.BaseVolume = 0.5f;

            data.SetAudio(new Audio { multiplier = 0.4f });

            Assert.AreEqual(0.2f, data.Source.volume, 1e-4f);
        }

        [Test]
        public void IsMute_WhenBaseVolumeZero_ReturnsTrue()
        {
            data.SetAudio(new Audio { multiplier = 1f });

            data.BaseVolume = 0f;

            Assert.IsTrue(data.IsMute);
        }
    }
}
