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
        public static readonly Func<string, Scimesh.Base.Mesh> polydataToMesh = (relPath) =>
        {
            string absPath = Path.Combine(Application.dataPath, relPath);
            UnityEngine.Debug.Log("Reading from " + absPath);
            Stopwatch stopwatch = Stopwatch.StartNew();
            vtkPolyDataReader reader = vtkPolyDataReader.New();
            reader.SetFileName(absPath);
            reader.Update();
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format("Reading time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            stopwatch = Stopwatch.StartNew();
            vtkPolyData polyData = reader.GetOutput();
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format("Elapsed time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            UnityEngine.Debug.Log(string.Format("Number of cells: {0}", polyData.GetNumberOfCells()));
            UnityEngine.Debug.Log(string.Format("Number of lines: {0}", polyData.GetNumberOfLines()));
            UnityEngine.Debug.Log(string.Format("Number of pieces: {0}", polyData.GetNumberOfPieces()));
            UnityEngine.Debug.Log(string.Format("Number of points: {0}", polyData.GetNumberOfPoints()));
            UnityEngine.Debug.Log(string.Format("Number of polys: {0}", polyData.GetNumberOfPolys()));
            UnityEngine.Debug.Log(string.Format("Number of stripts: {0}", polyData.GetNumberOfStrips()));
            UnityEngine.Debug.Log(string.Format("Number of vertices: {0}", polyData.GetNumberOfVerts()));
            UnityEngine.Debug.Log("Scimesh");
            stopwatch = Stopwatch.StartNew();
            Scimesh.Base.Point[] points = new Scimesh.Base.Point[polyData.GetNumberOfPoints()];
            for (int i = 0; i < polyData.GetNumberOfPoints(); i++)
            {
                double[] cs = polyData.GetPoint(i);
                points[i] = new Scimesh.Base.Point(new float[] { (float)cs[0], (float)cs[2], (float)cs[1] });
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format("Points importing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
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
            UnityEngine.Debug.Log(string.Format("Cells importing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
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
            UnityEngine.Debug.Log(string.Format("Faces creating time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            UnityEngine.Debug.Log(string.Format("Number of scimesh points: {0}", points.Length));
            UnityEngine.Debug.Log(string.Format("Number of scimesh faces: {0}", faces.Count));
            UnityEngine.Debug.Log(string.Format("Number of scimesh cells: {0}", cells.Length));
            return new Scimesh.Base.Mesh(points, faces.ToArray(), cells);
        };

        public static readonly Func<string, Scimesh.Base.Mesh[]> xmlMultiBlockDataToMesh = (relPath) =>
        {
            string absPath = Path.Combine(Application.dataPath, relPath);
            UnityEngine.Debug.Log("Reading from " + absPath);
            Stopwatch stopwatch = Stopwatch.StartNew();
            vtkXMLMultiBlockDataReader reader = vtkXMLMultiBlockDataReader.New();
            reader.SetFileName(absPath);
            reader.Update();
            //reader.UpdateInformation();
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format("Reading time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            stopwatch = Stopwatch.StartNew();
            vtkMultiBlockDataSet multiBlock = vtkMultiBlockDataSet.SafeDownCast(reader.GetOutput());
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format("Initializing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            //multiBlock.Update();
            //multiBlock.UpdateData();
            //multiBlock.UpdateInformation();
            //vtkCompositeDataSet cds = reader.GetOutput();
            //vtkCompositeDataIterator iter = cds.NewIterator();
            //for (iter.InitTraversal(); iter.IsDoneWithTraversal() != 1; iter.GoToNextItem())
            //    UnityEngine.Debug.Log(string.Format("Has metadata: {0}", cds.HasMetaData(iter)));
            //    UnityEngine.Debug.Log(cds.HasMetaData(iter));
            //    UnityEngine.Debug.Log(cds.GetMetaData(iter).ToString());
            UnityEngine.Debug.Log(string.Format("Number of blocks: {0}", multiBlock.GetNumberOfBlocks()));
            Scimesh.Base.Mesh[] meshes = new Scimesh.Base.Mesh[multiBlock.GetNumberOfBlocks()];
            for (uint i = 0; i < multiBlock.GetNumberOfBlocks(); i++)
            {
                UnityEngine.Debug.Log(string.Format("Block: {0}", i));
                UnityEngine.Debug.Log(string.Format("Has metadata: {0}", multiBlock.HasMetaData(i)));
                vtkInformation inf = multiBlock.GetMetaData(i);
                //UnityEngine.Debug.Log(inf.ToString());
                string name = inf.Get(vtkMultiBlockDataSet.NAME());
                UnityEngine.Debug.Log(name);
                vtkUnstructuredGrid ug = vtkUnstructuredGrid.SafeDownCast(multiBlock.GetBlock(i));
                UnityEngine.Debug.Log(string.Format("Number of points: {0}", ug.GetNumberOfPoints()));
                UnityEngine.Debug.Log(string.Format("Number of cells: {0}", ug.GetNumberOfCells()));
                UnityEngine.Debug.Log(string.Format("Number of pieces: {0}", ug.GetNumberOfPieces()));
                UnityEngine.Debug.Log("Scimesh");
                stopwatch = Stopwatch.StartNew();
                Scimesh.Base.Point[] points = new Scimesh.Base.Point[ug.GetNumberOfPoints()];
                for (int j = 0; j < ug.GetNumberOfPoints(); j++)
                {
                    double[] cs = ug.GetPoint(j);
                    points[j] = new Scimesh.Base.Point(new float[] { (float)cs[0], (float)cs[2], (float)cs[1] });
                }
                stopwatch.Stop();
                UnityEngine.Debug.Log(string.Format("Points importing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
                stopwatch = Stopwatch.StartNew();
                Scimesh.Base.Cell[] cells = new Scimesh.Base.Cell[ug.GetNumberOfCells()];
                int[] cellTypes = new int[ug.GetNumberOfCells()];
                for (int j = 0; j < ug.GetNumberOfCells(); j++)
                {
                    vtkCell cell = ug.GetCell(j);
                    int[] pointsIds = new int[cell.GetNumberOfPoints()];
                    for (int k = 0; k < cell.GetNumberOfPoints(); k++)
                    {
                        pointsIds[k] = (int)cell.GetPointId(k);
                    }
                    cells[j] = new Scimesh.Base.Cell(pointsIds);
                    cellTypes[j] = cell.GetCellType();
                }
                stopwatch.Stop();
                UnityEngine.Debug.Log(string.Format("Cells importing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
                stopwatch = Stopwatch.StartNew();
                List<Scimesh.Base.Face> faces = new List<Scimesh.Base.Face>();
                for (int j = 0; j < cellTypes.Length; j++)
                {
                    int[] triangles = Scimesh.Vtk.To.Base.cellTypeToTrianglesMap[cellTypes[j]];
                    int nFaces = triangles.Length / 3;
                    Scimesh.Base.Cell cell = cells[j];
                    cell.facesIndices = new int[nFaces];
                    for (int k = 0; k < nFaces; k++)
                    {
                        int[] pointsIndices = new int[] {
                            cell.pointsIndices [triangles [3 * k]],
                            cell.pointsIndices [triangles [3 * k + 1]],
                            cell.pointsIndices [triangles [3 * k + 2]]
                        };
                        faces.Add(new Scimesh.Base.Face(pointsIndices));
                        cell.facesIndices[k] = faces.Count - 1;
                    }
                }
                stopwatch.Stop();
                UnityEngine.Debug.Log(string.Format("Faces creating time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
                UnityEngine.Debug.Log(string.Format("Number of scimesh points: {0}", points.Length));
                UnityEngine.Debug.Log(string.Format("Number of scimesh faces: {0}", faces.Count));
                UnityEngine.Debug.Log(string.Format("Number of scimesh cells: {0}", cells.Length));
                Scimesh.Base.Mesh mesh = new Scimesh.Base.Mesh(points, faces.ToArray(), cells);
                meshes[i] = mesh;
            }
            return meshes;
        };
    }
}