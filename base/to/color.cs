using System.Collections.Generic;
using System;

namespace Scimesh.Base.To
{
	public static class Color
	{
		public static Scimesh.Base.PointField PointFieldToColorFieldRGBA (Scimesh.Base.PointField pointField, string colormap)
		{
			if (pointField.nComponents != 1) {
				throw new ArgumentException ("pointField must be a scalar field (pointField.nComponents == 1)");
			}
			float?[] normedData = pointField.GetNormedData ();
			List<float?> colors = new List<float?> ();
			for (int i = 0; i < normedData.Length; i++) {
				float?[] color = new float?[4];
				if (normedData [i].HasValue) {
					float[] colorRGBA = Scimesh.Color.Colormaps.dictionary [colormap].GetSmoothColor ((float)normedData [i]);
					for (int j = 0; j < colorRGBA.Length; j++) {
						color [j] = colorRGBA [j];
					}
				}
				colors.AddRange (color);
			}
			return new Scimesh.Base.PointField (
				pointField.name + "Color",
				4,
				pointField.dim,
				pointField.nTimeValues,
				colors.ToArray (),
				pointField.times,
				pointField.mesh
			);
		}
	}
}
