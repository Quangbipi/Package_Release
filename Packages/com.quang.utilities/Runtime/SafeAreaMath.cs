using UnityEngine;

namespace Quang.Utilities
{
    /// <summary>
    /// Single source of truth for the device Safe Area (notch, home indicator) in normalised 0..1 screen
    /// coordinates. UI (<c>Quang.UI.SafeAreaFitter</c>) and world layout code should both read it here so
    /// a HUD and the world it frames can never disagree about where the screen ends.
    /// </summary>
    public static class SafeAreaMath
    {
        private static readonly Rect FullScreen = new(0f, 0f, 1f, 1f);

        /// <summary>
        /// Converts a safe area in screen pixels to 0..1. Returns the full screen for a zero-size screen,
        /// an empty safe area, or a safe area larger than the screen (the Editor Game view can report the
        /// simulated device's safe area for a smaller rendered view).
        /// </summary>
        public static Rect Normalize(Rect safeArea, Vector2 screenSize)
        {
            if (screenSize.x <= 0f || screenSize.y <= 0f) return FullScreen;
            if (safeArea.width <= 0f || safeArea.height <= 0f) return FullScreen;
            if (safeArea.xMax > screenSize.x + 1f || safeArea.yMax > screenSize.y + 1f) return FullScreen;

            return Rect.MinMaxRect(
                Mathf.Clamp01(safeArea.xMin / screenSize.x), Mathf.Clamp01(safeArea.yMin / screenSize.y),
                Mathf.Clamp01(safeArea.xMax / screenSize.x), Mathf.Clamp01(safeArea.yMax / screenSize.y));
        }

        /// <summary>The current device safe area, normalised.</summary>
        public static Rect Current() => Normalize(Screen.safeArea, new Vector2(Screen.width, Screen.height));
    }
}
