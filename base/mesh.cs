using System.Collections.Generic;
using System;

namespace Scimesh.Base
{
    [Serializable]
    public class Point
    {
        public float[] coordinates;
        public int[] cellsIndices;
        public int[] facesIndices;
        public int[] edgesIndices;
        public int[] neighbourPointsIndices;

        public Point(float[] coordinates)
        {
            this.coordinates = coordinates;
            this.cellsIndices = new int[0];
            this.facesIndices = new int[0];
            this.edgesIndices = new int[0];
            this.neighbourPointsIndices = new int[0];
        }
    }

    [Serializable]
    public class Edge
    {
        public int[] pointsIndices;

        public Edge(int[] pointsIndices)
        {
            this.pointsIndices = pointsIndices;
        }
    }

    [Serializable]
    public class Face
    {
        public int[] pointsIndices;
        public int[] edgesIndices;

        public Face(int[] pointsIndices, int[] edgesIndices)
        {
            this.pointsIndices = pointsIndices;
            this.edgesIndices = edgesIndices;
        }

        public Face(int[] pointsIndices)
        {
            this.pointsIndices = pointsIndices;
            this.edgesIndices = new int[0];
        }
    }

    [Serializable]
    public class Cell
    {
        public int[] pointsIndices;
        public int[] facesIndices;
        public int[] neighbourCellsIndices;

        public Cell(int[] pointsIndices, int[] facesIndices)
        {
            this.pointsIndices = pointsIndices;
            this.facesIndices = facesIndices;
            this.neighbourCellsIndices = new int[0];
        }

        public Cell(int[] pointsIndices)
        {
            this.pointsIndices = pointsIndices;
            this.facesIndices = new int[0];
            this.neighbourCellsIndices = new int[0];
        }
    }

    [Serializable]
    public class Mesh
    {
        public Point[] points;
        public Edge[] edges;
        public Face[] faces;
        public Cell[] cells;

        int maxDim;
        int minDim;

        public int MaxDim { get { return maxDim; } }

        public int MinDim { get { return maxDim; } }

        public enum Neighbours { InEdges, InFaces, InCells };

        bool pointsCellsEvaluated = false;
        bool pointsFacesEvaluated = false;
        bool pointsEdgesEvaluated = false;

        public Mesh(Point[] points, Edge[] edges, Face[] faces, Cell[] cells)
        {
            this.points = points;
            this.edges = edges;
            this.faces = faces;
            this.cells = cells;
            EvaluateDims();
        }

        public Mesh(Point[] points, Face[] faces, Cell[] cells) : this(points, new Edge[0], faces, cells)
        {
        }

        public void EvaluatePointsCells()
        {
            if (cells.Length > 0)
            {
                // Defining the temprorary array
                List<List<int>> pointsCells = new List<List<int>>();
                for (int i = 0; i < points.Length; i++)
                {
                    pointsCells.Add(new List<int>());
                }
                // Initializing the temprorary array
                for (int i = 0; i < cells.Length; i++)
                {
                    for (int j = 0; j < cells[i].pointsIndices.Length; j++)
                    {
                        pointsCells[cells[i].pointsIndices[j]].Add(i);
                    }
                }
                // Writing the temprorary array to the mesh points cellsIndices arrays
                for (int i = 0; i < points.Length; i++)
                {
                    points[i].cellsIndices = pointsCells[i].ToArray();
                }
                pointsCellsEvaluated = true;
            }
        }

        public void EvaluatePointsFaces()
        {
            if (faces.Length > 0)
            {
                // Defining the temprorary array
                List<List<int>> pointsFaces = new List<List<int>>();
                for (int i = 0; i < points.Length; i++)
                {
                    pointsFaces.Add(new List<int>());
                }
                // Initializing the temprorary array
                for (int i = 0; i < faces.Length; i++)
                {
                    for (int j = 0; j < faces[i].pointsIndices.Length; j++)
                    {
                        pointsFaces[faces[i].pointsIndices[j]].Add(i);
                    }
                }
                // Assigning the temprorary array to the mesh points facesIndices arrays
                for (int i = 0; i < points.Length; i++)
                {
                    points[i].facesIndices = pointsFaces[i].ToArray();
                }
                pointsFacesEvaluated = true;
            }
        }

