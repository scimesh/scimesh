using System.Collections.Generic;
using System;

namespace Scimesh.Base.To
{
	/// <summary>
	/// Base object to itself functions.
	/// </summary>
	public static class Base
	{
		/// <summary>
		/// The cell field to point field.
		/// </summary>
		public static readonly Func<MeshCellField, MeshPointField> cellFieldToPointField = (cf) => {
			float?[] values = new float?[cf.Mesh.points.Length * cf.NComponents];
			MeshPointField pf = new MeshPointField (cf.Name, cf.NComponents, values.Length, values, cf.Mesh);

			// Cells values to points values
			// Weighted (by square distances from centroids) arithmetic mean algorithm
			Mesh mesh = cf.Mesh;

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
			for (int i = 0; i < mesh.points.Length; i++) {
				float sumWeight = 0; // sumW=D1+D2+...+Di (Di - distance to ith cell centroid)
				// sumWVi=D1*V1i+D2*V2i+...+Dj*Vji (Vji - ith component of the value in jth cell)
				float?[] sumWeightValues = new float?[cf.NComponents];
				for (int j = 0; j < sumWeightValues.Length; j++) {
					sumWeightValues [j] = 0;
				}
				;
				for (int j = 0; j < pointsToCentroidsDistances [i].Count; j++) {
					int cellIdx = mesh.points [i].cellsIndices [j];
					for (int m = 0; m < sumWeightValues.Length; m++) {
						sumWeightValues [m] += pointsToCentroidsDistances [i] [j] * cf [cellIdx] [m];
					}
					sumWeight += pointsToCentroidsDistances [i] [j];
				}
				// Vi = sumWVi/sumW (Vi - ith component of the value in the point)
				float?[] value = new float?[cf.NComponents];
				for (int j = 0; j < cf.NComponents; j++) {
					if (sumWeight != 0) {
						value [j] = sumWeightValues [j] / sumWeight;
					} else {
						value [j] = null;
					}
				}
				pf [i] = value;
			}
			return pf;
		};
			
		/// <summary>
		/// Create cell mesh filter by mesh point scalar field and isovalue.
		/// If cell points have field values with different result of expression: 
		/// isovalue > pointValue, then it is visible.
		/// Also, cell shouldn't contain null values.
		/// </summary>
		public static readonly Func<MeshPointField, float?, MeshFilter> pointIsovalueCellMeshFilter = (pf, isovalue) => {
			if (pf.NComponents != 1) {
				throw new ArgumentException ("MeshPointField must be a scalar field (nComponents == 1)");
			}
			List<int> visibleCells = new List<int> ();
			for (int i = 0; i < pf.Mesh.cells.Length; i++) {
				bool isVisible = false;
				int[] psIs = pf.Mesh.cells [i].pointsIndices;
				if (pf [psIs [0]] != null) {
					bool firstValue = isovalue > pf [psIs [0]] [0];
					for (int j = 1; j < psIs.Length; j++) {
						if (pf [psIs [j]] != null) {
							if (firstValue != isovalue > pf [psIs [j]] [0]) {
								isVisible = true;
							}
						} else {
							isVisible = false;
							break;
						}
					}
				}
				if (isVisible) {
					visibleCells.Add (i);
				}
			}
			return new MeshFilter (visibleCells.ToArray ());
		};

		/// <summary>
		/// The lightning algorithm.
		/// </summary>
		public static readonly Action<MeshPointField, float, float[]> lightning = (pf, isoSquareDist, position) => {
			if (pf.NComponents != 1) {
				throw new ArgumentException ("MeshPointField must be a scalar field (nComponents == 1)");
			}

			// Square distance function
			Func<float[], float[], float> squareDist = 
				(pointCs, positionCs) => 
				(pointCs [0] - positionCs [0]) * (pointCs [0] - positionCs [0])
				+ (pointCs [1] - positionCs [1]) * (pointCs [1] - positionCs [1])
				+ (pointCs [2] - positionCs [2]) * (pointCs [2] - positionCs [2]);

			// Search start point
			int startPointIdx = -1;
			for (int i = 0; i < pf.NValues; i++) {
				if (pf [i] [0] != null) {
					startPointIdx = i;
					break;
				}
			}
			if (startPointIdx == -1) {
				for (int i = 0; i < pf.Mesh.points.Length; i++) {
					float[] pCs = pf.Mesh.points [i].coordinates;
					float pV = squareDist (pCs, position);
					int[] npIs = pf.Mesh.points [i].neighbourPointsIndices;
					for (int j = 1; j < npIs.Length; j++) {
						float[] npCs = pf.Mesh.points [npIs [i]].coordinates;
						float npV = squareDist (npCs, position);
						if (isoSquareDist > pV != isoSquareDist > npV) {
							startPointIdx = i;
							break;
						}
					}
				}
			}

			float[] spCs = pf.Mesh.points [startPointIdx].coordinates;

			// Reset point field
			pf.ResetValues ();

			// Lightning algorithm
			pf [startPointIdx] = new float?[] { squareDist (spCs, position) };
			HashSet<int> points = new HashSet<int> ();
			points.Add (startPointIdx);
			while (points.Count != 0) {
				HashSet<int> newPoints = new HashSet<int> ();
				foreach (int point in points) {
					int[] npIs = pf.Mesh.points [point].neighbourPointsIndices;
					for (int i = 0; i < npIs.Length; i++) {
						if (pf [npIs [i]] [0] == null) {
							float[] npCs = pf.Mesh.points [npIs [i]].coordinates;
							float npV = squareDist (npCs, position);
							if (isoSquareDist > pf [point] [0] != isoSquareDist > npV) {
								pf [npIs [i]] = new float?[] { npV };
								newPoints.Add (npIs [i]);
							}
						}
					}
				}
				points = newPoints;
			}
		};
	}
}
