using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Quang.UI.Editor
{
    /// <summary>
    /// Measures a mockup image: finds horizontal bands whose pixels differ from the background and reports
    /// their vertical and horizontal extents in canvas units. Use it to turn a mockup into numbers for a
    /// design doc instead of eyeballing.
    /// </summary>
    public static class MockupProbe
    {
        /// <summary>
        /// Returns one line per content band: <c>y top-bottom  x left-right</c> in canvas units, measured from the
        /// top-left corner, plus the sampled background color.
        /// </summary>
        /// <param name="pngPath">Mockup file, absolute or project-relative.</param>
        /// <param name="unitsPerPixel">Canvas units per mockup pixel, e.g. 1/3 for a 1170 px mockup on a 390-wide canvas.</param>
        /// <param name="backgroundSample">Pixel (from top-left) whose color is treated as background.</param>
        /// <param name="threshold">Sum of absolute RGB differences above which a pixel counts as content.</param>
        public static string MeasureRows(string pngPath, float unitsPerPixel, Vector2Int backgroundSample, int threshold = 40)
        {
            if (!File.Exists(pngPath)) throw new FileNotFoundException("Mockup not found", pngPath);
            var tex = new Texture2D(2, 2);
            try
            {
                tex.LoadImage(File.ReadAllBytes(pngPath));
                int w = tex.width, h = tex.height;
                Color32[] px = tex.GetPixels32();
                Color32 Pixel(int x, int yFromTop) => px[(h - 1 - yFromTop) * w + x];

                Color32 bg = Pixel(Mathf.Clamp(backgroundSample.x, 0, w - 1), Mathf.Clamp(backgroundSample.y, 0, h - 1));
                var sb = new StringBuilder();
                sb.AppendLine($"mockup {w}x{h} px = {w * unitsPerPixel:0.#}x{h * unitsPerPixel:0.#} units, background #{bg.r:X2}{bg.g:X2}{bg.b:X2}");

                bool inBand = false;
                int start = 0, minX = w, maxX = 0;
                for (int y = 0; y < h; y++)
                {
                    int first = -1, last = -1;
                    for (int x = 0; x < w; x++)
                    {
                        Color32 c = Pixel(x, y);
                        if (Math.Abs(c.r - bg.r) + Math.Abs(c.g - bg.g) + Math.Abs(c.b - bg.b) <= threshold) continue;
                        if (first < 0) first = x;
                        last = x;
                    }

                    bool on = first >= 0;
                    if (on && !inBand) { start = y; minX = w; maxX = 0; }
                    if (on) { minX = Math.Min(minX, first); maxX = Math.Max(maxX, last); }
                    bool closes = inBand && (!on || y == h - 1);
                    if (closes)
                    {
                        int end = on ? y + 1 : y;
                        sb.AppendLine($"y {start * unitsPerPixel:0}-{end * unitsPerPixel:0}  x {minX * unitsPerPixel:0}-{(maxX + 1) * unitsPerPixel:0}");
                    }
                    inBand = on;
                }
                return sb.ToString();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tex);
            }
        }
    }
}
