using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

namespace Scimesh.Base.To
{
	public static class Unity
	{
		public static int MAX_MESH_VERTICES = 65534;

		public static UnityEngine.Mesh[] IsosurfaceToUnityMeshes (
			Scimesh.Base.Isocells isocells, 
			bool duplicateVerticesForTriangles
		)
		{
			// Unity Meshes vertices and triangles (with max vertices[i].Length == MAX_MESH_VERTICES)
			List<Vector3[]> vertices = new List<Vector3[]> ();
			List<int[]> triangles = new List<int[]> ();

			if (!duplicateVerticesForTriangles) {
				Dictionary <int, int> meshToUnityMeshVerticesMap = new Dictionary<int, int> ();
				List<int> meshTriangles = new List<int> ();
				Vector3[] meshVertices = new Vector3[0];
				int vertexCnt = 0;
				// Convert only visible mesh cell faces to unity mesh triangles
				for (int i = 0; i < isocells.visibleCells.Length; i++) {
					for (int j = 0; j < isocells.mesh.cells [isocells.visibleCells [i]].facesIndices.Length; j++) {
						if (vertexCnt > MAX_MESH_VERTICES - 3) { 
							meshVertices = new Vector3[meshToUnityMeshVerticesMap.Count];
							foreach (KeyValuePair<int, int> entry in meshToUnityMeshVerticesMap) {
								float[] coordinates = isocells.mesh.points [entry.Key].coordinates;
								Vector3 vertex = new Vector3 (coordinates [0], coordinates [1], coordinates [2]);
								meshVertices [entry.Value] = vertex;
							}
							vertices.Add (meshVertices);
							meshToUnityMeshVerticesMap.Clear ();
							triangles.Add (meshTriangles.ToArray ());
							meshTriangles = new List<int> ();
							vertexCnt = 0;
						}
						int[] facePointsIndices = isocells.mesh.faces [isocells.mesh.cells [isocells.visibleCells [i]].facesIndices [j]].pointsIndices;
						for (int k = 0; k < facePointsIndices.Length; k++) {
							if (!meshToUnityMeshVerticesMap.ContainsKey (facePointsIndices [k])) {
								meshToUnityMeshVerticesMap.Add (facePointsIndices [k], vertexCnt);
								vertexCnt += 1;
							}
							meshTriangles.Add (meshToUnityMeshVerticesMap [facePointsIndices [k]]);
						}
					}
				}
				// Remaining vertices and triangles
				if (vertexCnt > 0) {
					meshVertices = new Vector3[meshToUnityMeshVerticesMap.Count];
					foreach (KeyValuePair<int, int> entry in meshToUnityMeshVerticesMap) {
						float[] coordinates = isocells.mesh.points [entry.Key].coordinates;
						Vector3 vertex = new Vector3 (coordinates [0], coordinates [1], coordinates [2]);
						meshVertices [entry.Value] = vertex;
					}
					vertices.Add (meshVertices);
					triangles.Add (meshTriangles.ToArray ());
				}
			} else {
				List<int> meshTriangles = new List<int> ();
				List<Vector3> meshVertices = new List<Vector3> ();
				int vertexCnt = 0;
				// Convert only visible mesh cell faces to unity mesh triangles
				for (int i = 0; i < isocells.visibleCells.Length; i++) {
					for (int j = 0; j < isocells.mesh.cells [isocells.visibleCells [i]].facesIndices.Length; j++) {
						if (vertexCnt > MAX_MESH_VERTICES - 3) { 
							vertices.Add (meshVertices.ToArray ());
							triangles.Add (meshTriangles.ToArray ());
							meshVertices = new List<Vector3> ();
							meshTriangles = new List<int> ();
							vertexCnt = 0;
						}
						int[] facePointsIndices = isocells.mesh.faces [isocells.mesh.cells [isocells.visibleCells [i]].facesIndices [j]].pointsIndices;
						for (int k = 0; k < facePointsIndices.Length; k++) {
							float[] coordinates = isocells.mesh.points [facePointsIndices [k]].coordinates;
							meshVertices.Add (new Vector3 (coordinates [0], coordinates [1], coordinates [2]));
							meshTriangles.Add (vertexCnt);
							vertexCnt += 1;
						}
					}
				}
				// Remaining vertices and triangles
				if (vertexCnt > 0) {
					vertices.Add (meshVertices.ToArray ());
					triangles.Add (meshTriangles.ToArray ());
				}
			}	
			// Create and return Unity Meshes
			UnityEngine.Mesh[] unityMeshes = new UnityEngine.Mesh[vertices.Count];
			for (int i = 0; i < unityMeshes.Length; i++) {
				unityMeshes [i] = new UnityEngine.Mesh ();
				unityMeshes [i].vertices = vertices [i];
				unityMeshes [i].triangles = triangles [i];
				unityMeshes [i].RecalculateBounds ();
				unityMeshes [i].RecalculateNormals ();
				unityMeshes [i].RecalculateTangents ();
			}
			return unityMeshes;
		}

