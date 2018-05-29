using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace Scimesh.Base.To
{
    /// <summary>
    /// Base object to itself functions.
    /// </summary>
    public static class Base
    {
        /// <summary>
        /// Cell field to point field.
        /// Weighted (by square distances from centroids) arithmetic mean algorithm 
        /// TODO To think about MultiDim mesh...
        /// </summary>
        public static readonly Func<MeshCellField, MeshPointField> cellFieldToPointField = (cf) =>
        {
            float?[] data = new float?[cf.Mesh.points.Length * cf.NComponents]; // data array for point field
            cf.Mesh.EvaluatePointsCells();  // Point-Cells connection needs to this algo
            // Calculate cells centroids coordiantes array
            float[,] cellsCentroids = new float[cf.Mesh.cells.Length, cf.Mesh.MinDim];
            for (int i = 0; i < cf.Mesh.cells.Length; i++)
            {
                float[] centroidCoordinates = cf.Mesh.CellCentroid(i);
                for (int j = 0; j < cf.Mesh.MinDim; j++)
                {
                    cellsCentroids[i, j] = centroidCoordinates[j];
                }
            }
            // Calculate square distances from points to neighbour cells centroids
            List<List<float>> pointsToCentroidsDistances = new List<List<float>>();
            for (int i = 0; i < cf.Mesh.points.Length; i++)
            {
                List<float> distances = new List<float>();
                float[] pointCoordiantes = cf.Mesh.points[i].coordinates;
                for (int j = 0; j < cf.Mesh.points[i].cellsIndices.Length; j++)
                {
                    float[] vector = new float[cf.Mesh.MinDim];
                    for (int k = 0; k < cf.Mesh.MinDim; k++)
                    {
                        vector[k] = cellsCentroids[cf.Mesh.points[i].cellsIndices[j], k] - pointCoordiantes[k];
                    }
                    float distance = 0;
                    for (int k = 0; k < cf.Mesh.MinDim; k++)
                    {
                        distance += vector[k] * vector[k];
                    }
                    distances.Add(distance);
                }
                pointsToCentroidsDistances.Add(distances);
            }
            // Set weighted (by square distances from centroids) arithmetic mean value to points
            for (int i = 0; i < cf.Mesh.points.Length; i++)
            {
                float sumWeight = 0; // sumW=D1+D2+...+Di (Di - distance to ith cell centroid)
                                     // sumWVi=D1*V1i+D2*V2i+...+Dj*Vji (Vji - ith component of the value in jth cell)
                float?[] sumWeightValues = new float?[cf.NComponents];
                for (int j = 0; j < sumWeightValues.Length; j++)
                {
                    sumWeightValues[j] = 0;
                }
                for (int j = 0; j < pointsToCentroidsDistances[i].Count; j++)
                {
                    int cellIdx = cf.Mesh.points[i].cellsIndices[j];
                    for (int m = 0; m < sumWeightValues.Length; m++)
                    {
                        sumWeightValues[m] += pointsToCentroidsDistances[i][j] * cf[cellIdx][m];
                    }
                    sumWeight += pointsToCentroidsDistances[i][j];
                }
                // Vi = sumWVi/sumW (Vi - ith component of the value in the point)
                float?[] value = new float?[cf.NComponents];
                for (int j = 0; j < cf.NComponents; j++)
                {
                    if (sumWeight != 0)
                    {
                        value[j] = sumWeightValues[j] / sumWeight;
                    }
                    else
                    {
                        value[j] = null;
                    }
                }
                for (int j = 0; j < cf.NComponents; j++)
                {
                    data[i * cf.NComponents + j] = value[j];
                }
            }
            return new MeshPointField(cf.Name, cf.NComponents, data, cf.Mesh); ;
        };

        /// <summary>
        /// Create cell mesh filter by mesh point scalar field and isovalue.
        /// If cell points have field values with different result of expression: 
        /// isovalue > pointValue, then it is visible.
        /// Also, cell shouldn't contain null values.
        /// </summary>
        public static readonly Func<MeshPointField, float?, MeshFilter> pointIsovalueCellsMeshFilter = (pf, isovalue) =>
        {
            Debug.Assert(pf.NComponents != 1);
            List<int> visibleCells = new List<int>();
            for (int i = 0; i < pf.Mesh.cells.Length; i++)
            {
                bool isVisible = false;
                int[] psIs = pf.Mesh.cells[i].pointsIndices;
                if (pf[psIs[0]] != null)
                {
                    bool firstValue = isovalue > pf[psIs[0]][0];
                    for (int j = 1; j < psIs.Length; j++)
                    {
                        if (pf[psIs[j]] != null)
                        {
                            if (firstValue != isovalue > pf[psIs[j]][0])
                            {
                                isVisible = true;
                            }
                        }
                        else
                        {
                            isVisible = false;
                            break;
                        }
                    }
                }
                if (isVisible)
                {
                    visibleCells.Add(i);
                }
            }
            return new MeshFilter(new int[0], new int[0], new int[0], visibleCells.ToArray());
        };

        /// <summary>
        /// The lightning algorithm.
        /// </summary>
        public static readonly Action<MeshPointField, float, float[]> lightning = (pf, isoSquareDist, position) =>
        {
            Debug.Assert(pf.NComponents != 1);
            // Square distance function
            Func<float[], float[], float> squareDist =
                (pointCs, positionCs) =>
                (pointCs[0] - positionCs[0]) * (pointCs[0] - positionCs[0])
                + (pointCs[1] - positionCs[1]) * (pointCs[1] - positionCs[1])
                + (pointCs[2] - positionCs[2]) * (pointCs[2] - positionCs[2]);
            // Search start point
            int startPointIdx = -1;
            for (int i = 0; i < pf.NValues; i++)
            {
                if (pf[i][0] != null)
                {
                    startPointIdx = i;
                    break;
                }
            }
            if (startPointIdx == -1)
            {
                for (int i = 0; i < pf.Mesh.points.Length; i++)
                {
                    float[] pCs = pf.Mesh.points[i].coordinates;
                    float pV = squareDist(pCs, position);
                    int[] npIs = pf.Mesh.points[i].neighbourPointsIndices;
                    for (int j = 1; j < npIs.Length; j++)
                    {
                        float[] npCs = pf.Mesh.points[npIs[i]].coordinates;
                        float npV = squareDist(npCs, position);
                        if (isoSquareDist > pV != isoSquareDist > npV)
                        {
                            startPointIdx = i;
                            break;
                        }
                    }
                }
            }
            float[] spCs = pf.Mesh.points[startPointIdx].coordinates;
            // Reset point field
            pf.ResetValues();
            // Lightning algorithm
            pf[startPointIdx] = new float?[] { squareDist(spCs, position) };
            HashSet<int> points = new HashSet<int>();
            points.Add(startPointIdx);
            while (points.Count != 0)
            {
                HashSet<int> newPoints = new HashSet<int>();
                foreach (int point in points)
                {
                    int[] npIs = pf.Mesh.points[point].neighbourPointsIndices;
                    for (int i = 0; i < npIs.Length; i++)
                    {
                        if (pf[npIs[i]][0] == null)
                        {
                            float[] npCs = pf.Mesh.points[npIs[i]].coordinates;
                            float npV = squareDist(npCs, position);
                            if (isoSquareDist > pf[point][0] != isoSquareDist > npV)
                            {
                                pf[npIs[i]] = new float?[] { npV };
                                newPoints.Add(npIs[i]);
                            }
                        }
                    }
                }
                points = newPoints;
            }
        };

        /// <summary>
        /// Mesh filter that return only boundary faces (FIXME very slow - SLUGGISH...)
        /// Complexity grows by ~ k^2*n^2, k - number of faces points, n - number of faces
        /// n = 10k quadrangle faces time ~ 8s
        /// n = 90k quadrangle faces time ~ 660s
		/// </summary>
        public static readonly Func<Mesh, MeshFilter> boundaryFacesMeshFilter = (m) =>
        {
            List<int> facesIndices = new List<int>(); // Faces indices to MeshFilter
            HashSet<int> hashFacesIndices = new HashSet<int>();  // Temporary HashSet for faces indices
            for (int i = 0; i < m.faces.Length; i++)
            {
                hashFacesIndices.Add(i);
            }
            for (int i = 0; i < m.faces.Length; i++)
            {
                if (hashFacesIndices.Contains(i))
                {
                    hashFacesIndices.Remove(i);
                    int[] facePointsIndices = m.faces[i].pointsIndices;
                    bool haveOppositeFace = false;
                    foreach (int hashFaceIndex in hashFacesIndices)
                    {
                        int[] hashFacePointsIndices = m.faces[hashFaceIndex].pointsIndices;
                        haveOppositeFace = true;
                        for (int j = 0; j < facePointsIndices.Length; j++)
                        {
                            int facePoint = facePointsIndices[j];
                            bool haveOppositePoint = false;
                            for (int k = 0; k < hashFacePointsIndices.Length; k++)
                            {
                                int hashPoint = hashFacePointsIndices[k];
                                if (facePoint == hashPoint)
                                {
                                    haveOppositePoint = true;
                                    break;
                                }
                            }
                            if (!haveOppositePoint)
                            {
                                haveOppositeFace = false;
                                break;
                            }
                        }
                        if (haveOppositeFace)
                        {
                            hashFacesIndices.Remove(hashFaceIndex);
                            break;
                        }
                    }
                    if (!haveOppositeFace)
                    {
                        facesIndices.Add(i);
                    }
                }
            }
            return new MeshFilter(new int[0], new int[0], facesIndices.ToArray(), new int[0]);
        };

        /// <summary>
        /// Mesh filter that return only boundary faces through face neighbour faces
        /// Complexity grows by ~ k^2*n, k - number of faces points, n - number of faces
        /// n = 10k quadrangle faces time ~ 0.13s
        /// n = 90k quadrangle faces time ~ 1.30s
        /// n = 250k quadrangle faces time ~ 3.80s
		/// </summary>
        public static readonly Func<Mesh, MeshFilter> boundaryFacesMeshFilter2 = (m) =>
        {
            m.EvaluateFacesNeighbourFaces();
            List<int> facesIndices = new List<int>(); // Faces indices to MeshFilter
            HashSet<int> hashFacesIndices = new HashSet<int>();  // Temporary HashSet for faces indices
            for (int i = 0; i < m.faces.Length; i++)
            {
                hashFacesIndices.Add(i);
            }
            for (int i = 0; i < m.faces.Length; i++)
            {
                if (hashFacesIndices.Contains(i))
                {
                    hashFacesIndices.Remove(i);
                    int[] facePointsIndices = m.faces[i].pointsIndices;
                    bool haveOppositeFace = false;
                    int[] neighbourFacesIndices = m.faces[i].neighbourFacesIndices;
                    for (int n = 0; n < neighbourFacesIndices.Length; n++)
                    {
                        int[] neighbourFacePointsIndices = m.faces[neighbourFacesIndices[n]].pointsIndices;
                        haveOppositeFace = true;
                        for (int j = 0; j < facePointsIndices.Length; j++)
                        {
                            int facePoint = facePointsIndices[j];
                            bool haveOppositePoint = false;
                            for (int k = 0; k < neighbourFacePointsIndices.Length; k++)
                            {
                                int neighbourPoint = neighbourFacePointsIndices[k];
                                if (facePoint == neighbourPoint)
                                {
                                    haveOppositePoint = true;
                                    break;
                                }
                            }
                            if (!haveOppositePoint)
                            {
                                haveOppositeFace = false;
                                break;
                            }
                        }
                        if (haveOppositeFace)
                        {
                            hashFacesIndices.Remove(m.faces[i].neighbourFacesIndices[n]);
                            break;
                        }
                    }
                    if (!haveOppositeFace)
                    {
                        facesIndices.Add(i);
                    }
                }
            }
            return new MeshFilter(new int[0], new int[0], facesIndices.ToArray(), new int[0]);
        };

        /// <summary>
        /// Mesh filter that return all cells
		/// </summary>
        public static readonly Func<Mesh, MeshFilter> allCellsMeshFilter = (m) =>
        {
            int[] cellsIndices = new int[m.cells.Length];
            for (int i = 0; i < m.cells.Length; i++)
            {
                cellsIndices[i] = i;
            }
            return new MeshFilter(new int[0], new int[0], new int[0], cellsIndices);
        };

        /// <summary>
        /// Mesh filter that return all faces
		/// </summary>
        public static readonly Func<Mesh, MeshFilter> allFacesMeshFilter = (m) =>
        {
            int[] facesIndices = new int[m.faces.Length];
            for (int i = 0; i < m.faces.Length; i++)
            {
                facesIndices[i] = i;
            }
            return new MeshFilter(new int[0], new int[0], facesIndices, new int[0]);
        };

        /// <summary>
        /// Create test Mesh by type
        /// 0 - cube with 2 internal diagonals for boundary mesh filter testing
        /// 1 - quadrangle ribbon for boundary mesh filter testing
        /// else - empty mesh
		/// </summary>
        public static readonly Func<int, Mesh> testMesh = (type) =>
        {
            if (type == 0)
            {
                Point[] points = new Point[8];
                points[0] = new Point(new float[] { 0f, 0f, 0f });
                points[1] = new Point(new float[] { 1f, 0f, 0f });
                points[2] = new Point(new float[] { 1f, 1f, 0f });
                points[3] = new Point(new float[] { 0f, 1f, 0f });
                points[4] = new Point(new float[] { 0f, 0f, 1f });
                points[5] = new Point(new float[] { 1f, 0f, 1f });
                points[6] = new Point(new float[] { 1f, 1f, 1f });
                points[7] = new Point(new float[] { 0f, 1f, 1f });
                Edge[] edges = new Edge[0];
                Face[] faces = new Face[8];
                faces[0] = new Face(new int[] { 0, 1, 2, 3 }); // NZ
                faces[1] = new Face(new int[] { 4, 7, 6, 5 }); // Z
                faces[2] = new Face(new int[] { 0, 3, 7, 4 }); // NX
                faces[3] = new Face(new int[] { 2, 1, 5, 6 }); // X
                faces[4] = new Face(new int[] { 1, 0, 4, 5 }); // NY
                faces[5] = new Face(new int[] { 3, 2, 6, 7 }); // Y
                faces[6] = new Face(new int[] { 0, 2, 6, 4 }); // XY Diagonal
                faces[7] = new Face(new int[] { 2, 0, 4, 6 }); // NXY Diagonal
                Cell[] cells = new Cell[0];
                return new Mesh(points, edges, faces, cells);
            }
            else if (type == 1)
            {
                int nFaces = 50;
                float dx = 1;
                float z = 1;
                List<Point> ps = new List<Point>();
                List<Face> fs = new List<Face>();
                for (int i = 0; i < nFaces; i++)
                {
                    ps.Add(new Point(new float[] { (i + 1) * dx, 0f, z }));
                    ps.Add(new Point(new float[] { (i + 1) * dx, 0f, 0f }));
                    ps.Add(new Point(new float[] { i * dx, 0f, 0f }));
                    ps.Add(new Point(new float[] { i * dx, 0f, z }));
                    fs.Add(new Face(new int[] { i * 4, i * 4 + 1, i * 4 + 2, i * 4 + 3 }));
                }
                Edge[] edges = new Edge[0];
                Cell[] cells = new Cell[0];
                return new Mesh(ps.ToArray(), edges, fs.ToArray(), cells);
            }
            else if (type == 2)
            {
                int nFacesX = 500;
                int nFacesZ = 500;
                float dx = 1;
                float dz = 1;
                List<Point> ps = new List<Point>();
                List<Face> fs = new List<Face>();
                for (int i = 0; i < nFacesX; i++)
                {
                    for (int j = 0; j < nFacesZ; j++)
                    {
                        ps.Add(new Point(new float[] { (i + 1) * dx, 0f, (j + 1) * dz }));
                        ps.Add(new Point(new float[] { (i + 1) * dx, 0f, j * dz }));
                        ps.Add(new Point(new float[] { i * dx, 0f, j * dz }));
                        ps.Add(new Point(new float[] { i * dx, 0f, (j + 1) * dz }));
                        int startPoint = i * 4 * nFacesZ + j * 4;
                        fs.Add(new Face(new int[] { startPoint, startPoint + 1, startPoint + 2, startPoint + 3 }));
                    }
                }
                Edge[] edges = new Edge[0];
                Cell[] cells = new Cell[0];
                return new Mesh(ps.ToArray(), edges, fs.ToArray(), cells);
            }
            else
            {
                Point[] points = new Point[0];
                Edge[] edges = new Edge[0];
                Face[] faces = new Face[0];
                Cell[] cells = new Cell[0];
                return new Mesh(points, edges, faces, cells);
            }
        };

        /// <summary>
        /// Create test MeshPointField by type
        /// 0 - cube with 2 internal diagonals and scalar field
        /// 1 - cube with 2 internal diagonals and 3D vector field
        /// 2 - quadrangle ribbon with scalar field
        /// 3 - quadrangle plane with scalar field
        /// else - type = 0
		/// </summary>
        public static readonly Func<int, MeshPointField> testMeshPointField = (type) =>
        {
            if (type == 0)
            {
                Mesh mesh = testMesh(0);
                int nComponents = 1;
                float?[] values = new float?[mesh.points.Length * nComponents];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (float)i;
                }
                return new MeshPointField("Test", nComponents, values, mesh);
            }
            else if (type == 1)
            {
                Mesh mesh = testMesh(0);
                int nComponents = 3;
                float?[] values = new float?[mesh.points.Length * nComponents];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (float)i;
                }
                return new MeshPointField("Test", nComponents, values, mesh);
            }
            else if (type == 2)
            {
                Mesh mesh = testMesh(1);
                int nComponents = 1;
                float?[] values = new float?[mesh.points.Length * nComponents];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (float)i;
                }
                return new MeshPointField("Test", nComponents, values, mesh);
            }
            else if (type == 3)
            {
                Mesh mesh = testMesh(2);
                int nComponents = 1;
                float?[] values = new float?[mesh.points.Length * nComponents];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (float)i;
                }
                return new MeshPointField("Test", nComponents, values, mesh);
            }
            else
            {
                Mesh mesh = testMesh(0);
                int nComponents = 1;
                float?[] values = new float?[mesh.points.Length * nComponents];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (float)i;
                }
                return new MeshPointField("Test", nComponents, values, mesh);
            }
        };

        /// <summary>
        /// Triangulate Face into Triangle Faces for Unity export
		/// </summary>
        public static readonly Func<Face, Face[]> triangulateFace = (f) =>
        {
            int nFaces = f.pointsIndices.Length - 2;
            Face[] fs = new Face[nFaces];
            for (int i = 0; i < nFaces; i++)
            {
                int[] pointsIndices = new int[3];
                pointsIndices[0] = f.pointsIndices[0];
                pointsIndices[1] = f.pointsIndices[i + 1];
                pointsIndices[2] = f.pointsIndices[i + 2];
                fs[i] = new Face(pointsIndices);
            }
            return fs;
        };
    }
}
