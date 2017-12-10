using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

namespace Scimesh.Unity.To
{
	public static class Base
	{
		public static Scimesh.Base.Mesh UnityMeshToMesh (UnityEngine.Mesh unityMesh)
		{
			Scimesh.Base.Point[] points = new Scimesh.Base.Point[unityMesh.vertices.Length];
			Scimesh.Base.Face[] faces = new Scimesh.Base.Face[unityMesh.triangles.Length / 3];
			Scimesh.Base.Cell[] cells = new Scimesh.Base.Cell[unityMesh.triangles.Length / 3];

			Stopwatch stopwatch = Stopwatch.StartNew ();
			// Convert unity mesh vertices to mesh points
			// Creating copy of unityMesh.vertices[] for performance goals
			Vector3[] vertices = unityMesh.vertices; 
			for (int i = 0; i < points.Length; i++) {
				float[] coordinates = new float[] {
					vertices [i].x,
					vertices [i].y,
					vertices [i].z
				};
				points [i] = new Scimesh.Base.Point (coordinates);
			}
			stopwatch.Stop ();
			UnityEngine.Debug.Log ("Points Import Time:" + stopwatch.ElapsedMilliseconds);

			stopwatch = Stopwatch.StartNew ();
			// Convert unity mesh triangles to mesh faces
			// Creating copy of unityMesh.triangles[] for performance goals
			int[] triangles = unityMesh.triangles;
			for (int i = 0; i < faces.Length; i++) {
				int[] pointsIndices = new int[] {
					triangles [3 * i],
					triangles [3 * i + 1],
					triangles [3 * i + 2]
				};
				faces [i] = new Scimesh.Base.Face (pointsIndices);
			}
			stopwatch.Stop ();
			UnityEngine.Debug.Log ("Faces Import Time:" + stopwatch.ElapsedMilliseconds);

			stopwatch = Stopwatch.StartNew ();
			// Convert unity mesh triangles to mesh cells
			// Creating copy of unityMesh.triangles[] for performance goals
//			int[] triangles = unityMesh.triangles;
			for (int i = 0; i < cells.Length; i++) {
				int[] pointsIndices = new int[] {
					triangles [3 * i],
					triangles [3 * i + 1],
					triangles [3 * i + 2]
				};
				int[] facesIndices = new int[] { i }; // In unity mesh: cell == face
				cells [i] = new Scimesh.Base.Cell (pointsIndices, facesIndices);
			}
			stopwatch.Stop ();
			UnityEngine.Debug.Log ("Cells Import Time:" + stopwatch.ElapsedMilliseconds);

			// Create and return mesh
			Scimesh.Base.Mesh mesh = new Scimesh.Base.Mesh (points, faces, cells);
			stopwatch = Stopwatch.StartNew ();
			mesh.EvaluateCellsNeighbourCells ();
			mesh.EvaluatePointsNeighbourPoints (Scimesh.Base.Mesh.Neighbours.InFaces);
			stopwatch.Stop ();
			UnityEngine.Debug.Log ("Neighbours Evaluating Time:" + stopwatch.ElapsedMilliseconds);
			// Log
//			foreach (Point point in mesh.points) {
//				Debug.Log (string.Format ("x:{0:G}, y:{1:G}, z:{2:G}", 
//					point.coordinates [0], 
//					point.coordinates [1], 
//					point.coordinates [2]
//				));
//			}
//			for (int i = 0; i < mesh.cells.Length; i++) {
//				Debug.Log (string.Format ("cell:{0:G}, neighbourCellsIndices.Length:{1:G}", 
//					i,
//					mesh.cells[i].neighbourCellsIndices.Length) 
//				);
//			}
//			for (int i = 0; i < mesh.points.Length; i++) {
//				UnityEngine.Debug.Log (string.Format ("point:{0:G}, neighbourPointsIndices.Length:{1:G}", 
//					i,
//					mesh.points[i].neighbourPointsIndices.Length) 
//				);
//			}
			return mesh;
		}
	}
}