		public static UnityEngine.Mesh[] IsosurfaceToUnityMeshes (
			Scimesh.Base.Isocells isocells, 
			Scimesh.Base.PointField colorFieldRGBA, 
			int timeValueIndex,
			bool duplicateVerticesForTriangles
		)
		{
			// Unity Meshes vertices, triangles and colors (with max vertices[i].Length == MAX_MESH_VERTICES)
			List<Vector3[]> vertices = new List<Vector3[]> ();
			List<int[]> triangles = new List<int[]> ();
			List<UnityEngine.Color[]> colors = new List<UnityEngine.Color[]> ();

			if (!duplicateVerticesForTriangles) {
				Dictionary <int, int> meshToUnityMeshVerticesMap = new Dictionary<int, int> ();
				List<int> meshTriangles = new List<int> ();
				Vector3[] meshVertices = new Vector3[0];
				UnityEngine.Color[] verticesColors = new UnityEngine.Color[0];
				int vertexCnt = 0;
				// Convert only visible mesh cell faces to unity mesh triangles
				for (int i = 0; i < isocells.visibleCells.Length; i++) {
					for (int j = 0; j < isocells.mesh.cells [isocells.visibleCells [i]].facesIndices.Length; j++) {
						if (vertexCnt > MAX_MESH_VERTICES - 3) {
							meshVertices = new Vector3[meshToUnityMeshVerticesMap.Count];
							verticesColors = new UnityEngine.Color[meshToUnityMeshVerticesMap.Count];
							foreach (KeyValuePair<int, int> entry in meshToUnityMeshVerticesMap) {
								// Point float[3] coordinates to Unity vertex Vector3 coordinates
								float[] coordinates = isocells.mesh.points [entry.Key].coordinates;
								Vector3 vertex = new Vector3 (coordinates [0], coordinates [1], coordinates [2]);
								meshVertices [entry.Value] = vertex;
								// float[3] rgba color to Unity vertex Color color
								float?[] color = colorFieldRGBA.GetValue (entry.Key, timeValueIndex);
								// If rgba color component is null => assign 0
								verticesColors [entry.Value] = new UnityEngine.Color (color [0] ?? 0, color [1] ?? 0, color [2] ?? 0, color [3] ?? 0);
							}
							vertices.Add (meshVertices);
							colors.Add (verticesColors);
							meshToUnityMeshVerticesMap.Clear ();
							triangles.Add (meshTriangles.ToArray ());
							meshTriangles = new List<int> ();
							vertexCnt = 0;
						}
						int[] facePointsIndices = isocells.mesh.faces [isocells.mesh.cells [isocells.visibleCells [i]].facesIndices [j]].pointsIndices;
						for (int k = 0; k < facePointsIndices.Length; k++) {
							if (!meshToUnityMeshVerticesMap.ContainsKey (facePointsIndices [k])) {
								meshToUnityMeshVerticesMap.Add (facePointsIndices [k], vertexCnt);
								vertexCnt += 1;
							}
							meshTriangles.Add (meshToUnityMeshVerticesMap [facePointsIndices [k]]);
						}
					}
				}
				// Remaining vertices and triangles
				if (vertexCnt > 0) {
					meshVertices = new Vector3[meshToUnityMeshVerticesMap.Count];
					verticesColors = new UnityEngine.Color[meshToUnityMeshVerticesMap.Count];
					foreach (KeyValuePair<int, int> entry in meshToUnityMeshVerticesMap) {
						float[] coordinates = isocells.mesh.points [entry.Key].coordinates;
						Vector3 vertex = new Vector3 (coordinates [0], coordinates [1], coordinates [2]);
						meshVertices [entry.Value] = vertex;
						float?[] color = colorFieldRGBA.GetValue (entry.Key, timeValueIndex);
						verticesColors [entry.Value] = new UnityEngine.Color (color [0] ?? 0, color [1] ?? 0, color [2] ?? 0, color [3] ?? 0);
					}
					vertices.Add (meshVertices);
					colors.Add (verticesColors);
					triangles.Add (meshTriangles.ToArray ());
				}
			} else {
				List<int> meshTriangles = new List<int> ();
				List<Vector3> meshVertices = new List<Vector3> ();
				List<UnityEngine.Color> verticesColors = new List<UnityEngine.Color> ();
				int vertexCnt = 0;
				// Convert only visible mesh cell faces to unity mesh triangles
				for (int i = 0; i < isocells.visibleCells.Length; i++) {
					for (int j = 0; j < isocells.mesh.cells [isocells.visibleCells [i]].facesIndices.Length; j++) {
						if (vertexCnt > MAX_MESH_VERTICES - 3) { 
							vertices.Add (meshVertices.ToArray ());
							triangles.Add (meshTriangles.ToArray ());
							colors.Add (verticesColors.ToArray ());
							meshVertices = new List<Vector3> ();
							meshTriangles = new List<int> ();
							verticesColors = new List<UnityEngine.Color> ();
							vertexCnt = 0;
						}
						int[] facePointsIndices = isocells.mesh.faces [isocells.mesh.cells [isocells.visibleCells [i]].facesIndices [j]].pointsIndices;
						for (int k = 0; k < facePointsIndices.Length; k++) {
							float[] coordinates = isocells.mesh.points [facePointsIndices [k]].coordinates;
							meshVertices.Add (new Vector3 (coordinates [0], coordinates [1], coordinates [2]));
							float?[] color = colorFieldRGBA.GetValue (facePointsIndices [k], timeValueIndex);
							verticesColors.Add (new UnityEngine.Color (color [0] ?? 0, color [1] ?? 0, color [2] ?? 0, color [3] ?? 0));
							meshTriangles.Add (vertexCnt);
							vertexCnt += 1;
						}
					}
				}
				// Remaining vertices and triangles
				if (vertexCnt > 0) {
					vertices.Add (meshVertices.ToArray ());
					triangles.Add (meshTriangles.ToArray ());
					colors.Add (verticesColors.ToArray ());
				}
			}	
			// Create and return Unity Meshes
			UnityEngine.Mesh[] unityMeshes = new UnityEngine.Mesh[vertices.Count];
			for (int i = 0; i < unityMeshes.Length; i++) {
				unityMeshes [i] = new UnityEngine.Mesh ();
				unityMeshes [i].vertices = vertices [i];
				unityMeshes [i].colors = colors [i];
				unityMeshes [i].triangles = triangles [i];
				unityMeshes [i].RecalculateBounds ();
				unityMeshes [i].RecalculateNormals ();
				unityMeshes [i].RecalculateTangents ();
			}
			return unityMeshes;
		}
	}
}