        public void EvaluatePointsEdges()
        {
            if (edges.Length > 0)
            {
                // Defining the temprorary array
                List<List<int>> pointsEdges = new List<List<int>>();
                for (int i = 0; i < points.Length; i++)
                {
                    pointsEdges.Add(new List<int>());
                }
                // Initializing the temprorary array
                for (int i = 0; i < edges.Length; i++)
                {
                    for (int j = 0; j < edges[i].pointsIndices.Length; j++)
                    {
                        pointsEdges[edges[i].pointsIndices[j]].Add(i);
                    }
                }
                // Assigning the temprorary array to the mesh points edgesIndices arrays
                for (int i = 0; i < points.Length; i++)
                {
                    points[i].edgesIndices = pointsEdges[i].ToArray();
                }
                pointsEdgesEvaluated = true;
            }
        }

        public void EvaluatePointsNeighbourPoints(Neighbours type)
        {
            if (type == Neighbours.InEdges && edges.Length > 0)
            { // Neighbours in the edges
              // PointsEdges arrays are needed for an efficient algorithm
                if (!pointsEdgesEvaluated)
                {
                    EvaluatePointsEdges();
                }
                // For each point
                for (int i = 0; i < points.Length; i++)
                {
                    // Using HashSet to create unique value list
                    HashSet<int> neighbourPoints = new HashSet<int>();
                    for (int j = 0; j < points[i].edgesIndices.Length; j++)
                    {
                        for (int k = 0; k < edges[points[i].edgesIndices[j]].pointsIndices.Length; k++)
                        {
                            neighbourPoints.Add(edges[points[i].edgesIndices[j]].pointsIndices[k]);
                        }
                    }
                    neighbourPoints.Remove(i); // Remove itself
                                               // Converting HashSet to array int[] and assign it to points neighbourPointsIndices array
                    points[i].neighbourPointsIndices = new int[neighbourPoints.Count];
                    neighbourPoints.CopyTo(points[i].neighbourPointsIndices);
                }
            }
            else if (type == Neighbours.InFaces && faces.Length > 0)
            { // Neighbours in the faces
              // PointsFaces arrays are needed for an efficient algorithm
                if (!pointsEdgesEvaluated)
                {
                    EvaluatePointsFaces();
                }
                // For each point
                for (int i = 0; i < points.Length; i++)
                {
                    // Using HashSet to create unique value list
                    HashSet<int> neighbourPoints = new HashSet<int>();
                    for (int j = 0; j < points[i].facesIndices.Length; j++)
                    {
                        for (int k = 0; k < faces[points[i].facesIndices[j]].pointsIndices.Length; k++)
                        {
                            neighbourPoints.Add(faces[points[i].facesIndices[j]].pointsIndices[k]);
                        }
                    }
                    neighbourPoints.Remove(i); // Remove itself
                                               // Converting HashSet to array int[] and assign it to points neighbourPointsIndices array
                    points[i].neighbourPointsIndices = new int[neighbourPoints.Count];
                    neighbourPoints.CopyTo(points[i].neighbourPointsIndices);
                }
            }
            else if (type == Neighbours.InCells && cells.Length > 0)
            { // Neighbours in the cells
              // PointsCells arrays are needed for an efficient algorithm
                if (!pointsCellsEvaluated)
                {
                    EvaluatePointsCells();
                }
                // For each point
                for (int i = 0; i < points.Length; i++)
                {
                    // Using HashSet to create unique value list
                    HashSet<int> neighbourPoints = new HashSet<int>();
                    for (int j = 0; j < points[i].cellsIndices.Length; j++)
                    {
                        for (int k = 0; k < cells[points[i].cellsIndices[j]].pointsIndices.Length; k++)
                        {
                            neighbourPoints.Add(cells[points[i].cellsIndices[j]].pointsIndices[k]);
                        }
                    }
                    neighbourPoints.Remove(i); // Remove itself
                                               // Converting HashSet to array int[] and assign it to points neighbourPointsIndices array
                    points[i].neighbourPointsIndices = new int[neighbourPoints.Count];
                    neighbourPoints.CopyTo(points[i].neighbourPointsIndices);
                }
            }
        }

