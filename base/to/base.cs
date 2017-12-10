using System.Collections.Generic;
using System;

namespace Scimesh.Base.To
{
	public static class Base
	{
		public static Isocells PointFieldToIsosurface (PointField pointField, int timeValueIndex, float isovalue)
		{
			if (pointField.nComponents != 1) {
				throw new ArgumentException ("pointField must be a scalar field (pointField.nComponents == 1)");
			}

			return new Isocells (pointField.GetTimeValues (timeValueIndex), isovalue, pointField.mesh);
		}

		public static PointField CellFieldToPointField (CellField cellField)
		{
			float?[] values = new float?[cellField.mesh.points.Length * cellField.nComponents * cellField.nTimeValues];

			// Cells values to points values
			// Weighted (by square distances from centroids) arithmetic mean algorithm
			Mesh mesh = cellField.mesh;

			// Calculate cells centroids coordiantes array
			float[,] cellsCentroids = new float[mesh.cells.Length, 3];
			for (int i = 0; i < mesh.cells.Length; i++) {
				float[] centroidCoordinates = mesh.CellCentroid (i);
				cellsCentroids [i, 0] = centroidCoordinates [0];
				cellsCentroids [i, 1] = centroidCoordinates [1];
				cellsCentroids [i, 2] = centroidCoordinates [2];
			}
				
			// Calculate square distances from points to neighbour cells centroids
			List<List<float>> pointsToCentroidsDistances = new List<List<float>> ();
			for (int i = 0; i < mesh.points.Length; i++) {
				List<float> distances = new List<float> ();
				float[] pointCoordiantes = mesh.points [i].coordinates;
				for (int j = 0; j < mesh.points [i].cellsIndices.Length; j++) {
					float[] vector = new float[3] {
						cellsCentroids [mesh.points [i].cellsIndices [j], 0] - pointCoordiantes [0],
						cellsCentroids [mesh.points [i].cellsIndices [j], 1] - pointCoordiantes [1],
						cellsCentroids [mesh.points [i].cellsIndices [j], 2] - pointCoordiantes [2]
					};
					float distance = vector [0] * vector [0] + vector [1] * vector [1] + vector [2] * vector [2];
					distances.Add (distance);
				}
				pointsToCentroidsDistances.Add (distances);
			}

			// Set weighted (by square distances from centroids) arithmetic mean value to points
			for (int i = 0; i < cellField.nTimeValues; i++) {
				for (int j = 0; j < mesh.points.Length; j++) {
					float sumWeight = 0; // sumW=D1+D2+...+Di (Di - distance to ith cell centroid)
					// sumWVi=D1*V1i+D2*V2i+...+Dj*Vji (Vji - ith component of the value in jth cell)
					float?[] sumWeightValues = new float?[cellField.nComponents];
					for (int k = 0; k < sumWeightValues.Length; k++) {
						sumWeightValues [k] = 0;
					}
					;
					for (int k = 0; k < pointsToCentroidsDistances [j].Count; k++) {
						int cellIdx = mesh.points [j].cellsIndices [k];
						for (int m = 0; m < sumWeightValues.Length; m++) {
							sumWeightValues [m] += pointsToCentroidsDistances [j] [k]
							* cellField.values [i * cellField.nValues * cellField.nComponents
							+ cellIdx * cellField.nComponents + m];
						}
						sumWeight += pointsToCentroidsDistances [j] [k];
					}
					// Vi = sumWVi/sumW (Vi - ith component of the value in the point)
					for (int k = 0; k < cellField.nComponents; k++) {
						if (sumWeight != 0) {
							values [i * mesh.points.Length * cellField.nComponents
							+ j * cellField.nComponents + k] = sumWeightValues [k] / sumWeight;
						} else {
							values [i * mesh.points.Length * cellField.nComponents
							+ j * cellField.nComponents + k] = null;
						}
					}
				}
			}

			return new PointField (cellField.name, cellField.nComponents, cellField.dim,
				cellField.nTimeValues,
				values,
				cellField.times,
				cellField.mesh
			);
		}
	}
}
