using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public static class ExtensionMethods
    {
        private static Dictionary<ResourceTypes, Texture2D> cache = new Dictionary<ResourceTypes, Texture2D>(); 

        public static Texture2D GetIcon(this ResourceTypes res)
        {
            Texture2D result;
            if (!cache.TryGetValue(res, out result))
            {
                result = Resources.Load<Texture2D>("ResourcesIcon/" + res.ToString());
                cache[res] = result;
            }
            return result;
        }
    }
}