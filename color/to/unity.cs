using System;

namespace Scimesh.Color.To
{
    public static class Unity
    {
        public static readonly Func<Colormap, float?, UnityEngine.Color> ColormapColorToUnityColorNullable = (cm, nv) =>
        {
            switch (cm.Colorspace)
            {
                case "RGB":
                    if (nv != null)
                    {
                        float[] color = cm.GetColor((float)nv);
                        return new UnityEngine.Color(color[0], color[1], color[2], 1);
                    }
                    else
                    {
                        return UnityEngine.Color.white;
                    }
                case "RGBA":
                    if (nv != null)
                    {
                        float[] color = cm.GetColor((float)nv);
                        return new UnityEngine.Color(color[0], color[1], color[2], color[3]);
                    }
                    else
                    {
                        return new UnityEngine.Color(1, 1, 1, 0);
                    }
                default:
                    return UnityEngine.Color.white;
            }
        };

        public static readonly Func<Colormap, float?[], UnityEngine.Color[]> ColormapColorToUnityColorNullableArray = (cm, nvs) =>
        {
            UnityEngine.Color[] cs = new UnityEngine.Color[nvs.Length];
            for (int i = 0; i < nvs.Length; i++)
            {
                cs[i] = ColormapColorToUnityColorNullable(cm, nvs[i]);
            }
            return cs;
        };

        public static readonly Func<Colormap, float, UnityEngine.Color> ColormapColorToUnityColor = (cm, nv) =>
        {
            float[] color;
            switch (cm.Colorspace)
            {
                case "RGB":
                    color = cm.GetColor(nv);
                    return new UnityEngine.Color(color[0], color[1], color[2], 1);
                case "RGBA":
                    color = cm.GetColor(nv);
                    return new UnityEngine.Color(color[0], color[1], color[2], color[3]);
                default:
                    return UnityEngine.Color.white;
            }
        };

        public static readonly Func<Colormap, float[], UnityEngine.Color[]> ColormapColorToUnityColorArray = (cm, nvs) =>
        {
            UnityEngine.Color[] cs = new UnityEngine.Color[nvs.Length];
            for (int i = 0; i < nvs.Length; i++)
            {
                cs[i] = ColormapColorToUnityColor(cm, nvs[i]);
            }
            return cs;
        };
    }
}
