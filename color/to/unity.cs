using UnityEngine;

namespace Scimesh.Color.To
{
	public static class Unity
	{
		public static UnityEngine.Color RGBColorToUnityColor (float[] color)
		{
			return new UnityEngine.Color (color [0], color [1], color [2], 1);
		}

		public static UnityEngine.Color RGBAColorToUnityColor (float[] color)
		{
			return new UnityEngine.Color (color [0], color [1], color [2], color [3]);
		}

		public static UnityEngine.Color ColormapColorToUnityColor (float normedValue, string colormap)
		{
			float[] color = Scimesh.Color.Colormaps.dictionary [colormap].GetSmoothColor (normedValue);
			if (Scimesh.Color.Colormaps.dictionary [colormap].colorspace == "RGB") {
				return RGBColorToUnityColor (color);
			} else if (Scimesh.Color.Colormaps.dictionary [colormap].colorspace == "RGBA") {
				return RGBAColorToUnityColor (color);
			} else {
				return UnityEngine.Color.black;
			}
		}
	}
}
