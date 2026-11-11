using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Quang.UI.Editor
{
    /// <summary>
    /// Renders a UI canvas prefab to PNG on several screens without entering Play mode or touching the open scene.
    /// The canvas size comes from the project's real <see cref="CanvasScaler"/>; safe-area insets are simulated by
    /// anchoring every <see cref="SafeAreaFitter"/>; <see cref="UniformScaleFitter"/>, <see cref="AspectRatioFitter"/>
    /// and layout groups are re-applied before capture.
    /// </summary>
    public static class ResponsivePreview
    {
        /// <summary>
        /// Renders <paramref name="prefabPath"/> on each device into <paramref name="outDir"/> as
        /// <c>&lt;Prefab&gt;_&lt;device&gt;_&lt;W&gt;x&lt;H&gt;.png</c>. Returns a report with the canvas size and the scale
        /// and available size of every <see cref="UniformScaleFitter"/>.
        /// </summary>
        /// <param name="scalerPrefabPath">Prefab (or the canvas prefab itself) that holds the project's CanvasScaler.</param>
        public static string Render(string prefabPath, string scalerPrefabPath, string outDir, IList<PreviewDevice> devices = null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) throw new FileNotFoundException("Prefab not found", prefabPath);
            ReadScaler(scalerPrefabPath, out Vector2 reference, out float match);
            Directory.CreateDirectory(outDir);

            var sb = new StringBuilder();
            sb.AppendLine($"{prefabPath}  scaler ref {reference.x}x{reference.y} match {match}");
            foreach (PreviewDevice device in devices ?? PreviewDevice.Defaults)
            {
                sb.AppendLine(RenderOne(prefab, device, reference, match, outDir));
            }
            return sb.ToString();
        }

        /// <summary><see cref="Render"/> for several prefabs.</summary>
        public static string RenderAll(IEnumerable<string> prefabPaths, string scalerPrefabPath, string outDir, IList<PreviewDevice> devices = null)
        {
            var sb = new StringBuilder();
            foreach (string path in prefabPaths) sb.Append(Render(path, scalerPrefabPath, outDir, devices));
            return sb.ToString();
        }

        private static void ReadScaler(string scalerPrefabPath, out Vector2 reference, out float match)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(scalerPrefabPath);
            CanvasScaler scaler = go != null ? go.GetComponentInChildren<CanvasScaler>(true) : null;
            if (scaler == null) throw new FileNotFoundException("No CanvasScaler found in prefab", scalerPrefabPath);
            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize ||
                scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            {
                Debug.LogWarning($"[ResponsivePreview] {scalerPrefabPath}: only Scale With Screen Size / Match Width Or Height is modelled.");
            }
            reference = scaler.referenceResolution;
            match = scaler.matchWidthOrHeight;
        }

        private static string RenderOne(GameObject prefab, PreviewDevice device, Vector2 reference, float match, string outDir)
        {
            Scene scene = EditorSceneManager.NewPreviewScene();
            var rt = new RenderTexture(device.Width, device.Height, 24);
            Texture2D tex = null;
            try
            {
                var camGo = new GameObject("PreviewCamera");
                SceneManager.MoveGameObjectToScene(camGo, scene);
                var cam = camGo.AddComponent<Camera>();
                cam.scene = scene;
                cam.orthographic = true;
                cam.orthographicSize = 1f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.aspect = (float)device.Width / device.Height;
                cam.targetTexture = rt;

                Vector2 canvasSize = CanvasScalerMath.CanvasSize(new Vector2(device.Width, device.Height), reference, match);
                var rootGo = new GameObject("PreviewCanvas", typeof(RectTransform));
                SceneManager.MoveGameObjectToScene(rootGo, scene);
                rootGo.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                var root = (RectTransform)rootGo.transform;
                root.sizeDelta = canvasSize;
                root.position = new Vector3(0f, 0f, 10f);
                root.localScale = Vector3.one * (2f / canvasSize.y);

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.transform.SetParent(root, false);
                var irt = (RectTransform)instance.transform;
                irt.anchorMin = Vector2.zero;
                irt.anchorMax = Vector2.one;
                irt.offsetMin = Vector2.zero;
                irt.offsetMax = Vector2.zero;

                foreach (SafeAreaFitter fitter in instance.GetComponentsInChildren<SafeAreaFitter>(true))
                {
                    fitter.enabled = false;
                    var r = (RectTransform)fitter.transform;
                    r.anchorMin = new Vector2(0f, device.SafeBottom);
                    r.anchorMax = new Vector2(1f, 1f - device.SafeTop);
                    r.offsetMin = Vector2.zero;
                    r.offsetMax = Vector2.zero;
                }

                Canvas.ForceUpdateCanvases();
                foreach (LayoutGroup group in instance.GetComponentsInChildren<LayoutGroup>(true))
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)group.transform);
                }
                foreach (AspectRatioFitter f in instance.GetComponentsInChildren<AspectRatioFitter>(true)) { f.enabled = false; f.enabled = true; }

                var report = new StringBuilder();
                foreach (UniformScaleFitter f in instance.GetComponentsInChildren<UniformScaleFitter>(true))
                {
                    f.enabled = false;
                    f.enabled = true;
                    Vector2 available = f.transform.parent is RectTransform p ? p.rect.size : Vector2.zero;
                    report.Append($"  {f.name} scale {f.transform.localScale.x:0.000} in {available.x:0}x{available.y:0}");
                }
                foreach (TMPro.TMP_Text t in instance.GetComponentsInChildren<TMPro.TMP_Text>(true)) t.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases();
                cam.Render();
                cam.Render();

                RenderTexture.active = rt;
                tex = new Texture2D(device.Width, device.Height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, device.Width, device.Height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;
                cam.targetTexture = null;

                string file = Path.Combine(outDir, $"{prefab.name}_{device.Name}_{device.Width}x{device.Height}.png");
                File.WriteAllBytes(file, tex.EncodeToPNG());
                return $"{device}: canvas {canvasSize.x:0}x{canvasSize.y:0}{report} -> {file}";
            }
            finally
            {
                RenderTexture.active = null;
                Object.DestroyImmediate(rt);
                if (tex != null) Object.DestroyImmediate(tex);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
