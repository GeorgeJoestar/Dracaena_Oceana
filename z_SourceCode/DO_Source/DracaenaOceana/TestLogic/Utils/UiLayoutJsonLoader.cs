using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Verse;
using RwUi.Shared;

namespace RimWorldProj.TestLogic.UI
{
    public static class UiLayoutJsonLoader
    {
        const bool Verbose = false;

        public static bool TryLoadFromBundles(string jsonAssetName, out UiLayout layout)
        {
            if (string.IsNullOrEmpty(jsonAssetName))
                jsonAssetName = "WelcomeWnd.json";

            var targetWithExt   = jsonAssetName;
            var targetNoExt     = TrimJson(jsonAssetName);
            var targetWithExtLC = Normalize(targetWithExt);
            var targetNoExtLC   = Normalize(targetNoExt);

            if (Verbose)
                Log.Message($"[RwUi] TryLoadFromBundles target='{targetWithExt}'");

            foreach (var m in LoadedModManager.RunningMods)
            {
                var bundles = m.assetBundles?.loadedAssetBundles;
                if (bundles == null) continue;

                foreach (var ab in bundles)
                {
                    var names = ab.GetAllAssetNames() ?? Array.Empty<string>();

                    var ta = ab.LoadAsset<TextAsset>(targetWithExt) ?? ab.LoadAsset<TextAsset>(targetNoExt);
                    if (ta != null)
                    {
                        if (Verbose) Log.Message($"[RwUi] Direct hit: {ta.name}");
                        return TryCreateLayoutFromJson(ta.text, ab, out layout);
                    }

                    var match = names.FirstOrDefault(n =>
                    {
                        var nLC = Normalize(n);
                        if (nLC.Equals(targetWithExtLC)) return true;

                        var baseName     = Path.GetFileName(nLC);
                        var baseNameNoEx = Path.GetFileNameWithoutExtension(nLC);

                        return baseName.Equals(targetWithExtLC) ||
                               baseName.Equals(targetNoExtLC) ||
                               baseNameNoEx.Equals(targetNoExtLC);
                    });

                    if (!string.IsNullOrEmpty(match))
                    {
                        if (Verbose) Log.Message($"[RwUi] Path match: {match}");
                        var ta2 = ab.LoadAsset<TextAsset>(match);
                        if (ta2 != null)
                            return TryCreateLayoutFromJson(ta2.text, ab, out layout);
                    }
                }
            }

            Log.Warning($"[RwUi] JSON TextAsset '{jsonAssetName}' was not found in any loaded mod bundle.");
            layout = null;
            return false;
        }

        static string TrimJson(string s) =>
            s != null && s.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? s.Substring(0, s.Length - 5)
                : s;

        static string Normalize(string s) =>
            (s ?? "").Trim().Replace('\\', '/').ToLowerInvariant();

        static bool TryCreateLayoutFromJson(string json, AssetBundle bundleHint, out UiLayout layout)
        {
            layout = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                Log.Warning("[RwUi] JSON is empty.");
                return false;
            }

            json = StripBom(json);
            json = StripCommentsAndTrailingCommas(json);

            try
            {
                var jobj = JObject.Parse(json);

                int refW = (int?)jobj["referenceWidth"]  ?? 1280;
                int refH = (int?)jobj["referenceHeight"] ?? 720;
                string skinName = (string)jobj["skin"];

                var jEls = (JArray)jobj["elements"];
                if (jEls == null || jEls.Count == 0)
                {
                    Log.Warning("[RwUi] JSON 'elements' array missing or empty.");
                    return false;
                }

                var elems = new UiLayoutJsonElement[jEls.Count];
                for (int i = 0; i < jEls.Count; i++)
                {
                    var je = (JObject)jEls[i];

                    var rectObj = (JObject)je["rect"];
                    float x = (float?)rectObj?["x"] ?? 0f;
                    float y = (float?)rectObj?["y"] ?? 0f;
                    float w = (float?)rectObj?["width"] ?? 0f;
                    float h = (float?)rectObj?["height"] ?? 0f;
                    ColorDTO colorDto = new ColorDTO { r=0, g=0, b=0, a=0 };
                    var jc = je["textColor"] as JObject;
                    if (jc != null)
                    {
                        colorDto.r = (float?)jc["r"] ?? 0f;
                        colorDto.g = (float?)jc["g"] ?? 0f;
                        colorDto.b = (float?)jc["b"] ?? 0f;
                        colorDto.a = (float?)jc["a"] ?? 1f;
                    }

                    elems[i] = new UiLayoutJsonElement
                    {
                        id        = (string)je["id"],
                        type      = (string)je["type"],
                        rect      = new RectDTO { x = x, y = y, width = w, height = h },
                        text      = (string)je["text"],
                        image     = (string)je["image"],
                        styleName = (string)je["styleName"],

                        clickable  = (bool?)je["clickable"] ?? false,
                        bindingKey = (string)je["bindingKey"],

                        font     = (string)je["font"],
                        fontSize = (int?)je["fontSize"] ?? 0,
                        enabled  = (bool?)je["enabled"] ?? true,
                        overrideTextColor = (bool?)je["overrideTextColor"] ?? false,
                        textColor = colorDto,
                        textAlign = (string)je["textAlign"],
                    };
                }

                return BuildLayout(refW, refH, skinName, elems, bundleHint, out layout);
            }
            catch (Exception ex)
            {
                Log.Warning($"[RwUi] JSON parse failed: {ex.Message}");
                return false;
            }
        }

