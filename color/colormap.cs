using System.Collections.Generic;
using System;

namespace Scimesh.Color
{
    public class Colormap
    {
        public enum Name
        {
            Rainbow,
            RainbowAlpha,
            RainbowAlphaBlendedTransparent,
            HotAndCold
        };
        public enum Colorspace
        {
            RGB,
            RGBA
        };
        static Dictionary<Colormap.Name, Colormap> colormaps = new Dictionary<Colormap.Name, Colormap>
        {
            {
                Colormap.Name.Rainbow, new Colormap(
                    Colormap.Name.Rainbow,
                    new float[5, 3] {
                        { 0, 0, 1 },
                        { 0, 1, 1 },
                        { 0, 1, 0 },
                        { 1, 1, 0 },
                        { 1, 0, 0 }
                    }, 
                    Colorspace.RGB, true)
            },
            {
                Colormap.Name.RainbowAlpha, new Colormap(
                    Colormap.Name.RainbowAlpha,
                    new float[5, 4] {
                        { 0, 0, 1, 1 },
                        { 0, 1, 1, 1 },
                        { 0, 1, 0, 1 },
                        { 1, 1, 0, 1 },
                        { 1, 0, 0, 1 }
                    }, 
                    Colorspace.RGBA, true)
            },
            {
                Colormap.Name.RainbowAlphaBlendedTransparent, new Colormap(
                    Colormap.Name.RainbowAlphaBlendedTransparent,
                    new float[6, 4] {
                        { 0, 0, 1, 0 },
                        { 0, 0, 1, 1 },
                        { 0, 1, 1, 1 },
                        { 0, 1, 0, 1 },
                        { 1, 1, 0, 1 },
                        { 1, 0, 0, 1 }
                    }, 
                    Colorspace.RGBA, true)
            },
            {
                Colormap.Name.HotAndCold, new Colormap(
                    Colormap.Name.HotAndCold,
                    new float[3, 4] {
                        { 0, 0, 1, 1 },
                        { 1, 1, 1, 1 },
                        { 1, 0, 0, 1 }
                    }, 
                    Colorspace.RGBA, true)
            }
        };
        public static Colormap Get(Name name)
        {
            return colormaps[name];
        }

        public Name ColormapName { get; private set; }
        public float[,] Colors { get; private set; }
        public int NColors { get; private set; }
        public int NComponents { get; private set; }
        public Colorspace ColormapColorspace { get; private set; }
        public bool Smooth { get; private set; }

        Colormap(Name name, float[,] colors, Colorspace colorspace, bool smooth)
        {
            this.ColormapName = name;
            this.Colors = colors;
            this.NColors = colors.GetLength(0);
            this.NComponents = colors.GetLength(1);
            this.ColormapColorspace = colorspace;
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
