using System.Collections.Generic;
using System.IO;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Scimesh.Vtk.To
{
	public static class Base
	{
		/// <summary>
		/// VTK Cell Type to Unity Triangles map.
		/// </summary>
		public static readonly Dictionary<int, int[]> cellTypeToTrianglesMap;

		public static readonly Dictionary<int, int> polygonTypeToCellType;

		static Base ()
		{
			cellTypeToTrianglesMap = new Dictionary<int, int[]> ();
            // VTK_LINE TODO by line renderer?
            cellTypeToTrianglesMap.Add(3, new int[] {              
            });
            // VTK_POLY_LINE TODO by line renderer?
            cellTypeToTrianglesMap.Add(4, new int[] { 
            });
            // VTK_TRIANGLE
            cellTypeToTrianglesMap.Add (5, new int[] {
				0, 1, 2
			});
			// VTK_QUAD
			cellTypeToTrianglesMap.Add (9, new int[] {
				0, 1, 2,
				2, 3, 0
			});
			// VTK_TETRA
			cellTypeToTrianglesMap.Add (10, new int[] {
				0, 1, 2,
				0, 3, 1,
				1, 3, 2,
				2, 3, 0
			});
			// VTK_HEXAHEDRON
			cellTypeToTrianglesMap.Add (12, new int[] {
				0, 1, 2,
				2, 3, 0,
				0, 4, 5,
				5, 1, 0,
				1, 5, 6,
				6, 2, 1,
				2, 6, 7,
				7, 3, 2,
				3, 7, 4,
				4, 0, 3,
				4, 7, 6,
				6, 5, 4
			});
			// VTK_WEDGE
			cellTypeToTrianglesMap.Add (13, new int[] {
				0, 2, 1,
				3, 4, 5,
				1, 4, 3,
				1, 3, 0,
				2, 0, 5,
				0, 3, 5,
				1, 2, 4,
				2, 5, 4
			});
			// VTK_PYRAMID
			cellTypeToTrianglesMap.Add (14, new int[] {
				0, 1, 2,
				0, 2, 3,
				0, 3, 4,
				0, 4, 1,
				1, 4, 2,
				2, 4, 3
			});
			polygonTypeToCellType = new Dictionary<int, int> ();
			polygonTypeToCellType.Add (4, 9);
		}

		public static readonly Func<string, Scimesh.Base.Mesh> polydataToMesh = (filename) => {
			int[] intData = new int[0];
			float[] floatData = new float[0];
			Scimesh.Base.Point[] points = new Scimesh.Base.Point[0];
			bool isPoints = false;
			Scimesh.Base.Cell[] cells = new Scimesh.Base.Cell[0];
			bool isCells = false;
			int[] cellTypes = new int[0];
			int cnt = 0;
			using (StreamReader sr = new StreamReader (filename)) {
				string line;
				while ((line = sr.ReadLine ()) != null) {
					if (!string.IsNullOrEmpty (line)) {
						string[] tokens = line.Split ();
						// Check flags
						if (tokens [0] == "POINTS") {
							UnityEngine.Debug.Log ("Reading POINTS: " + tokens [1]);
							isPoints = true;
							points = new Scimesh.Base.Point[int.Parse (tokens [1])];
							floatData = new float[points.Length * 3];
							cnt = 0;
							continue;
						} else if (tokens [0] == "POLYGONS") {
							UnityEngine.Debug.Log ("Reading CELLS " + tokens [1] + " " + tokens [2]);
							isPoints = false;
							isCells = true;
							cells = new Scimesh.Base.Cell[int.Parse (tokens [1])];
							cellTypes = new int[cells.Length];
							intData = new int[int.Parse (tokens [2])];
							cnt = 0;
							continue;
						}
						// Parse data
						if (isPoints) {
							for (int i = 0; i < tokens.Length; i++) {
								if (!string.IsNullOrEmpty (tokens [i])) {
									floatData [cnt] = float.Parse (tokens [i]);
									cnt += 1;
									if (cnt == floatData.Length) {
										List<int> types = new List<int> ();
										int pointsCnt = 0;
										int j = 0;
										while (j < floatData.Length) {
											// Map X -> X, Y -> Z, Z -> Y (Unity Y is VTK Z)
											points [pointsCnt] = new Scimesh.Base.Point (new float[] {
												floatData [j], floatData [j + 2], floatData [j + 1]
											});
											pointsCnt += 1;
											j += 3;
										}
										isPoints = false;
										break;
									}
								}
							}
						} else if (isCells) {
							for (int i = 0; i < tokens.Length; i++) {
								if (!string.IsNullOrEmpty (tokens [i])) {
									intData [cnt] = int.Parse (tokens [i]);
									cnt += 1;
									if (cnt == intData.Length) {
										int cellsCnt = 0;
										int j = 0;
										while (j < intData.Length) {
											int[] pointsIndices = new int[intData [j]];
											for (int k = 0; k < pointsIndices.Length; k++) {
												pointsIndices [k] = intData [j + 1 + k];
											}
											cells [cellsCnt] = new Scimesh.Base.Cell (pointsIndices);
											cellTypes[cellsCnt] = polygonTypeToCellType[intData[j]];
											cellsCnt += 1;
											j += 1 + intData [j];
										}
										isCells = false;
										break;
									}
								}
							}
						}
					}
				}
			}
			// Faces
			List<Scimesh.Base.Face> faces = new List<Scimesh.Base.Face> ();
			for (int i = 0; i < cellTypes.Length; i++) {
				int[] triangles = cellTypeToTrianglesMap [cellTypes [i]];
				int nFaces = triangles.Length / 3;
				Scimesh.Base.Cell cell = cells [i];
				cell.facesIndices = new int[nFaces];
				for (int j = 0; j < nFaces; j++) {
					int[] pointsIndices = new int[] {
						cell.pointsIndices [triangles [3 * j]],
						cell.pointsIndices [triangles [3 * j + 1]],
						cell.pointsIndices [triangles [3 * j + 2]]
					};
					faces.Add (new Scimesh.Base.Face (pointsIndices));
					cell.facesIndices [j] = faces.Count - 1;
				}
			}
			UnityEngine.Debug.Log (points.Length);
			UnityEngine.Debug.Log (faces.Count);
			UnityEngine.Debug.Log (cells.Length);
			Scimesh.Base.Mesh mesh = new Scimesh.Base.Mesh (points, faces.ToArray (), cells);
			return mesh;
		};

		/// <summary>
		/// Read .vtk file from project base folder.
		/// </summary>
		public static readonly Func<string, Scimesh.Base.Mesh> unstructuredGridToMesh = (filename) => {
			int[] intData = new int[0];
			float[] floatData = new float[0];
			Scimesh.Base.Point[] points = new Scimesh.Base.Point[0];
			bool isPoints = false;
			Scimesh.Base.Cell[] cells = new Scimesh.Base.Cell[0];
			bool isCells = false;
			int[] cellTypes = new int[0];
			bool isCellTypes = false;
			int cnt = 0;
			using (StreamReader sr = new StreamReader (filename)) {
				string line;
				while ((line = sr.ReadLine ()) != null) {
					if (!string.IsNullOrEmpty (line)) {
						string[] tokens = line.Split ();
						if (tokens [0] == "POINTS") {
//							UnityEngine.Debug.Log ("Reading POINTS: " + tokens [1]);
							isPoints = true;
							points = new Scimesh.Base.Point[int.Parse (tokens [1])];
							floatData = new float[points.Length * 3];
							cnt = 0;
							continue;
						} else if (tokens [0] == "CELLS") {
//							UnityEngine.Debug.Log ("Reading CELLS " + tokens [1] + " " + tokens [2]);
							isPoints = false;
							isCells = true;
							cells = new Scimesh.Base.Cell[int.Parse (tokens [1])];
							intData = new int[int.Parse (tokens [2])];
							cnt = 0;
							continue;
						} else if (tokens [0] == "CELL_TYPES") {
//							UnityEngine.Debug.Log ("Reading CELL_TYPES " + tokens [1]);
							isCells = false;
							isCellTypes = true;
							cellTypes = new int[int.Parse (tokens [1])];
							intData = new int[cellTypes.Length];
							cnt = 0;
							continue;
						}
						if (isPoints) {
							for (int i = 0; i < tokens.Length; i++) {
								if (!string.IsNullOrEmpty (tokens [i])) {
									floatData [cnt] = float.Parse (tokens [i]);
									cnt += 1;
									if (cnt == floatData.Length) {
										int pointsCnt = 0;
										int j = 0;
										while (j < floatData.Length) {
											// Map X -> X, Y -> Z, Z -> Y (Unity Y is VTK Z)
											points [pointsCnt] = new Scimesh.Base.Point (new float[] {
												floatData [j], floatData [j + 2], floatData [j + 1]
											});
											pointsCnt += 1;
											j += 3;
										}
										isPoints = false;
										break;
									}
								}
							}
						} else if (isCells) {
							for (int i = 0; i < tokens.Length; i++) {
								if (!string.IsNullOrEmpty (tokens [i])) {
									intData [cnt] = int.Parse (tokens [i]);
									cnt += 1;
									if (cnt == intData.Length) {
										int cellsCnt = 0;
										int j = 0;
										while (j < intData.Length) {
											int[] pointsIndices = new int[intData [j]];
											for (int k = 0; k < pointsIndices.Length; k++) {
												pointsIndices [k] = intData [j + 1 + k];
											}
											cells [cellsCnt] = new Scimesh.Base.Cell (pointsIndices);
											cellsCnt += 1;
											j += 1 + pointsIndices.Length;
										}
										isCells = false;
										break;
									}
								}
							}
						} else if (isCellTypes) {
							for (int i = 0; i < tokens.Length; i++) {
								if (!string.IsNullOrEmpty (tokens [i])) {
									intData [cnt] = int.Parse (tokens [i]);
									cnt += 1;
									if (cnt == intData.Length) {
										for (int j = 0; j < intData.Length; j++) {
											cellTypes [j] = intData [j];
										}
										isCellTypes = false;
										break;
									}
								}
							}
						}
					}
				}
			}
			// Faces
			List<Scimesh.Base.Face> faces = new List<Scimesh.Base.Face> ();
			for (int i = 0; i < cellTypes.Length; i++) {
				int[] triangles = cellTypeToTrianglesMap [cellTypes [i]];
				int nFaces = triangles.Length / 3;
				Scimesh.Base.Cell cell = cells [i];
				cell.facesIndices = new int[nFaces];
				for (int j = 0; j < nFaces; j++) {
					int[] pointsIndices = new int[] {
						cell.pointsIndices [triangles [3 * j]],
						cell.pointsIndices [triangles [3 * j + 1]],
						cell.pointsIndices [triangles [3 * j + 2]]
					};
					faces.Add (new Scimesh.Base.Face (pointsIndices));
					cell.facesIndices [j] = faces.Count - 1;
				}
			}
			// Mesh
			Scimesh.Base.Mesh mesh = new Scimesh.Base.Mesh (points, faces.ToArray (), cells);
			return mesh;
		};

		/// <summary>
		/// Read .txt file from Resources folder (filename without .txt extension).
		/// </summary>
		public static readonly Func<string, Scimesh.Base.Mesh> unstructuredGridToMeshResources = (filename) => {
			int[] intData = new int[0];
			float[] floatData = new float[0];
			Scimesh.Base.Point[] points = new Scimesh.Base.Point[0];
			bool isPoints = false;
			Scimesh.Base.Cell[] cells = new Scimesh.Base.Cell[0];
			bool isCells = false;
			int[] cellTypes = new int[0];
			bool isCellTypes = false;
			int cnt = 0;
			TextAsset textAsset = Resources.Load (filename, typeof(TextAsset)) as TextAsset;
//			UnityEngine.Debug.Log (textAsset.text);
			string[] lines = textAsset.text.Split ("\n" [0]);
			foreach (string line in lines) {
				if (!string.IsNullOrEmpty (line)) {
					string[] tokens = line.Split ();
					if (tokens [0] == "POINTS") {
//						UnityEngine.Debug.Log ("Reading POINTS: " + tokens [1]);
						isPoints = true;
						points = new Scimesh.Base.Point[int.Parse (tokens [1])];
						floatData = new float[points.Length * 3];
						cnt = 0;
						continue;
					} else if (tokens [0] == "CELLS") {
//						UnityEngine.Debug.Log ("Reading CELLS " + tokens [1] + " " + tokens [2]);
						isPoints = false;
						isCells = true;
						cells = new Scimesh.Base.Cell[int.Parse (tokens [1])];
						intData = new int[int.Parse (tokens [2])];
						cnt = 0;
						continue;
					} else if (tokens [0] == "CELL_TYPES") {
//						UnityEngine.Debug.Log ("Reading CELL_TYPES " + tokens [1]);
						isCells = false;
						isCellTypes = true;
						cellTypes = new int[int.Parse (tokens [1])];
						intData = new int[cellTypes.Length];
						cnt = 0;
						continue;
					}
					if (isPoints) {
						for (int i = 0; i < tokens.Length; i++) {
							if (!string.IsNullOrEmpty (tokens [i])) {
								floatData [cnt] = float.Parse (tokens [i]);
								cnt += 1;
								if (cnt == floatData.Length) {
									int pointsCnt = 0;
									int j = 0;
									while (j < floatData.Length) {
										// Map X -> X, Y -> Z, Z -> Y (Unity Y is VTK Z)
										points [pointsCnt] = new Scimesh.Base.Point (new float[] {
											floatData [j], floatData [j + 2], floatData [j + 1]
										});
										pointsCnt += 1;
										j += 3;
									}
									isPoints = false;
									break;
								}
							}
						}
					} else if (isCells) {
						for (int i = 0; i < tokens.Length; i++) {
							if (!string.IsNullOrEmpty (tokens [i])) {
								intData [cnt] = int.Parse (tokens [i]);
								cnt += 1;
								if (cnt == intData.Length) {
									int cellsCnt = 0;
									int j = 0;
									while (j < intData.Length) {
										int[] pointsIndices = new int[intData [j]];
										for (int k = 0; k < pointsIndices.Length; k++) {
											pointsIndices [k] = intData [j + 1 + k];
										}
										cells [cellsCnt] = new Scimesh.Base.Cell (pointsIndices);
										cellsCnt += 1;
										j += 1 + pointsIndices.Length;
									}
									isCells = false;
									break;
								}
							}
						}
					} else if (isCellTypes) {
						for (int i = 0; i < tokens.Length; i++) {
							if (!string.IsNullOrEmpty (tokens [i])) {
								intData [cnt] = int.Parse (tokens [i]);
								cnt += 1;
								if (cnt == intData.Length) {
									for (int j = 0; j < intData.Length; j++) {
										cellTypes [j] = intData [j];
									}
									isCellTypes = false;
									break;
								}
							}
						}
					}
				}
			}
			// Faces
			List<Scimesh.Base.Face> faces = new List<Scimesh.Base.Face> ();
			for (int i = 0; i < cellTypes.Length; i++) {
				int[] triangles = cellTypeToTrianglesMap [cellTypes [i]];
				int nFaces = triangles.Length / 3;
				Scimesh.Base.Cell cell = cells [i];
				cell.facesIndices = new int[nFaces];
				for (int j = 0; j < nFaces; j++) {
					int[] pointsIndices = new int[] {
						cell.pointsIndices [triangles [3 * j]],
						cell.pointsIndices [triangles [3 * j + 1]],
						cell.pointsIndices [triangles [3 * j + 2]]
					};
					faces.Add (new Scimesh.Base.Face (pointsIndices));
					cell.facesIndices [j] = faces.Count - 1;
				}
			}
			// Mesh
			Scimesh.Base.Mesh mesh = new Scimesh.Base.Mesh (points, faces.ToArray (), cells);
			return mesh;
		};

		/// <summary>
		/// Read .vtk file from project base folder.
		/// The unstructured grid to point fields. 
		/// </summary>
		public static readonly Func<string, Scimesh.Base.Mesh, List<Scimesh.Base.MeshPointField>> unstructuredGridToPointFields = (filename, m) => {
			List<List<float?>> fieldsData = new List<List<float?>> ();
			List<int> fieldsNComponents = new List<int> ();
			List<string> fieldsNames = new List<string> ();
			int fieldDataSize = 0;
			int nPoints = 0;
			bool isPointData = false;
			bool isField = false;
			List<float?> fieldData = new List<float?> ();
			try {
				using (StreamReader sr = new StreamReader (filename)) {
					string line;
					while ((line = sr.ReadLine ()) != null) {
						if (!string.IsNullOrEmpty (line)) {
							string[] tokens = line.Split ();
							if (tokens [0] == "POINTS") {
								UnityEngine.Debug.Log ("Reading POINTS: " + tokens [1]);
								nPoints = int.Parse (tokens [1]);
								continue;
							} else if (tokens [0] == "POINT_DATA") {
								UnityEngine.Debug.Log ("Reading POINT_DATA " + tokens [1]);
								nPoints = int.Parse (tokens [1]);
								isPointData = true;
								continue;
							}
							if (isPointData) {
								if (tokens [0] == "SCALARS") {
									isField = true;
									fieldData = new List<float?> ();
									fieldsNames.Add (tokens [1]);
									int nComponents = int.Parse (tokens [3]);
									fieldsNComponents.Add (nComponents);
									fieldDataSize = nPoints * nComponents;
								} else if (tokens [0] == "LOOKUP_TABLE") {
									continue;
								} else if (isField) {
									for (int j = 0; j < tokens.Length; j++) {
										if (!string.IsNullOrEmpty (tokens [j])) {
											fieldData.Add (float.Parse (tokens [j]));
											if (fieldData.Count == fieldDataSize) {
												fieldsData.Add (fieldData);
												isField = false;
												continue;
											}
										}
									}
								}
							}
						}
					}
				}
			} catch (Exception e) {
				UnityEngine.Debug.Log (string.Format ("Error reading {0}: {1} {2}", filename, e.Message, e.StackTrace));
			}
			List<Scimesh.Base.MeshPointField> pointFields = new List<Scimesh.Base.MeshPointField> ();
			for (int i = 0; i < fieldsNames.Count; i++) {
				pointFields.Add (new Scimesh.Base.MeshPointField (
					fieldsNames [i], fieldsNComponents [i], fieldsData [i].Count, fieldsData [i].ToArray (), m));
			}
			return pointFields;
		};
			
		/// <summary>
		/// The unstructured grid to cell fields.
		/// </summary>
		public static readonly Func<string, Scimesh.Base.Mesh, List<Scimesh.Base.MeshCellField>> unstructuredGridToCellFields = (filename, m) => {
			List<List<float?>> fieldsData = new List<List<float?>> ();
			List<int> fieldsNComponents = new List<int> ();
			List<string> fieldsNames = new List<string> ();
			int fieldDataSize = 0;
			int nCells = 0;
			bool isCellData = false;
			bool isField = false;
			List<float?> fieldData = new List<float?> ();
			try {
				using (StreamReader sr = new StreamReader (filename)) {
					string line;
					while ((line = sr.ReadLine ()) != null) {
						if (!string.IsNullOrEmpty (line)) {
							string[] tokens = line.Split ();
							if (tokens [0] == "CELL_DATA") {
								UnityEngine.Debug.Log ("Reading CELL_DATA " + tokens [1]);
								nCells = int.Parse (tokens [1]);
								isCellData = true;
								continue;
							}
							if (isCellData) {
								if (tokens [0] == "SCALARS") {
									isField = true;
									fieldData = new List<float?> ();
									fieldsNames.Add (tokens [1]);
									int nComponents = int.Parse (tokens [3]);
									fieldsNComponents.Add (nComponents);
									fieldDataSize = nCells * nComponents;
								} else if (tokens [0] == "LOOKUP_TABLE") {
									continue;
								} else if (isField) {
									for (int j = 0; j < tokens.Length; j++) {
										if (!string.IsNullOrEmpty (tokens [j])) {
											fieldData.Add (float.Parse (tokens [j]));
											if (fieldData.Count == fieldDataSize) {
												fieldsData.Add (fieldData);
												isField = false;
												continue;
											}
										}
									}
								}
							}
						}
					}
				}
			} catch (Exception e) {
				UnityEngine.Debug.Log (string.Format ("Error reading {0}: {1} {2}", filename, e.Message, e.StackTrace));
			}
			List<Scimesh.Base.MeshCellField> cellFields = new List<Scimesh.Base.MeshCellField> ();
			for (int i = 0; i < fieldsNames.Count; i++) {
				cellFields.Add (new Scimesh.Base.MeshCellField (
					fieldsNames [i], fieldsNComponents [i], fieldsData [i].Count, fieldsData [i].ToArray (), m));
			}
			return cellFields;
		};



		//		public static List<Scimesh.Base.CellField> VtkUnstructuredGridToCellFieldsResources (string filename)
		//		{
		//			List<List<float?>> fieldsData = new List<List<float?>> ();
		//			List<float?> fieldData = new List<float?> ();
		//			List<int> fieldsNComponents = new List<int> ();
		//			List<string> fieldsNames = new List<string> ();
		//			int fieldDataSize = 0;
		//			int nCells = 0;
		//			bool isCellData = false;
		//			bool isField = false;
		//			TextAsset textAsset = Resources.Load (filename, typeof(TextAsset)) as TextAsset;
		////			UnityEngine.Debug.Log (textAsset.text);
		//			string[] lines = textAsset.text.Split ("\n" [0]);
		//			foreach (string line in lines) {
		//				if (!string.IsNullOrEmpty (line)) {
		//					string[] tokens = line.Split ();
		//					if (tokens [0] == "CELL_DATA") {
		//						UnityEngine.Debug.Log ("Reading CELL_DATA " + tokens [1]);
		//						nCells = int.Parse (tokens [1]);
		//						isCellData = true;
		//						continue;
		//					}
		//					if (isCellData) {
		//						if (tokens [0] == "SCALARS") {
		//							isField = true;
		//							fieldData = new List<float?> ();
		//							fieldsNames.Add (tokens [1]);
		//							int nComponents = int.Parse (tokens [3]);
		//							fieldsNComponents.Add (nComponents);
		//							fieldDataSize = nCells * nComponents;
		//						} else if (tokens [0] == "LOOKUP_TABLE") {
		//							continue;
		//						} else if (isField) {
		//							for (int j = 0; j < tokens.Length; j++) {
		//								if (!string.IsNullOrEmpty (tokens [j])) {
		//									fieldData.Add (float.Parse (tokens [j]));
		//									if (fieldData.Count == fieldDataSize) {
		//										fieldsData.Add (fieldData);
		//										isField = false;
		//										continue;
		//									}
		//								}
		//							}
		//						}
		//					}
		//				}
		//			}
		//			//			for (int i = 0; i < fieldsNames.Count; i++) {
		//			//				UnityEngine.Debug.Log (i);
		//			//				UnityEngine.Debug.Log (fieldsNames [i]);
		//			//				UnityEngine.Debug.Log (fieldsNComponents [i]);
		//			//				UnityEngine.Debug.Log (fieldsData [i].Count);
		//			//			}
		//			Scimesh.Base.Mesh mesh = Scimesh.Vtk.To.Base.UnstructuredGridToMeshResources (filename);
		//			List<Scimesh.Base.CellField> cellFields = new List<Scimesh.Base.CellField> ();
		//			float[] times = new float[1];
		//			for (int i = 0; i < fieldsNames.Count; i++) {
		//				cellFields.Add (new Scimesh.Base.CellField (fieldsNames [i], fieldsNComponents [i], 4, times.Length, fieldsData [i].ToArray (), times, mesh));
		//			}
		//			return cellFields;
		//		}

		//		public static List<Scimesh.Base.PointField> VtkUnstructuredGridToPointFields (string filename, Scimesh.Base.Mesh mesh)
		//		{
		//			List<List<float?>> fieldsData = new List<List<float?>> ();
		//			List<float?> fieldData = new List<float?> ();
		//			List<int> fieldsNComponents = new List<int> ();
		//			List<string> fieldsNames = new List<string> ();
		//			int fieldDataSize = 0;
		//			int nPoints = 0;
		//			bool isPointData = false;
		//			bool isField = false;
		//			try {
		//				using (StreamReader sr = new StreamReader (filename)) {
		//					string line;
		//					while ((line = sr.ReadLine ()) != null) {
		//						if (!string.IsNullOrEmpty (line)) {
		//							string[] tokens = line.Split ();
		//							if (tokens [0] == "POINTS") {
		//								UnityEngine.Debug.Log ("Reading POINTS: " + tokens [1]);
		//								nPoints = int.Parse (tokens [1]);
		//								continue;
		//							} else if (tokens [0] == "POINT_DATA") {
		//								UnityEngine.Debug.Log ("Reading POINT_DATA " + tokens [1]);
		//								nPoints = int.Parse (tokens [1]);
		//								isPointData = true;
		//								continue;
		//							}
		//							if (isPointData) {
		//								if (tokens [0] == "SCALARS") {
		//									isField = true;
		//									fieldData = new List<float?> ();
		//									fieldsNames.Add (tokens [1]);
		//									int nComponents = int.Parse (tokens [3]);
		//									fieldsNComponents.Add (nComponents);
		//									fieldDataSize = nPoints * nComponents;
		//								} else if (tokens [0] == "LOOKUP_TABLE") {
		//									continue;
		//								} else if (isField) {
		//									for (int j = 0; j < tokens.Length; j++) {
		//										if (!string.IsNullOrEmpty (tokens [j])) {
		//											fieldData.Add (float.Parse (tokens [j]));
		//											if (fieldData.Count == fieldDataSize) {
		//												fieldsData.Add (fieldData);
		//												isField = false;
		//												continue;
		//											}
		//										}
		//									}
		//								}
		//							}
		//						}
		//					}
		//				}
		//			} catch (Exception e) {
		//				UnityEngine.Debug.Log (string.Format ("Error reading {0}: {1} {2}", filename, e.Message, e.StackTrace));
		//			}
		//			List<Scimesh.Base.PointField> pointFields = new List<Scimesh.Base.PointField> ();
		//			float[] times = new float[1];
		//			for (int i = 0; i < fieldsNames.Count; i++) {
		//				pointFields.Add (new Scimesh.Base.PointField (fieldsNames [i], fieldsNComponents [i], 4, times.Length, fieldsData [i].ToArray (), times, mesh));
		//			}
		//			return pointFields;
		//		}

		//		public static List<Scimesh.Base.CellField> VtkUnstructuredGridToCellFields (string filename, Scimesh.Base.Mesh mesh)
		//		{
		//			List<List<float?>> fieldsData = new List<List<float?>> ();
		//			List<float?> fieldData = new List<float?> ();
		//			List<int> fieldsNComponents = new List<int> ();
		//			List<string> fieldsNames = new List<string> ();
		//			int fieldDataSize = 0;
		//			int nCells = 0;
		//			bool isCellData = false;
		//			bool isField = false;
		//			try {
		//				using (StreamReader sr = new StreamReader (filename)) {
		//					string line;
		//					while ((line = sr.ReadLine ()) != null) {
		//						if (!string.IsNullOrEmpty (line)) {
		//							string[] tokens = line.Split ();
		//							if (tokens [0] == "CELL_DATA") {
		//								UnityEngine.Debug.Log ("Reading CELL_DATA " + tokens [1]);
		//								nCells = int.Parse (tokens [1]);
		//								isCellData = true;
		//								continue;
		//							}
		//							if (isCellData) {
		//								if (tokens [0] == "SCALARS") {
		//									isField = true;
		//									fieldData = new List<float?> ();
		//									fieldsNames.Add (tokens [1]);
		//									int nComponents = int.Parse (tokens [3]);
		//									fieldsNComponents.Add (nComponents);
		//									fieldDataSize = nCells * nComponents;
		//								} else if (tokens [0] == "LOOKUP_TABLE") {
		//									continue;
		//								} else if (isField) {
		//									for (int j = 0; j < tokens.Length; j++) {
		//										if (!string.IsNullOrEmpty (tokens [j])) {
		//											fieldData.Add (float.Parse (tokens [j]));
		//											if (fieldData.Count == fieldDataSize) {
		//												fieldsData.Add (fieldData);
		//												isField = false;
		//												continue;
		//											}
		//										}
		//									}
		//								}
		//							}
		//						}
		//					}
		//				}
		//			} catch (Exception e) {
		//				UnityEngine.Debug.Log (string.Format ("Error reading {0}: {1} {2}", filename, e.Message, e.StackTrace));
		//			}
		//
		//			List<Scimesh.Base.CellField> cellFields = new List<Scimesh.Base.CellField> ();
		//			float[] times = new float[1];
		//			for (int i = 0; i < fieldsNames.Count; i++) {
		//				cellFields.Add (new Scimesh.Base.CellField (fieldsNames [i], fieldsNComponents [i], 4, times.Length, fieldsData [i].ToArray (), times, mesh));
		//			}
		//			return cellFields;
		//		}
			
		//		public static List<PointField> VtkUnstructuredGridToPointFields (string dirname)
		//		{
		//			DirectoryInfo di = new DirectoryInfo (dirname);
		//			FileInfo[] fi = di.GetFiles ("*.vtk");
		//			Array.Sort (fi, (f1, f2) => float.Parse (Regex.Match (f1.Name, @"\d+").Value).CompareTo (float.Parse (Regex.Match (f2.Name, @"\d+").Value)));
	}
}
