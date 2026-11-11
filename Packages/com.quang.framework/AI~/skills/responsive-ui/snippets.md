# Responsive UI snippets

Run with Unity MCP `execute_code`. The default compiler may be CodeDom (C# 6): no local functions, no target-typed
`new()`; write `UnityEngine.Object`, and get game types with `System.Type.GetType("UI.CanvasHome, <CANVAS_ASSEMBLY>")`.
Write output to the session scratchpad.

## 1. Measure the mockup

```csharp
return Quang.UI.Editor.MockupProbe.MeasureRows(
    "<MOCKUP_DIR>/01_home.png",
    1f / 3f,                    // <MOCKUP_TO_CANVAS>
    new Vector2Int(5, 900));    // a pixel that is plain background
```

Each line is one content band: `y top-bottom  x left-right` in canvas units from the top-left. Sample a colour or a
narrower region with `Texture2D.GetPixel(x, height - 1 - y)` when a band holds several elements.

## 2. Inventory a prefab

```csharp
const string PREFAB = "<CANVAS_DIR>/CanvasHome.prefab";
var root = PrefabUtility.LoadPrefabContents(PREFAB);
var sb = new System.Text.StringBuilder();
try
{
    System.Action<Transform, int> dump = null;
    dump = (t, d) =>
    {
        var rt = t as RectTransform;
        sb.Append(new string(' ', d * 2)).Append(t.name).Append(t.gameObject.activeSelf ? "" : " [off]");
        if (rt) sb.AppendFormat(" a({0},{1}) p{2} sz{3} pos{4}", rt.anchorMin, rt.anchorMax, rt.pivot, rt.sizeDelta, rt.anchoredPosition);
        foreach (var c in t.GetComponents<Component>())
        {
            if (c is Transform) continue;
            sb.Append(" |").Append(c ? c.GetType().Name : "MISSING");
            var tmp = c as TMPro.TMP_Text; if (tmp) sb.AppendFormat("('{0}' {1}pt {2} m{3})", tmp.text, tmp.fontSize, tmp.font ? tmp.font.name : "-", tmp.margin);
            var img = c as UnityEngine.UI.Image; if (img) sb.AppendFormat("({0} {1})", img.sprite ? img.sprite.name : "none", img.type);
        }
        sb.Append('\n');
        foreach (Transform ch in t) dump(ch, d + 1);
    };
    dump(root.transform, 0);
    var script = root.GetComponent(System.Type.GetType("UI.CanvasHome, <CANVAS_ASSEMBLY>"));
    var so = new SerializedObject(script); var it = so.GetIterator(); it.NextVisible(true);
    while (it.NextVisible(false))
        sb.AppendFormat("{0}={1}\n", it.name, it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue ? it.objectReferenceValue.name : it.propertyType.ToString());
}
finally { PrefabUtility.UnloadPrefabContents(root); }
return sb.ToString();
```

Sprite facts: `AssetDatabase.LoadAssetAtPath<Sprite>(path)` → `rect.size`, `border`, `pixelsPerUnit`.

## 3. Build template

```csharp
const string path = "<CANVAS_DIR>/CanvasHome.prefab";
var root = PrefabUtility.LoadPrefabContents(path);
try
{
    // Helpers as lambdas (C# 6): Node(name, parent) reuses or creates a child and moves it last;
    // Place(rt, anchorMin, anchorMax, pivot, size, pos) sets every RectTransform field explicitly.
    // Reuse nodes that carry serialized refs (SetParent(parent, false)), never recreate them.
    // Flexible zone: stretch between fixed zones; content child with UniformScaleFitter:
    //   var f = hero.gameObject.AddComponent(System.Type.GetType("Quang.UI.UniformScaleFitter, Quang.UI"));
    //   var fso = new SerializedObject(f); fso.FindProperty("designSize").vector2Value = new Vector2(390, 430);
    //   fso.ApplyModifiedPropertiesWithoutUndo();
    // Safe area: node "SafeArea" stretched + Quang.UI.SafeAreaFitter.
    // Re-assign canvas script refs through SerializedObject at the end.
    PrefabUtility.SaveAsPrefabAsset(root, path);
}
finally { PrefabUtility.UnloadPrefabContents(root); }
AssetDatabase.SaveAssets();
return System.IO.File.ReadAllText(path).Contains("m_Name: <NewNode>"); // proves it is on disk
```

## 4. Render on the device matrix

```csharp
return Quang.UI.Editor.ResponsivePreview.RenderAll(
    new[] { "<CANVAS_DIR>/CanvasHome.prefab" },
    "<UI_SCALER_PREFAB>",
    "/path/to/scratchpad/render/");
// custom matrix: pass a PreviewDevice[] as the 4th argument, e.g.
// new[] { new Quang.UI.Editor.PreviewDevice("phone-notch", 1179, 2556, 59f/852f, 34f/852f) }
```

The report lists the canvas size per device and, for each `UniformScaleFitter`, its scale and available size.
Read the PNGs next to the mockup. To prove a refactor changed nothing, render before and after and compare pixels:

```csharp
var a = new Texture2D(2, 2); a.LoadImage(System.IO.File.ReadAllBytes(BEFORE));
var b = new Texture2D(2, 2); b.LoadImage(System.IO.File.ReadAllBytes(AFTER));
var pa = a.GetPixels32(); var pb = b.GetPixels32(); int diff = 0;
for (int i = 0; i < pa.Length; i++)
    if (System.Math.Abs(pa[i].r - pb[i].r) + System.Math.Abs(pa[i].g - pb[i].g) + System.Math.Abs(pa[i].b - pb[i].b) > 12) diff++;
return diff + " px differ";
```

## 5. Refs and Play mode

```csharp
// refs on disk
var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("<CANVAS_DIR>/CanvasHome.prefab");
var so = new SerializedObject(prefab.GetComponent(System.Type.GetType("UI.CanvasHome, <CANVAS_ASSEMBLY>")));
var sb = new System.Text.StringBuilder();
foreach (var f in new[] { "playButton", "levelText" }) { var v = so.FindProperty(f).objectReferenceValue; sb.Append(f + "=" + (v ? v.name : "NULL") + " "); }
return sb.ToString();
```

Play mode: `manage_editor play` from the boot scene, then find the canvas with `UnityEngine.Object.FindFirstObjectByType(type)`,
read its texts, and invoke buttons with `button.onClick.Invoke()` (or pointer events for custom buttons).
Canvas open/close is animated: if a state looks wrong right after a click, read it again on the next call.
`manage_editor stop`, then `read_console` for errors.
