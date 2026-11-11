using NUnit.Framework;
using UnityEngine;

namespace Quang.Utilities.Tests
{
    public class SafeAreaMathTests
    {
        private const float Tolerance = 0.0001f;

        private static void AssertRect(Rect expected, Rect actual)
        {
            Assert.AreEqual(expected.xMin, actual.xMin, Tolerance, "xMin");
            Assert.AreEqual(expected.yMin, actual.yMin, Tolerance, "yMin");
            Assert.AreEqual(expected.xMax, actual.xMax, Tolerance, "xMax");
            Assert.AreEqual(expected.yMax, actual.yMax, Tolerance, "yMax");
        }

        [Test]
        public void Normalize_FullScreen_ReturnsUnitRect()
        {
            AssertRect(new Rect(0f, 0f, 1f, 1f), SafeAreaMath.Normalize(new Rect(0f, 0f, 1080f, 1920f), new Vector2(1080f, 1920f)));
        }

        [Test]
        public void Normalize_NotchInsets_ReturnsFractions()
        {
            Rect safe = Rect.MinMaxRect(0f, 102f, 1179f, 2556f - 177f);
            Rect expected = Rect.MinMaxRect(0f, 102f / 2556f, 1f, (2556f - 177f) / 2556f);
            AssertRect(expected, SafeAreaMath.Normalize(safe, new Vector2(1179f, 2556f)));
        }

        [Test]
        public void Normalize_ZeroScreen_ReturnsUnitRect()
        {
            AssertRect(new Rect(0f, 0f, 1f, 1f), SafeAreaMath.Normalize(new Rect(0f, 0f, 100f, 100f), Vector2.zero));
        }

        [Test]
        public void Normalize_EmptySafeArea_ReturnsUnitRect()
        {
            AssertRect(new Rect(0f, 0f, 1f, 1f), SafeAreaMath.Normalize(new Rect(0f, 0f, 0f, 0f), new Vector2(1080f, 1920f)));
        }

        [Test]
        public void Normalize_SafeAreaLargerThanScreen_ReturnsUnitRect()
        {
            // Editor Game view can report a device-sized safe area for a smaller rendered view.
            AssertRect(new Rect(0f, 0f, 1f, 1f), SafeAreaMath.Normalize(new Rect(0f, 102f, 1170f, 2289f), new Vector2(904f, 589f)));
        }

        [Test]
        public void Normalize_NegativeOrigin_ClampsToZero()
        {
            Rect result = SafeAreaMath.Normalize(Rect.MinMaxRect(-10f, -10f, 1080f, 1900f), new Vector2(1080f, 1920f));
            Assert.AreEqual(0f, result.xMin, Tolerance);
            Assert.AreEqual(0f, result.yMin, Tolerance);
            Assert.AreEqual(1900f / 1920f, result.yMax, Tolerance);
        }
    }
}
