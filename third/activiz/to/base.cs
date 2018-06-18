using Kitware.VTK;
using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace Scimesh.Third.Activiz.To
{
    public static class Base
    {
        /// <summary>
        /// Read Polydata To Mesh
        /// </summary>
        public static readonly Func<string, Scimesh.Base.Mesh> rPolydataToMesh = (absPath) =>
        {
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

        /// <summary>
        /// Read Unstructured Grid To Mesh
        /// </summary>
        public static readonly Func<string, Scimesh.Base.Mesh> rXmlUGridToMesh = (absPath) =>
        {
            vtkXMLUnstructuredGridReader reader = vtkXMLUnstructuredGridReader.New();
            reader.SetFileName(absPath);
            reader.Update();
            vtkUnstructuredGrid ug = vtkUnstructuredGrid.SafeDownCast(reader.GetOutput());
            //UnityEngine.Debug.Log(ug);
            vtkInformation info = ug.GetInformation();
            //UnityEngine.Debug.Log(info);
            return uGridToMesh(ug);
        };

        /// <summary>
        /// Read XmlUnstructuredGrid's PointDataArray To MeshPointField
		/// </summary>
        public static readonly Func<string, int, Scimesh.Base.MeshPointFieldNullable> rXmlUGridPDArrayToMPField = (absPath, arrayIndex) =>
        {
            vtkXMLUnstructuredGridReader reader = vtkXMLUnstructuredGridReader.New();
            reader.SetFileName(absPath);
            reader.Update();
            vtkUnstructuredGrid ug = vtkUnstructuredGrid.SafeDownCast(reader.GetOutput());
            //UnityEngine.Debug.Log(ug);
            vtkInformation info = ug.GetInformation();
            //UnityEngine.Debug.Log(info);
            vtkPointData pd = ug.GetPointData();
            //UnityEngine.Debug.Log(pd);
            vtkDataArray a = pd.GetArray(arrayIndex);
            //UnityEngine.Debug.Log(a);
            string name = a.GetName();
            int nComponents = a.GetNumberOfComponents();
            long nPoints = a.GetNumberOfTuples();
            List<float?> data = new List<float?>();
            for (long i = 0; i < nPoints; i++)
            {
                double[] cs;
                switch (nComponents)  // FIXME Tuple6 implementation? By Tuple9? ...
                {
                    case 1:
                        cs = new double[] { a.GetTuple1(i) };
                        break;
                    case 2:
                        cs = a.GetTuple2(i);
                        break;
                    case 3:
                        cs = a.GetTuple3(i);
                        break;
                    case 4:
                        cs = a.GetTuple4(i);
                        break;
                    case 9:
                        cs = a.GetTuple9(i);
                        break;
                    default:
                        throw new NotImplementedException(string.Format("Tuple with {0} components", nComponents));
                }
                foreach (double c in cs)
                {
                    data.Add((float)c);
                }
            }
            Scimesh.Base.Mesh m = uGridToMesh(ug);
            return new Scimesh.Base.MeshPointFieldNullable(name, nComponents, data.ToArray(), m);
        };

        /// <summary>
        /// Read XmlUnstructuredGrid's PointDataArray To MeshPointFieldNullable with external Mesh.
        /// Used to create MeshPointField without creating a new Mesh. 
        /// It's convienent for constant in time Mesh, when we need to create the same Mesh from each time step file.
        /// </summary>
        public static readonly Func<string, int, Scimesh.Base.Mesh, Scimesh.Base.MeshPointFieldNullable> rXmlUGridPDArrayToMPFNullableNoMesh = (absPath, arrayIndex, m) =>
        {
            vtkXMLUnstructuredGridReader reader = vtkXMLUnstructuredGridReader.New();
            reader.SetFileName(absPath);
            reader.Update();
            vtkUnstructuredGrid ug = vtkUnstructuredGrid.SafeDownCast(reader.GetOutput());
            //UnityEngine.Debug.Log(ug);
            vtkInformation info = ug.GetInformation();
            //UnityEngine.Debug.Log(info);
            vtkPointData pd = ug.GetPointData();
            //UnityEngine.Debug.Log(pd);
            vtkDataArray a = pd.GetArray(arrayIndex);
            //UnityEngine.Debug.Log(a);
            string name = a.GetName();
            int nComponents = a.GetNumberOfComponents();
            long nPoints = a.GetNumberOfTuples();
            List<float?> data = new List<float?>();
            for (long i = 0; i < nPoints; i++)
            {
                double[] cs;
                switch (nComponents)  // FIXME How to implement Tuple6 implementation? By Tuple9? ...
                {
                    case 1:
                        cs = new double[] { a.GetTuple1(i) };
                        break;
                    case 2:
                        cs = a.GetTuple2(i);
                        break;
                    case 3:
                        cs = a.GetTuple3(i);
                        break;
                    case 4:
                        cs = a.GetTuple4(i);
                        break;
                    case 9:
                        cs = a.GetTuple9(i);
                        break;
                    default:
                        throw new NotImplementedException(string.Format("Tuple with {0} components", nComponents));
                }
                foreach (double c in cs)
                {
                    data.Add((float)c);
                }
            }
            return new Scimesh.Base.MeshPointFieldNullable(name, nComponents, data.ToArray(), m);
        };

        /// <summary>
        /// Read XmlUnstructuredGrid's PointDataArray To MeshPointField with external Mesh.
        /// Used to create MeshPointField without creating a new Mesh. 
        /// It's convienent for constant in time Mesh, when we need to create the same Mesh from each time step file.
        /// </summary>
        public static readonly Func<string, int, Scimesh.Base.Mesh, Scimesh.Base.MeshPointField> rXmlUGridPDArrayToMPFieldNoMesh = (absPath, arrayIndex, m) =>
        {
            vtkXMLUnstructuredGridReader reader = vtkXMLUnstructuredGridReader.New();
            reader.SetFileName(absPath);
            reader.Update();
            vtkUnstructuredGrid ug = vtkUnstructuredGrid.SafeDownCast(reader.GetOutput());
            //UnityEngine.Debug.Log(ug);
            vtkInformation info = ug.GetInformation();
            //UnityEngine.Debug.Log(info);
            vtkPointData pd = ug.GetPointData();
            //UnityEngine.Debug.Log(pd);
            vtkDataArray a = pd.GetArray(arrayIndex);
            //UnityEngine.Debug.Log(a);
            string name = a.GetName();
            int nComponents = a.GetNumberOfComponents();
            long nPoints = a.GetNumberOfTuples();
            List<float> data = new List<float>();
            for (long i = 0; i < nPoints; i++)
            {
                double[] cs;
                switch (nComponents)  // FIXME How to implement Tuple6 implementation? By Tuple9? ...
                {
                    case 1:
                        cs = new double[] { a.GetTuple1(i) };
                        break;
                    case 2:
                        cs = a.GetTuple2(i);
                        break;
                    case 3:
                        cs = a.GetTuple3(i);
                        break;
                    case 4:
                        cs = a.GetTuple4(i);
                        break;
                    case 9:
                        cs = a.GetTuple9(i);
                        break;
                    default:
                        throw new NotImplementedException(string.Format("Tuple with {0} components", nComponents));
                }
                foreach (double c in cs)
                {
                    data.Add((float)c);
                }
            }
            return new Scimesh.Base.MeshPointField(name, nComponents, data.ToArray(), m);
        };

        /// <summary>
        /// Read XmlUnstructuredGrid's CellDataArray To CellPointField
        /// </summary>
        public static readonly Func<string, int, Scimesh.Base.MeshCellFieldNullable> rXmlUGridCDArrayToMCField = (absPath, arrayIndex) =>
        {
            vtkXMLUnstructuredGridReader reader = vtkXMLUnstructuredGridReader.New();
            reader.SetFileName(absPath);
            reader.Update();
            vtkUnstructuredGrid ug = vtkUnstructuredGrid.SafeDownCast(reader.GetOutput());
            //UnityEngine.Debug.Log(ug);
            vtkInformation info = ug.GetInformation();
            //UnityEngine.Debug.Log(info);
            vtkCellData cd = ug.GetCellData();
            //UnityEngine.Debug.Log(cd);
            vtkDataArray a = cd.GetArray(arrayIndex);
            //UnityEngine.Debug.Log(a);
            string name = a.GetName();
            int nComponents = a.GetNumberOfComponents();
            long nPoints = a.GetNumberOfTuples();
            List<float?> data = new List<float?>();
            for (long i = 0; i < nPoints; i++)
            {
                double[] cs;
                switch (nComponents)  // FIXME Tuple6 implementation? By Tuple9? ...
                {
                    case 1:
                        cs = new double[] { a.GetTuple1(i) };
                        break;
                    case 2:
                        cs = a.GetTuple2(i);
                        break;
                    case 3:
                        cs = a.GetTuple3(i);
                        break;
                    case 4:
                        cs = a.GetTuple4(i);
                        break;
                    case 9:
                        cs = a.GetTuple9(i);
                        break;
                    default:
                        throw new NotImplementedException(string.Format("Tuple with {0} components", nComponents));
                }
                foreach (double c in cs)
                {
                    data.Add((float)c);
                }
            }
            Scimesh.Base.Mesh m = uGridToMesh(ug);
            Scimesh.Base.MeshCellFieldNullable mcf = new Scimesh.Base.MeshCellFieldNullable(name, nComponents, data.ToArray(), m);
            return mcf;
        };

        /// <summary>
        /// Read XmlUnstructuredGrid's CellDataArray To MeshCellFieldNullable with external Mesh.
        /// Used to create MeshCellField without creating a new Mesh.
        /// It's convienent for constant in time Mesh, when we need to create the same Mesh from each time step file.
        /// </summary>
        public static readonly Func<string, int, Scimesh.Base.Mesh, Scimesh.Base.MeshCellFieldNullable> rXmlUGridCDArrayToMCFNullableNoMesh = (absPath, arrayIndex,m) =>
        {
            vtkXMLUnstructuredGridReader reader = vtkXMLUnstructuredGridReader.New();
            reader.SetFileName(absPath);
            reader.Update();
            vtkUnstructuredGrid ug = vtkUnstructuredGrid.SafeDownCast(reader.GetOutput());
            //UnityEngine.Debug.Log(ug);
            vtkInformation info = ug.GetInformation();
            //UnityEngine.Debug.Log(info);
            vtkCellData cd = ug.GetCellData();
            //UnityEngine.Debug.Log(cd);
            vtkDataArray a = cd.GetArray(arrayIndex);
            //UnityEngine.Debug.Log(a);
            string name = a.GetName();
            int nComponents = a.GetNumberOfComponents();
            long nPoints = a.GetNumberOfTuples();
            List<float?> data = new List<float?>();
            for (long i = 0; i < nPoints; i++)
            {
                double[] cs;
                switch (nComponents)  // FIXME Tuple6 implementation? By Tuple9? ...
                {
                    case 1:
                        cs = new double[] { a.GetTuple1(i) };
                        break;
                    case 2:
                        cs = a.GetTuple2(i);
                        break;
                    case 3:
                        cs = a.GetTuple3(i);
                        break;
                    case 4:
                        cs = a.GetTuple4(i);
                        break;
                    case 9:
                        cs = a.GetTuple9(i);
                        break;
                    default:
                        throw new NotImplementedException(string.Format("Tuple with {0} components", nComponents));
                }
                foreach (double c in cs)
                {
                    data.Add((float)c);
                }
            }
            return new Scimesh.Base.MeshCellFieldNullable(name, nComponents, data.ToArray(), m);
        };

        /// <summary>
        /// Read XmlUnstructuredGrid's CellDataArray To MeshCellField with external Mesh.
        /// Used to create MeshCellField without creating a new Mesh.
        /// It's convienent for constant in time Mesh, when we need to create the same Mesh from each time step file.
        /// </summary>
        public static readonly Func<string, int, Scimesh.Base.Mesh, Scimesh.Base.MeshCellField> rXmlUGridCDArrayToMCFieldNoMesh = (absPath, arrayIndex, m) =>
        {
            vtkXMLUnstructuredGridReader reader = vtkXMLUnstructuredGridReader.New();
            reader.SetFileName(absPath);
            reader.Update();
            vtkUnstructuredGrid ug = vtkUnstructuredGrid.SafeDownCast(reader.GetOutput());
            //UnityEngine.Debug.Log(ug);
            vtkInformation info = ug.GetInformation();
            //UnityEngine.Debug.Log(info);
            vtkCellData cd = ug.GetCellData();
            //UnityEngine.Debug.Log(cd);
            vtkDataArray a = cd.GetArray(arrayIndex);
            //UnityEngine.Debug.Log(a);
            string name = a.GetName();
            int nComponents = a.GetNumberOfComponents();
            long nPoints = a.GetNumberOfTuples();
            List<float> data = new List<float>();
            for (long i = 0; i < nPoints; i++)
            {
                double[] cs;
                switch (nComponents)  // FIXME Tuple6 implementation? By Tuple9? ...
                {
                    case 1:
                        cs = new double[] { a.GetTuple1(i) };
                        break;
                    case 2:
                        cs = a.GetTuple2(i);
                        break;
                    case 3:
                        cs = a.GetTuple3(i);
                        break;
                    case 4:
                        cs = a.GetTuple4(i);
                        break;
                    case 9:
                        cs = a.GetTuple9(i);
                        break;
                    default:
                        throw new NotImplementedException(string.Format("Tuple with {0} components", nComponents));
                }
                foreach (double c in cs)
                {
                    data.Add((float)c);
                }
            }
            return new Scimesh.Base.MeshCellField(name, nComponents, data.ToArray(), m);
        };

        /// <summary>
        /// Read XmlMultiBlockData To Mesh
        /// </summary>
        public static readonly Func<string, Scimesh.Base.Mesh[]> rXmlMBDataToMesh = (absPath) =>
        {
            vtkXMLMultiBlockDataReader reader = vtkXMLMultiBlockDataReader.New();
            reader.SetFileName(absPath);
            reader.Update();
            vtkMultiBlockDataSet multiBlock = vtkMultiBlockDataSet.SafeDownCast(reader.GetOutput());
            vtkInformation[] metaData = Activiz.readXmlMultiBlockMetaData(absPath); // FIXME Workaround because Activiz doesn't read MetaData
            Scimesh.Base.Mesh[] meshes = new Scimesh.Base.Mesh[multiBlock.GetNumberOfBlocks()];
            for (uint i = 0; i < multiBlock.GetNumberOfBlocks(); i++)
            {
                UnityEngine.Debug.Log(string.Format("Block: {0}", i));
                //UnityEngine.Debug.Log(string.Format("Has metadata: {0}", multiBlock.HasMetaData(i)));
                //vtkInformation info = multiBlock.GetMetaData(i);
                vtkInformation info = metaData[i]; // FIXME Workaround because Activiz doesn't read MetaData
                UnityEngine.Debug.Log(info.Get(vtkCompositeDataSet.DATA_PIECE_NUMBER()));
                UnityEngine.Debug.Log(info.Get(vtkCompositeDataSet.NAME()));
                UnityEngine.Debug.Log(info.Get(vtkCompositeDataSet.FIELD_NAME()));
                vtkUnstructuredGrid ug = vtkUnstructuredGrid.SafeDownCast(multiBlock.GetBlock(i));
                meshes[i] = uGridToMesh(ug);
            }
            return meshes;
        };

        /// <summary>
        /// Unstructured Grid To Mesh
        /// </summary>
        public static readonly Func<vtkUnstructuredGrid, Scimesh.Base.Mesh> uGridToMesh = (ug) =>
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Scimesh.Base.Point[] points = new Scimesh.Base.Point[ug.GetNumberOfPoints()];
            for (int i = 0; i < ug.GetNumberOfPoints(); i++)
            {
                double[] cs = ug.GetPoint(i);
                points[i] = new Scimesh.Base.Point(new float[] { (float)cs[0], (float)cs[2], (float)cs[1] });
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format("Points importing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            stopwatch = Stopwatch.StartNew();
            Scimesh.Base.Cell[] cells = new Scimesh.Base.Cell[ug.GetNumberOfCells()];
            int[] cellTypes = new int[ug.GetNumberOfCells()];
            for (int i = 0; i < ug.GetNumberOfCells(); i++)
            {
                vtkCell cell = ug.GetCell(i);
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
            UnityEngine.Debug.Log(string.Format("Faces importing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            UnityEngine.Debug.Log(string.Format("Number of scimesh points: {0}", points.Length));
            UnityEngine.Debug.Log(string.Format("Number of scimesh faces: {0}", faces.Count));
            UnityEngine.Debug.Log(string.Format("Number of scimesh cells: {0}", cells.Length));
            return new Scimesh.Base.Mesh(points, faces.ToArray(), cells);
        };
    }
}