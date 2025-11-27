using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorldProj.TestLogic.Utils;

public class FontLoader
{
    private static readonly Dictionary<string, Font> cache = new();

    public static bool TryGet(string fontName, out Font font)
    {
        if (cache.TryGetValue(fontName, out font)) return true;

        foreach (ModContentPack mcp in LoadedModManager.RunningMods)
        foreach (AssetBundle ab in mcp.assetBundles.loadedAssetBundles)
        {
            Log.Warning($"Font: Font '{ab.name}' found in mod '{mcp.Name}'");
            if ((font = ab.LoadAsset<Font>(fontName)) != null)
            {
                cache.Add(fontName, font);
                return true;
            }
        }

        font = null; return false;
    }
}