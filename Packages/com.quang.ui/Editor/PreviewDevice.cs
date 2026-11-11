using System;

namespace Quang.UI.Editor
{
    /// <summary>A screen to preview a UI prefab on: pixel size plus safe-area insets as fractions of the height.</summary>
    [Serializable]
    public struct PreviewDevice
    {
        public string Name;
        public int Width;
        public int Height;
        public float SafeTop;
        public float SafeBottom;

        public PreviewDevice(string name, int width, int height, float safeTop = 0f, float safeBottom = 0f)
        {
            Name = name;
            Width = width;
            Height = height;
            SafeTop = safeTop;
            SafeBottom = safeBottom;
        }

        /// <summary>
        /// Standard matrix: reference iPhone, the shortest phone (16:9), a tall Android (20:9),
        /// a notched iPhone (top 59pt / bottom 34pt of 852pt) and an iPad (3:4, the height constraint).
        /// </summary>
        public static readonly PreviewDevice[] Defaults =
        {
            new("phone-ref", 1170, 2532),
            new("phone-16x9", 1080, 1920),
            new("phone-20x9", 1080, 2400),
            new("phone-notch", 1179, 2556, 59f / 852f, 34f / 852f),
            new("tablet-4x3", 1536, 2048),
        };

        public override string ToString() => $"{Name} {Width}x{Height}";
    }
}
