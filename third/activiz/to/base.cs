using UnityEngine;
using Kitware.VTK;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace Scimesh.Third.Activiz.To
{
    public static class Base
    {
        public static readonly Func<string, Scimesh.Base.Mesh> activizToMesh = (relPath) =>
        {
            string absPath = Path.Combine(Application.dataPath, relPath);
            UnityEngine.Debug.Log("Reading from " + absPath);
            Stopwatch stopwatch = Stopwatch.StartNew();
            vtkPolyDataReader reader = vtkPolyDataReader.New();
            reader.SetFileName(absPath);
            reader.Update();
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format(
                "Elapsed time: {0:E0} ms, {1:E0} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            UnityEngine.Debug.Log("Initializing");
            stopwatch = Stopwatch.StartNew();
            vtkPolyData polyData = reader.GetOutput();
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format(
                "Elapsed time: {0:E0} ms, {1:E0} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            UnityEngine.Debug.Log(string.Format("Number of cells: {0}", polyData.GetNumberOfCells()));
            UnityEngine.Debug.Log(string.Format("Number of lines: {0}", polyData.GetNumberOfLines()));
            UnityEngine.Debug.Log(string.Format("Number of pieces: {0}", polyData.GetNumberOfPieces()));
            UnityEngine.Debug.Log(string.Format("Number of points: {0}", polyData.GetNumberOfPoints()));
            UnityEngine.Debug.Log(string.Format("Number of polys: {0}", polyData.GetNumberOfPolys()));
            UnityEngine.Debug.Log(string.Format("Number of stripts: {0}", polyData.GetNumberOfStrips()));
            UnityEngine.Debug.Log(string.Format("Number of vertices: {0}", polyData.GetNumberOfVerts()));
            UnityEngine.Debug.Log("Scimesh");
            UnityEngine.Debug.Log("Importing Points");
            stopwatch = Stopwatch.StartNew();
            Scimesh.Base.Point[] points = new Scimesh.Base.Point[polyData.GetNumberOfPoints()];
            for (int i = 0; i < polyData.GetNumberOfPoints(); i++)
            {
                double[] cs = polyData.GetPoint(i);
                points[i] = new Scimesh.Base.Point(new float[] { (float)cs[0], (float)cs[2], (float)cs[1] });
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format(
                "Elapsed time: {0:E0} ms, {1:E0} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            UnityEngine.Debug.Log("Importing Cells");
            stopwatch = Stopwatch.StartNew();
            Scimesh.Base.Cell[] cells = new Scimesh.Base.Cell[polyData.GetNumberOfCells()];
            int[] cellTypes = new int[polyData.GetNumberOfCells()];
            for (int i = 0; i < polyData.GetNumberOfCells(); i++)
            {
                vtkCell cell = polyData.GetCell(i);
                int[] pointsIds = new int[cell.GetNumberOfPoints()];
                for (int j = 0; j < cell.GetNumberOfPoints(); j++)
                {
                    pointsIds[j] = (int)cell.GetPointId(j);
                }
                cells[i] = new Scimesh.Base.Cell(pointsIds);
                cellTypes[i] = cell.GetCellType();
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format(
                "Elapsed time: {0:E0} ms, {1:E0} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            UnityEngine.Debug.Log("Creating Faces");
            stopwatch = Stopwatch.StartNew();
            List<Scimesh.Base.Face> faces = new List<Scimesh.Base.Face>();
            for (int i = 0; i < cellTypes.Length; i++)
            {
                int[] triangles = Scimesh.Vtk.To.Base.cellTypeToTrianglesMap[cellTypes[i]];
                int nFaces = triangles.Length / 3;
                Scimesh.Base.Cell cell = cells[i];
                cell.facesIndices = new int[nFaces];
                for (int j = 0; j < nFaces; j++)
                {
                    int[] pointsIndices = new int[] {
                        cell.pointsIndices [triangles [3 * j]],
                        cell.pointsIndices [triangles [3 * j + 1]],
                        cell.pointsIndices [triangles [3 * j + 2]]
                    };
                    faces.Add(new Scimesh.Base.Face(pointsIndices));
                    cell.facesIndices[j] = faces.Count - 1;
                }
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format(
                "Elapsed time: {0:E0} ms, {1:E0} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            UnityEngine.Debug.Log(points.Length);
            UnityEngine.Debug.Log(faces.Count);
            UnityEngine.Debug.Log(cells.Length);
            Scimesh.Base.Mesh mesh = new Scimesh.Base.Mesh(points, faces.ToArray(), cells);
            return mesh;
        };
    }
}