        static bool BuildLayout(int referenceWidth, int referenceHeight, string skinName, UiLayoutJsonElement[] elems, AssetBundle bundleHint, out UiLayout layout)
        {
            layout = null;

            var rw = referenceWidth  > 0 ? referenceWidth  : 1280;
            var rh = referenceHeight > 0 ? referenceHeight : 720;

            var lyt = ScriptableObject.CreateInstance<UiLayout>();
            lyt.referenceResolution = new Vector2Int(rw, rh);
            lyt.defaultSkin = ResolveSkin(skinName, bundleHint);

            if (elems == null || elems.Length == 0)
            {
                lyt.elements = Array.Empty<UiLayout.Element>();
                layout = lyt;
                return true;
            }

            var arr = new UiLayout.Element[elems.Length];
            for (int i = 0; i < elems.Length; i++)
            {
                var e = elems[i];

                var el = new UiLayout.Element
                {
                    id        = e.id,
                    rect      = e.rect.ToRect(),
                    text      = e.text,
                    styleName = e.styleName,

                    clickable  = e.clickable,
                    bindingKey = e.bindingKey,

                    font      = ResolveFont(e.font, bundleHint),
                    fontSize  = e.fontSize,
                    enabled   = e.enabled,
                    overrideTextColor = e.overrideTextColor,
                    textColor         = e.textColor.ToColor(),
                    textAlign =  UiLayout.TextAlign.Left 
                };

                if (!Enum.TryParse(e.type, true, out UiLayout.ElementType t))
                    t = UiLayout.ElementType.Label;
                el.type = t;

                if (!string.IsNullOrEmpty(e.textAlign) &&
                    Enum.TryParse(e.textAlign, true, out UiLayout.TextAlign ta))
                {
                    el.textAlign = ta;
                }
                
                el.image = ResolveTexture(e.image, bundleHint);

                if (float.IsNaN(el.rect.width)  || float.IsInfinity(el.rect.width))  el.rect.width  = 0f;
                if (float.IsNaN(el.rect.height) || float.IsInfinity(el.rect.height)) el.rect.height = 0f;

                // Back-compat defaults:
                if (string.IsNullOrEmpty(e.font)) el.font = null;
                if (e.fontSize < 0) el.fontSize = 0;
                if (e.enabled == false && IsEnabledMissingInOldJson(e)) el.enabled = true;

                arr[i] = el;
            }

            lyt.elements = arr;
            layout = lyt;
            return true;
        }

        static string StripBom(string s) =>
            !string.IsNullOrEmpty(s) && s[0] == '\uFEFF' ? s.Substring(1) : s;

        static string StripCommentsAndTrailingCommas(string s)
        {
            s = System.Text.RegularExpressions.Regex.Replace(
                s, @"^\s*//.*?$", "", System.Text.RegularExpressions.RegexOptions.Multiline);
            s = System.Text.RegularExpressions.Regex.Replace(
                s, @"/\*.*?\*/", "", System.Text.RegularExpressions.RegexOptions.Singleline);
            s = System.Text.RegularExpressions.Regex.Replace(
                s, @",(\s*[\]\}])", "$1");
            return s;
        }

        static GUISkin ResolveSkin(string name, AssetBundle hint)
        {
            if (string.IsNullOrEmpty(name)) return null;

            var skin = hint?.LoadAsset<GUISkin>(name);
            if (skin) return skin;

            foreach (var m in LoadedModManager.RunningMods)
            foreach (var ab in m.assetBundles.loadedAssetBundles)
            {
                skin = ab.LoadAsset<GUISkin>(name);
                if (skin) return skin;
            }

            if (Verbose)
                Log.Message($"[RwUi] GUISkin '{name}' not found.");

            return null;
        }

        static Texture2D ResolveTexture(string name, AssetBundle hint)
        {
            if (string.IsNullOrEmpty(name)) return null;

            var tex = hint?.LoadAsset<Texture2D>(name);
            if (tex) return tex;

            foreach (var m in LoadedModManager.RunningMods)
            foreach (var ab in m.assetBundles.loadedAssetBundles)
            {
                tex = ab.LoadAsset<Texture2D>(name);
                if (tex) return tex;
            }

            if (Verbose)
                Log.Message($"[RwUi] Texture '{name}' not found.");

            return null;
        }

        static bool IsEnabledMissingInOldJson(UiLayoutJsonElement e)
        {
            // old json wouldn't contain enabled; Newtonsoft gives default false.
            // If you want stricter detection, add a JToken presence check earlier.
            return true;
        }

        static Font ResolveFont(string name, AssetBundle hint)
        {
            if (string.IsNullOrEmpty(name)) return null;

            var f = hint?.LoadAsset<Font>(name);
            if (f) return f;

            foreach (var m in LoadedModManager.RunningMods)
            foreach (var ab in m.assetBundles.loadedAssetBundles)
            {
                f = ab.LoadAsset<Font>(name);
                if (f) return f;
            }
            return null;
        }
    }
}