        public void EvaluateCellsNeighbourCells()
        {
            // PointsCells arrays are needed for an efficient algorithm
            if (!pointsCellsEvaluated)
            {
                EvaluatePointsCells();
            }
            // For each cell
            for (int i = 0; i < cells.Length; i++)
            {
                // Using HashSet to create unique value list
                HashSet<int> cellsIndices = new HashSet<int>();
                for (int j = 0; j < cells[i].pointsIndices.Length; j++)
                {
                    for (int k = 0; k < points[cells[i].pointsIndices[j]].cellsIndices.Length; k++)
                    {
                        cellsIndices.Add(points[cells[i].pointsIndices[j]].cellsIndices[k]);
                    }
                }
                cellsIndices.Remove(i); // Remove itself
                                        // Converting HashSet to array int[] and assign it to cells neighbourCellsIndices array
                cells[i].neighbourCellsIndices = new int[cellsIndices.Count];
                cellsIndices.CopyTo(cells[i].neighbourCellsIndices);
            }
        }

        void EvaluateDims()
        {
            maxDim = int.MinValue;
            minDim = int.MaxValue;
            for (int i = 0; i < points.Length; i++)
            {
                int pointDim = points[i].coordinates.Length;
                if (maxDim < pointDim)
                {
                    maxDim = pointDim;
                }
                if (minDim > pointDim)
                {
                    minDim = pointDim;
                }
            }
        }

        public float[] CellCentroid(int cellIndex)
        {
            Cell cell = cells[cellIndex];
            float[] centroidCs = new float[minDim]; // TODO It's safe, but ok?
            for (int i = 0; i < cells[cellIndex].pointsIndices.Length; i++)
            {
                float[] pointCs = points[cell.pointsIndices[i]].coordinates;
                for (int j = 0; j < pointCs.Length; j++)
                {
                    centroidCs[j] += pointCs[j];
                }
            }
            for (int i = 0; i < centroidCs.Length; i++)
            {
                centroidCs[i] /= cell.pointsIndices.Length;
            }
            return centroidCs;
        }
    }

    [Serializable]
    public class MeshFilter
    {
        public int[] pointsIndices;
        public int[] edgesIndices;
        public int[] facesIndices;
        public int[] cellsIndices;

        public MeshFilter(int[] pointsIndices, int[] edgesIndices, int[] facesIndices, int[] cellsIndices)
        {
            this.pointsIndices = pointsIndices;
            this.edgesIndices = edgesIndices;
            this.facesIndices = facesIndices;
            this.cellsIndices = cellsIndices;
        }

        public MeshFilter(int[] cellsIndices) : this(new int[0], new int[0], new int[0], cellsIndices)
        {
        }

        /// <summary>
		/// </summary>
        public static readonly Func<Mesh, MeshFilter> boundaryCells = (m) =>
        {
            HashSet<int> cellsIndicesHash = new HashSet<int>();
            for (int i = 0; i < m.faces.Length; i++)
            {
                cellsIndicesHash.Add(i);
            }
            for (int i = 0; i < m.cells.Length; i++)
            {
                Cell c = m.cells[i];
                Cell[] nc = new Cell[c.neighbourCellsIndices.Length];
                for (int j = 0; j < c.neighbourCellsIndices.Length; j++)
                {
                    nc[j] = m.cells[c.neighbourCellsIndices[j]];
                }
                List<int[]> ncfps = new List<int[]>();
                for (int j = 0; j < nc.Length; j++)
                {
                    for (int k = 0; k < nc[j].facesIndices.Length; k++)
                    {
                        ncfps.Add(m.faces[nc[j].facesIndices[k]].pointsIndices);
                    }
                }
                for (int j = 0; j < c.facesIndices.Length; j++)
                {
                    int[] fps = m.faces[c.facesIndices[j]].pointsIndices;
                    for (int k = 0; k < ncfps.Count; k++)
                    {
                        if (fps.Length == ncfps[j].Length)
                        {

                        }
                    }
                }
            }
            int[] cellsIndices = new int[cellsIndicesHash.Count];
            cellsIndicesHash.CopyTo(cellsIndices);
            return new MeshFilter(new int[0], new int[0], new int[0], cellsIndices);
        };
    }
}