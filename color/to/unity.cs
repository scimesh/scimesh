using UnityEngine;

namespace Scimesh.Color.To
{
    public static class Unity
    {
        public static UnityEngine.Color ColormapColorToUnityColor(Colormap colormap, float? normedValue)
        {
            if (colormap.Colorspace == "RGB")
            {
                if (normedValue != null)
                {
                    float[] color = colormap.GetColor((float)normedValue);
                    return new UnityEngine.Color(color[0], color[1], color[2], 1);
                }
                else
                {
                    return UnityEngine.Color.white;
                }
            }
            else if (colormap.Colorspace == "RGBA")
            {
                if (normedValue != null)
                {
                    float[] color = colormap.GetColor((float)normedValue);
                    return new UnityEngine.Color(color[0], color[1], color[2], color[3]);
                }
                else
                {
                    return new UnityEngine.Color(1, 1, 1, 0);
                }
            }
            else
            {
                return UnityEngine.Color.black;
            }
        }
    }
}
