
using System.Collections.Generic;
using UnityEngine;

namespace BNR
{
    public class AssetCache
    {
        public static Dictionary<string, Sprite[]> compositionAnimation;
        public static Dictionary<string, bool> assetLoaded;

        public static void InitCache()
        {
            AssetCache.compositionAnimation = new Dictionary<string, Sprite[]>();
            AssetCache.assetLoaded = new Dictionary<string, bool>();
        }

        public static void LoadAsset(string assetName, Sprite[] frameSet)
        {
            AssetCache.compositionAnimation[assetName] = frameSet;
            AssetCache.assetLoaded[assetName] = true;
        }

        public static void InitAssetEntry(string assetName, Sprite[] frameSet)
        {
            AssetCache.compositionAnimation[assetName] = frameSet;
            AssetCache.assetLoaded[assetName] = false;
        }
    }
}
