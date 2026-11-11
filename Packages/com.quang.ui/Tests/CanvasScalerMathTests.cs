using NUnit.Framework;
using UnityEngine;

namespace Quang.UI.Tests
{
    public class CanvasScalerMathTests
    {
        private const float Tolerance = 0.5f;
        private static readonly Vector2 Ref390 = new(390f, 844f);
        private static readonly Vector2 Ref1080 = new(1080f, 1920f);

        [Test]
        public void CanvasSize_MatchWidth_KeepsReferenceWidth()
        {
            Vector2 size = CanvasScalerMath.CanvasSize(new Vector2(1536f, 2048f), Ref390, 0f);
            Assert.AreEqual(390f, size.x, Tolerance);
            Assert.AreEqual(520f, size.y, Tolerance);
        }

        [Test]
        public void CanvasSize_MatchWidth_ReferenceScreen_EqualsReference()
        {
            Vector2 size = CanvasScalerMath.CanvasSize(new Vector2(1170f, 2532f), Ref390, 0f);
            Assert.AreEqual(390f, size.x, Tolerance);
            Assert.AreEqual(844f, size.y, Tolerance);
        }

        [Test]
        public void CanvasSize_MatchHeight_KeepsReferenceHeight()
        {
            Vector2 size = CanvasScalerMath.CanvasSize(new Vector2(1536f, 2048f), Ref1080, 1f);
            Assert.AreEqual(1440f, size.x, Tolerance);
            Assert.AreEqual(1920f, size.y, Tolerance);
        }

        [Test]
        public void CanvasSize_MatchHalf_TallPhone_IsNarrowerAndTaller()
        {
            // scale = 2^((log2(1170/1080) + log2(2532/1920)) / 2) = sqrt(1.0833 * 1.31875) ≈ 1.19524
            Vector2 size = CanvasScalerMath.CanvasSize(new Vector2(1170f, 2532f), Ref1080, 0.5f);
            Assert.AreEqual(1170f / 1.19524f, size.x, Tolerance);
            Assert.AreEqual(2532f / 1.19524f, size.y, Tolerance);
        }

        [Test]
        public void CanvasSize_ZeroReference_ReturnsScreen()
        {
            Vector2 size = CanvasScalerMath.CanvasSize(new Vector2(1080f, 1920f), Vector2.zero, 0.5f);
            Assert.AreEqual(1080f, size.x, Tolerance);
            Assert.AreEqual(1920f, size.y, Tolerance);
        }

        [Test]
        public void ScaleFactor_MatchWidth_IsScreenOverReferenceWidth()
        {
            Assert.AreEqual(3f, CanvasScalerMath.ScaleFactor(new Vector2(1170f, 2532f), Ref390, 0f), 0.0001f);
        }
    }
}
