using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Scimesh.Base.To
{
	/// <summary>
	/// Base obect to Unity object functions.
	/// </summary>
	public static class Unity
	{
		/// <summary>
		/// Unity restriction on max vertices in the mesh.
		/// </summary>
		public const int MAX_MESH_VERTICES = 65534;

		public static readonly Func<Mesh, MeshFilter, UnityEngine.Mesh[]> MeshToUnityMesh = (m, mf) => {
			// Unity Meshes vertices and triangles (with max vertices[i].Length == MAX_MESH_VERTICES)
			List<Vector3[]> vs = new List<Vector3[]> ();
			List<int[]> ts = new List<int[]> ();
			List<int> meshTriangles = new List<int> ();
			List<Vector3> meshVertices = new List<Vector3> ();
			int vertexCnt = 0;
			for (int i = 0; i < mf.cellsIndices.Length; i++) {
				int cellIdx = mf.cellsIndices[i];
				for (int j = 0; j < m.cells [cellIdx].facesIndices.Length; j++) {
					if (vertexCnt <= MAX_MESH_VERTICES - 2) {
						int[] facePointsIndices = m.faces [m.cells [cellIdx].facesIndices [j]].pointsIndices;
						for (int k = 0; k < facePointsIndices.Length; k++) {
							float[] coordinates = m.points [facePointsIndices [k]].coordinates;
							meshVertices.Add (new Vector3 (coordinates [0], coordinates [1], coordinates [2]));
							meshTriangles.Add (vertexCnt);
							vertexCnt += 1;
						}
					} else {
						vs.Add (meshVertices.ToArray ());
						ts.Add (meshTriangles.ToArray ());
						meshVertices = new List<Vector3> ();
						meshTriangles = new List<int> ();
						vertexCnt = 0;
					}
				}
			}
			// Remaining vertices and triangles
			if (vertexCnt > 0) {
				vs.Add (meshVertices.ToArray ());
				ts.Add (meshTriangles.ToArray ());
			}
			// Create and return Unity Meshes
			UnityEngine.Mesh[] unityMeshes = new UnityEngine.Mesh[vs.Count];
			for (int i = 0; i < unityMeshes.Length; i++) {
				unityMeshes [i] = new UnityEngine.Mesh ();
				unityMeshes [i].vertices = vs [i];
				unityMeshes [i].triangles = ts [i];
				unityMeshes [i].RecalculateBounds ();
				unityMeshes [i].RecalculateNormals ();
				unityMeshes [i].RecalculateTangents ();
			}
			return unityMeshes;
		};


		/// <summary>
		/// The mesh to unity mesh. Only 3 points faces!
		/// </summary>
		//		public static readonly Func<Scimesh.Base.Mesh, UnityEngine.Mesh[]> meshToUnityMesh = (m) => {
		//			// Unity Meshes vertices and triangles (with max vertices[i].Length == MAX_MESH_VERTICES)
		//			List<Vector3[]> vs = new List<> ();
		//			List<int[]> ts = new List<> ();
		//			List<int> meshTriangles = new List<> ();
		//			List<Vector3> meshVertices = new List<> ();
		//			int vertexCnt = 0;
		//			for (int i = 0; i < m.cells.Length; i++) {
		//				for (int j = 0; j < m.cells [i].facesIndices.Length; j++) {
		//					if (vertexCnt <= MAX_MESH_VERTICES - 2) {
		//						int[] facePointsIndices = m.faces [m.cells [m.cells [i]].facesIndices [j]].pointsIndices;
		//						for (int k = 0; k < facePointsIndices.Length; k++) {
		//							float[] coordinates = m.points [facePointsIndices [k]].coordinates;
		//							meshVertices.Add (new Vector3 (coordinates [0], coordinates [1], coordinates [2]));
		//							meshTriangles.Add (vertexCnt);
		//							vertexCnt += 1;
		//						}
		//					} else {
		//						vs.Add (meshVertices.ToArray ());
		//						ts.Add (meshTriangles.ToArray ());
		//						meshVertices = new List<> ();
		//						meshTriangles = new List<> ();
		//						vertexCnt = 0;
		//					}
		//				}
		//			}
		//			// Remaining vertices and triangles
		//			if (vertexCnt > 0) {
		//				vs.Add (meshVertices.ToArray ());
		//				ts.Add (meshTriangles.ToArray ());
		//			}
		//
		//			// Create and return Unity Meshes
		//			UnityEngine.Mesh[] unityMeshes = new UnityEngine.Mesh[vs.Count];
		//			for (int i = 0; i < unityMeshes.Length; i++) {
		//				unityMeshes [i] = new UnityEngine.Mesh ();
		//				unityMeshes [i].vertices = vs [i];
		//				unityMeshes [i].triangles = ts [i];
		//				unityMeshes [i].RecalculateBounds ();
		//				unityMeshes [i].RecalculateNormals ();
		//				unityMeshes [i].RecalculateTangents ();
		//			}
		//			return unityMeshes;
		//		};
		//
		//		/// <summary>
		//		/// The isocells to unity mesh.
		//		/// </summary>
		//		public static readonly Func<Scimesh.Base.Isocells, UnityEngine.Mesh[]> isocellsToUnityMesh = (ic) => {
		//			// Unity Meshes vertices and triangles (with max vertices[i].Length == MAX_MESH_VERTICES)
		//			List<Vector3[]> vs = new List<> ();
		//			List<int[]> ts = new List<> ();
		//			List<int> meshTriangles = new List<> ();
		//			List<Vector3> meshVertices = new List<> ();
		//			int vertexCnt = 0;
		//			for (int i = 0; i < ic.visibleCells.Length; i++) { // Convert only visible cells
		//				for (int j = 0; j < ic.mesh.cells [ic.visibleCells [i]].facesIndices.Length; j++) {
		//					if (vertexCnt > MAX_MESH_VERTICES - 3) {
		//						vs.Add (meshVertices.ToArray ());
		//						ts.Add (meshTriangles.ToArray ());
		//						meshVertices = new List<Vector3> ();
		//						meshTriangles = new List<int> ();
		//						vertexCnt = 0;
		//					}
		//					int[] facePointsIndices = ic.mesh.faces [ic.mesh.cells [ic.visibleCells [i]].facesIndices [j]].pointsIndices;
		//					for (int k = 0; k < facePointsIndices.Length; k++) {
		//						float[] coordinates = ic.mesh.points [facePointsIndices [k]].coordinates;
		//						meshVertices.Add (new Vector3 (coordinates [0], coordinates [1], coordinates [2]));
		//						meshTriangles.Add (vertexCnt);
		//						vertexCnt += 1;
		//					}
		//				}
		//			}
		//			// Remaining vertices and triangles
		//			if (vertexCnt > 0) {
		//				vs.Add (meshVertices.ToArray ());
		//				ts.Add (meshTriangles.ToArray ());
		//			}
		//
		//			// Create and return Unity Meshes
		//			UnityEngine.Mesh[] unityMeshes = new UnityEngine.Mesh[vs.Count];
		//			for (int i = 0; i < unityMeshes.Length; i++) {
		//				unityMeshes [i] = new UnityEngine.Mesh ();
		//				unityMeshes [i].vertices = vs [i];
		//				unityMeshes [i].triangles = ts [i];
		//				unityMeshes [i].RecalculateBounds ();
		//				unityMeshes [i].RecalculateNormals ();
		//				unityMeshes [i].RecalculateTangents ();
		//			}
		//			return unityMeshes;
		//		};
		//
		//		public static readonly Func<Scimesh.Base.Isocells, UnityEngine.Mesh[]> isocellsToUnityMesh2 = (ic) => {
		//			// Unity Meshes vertices and triangles (with max vertices[i].Length == MAX_MESH_VERTICES)
		//			List<Vector3[]> vs = new List<> ();
		//			List<int[]> ts = new List<> ();
		//			Dictionary <int, int> vsMap = new Dictionary<,> (); // Map Isocell vertex index to UnityMesh one
		//			List<int> meshTs = new List<> ();
		//			Vector3[] meshVs = new Vector3[0];
		//			int vertexCnt = 0;
		//			for (int i = 0; i < ic.visibleCells.Length; i++) { // Convert only visible cells
		//				for (int j = 0; j < ic.mesh.cells [ic.visibleCells [i]].facesIndices.Length; j++) {
		//					if (vertexCnt < MAX_MESH_VERTICES - 3) { // Fill meshVs and meshTs arrays
		//						int[] facePointsIndices = ic.mesh.faces [ic.mesh.cells [ic.visibleCells [i]].facesIndices [j]].pointsIndices;
		//						for (int k = 0; k < facePointsIndices.Length; k++) {
		//							if (!vsMap.ContainsKey (facePointsIndices [k])) {
		//								vsMap.Add (facePointsIndices [k], vertexCnt);
		//								vertexCnt += 1;
		//							}
		//							meshTs.Add (vsMap [facePointsIndices [k]]);
		//						}
		//					} else { // Add meshVs and meshTs to vs and ts respectively
		//						meshVs = new Vector3[vsMap.Count];
		//						foreach (KeyValuePair<int,int> entry in vsMap) {
		//							float[] coordinates = ic.mesh.points [entry.Key].coordinates;
		//							meshVs [entry.Value] = new Vector3 (coordinates [0], coordinates [1], coordinates [2]);
		//							;
		//						}
		//						vs.Add (meshVs);
		//						vsMap.Clear ();
		//						ts.Add (meshTs.ToArray ());
		//						meshTs = new List<> ();
		//						vertexCnt = 0;
		//					}
		//				}
		//			}
		//			if (vertexCnt > 0) { // Remaining vertices and triangles
		//				meshVs = new Vector3[vsMap.Count];
		//				foreach (KeyValuePair<int, int> entry in vsMap) {
		//					float[] coordinates = ic.mesh.points [entry.Key].coordinates;
		//					Vector3 vertex = new Vector3 (coordinates [0], coordinates [1], coordinates [2]);
		//					meshVs [entry.Value] = vertex;
		//				}
		//				vs.Add (meshVs);
		//				ts.Add (meshTs.ToArray ());
		//			}
		//			// Create and return Unity Meshes
		//			UnityEngine.Mesh[] unityMeshes = new UnityEngine.Mesh[vs.Count];
		//			for (int i = 0; i < unityMeshes.Length; i++) {
		//				unityMeshes [i] = new UnityEngine.Mesh ();
		//				unityMeshes [i].vertices = vs [i];
		//				unityMeshes [i].triangles = ts [i];
		//				unityMeshes [i].RecalculateBounds ();
		//				unityMeshes [i].RecalculateNormals ();
		//				unityMeshes [i].RecalculateTangents ();
		//			}
		//			return unityMeshes;
		//		};
		//
		//		public static UnityEngine.Mesh[] IsosurfaceToUnityMeshes (
		//			Scimesh.Base.Isocells isocells,
		//			Scimesh.Base.PointField colorFieldRGBA,
		//			int timeValueIndex,
		//			bool duplicateVerticesForTriangles
		//		)
		//		{
		//			// Unity Meshes vertices, triangles and colors (with max vertices[i].Length == MAX_MESH_VERTICES)
		//			List<Vector3[]> vertices = new List<Vector3[]> ();
		//			List<int[]> triangles = new List<int[]> ();
		//			List<UnityEngine.Color[]> colors = new List<UnityEngine.Color[]> ();
		//
		//			if (!duplicateVerticesForTriangles) {
		//				Dictionary <int, int> meshToUnityMeshVerticesMap = new Dictionary<int, int> ();
		//				List<int> meshTriangles = new List<int> ();
		//				Vector3[] meshVertices = new Vector3[0];
		//				UnityEngine.Color[] verticesColors = new UnityEngine.Color[0];
		//				int vertexCnt = 0;
		//				// Convert only visible mesh cell faces to unity mesh triangles
		//				for (int i = 0; i < isocells.visibleCells.Length; i++) {
		//					for (int j = 0; j < isocells.mesh.cells [isocells.visibleCells [i]].facesIndices.Length; j++) {
		//						if (vertexCnt > MAX_MESH_VERTICES - 3) {
		//							meshVertices = new Vector3[meshToUnityMeshVerticesMap.Count];
		//							verticesColors = new UnityEngine.Color[meshToUnityMeshVerticesMap.Count];
		//							foreach (KeyValuePair<int, int> entry in meshToUnityMeshVerticesMap) {
		//								// Point float[3] coordinates to Unity vertex Vector3 coordinates
		//								float[] coordinates = isocells.mesh.points [entry.Key].coordinates;
		//								Vector3 vertex = new Vector3 (coordinates [0], coordinates [1], coordinates [2]);
		//								meshVertices [entry.Value] = vertex;
		//								// float[3] rgba color to Unity vertex Color color
		//								float?[] color = colorFieldRGBA.GetValue (entry.Key, timeValueIndex);
		//								// If rgba color component is null => assign 0
		//								verticesColors [entry.Value] = new UnityEngine.Color (color [0] ?? 0, color [1] ?? 0, color [2] ?? 0, color [3] ?? 0);
		//							}
		//							vertices.Add (meshVertices);
		//							colors.Add (verticesColors);
		//							meshToUnityMeshVerticesMap.Clear ();
		//							triangles.Add (meshTriangles.ToArray ());
		//							meshTriangles = new List<int> ();
		//							vertexCnt = 0;
		//						}
		//						int[] facePointsIndices = isocells.mesh.faces [isocells.mesh.cells [isocells.visibleCells [i]].facesIndices [j]].pointsIndices;
		//						for (int k = 0; k < facePointsIndices.Length; k++) {
		//							if (!meshToUnityMeshVerticesMap.ContainsKey (facePointsIndices [k])) {
		//								meshToUnityMeshVerticesMap.Add (facePointsIndices [k], vertexCnt);
		//								vertexCnt += 1;
		//							}
		//							meshTriangles.Add (meshToUnityMeshVerticesMap [facePointsIndices [k]]);
		//						}
		//					}
		//				}
		//				// Remaining vertices and triangles
		//				if (vertexCnt > 0) {
		//					meshVertices = new Vector3[meshToUnityMeshVerticesMap.Count];
		//					verticesColors = new UnityEngine.Color[meshToUnityMeshVerticesMap.Count];
		//					foreach (KeyValuePair<int, int> entry in meshToUnityMeshVerticesMap) {
		//						float[] coordinates = isocells.mesh.points [entry.Key].coordinates;
		//						Vector3 vertex = new Vector3 (coordinates [0], coordinates [1], coordinates [2]);
		//						meshVertices [entry.Value] = vertex;
		//						float?[] color = colorFieldRGBA.GetValue (entry.Key, timeValueIndex);
		//						verticesColors [entry.Value] = new UnityEngine.Color (color [0] ?? 0, color [1] ?? 0, color [2] ?? 0, color [3] ?? 0);
		//					}
		//					vertices.Add (meshVertices);
		//					colors.Add (verticesColors);
		//					triangles.Add (meshTriangles.ToArray ());
		//				}
		//			} else {
		//				List<int> meshTriangles = new List<int> ();
		//				List<Vector3> meshVertices = new List<Vector3> ();
		//				List<UnityEngine.Color> verticesColors = new List<UnityEngine.Color> ();
		//				int vertexCnt = 0;
		//				// Convert only visible mesh cell faces to unity mesh triangles
		//				for (int i = 0; i < isocells.visibleCells.Length; i++) {
		//					for (int j = 0; j < isocells.mesh.cells [isocells.visibleCells [i]].facesIndices.Length; j++) {
		//						if (vertexCnt > MAX_MESH_VERTICES - 3) {
		//							vertices.Add (meshVertices.ToArray ());
		//							triangles.Add (meshTriangles.ToArray ());
		//							colors.Add (verticesColors.ToArray ());
		//							meshVertices = new List<Vector3> ();
		//							meshTriangles = new List<int> ();
		//							verticesColors = new List<UnityEngine.Color> ();
		//							vertexCnt = 0;
		//						}
		//						int[] facePointsIndices = isocells.mesh.faces [isocells.mesh.cells [isocells.visibleCells [i]].facesIndices [j]].pointsIndices;
		//						for (int k = 0; k < facePointsIndices.Length; k++) {
		//							float[] coordinates = isocells.mesh.points [facePointsIndices [k]].coordinates;
		//							meshVertices.Add (new Vector3 (coordinates [0], coordinates [1], coordinates [2]));
		//							float?[] color = colorFieldRGBA.GetValue (facePointsIndices [k], timeValueIndex);
		//							verticesColors.Add (new UnityEngine.Color (color [0] ?? 0, color [1] ?? 0, color [2] ?? 0, color [3] ?? 0));
		//							meshTriangles.Add (vertexCnt);
		//							vertexCnt += 1;
		//						}
		//					}
		//				}
		//				// Remaining vertices and triangles
		//				if (vertexCnt > 0) {
		//					vertices.Add (meshVertices.ToArray ());
		//					triangles.Add (meshTriangles.ToArray ());
		//					colors.Add (verticesColors.ToArray ());
		//				}
		//			}
		//			// Create and return Unity Meshes
		//			UnityEngine.Mesh[] unityMeshes = new UnityEngine.Mesh[vertices.Count];
		//			for (int i = 0; i < unityMeshes.Length; i++) {
		//				unityMeshes [i] = new UnityEngine.Mesh ();
		//				unityMeshes [i].vertices = vertices [i];
		//				unityMeshes [i].colors = colors [i];
		//				unityMeshes [i].triangles = triangles [i];
		//				unityMeshes [i].RecalculateBounds ();
		//				unityMeshes [i].RecalculateNormals ();
		//				unityMeshes [i].RecalculateTangents ();
		//			}
		//			return unityMeshes;
		//		}
	}
}
