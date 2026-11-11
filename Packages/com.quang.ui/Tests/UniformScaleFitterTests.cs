using NUnit.Framework;
using UnityEngine;

namespace Quang.UI.Tests
{
    public class UniformScaleFitterTests
    {
        private static readonly Vector2 Design = new(390f, 406f);

        [Test]
        public void Compute_WhenParentLargerThanDesign_ReturnsMaxScale()
        {
            Assert.AreEqual(1f, UniformScaleFitter.Compute(new Vector2(390f, 533f), Design, 1f), 1e-4f);
        }

        [Test]
        public void Compute_WhenParentShorter_ScalesByHeight()
        {
            Assert.AreEqual(209f / 406f, UniformScaleFitter.Compute(new Vector2(390f, 209f), Design, 1f), 1e-4f);
        }

        [Test]
        public void Compute_WhenParentNarrower_ScalesByWidth()
        {
            Assert.AreEqual(300f / 390f, UniformScaleFitter.Compute(new Vector2(300f, 600f), Design, 1f), 1e-4f);
        }

        [Test]
        public void Compute_WhenDesignSizeZero_ReturnsMaxScale()
        {
            Assert.AreEqual(1f, UniformScaleFitter.Compute(new Vector2(390f, 500f), Vector2.zero, 1f), 1e-4f);
        }

        [Test]
        public void Compute_WhenParentSizeNegative_ReturnsZero()
        {
            Assert.AreEqual(0f, UniformScaleFitter.Compute(new Vector2(390f, -20f), Design, 1f), 1e-4f);
        }
    }
}
