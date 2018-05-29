using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

namespace Scimesh.Color
{
    public static class GetColormap
    {
        public enum Name { Rainbow, RainbowAlpha, RainbowAlphaBlendedTransparent, HotAndCold };
        public static Dictionary<Name, Colormap> byName;

        static GetColormap()
        {
            byName = new Dictionary<Name, Colormap>
            {
                {
                    Name.Rainbow, new Colormap(new float[5, 3] {
                    { 0, 0, 1 },
                    { 0, 1, 1 },
                    { 0, 1, 0 },
                    { 1, 1, 0 },
                    { 1, 0, 0 }
                    }, "RGB", true)
                },
                {
                    Name.RainbowAlpha, new Colormap(new float[5, 4] {
                    { 0, 0, 1, 1 },
                    { 0, 1, 1, 1 },
                    { 0, 1, 0, 1 },
                    { 1, 1, 0, 1 },
                    { 1, 0, 0, 1 }
                    }, "RGBA", true)
                },
                {
                    Name.RainbowAlphaBlendedTransparent, new Colormap(new float[6, 4] {
                    { 0, 0, 1, 0 },
                    { 0, 0, 1, 1 },
                    { 0, 1, 1, 1 },
                    { 0, 1, 0, 1 },
                    { 1, 1, 0, 1 },
                    { 1, 0, 0, 1 }
                    }, "RGBA", true)
                },
                {
                    Name.HotAndCold, new Colormap(new float[3, 4] {
                    { 0, 0, 1, 1 },
                    { 1, 1, 1, 1 },
                    { 1, 0, 0, 1 }
                    }, "RGBA", true)
                }
            };
        }
    }

    public class Colormap
    {
        public float[,] Colors { get; private set; }
        public int NColors { get; private set; }
        public int NComponents { get; private set; }
        public string Colorspace { get; private set; }
        public bool Smooth { get; private set; }

        public Colormap(float[,] colors, string colorspace, bool smooth)
        {
            this.Colors = colors;
            this.NColors = colors.GetLength(0);
            this.NComponents = colors.GetLength(1);
            this.Colorspace = colorspace;
            this.Smooth = smooth;
        }

        public float[] GetColor(float normedValue)
        {
            if (Smooth)
            {
                return GetSmoothColor(normedValue);
            }
            else
            {
                return GetSharpColor(normedValue);
            }
        }

        float[] GetSmoothColor(float normedValue)
        {
            int floorIndex = (int)Math.Floor((normedValue * (NColors - 1)));
            int ceilIndex = (int)Math.Ceiling((normedValue * (NColors - 1)));
            float[] color = new float[NComponents];
            if (floorIndex != ceilIndex)
            {
                float floatIndex = normedValue * (NColors - 1);
                float k = floatIndex - (float)floorIndex;
                for (int i = 0; i < NComponents; i++)
                {
                    float component = k * (Colors[ceilIndex, i] - Colors[floorIndex, i]) + Colors[floorIndex, i];
                    color[i] = component;
                }
            }
            else
            {
                for (int i = 0; i < NComponents; i++)
                {
                    color[i] = Colors[floorIndex, i];
                }
            }
            return color;
        }

        float[] GetSharpColor(float normedValue)
        {
            int colorIndex = (int)Math.Round((normedValue * (NColors - 1)));
            float[] color = new float[NComponents];
            for (int i = 0; i < NComponents; i++)
            {
                color[i] = Colors[colorIndex, i];
            }
            return color;
        }
    }
}
