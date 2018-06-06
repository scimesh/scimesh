using System;

namespace Scimesh.Color.To
{
    public static class Unity
    {
        public static readonly Func<Colormap, float?, UnityEngine.Color> ColormapColorToUnityColorNullable = (cm, normedValue) =>
        {
            switch (cm.ColormapColorspace)
            {
                case Colormap.Colorspace.RGB:
                    if (normedValue != null)
                    {
                        float[] color = cm.GetColor((float)normedValue);
                        return new UnityEngine.Color(color[0], color[1], color[2], 1);
                    }
                    else
                    {
                        return UnityEngine.Color.white;
                    }
                case Colormap.Colorspace.RGBA:
                    if (normedValue != null)
                    {
                        float[] color = cm.GetColor((float)normedValue);
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

        public static readonly Func<Colormap, float?[], UnityEngine.Color[]> ColormapColorToUnityColorNullableArray = (cm, normedValues) =>
        {
            UnityEngine.Color[] cs = new UnityEngine.Color[normedValues.Length];
            for (int i = 0; i < normedValues.Length; i++)
            {
                cs[i] = ColormapColorToUnityColorNullable(cm, normedValues[i]);
            }
            return cs;
        };

        public static readonly Func<Colormap, float, UnityEngine.Color> ColormapColorToUnityColor = (cm, normedValue) =>
        {
            float[] color;
            switch (cm.ColormapColorspace)
            {
                case Colormap.Colorspace.RGB:
                    color = cm.GetColor(normedValue);
                    return new UnityEngine.Color(color[0], color[1], color[2], 1);
                case Colormap.Colorspace.RGBA:
                    color = cm.GetColor(normedValue);
                    return new UnityEngine.Color(color[0], color[1], color[2], color[3]);
                default:
                    return UnityEngine.Color.white;
            }
        };

        public static readonly Func<Colormap, float[], UnityEngine.Color[]> ColormapColorToUnityColorArray = (cm, normedValues) =>
        {
            UnityEngine.Color[] cs = new UnityEngine.Color[normedValues.Length];
            for (int i = 0; i < normedValues.Length; i++)
            {
                cs[i] = ColormapColorToUnityColor(cm, normedValues[i]);
            }
            return cs;
        };
    }
}
