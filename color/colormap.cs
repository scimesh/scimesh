using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

namespace Scimesh.Color
{
	public static class Colormaps
	{
		public static Dictionary <string, Colormap> dictionary;

		static Colormaps ()
		{
			dictionary = new Dictionary<string, Colormap> ();
			dictionary.Add ("rainbow", new Colormap ("rainbow", new float[5, 3] {
				{ 0, 0, 1 },
				{ 0, 1, 1 },
				{ 0, 1, 0 },
				{ 1, 1, 0 },
				{ 1, 0, 0 }
			}, "RGB"));
			dictionary.Add ("rainbowAlpha", new Colormap ("rainbowAlpha", new float[5, 4] {
				{ 0, 0, 1, 1 },
				{ 0, 1, 1, 1 },
				{ 0, 1, 0, 1 },
				{ 1, 1, 0, 1 },
				{ 1, 0, 0, 0 }
			}, "RGBA"));
		}
	}

	public class Colormap
	{
		string name;
		float[,] colors;
		int nColors;
		int nComponents;
		public string colorspace;

		public Colormap (string name, float[,] colors, string colorspace)
		{
			this.name = name;
			this.colors = colors;
			this.nColors = colors.GetLength (0);
			this.nComponents = colors.GetLength (1);
			this.colorspace = colorspace;
		}

		public float[] GetSmoothColor (float normedValue)
		{
			int floorIndex = (int)Math.Floor ((normedValue * (nColors - 1)));
			int ceilIndex = (int)Math.Ceiling ((normedValue * (nColors - 1)));
			float[] color = new float[nComponents];
			if (floorIndex != ceilIndex) {
				float floatIndex = normedValue * (nColors - 1);
				float k = floatIndex - (float)floorIndex;
				for (int i = 0; i < nComponents; i++) {
					float component = k * (colors [ceilIndex, i] - colors [floorIndex, i]) + colors [floorIndex, i];
					color [i] = component;
				}
			} else {
				for (int i = 0; i < nComponents; i++) {
					color [i] = colors [floorIndex, i];
				}
			}
			return color;
		}

		public float[] GetSharpColor (float normedValue)
		{
			int colorIndex = (int)Math.Round ((normedValue * (nColors - 1)));
			float[] color = new float[nComponents];
			for (int i = 0; i < nComponents; i++) {
				color [i] = colors [colorIndex, i];
			}
			return color;
		}
	}
}
