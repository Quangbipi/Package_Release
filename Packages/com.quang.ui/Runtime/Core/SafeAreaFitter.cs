using Quang.Utilities;
using UnityEngine;

namespace Quang.UI
{
    /// <summary>
    /// Fits a RectTransform to Screen.safeArea (notch, home indicator) by driving its anchors.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        RectTransform rectTransform;
        Rect lastSafeArea;
        Vector2Int lastScreenSize;

        void OnEnable()
        {
            Apply();
        }

        void Update()
        {
            if (Screen.safeArea != lastSafeArea || Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
            {
                Apply();
            }
        }

        void Apply()
        {
            if (rectTransform == null)
            {
                rectTransform = (RectTransform)transform;
            }

            lastSafeArea = Screen.safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            ComputeAnchors(lastSafeArea, lastScreenSize, out Vector2 min, out Vector2 max);
            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Converts a safe area in screen pixels to normalized anchors via <see cref="SafeAreaMath.Normalize"/>.
        /// Returns full-screen anchors for a zero-size screen, an empty safe area, or a safe area larger than
        /// the screen (Editor Game view reporting a simulated device's safe area).
        /// </summary>
        public static void ComputeAnchors(Rect safeArea, Vector2 screenSize, out Vector2 min, out Vector2 max)
        {
            Rect normalized = SafeAreaMath.Normalize(safeArea, screenSize);
            min = normalized.min;
            max = normalized.max;
        }
    }
}
