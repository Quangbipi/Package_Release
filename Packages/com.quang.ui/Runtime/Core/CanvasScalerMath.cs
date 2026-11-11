using UnityEngine;

namespace Quang.UI
{
    /// <summary>
    /// Canvas size a <c>CanvasScaler</c> in Scale With Screen Size / Match Width Or Height mode produces for a
    /// screen. Same formula as Unity (logarithmic blend of the width and height ratios), so layouts can be
    /// reasoned about and previewed without entering Play mode.
    /// </summary>
    public static class CanvasScalerMath
    {
        /// <summary>Screen pixels per canvas unit. Returns 1 when the reference size is not positive.</summary>
        public static float ScaleFactor(Vector2 screen, Vector2 reference, float match)
        {
            if (reference.x <= 0f || reference.y <= 0f || screen.x <= 0f || screen.y <= 0f) return 1f;
            float logWidth = Mathf.Log(screen.x / reference.x, 2f);
            float logHeight = Mathf.Log(screen.y / reference.y, 2f);
            return Mathf.Pow(2f, Mathf.Lerp(logWidth, logHeight, Mathf.Clamp01(match)));
        }

        /// <summary>Canvas size in canvas units for <paramref name="screen"/> pixels.</summary>
        public static Vector2 CanvasSize(Vector2 screen, Vector2 reference, float match) =>
            screen / ScaleFactor(screen, reference, match);
    }
}
