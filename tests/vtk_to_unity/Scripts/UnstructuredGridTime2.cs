using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Scimesh.Unity
{
    public class UnstructuredGridTime2 : MonoBehaviour
    {
        [Tooltip("Any (to avoid serialization conflicts")]
        public string name;
        public string dirpath;
        public string filename;
        public enum FieldType
        {
            Point,
            Cell
        };
        [Tooltip("Type of field to import")]
        public FieldType fieldType;
        public int fieldIndex;
        public Material mat;
        public Color.Colormap.Name colormap;
        public int Colormap
        {
            get { return (int)colormap; }
            set
            {
                colormap = (Color.Colormap.Name)value;
                UpdateField();
            }
        }
        Base.Mesh m;
        Base.MeshPointField mpf;
        public enum MeshFilterType
        {
            BoundaryFaces,
            AllFaces,
            PlaneFaces,
            PlaneFacesUserCenter,
            PlaneCells,
            PlaneCellsUserCenter,
            SphereCellsUserCenter,
            Threshold
        };
        [Tooltip("Type of Mesh Filter")]
        public MeshFilterType filterType;
        public int FilterType
        {
            get { return (int)filterType; }
            set
            {
                filterType = (MeshFilterType)value;
                UpdateMeshFilter();
                UpdateMaps();
                UpdateMesh();
                UpdateField();
            }
        }
        Base.MeshFilter mf;
        int[][] maps;
        public bool twoSided;
        List<GameObject> meshes;
        public Vector3 planeCenter;
        public Vector3 planeNormal;
        public Vector3 sphereCenter;
        public float sphereRadius;
        public float SphereRadius { get { return sphereRadius; } set { sphereRadius = value; } }
        public float nMinThreshold;
        public float NMinThreshold { get { return nMinThreshold; } set { nMinThreshold = value; } }
        public float nMaxThreshold;
        public float NMaxThreshold { get { return nMaxThreshold; } set { nMaxThreshold = value; } }
        
        // Use this for initialization
        void Start()
        {
            meshes = new List<GameObject>();
            LoadMesh3();
            LoadField3();
            LoadMeshFilter3();
            //UpdateMeshFilter();
            UpdateMaps();
            UpdateMesh();
            UpdateField();
        }

        public void ClearMesh()
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }
            meshes.Clear();
        }
        public void Clear()
        {
            m = null;
            mf = null;
            mpf = null;
            maps = null;
        }
        public void Read()
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
            string relPath = Path.Combine(dirpath, filename);
            string absPath = Path.Combine(Application.dataPath, relPath);
            m = Third.Activiz.To.Base.rXmlUGridToMesh(absPath);
            SaveMesh3();
            UpdateMeshFilter();
            SaveMeshFilter3();
            if (fieldType == FieldType.Point)
            {
                mpf = Third.Activiz.To.Base.rXmlUGridPDArrayToMPFieldNoMesh(absPath, fieldIndex, m);
            }
            else
            {
                mpf = Base.To.Base.cellFieldToPointField(Third.Activiz.To.Base.rXmlUGridCDArrayToMCFieldNoMesh(absPath, fieldIndex, m));
            }
            SaveField3();
            Clear();
        }

        public void LoadMesh()
        {
            // Mesh
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            TextAsset asset = Resources.Load("m") as TextAsset;
            long length = 0;
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                length = ms.Length;
                using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    m = (Base.Mesh)bf.Deserialize(gs);
                    // Not faster algo ...
                    //using (MemoryStream dms = new MemoryStream())
                    //{
                    //    byte[] buffer = new byte[4096];
                    //    int count = 0;
                    //    while ((count = gs.Read(buffer, 0, buffer.Length)) != 0)
                    //    {
                    //        dms.Write(buffer, 0, count);
                    //    }
                    //    dms.Position = 0;
                    //    BinaryFormatter bf = new BinaryFormatter();
                    //    m = (Base.Mesh)bf.Deserialize(dms);
                    //    length = dms.Length;
                    //}
                }
            }
            stopwatch.Stop();
            Debug.Log("M size: " + length.ToString() + " bytes");
            Debug.Log("M deserializing time: " + stopwatch.ElapsedMilliseconds + " ms");
        }
        public void LoadMesh2()
        {
            // Mesh Points
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshPointData mpd;
            TextAsset asset = Resources.Load("m_points") as TextAsset;
            long length = 0;
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                length = ms.Length;
                using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    mpd = (MeshPointData)bf.Deserialize(gs);
                }
            }
            stopwatch.Stop();
            Debug.Log("M Points size: " + length.ToString() + " bytes");
            Debug.Log("M Points deserializing time: " + stopwatch.ElapsedMilliseconds + " ms");
            // Mesh Edges
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshEdgeData med;
            asset = Resources.Load("m_edges") as TextAsset;
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                length = ms.Length;
                using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    med = (MeshEdgeData)bf.Deserialize(gs);
                }
            }
            stopwatch.Stop();
            Debug.Log("M Edges size: " + length.ToString() + " bytes");
            Debug.Log("M Edges deserializing time: " + stopwatch.ElapsedMilliseconds + " ms");
            // Mesh Faces
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshFaceData mfd;
            asset = Resources.Load("m_faces") as TextAsset;
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                length = ms.Length;
                using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    mfd = (MeshFaceData)bf.Deserialize(gs);
                }
            }
            stopwatch.Stop();
            Debug.Log("M Faces size: " + length.ToString() + " bytes");
            Debug.Log("M Faces deserializing time: " + stopwatch.ElapsedMilliseconds + " ms");
            // Mesh Cells
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshCellData mcd;
            asset = Resources.Load("m_cells") as TextAsset;
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                length = ms.Length;
                using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    mcd = (MeshCellData)bf.Deserialize(gs);
                }
            }
            stopwatch.Stop();
            Debug.Log("M Cells size: " + length.ToString() + " bytes");
            Debug.Log("M Cells deserializing time: " + stopwatch.ElapsedMilliseconds + " ms");
            m = new Base.Mesh(mpd.GetPoints(), med.GetEdges(), mfd.GetFaces(), mcd.GetCells());
        }
        public void LoadMesh3()
        {
            // Mesh Points
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshPointData mpd;
            TextAsset asset = Resources.Load("m_points_" + name) as TextAsset;
            long length = 0;
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                length = ms.Length;
                BinaryFormatter bf = new BinaryFormatter();
                mpd = (MeshPointData)bf.Deserialize(ms);
            }
            stopwatch.Stop();
            Debug.Log("M Points size: " + length.ToString() + " bytes");
            Debug.Log(string.Format("M Points deserializing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            // Mesh Edges
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshEdgeData med;
            asset = Resources.Load("m_edges_" + name) as TextAsset;
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                length = ms.Length;
                BinaryFormatter bf = new BinaryFormatter();
                med = (MeshEdgeData)bf.Deserialize(ms);
            }
            stopwatch.Stop();
            Debug.Log("M Edges size: " + length.ToString() + " bytes");
            Debug.Log(string.Format("M Edges deserializing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            // Mesh Faces
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshFaceData mfd;
            asset = Resources.Load("m_faces_" + name) as TextAsset;
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                length = ms.Length;
                BinaryFormatter bf = new BinaryFormatter();
                mfd = (MeshFaceData)bf.Deserialize(ms);
            }
            stopwatch.Stop();
            Debug.Log("M Faces size: " + length.ToString() + " bytes");
            Debug.Log(string.Format("M Faces deserializing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            // Mesh Cells
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshCellData mcd;
            asset = Resources.Load("m_cells_" + name) as TextAsset;
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                length = ms.Length;
                BinaryFormatter bf = new BinaryFormatter();
                mcd = (MeshCellData)bf.Deserialize(ms);
            }
            stopwatch.Stop();
            Debug.Log("M Cells size: " + length.ToString() + " bytes");
            Debug.Log(string.Format("M Cells deserializing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            m = new Base.Mesh(mpd.GetPoints(), med.GetEdges(), mfd.GetFaces(), mcd.GetCells());
        }
        public void LoadField()
        {
            // MeshFilter
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            TextAsset asset = Resources.Load("mf") as TextAsset;
            long length = 0;
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                length = ms.Length;
                using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    mf = (Base.MeshFilter)bf.Deserialize(gs);
                }
            }
            stopwatch.Stop();
            Debug.Log("MF size: " + length.ToString() + " bytes");
            Debug.Log("MF deserializing time: " + stopwatch.ElapsedMilliseconds + " ms");
            // Mesh Point Field
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            asset = Resources.Load("mpf") as TextAsset;
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                length = ms.Length;
                using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    mpf = (Base.MeshPointField)bf.Deserialize(gs);
                    mpf.Mesh = m;
                }
            }
            stopwatch.Stop();
            Debug.Log("MPF size: " + length.ToString() + " bytes");
            Debug.Log("MPF deserializing time: " + stopwatch.ElapsedMilliseconds + " ms");
        }
        public void LoadField3()
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            TextAsset asset = Resources.Load("mpf_" + name) as TextAsset;
            long length = 0;
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                length = ms.Length;
                BinaryFormatter bf = new BinaryFormatter();
                mpf = (Base.MeshPointField)bf.Deserialize(ms);
                mpf.Mesh = m;
            }
            stopwatch.Stop();
            Debug.Log("MPF size: " + length.ToString() + " bytes");
            Debug.Log(string.Format("MPF deserializing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        }
        public void LoadMeshFilter3()
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            TextAsset asset = Resources.Load("mf_" + name) as TextAsset;
            long length = 0;
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                length = ms.Length;
                BinaryFormatter bf = new BinaryFormatter();
                mf = (Base.MeshFilter)bf.Deserialize(ms);
            }
            stopwatch.Stop();
            Debug.Log("MF size: " + length.ToString() + " bytes");
            Debug.Log(string.Format("MF deserializing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        }
        public void SaveMesh()
        {
            // Mesh
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (FileStream fs = new FileStream(Path.Combine(Application.dataPath,
                "scimesh/tests/vtk_to_unity/Resources/m.bytes"), FileMode.Create))
            {
                using (GZipStream gs = new GZipStream(fs, CompressionMode.Compress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(gs, m);
                }
            }
            stopwatch.Stop();
            Debug.Log("M serializing time: " + stopwatch.ElapsedMilliseconds + " ms");
        }
        public void SaveMesh2()
        {
            // Mesh Points
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshPointData mpd = new MeshPointData(m.points);
            using (FileStream fs = new FileStream(Path.Combine(Application.dataPath,
                "scimesh/tests/vtk_to_unity/Resources/m_points.bytes"), FileMode.Create))
            {
                using (GZipStream gs = new GZipStream(fs, CompressionMode.Compress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(gs, mpd);
                }
            }
            stopwatch.Stop();
            Debug.Log("M Points serializing time: " + stopwatch.ElapsedMilliseconds + " ms");
            // Mesh Edges
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshEdgeData med = new MeshEdgeData(m.edges);
            using (FileStream fs = new FileStream(Path.Combine(Application.dataPath,
                "scimesh/tests/vtk_to_unity/Resources/m_edges.bytes"), FileMode.Create))
            {
                using (GZipStream gs = new GZipStream(fs, CompressionMode.Compress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(gs, med);
                }
            }
            stopwatch.Stop();
            Debug.Log("M Edges serializing time: " + stopwatch.ElapsedMilliseconds + " ms");
            // Mesh Faces
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshFaceData mfd = new MeshFaceData(m.faces);
            using (FileStream fs = new FileStream(Path.Combine(Application.dataPath,
                "scimesh/tests/vtk_to_unity/Resources/m_faces.bytes"), FileMode.Create))
            {
                using (GZipStream gs = new GZipStream(fs, CompressionMode.Compress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(gs, mfd);
                }
            }
            stopwatch.Stop();
            Debug.Log("M Faces serializing time: " + stopwatch.ElapsedMilliseconds + " ms");
            // Mesh Cells
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshCellData mcd = new MeshCellData(m.cells);
            using (FileStream fs = new FileStream(Path.Combine(Application.dataPath,
                "scimesh/tests/vtk_to_unity/Resources/m_cells.bytes"), FileMode.Create))
            {
                using (GZipStream gs = new GZipStream(fs, CompressionMode.Compress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(gs, mcd);
                }
            }
            stopwatch.Stop();
            Debug.Log("M Cells serializing time: " + stopwatch.ElapsedMilliseconds + " ms");
        }
        public void SaveMesh3()
        {
            // Mesh Points
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshPointData mpd = new MeshPointData(m.points);
            using (FileStream fs = new FileStream(Path.Combine(Application.dataPath,
                "scimesh/tests/vtk_to_unity/Resources/m_points_" + name + ".bytes"), FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, mpd);
            }
            stopwatch.Stop();
            Debug.Log(string.Format("M Points serializing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            // Mesh Edges
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshEdgeData med = new MeshEdgeData(m.edges);
            using (FileStream fs = new FileStream(Path.Combine(Application.dataPath,
                "scimesh/tests/vtk_to_unity/Resources/m_edges_" + name + ".bytes"), FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, med);
            }
            stopwatch.Stop();
            Debug.Log(string.Format("M Edges serializing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            // Mesh Faces
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshFaceData mfd = new MeshFaceData(m.faces);
            using (FileStream fs = new FileStream(Path.Combine(Application.dataPath,
                "scimesh/tests/vtk_to_unity/Resources/m_faces_" + name + ".bytes"), FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, mfd);
            }
            stopwatch.Stop();
            Debug.Log(string.Format("M Faces serializing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            // Mesh Cells
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            MeshCellData mcd = new MeshCellData(m.cells);
            using (FileStream fs = new FileStream(Path.Combine(Application.dataPath,
                "scimesh/tests/vtk_to_unity/Resources/m_cells_" + name + ".bytes"), FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, mcd);
            }
            stopwatch.Stop();
            Debug.Log(string.Format("M Cells serializing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        }
        public void SaveField()
        {
            // Mesh Filter
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (FileStream fs = new FileStream(Path.Combine(Application.dataPath,
                "scimesh/tests/vtk_to_unity/Resources/mf.bytes"), FileMode.Create))
            {
                using (GZipStream gs = new GZipStream(fs, CompressionMode.Compress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(gs, mf);
                }
            }
            stopwatch.Stop();
            Debug.Log("MF serializing time: " + stopwatch.ElapsedMilliseconds + " ms");
            // Mesh Point Field
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (FileStream fs = new FileStream(Path.Combine(Application.dataPath,
                "scimesh/tests/vtk_to_unity/Resources/mpf.bytes"), FileMode.Create))
            {
                using (GZipStream gs = new GZipStream(fs, CompressionMode.Compress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(gs, mpf);
                }
            }
            stopwatch.Stop();
            Debug.Log("MPF serializing time: " + stopwatch.ElapsedMilliseconds + " ms");
        }
        public void SaveField3()
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (FileStream fs = new FileStream(Path.Combine(Application.dataPath,
                "scimesh/tests/vtk_to_unity/Resources/mpf_" + name + ".bytes"), FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, mpf);
            }
            stopwatch.Stop();
            Debug.Log(string.Format("MPF serializing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        }
        public void SaveMeshFilter3()
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (FileStream fs = new FileStream(Path.Combine(Application.dataPath,
                "scimesh/tests/vtk_to_unity/Resources/mf_" + name + ".bytes"), FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, mf);
            }
            stopwatch.Stop();
            Debug.Log(string.Format("MF serializing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        }

        public void UpdateMeshFilter()
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            float[] center;
            float[] normal;
            switch (filterType)
            {
                case MeshFilterType.BoundaryFaces:
                    mf = Base.To.Base.boundaryFacesMeshFilter2(m);
                    break;
                case MeshFilterType.AllFaces:
                    mf = Base.To.Base.allFacesMeshFilter(m);
                    break;
                case MeshFilterType.PlaneFaces:
                    center = new float[] { 0, 0, 0 };
                    normal = new float[] { planeNormal.x, planeNormal.y, planeNormal.z };
                    mf = Base.To.Base.planeFacesMeshFilter(m, center, normal);
                    break;
                case MeshFilterType.PlaneFacesUserCenter:
                    center = new float[] { planeCenter.x, planeCenter.y, planeCenter.z };
                    normal = new float[] { planeNormal.x, planeNormal.y, planeNormal.z };
                    mf = Base.To.Base.planeFacesMeshFilter(m, center, normal);
                    break;
                case MeshFilterType.PlaneCells:
                    center = new float[] { 0, 0, 0 };
                    normal = new float[] { planeNormal.x, planeNormal.y, planeNormal.z };
                    mf = Base.To.Base.planeCellsMeshFilter(m, center, normal);
                    break;
                case MeshFilterType.PlaneCellsUserCenter:
                    center = new float[] { planeCenter.x, planeCenter.y, planeCenter.z };
                    normal = new float[] { planeNormal.x, planeNormal.y, planeNormal.z };
                    mf = Base.To.Base.planeCellsMeshFilter(m, center, normal);
                    break;
                case MeshFilterType.SphereCellsUserCenter:
                    center = new float[] { sphereCenter.x, sphereCenter.y, sphereCenter.z };
                    mf = Base.To.Base.sphereCellsMeshFilter(m, center, SphereRadius);
                    break;
                case MeshFilterType.Threshold:
                    mf = Base.To.Base.pointFieldThresholdCellsMeshFilter(mpf, NMinThreshold, NMaxThreshold);
                    break;
                default:
                    mf = Base.To.Base.boundaryFacesMeshFilter2(m);
                    break;
            }
            Debug.Log(string.Format("UpdateMeshFilter time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        }
        public void UpdateMaps()
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            if (twoSided)
            {
                maps = Base.To.Unity.UMsVerticesToMPointsMapsTwoSided(m, mf);
            }
            else
            {
                maps = Base.To.Unity.UMsVerticesToMPointsMaps(m, mf);
            }
            Debug.Log(string.Format("UpdateMaps time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        }
        public void UpdateMesh()
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            ClearMesh();
            Mesh[] ums = Base.To.Unity.MeshToUMeshesByMaps(m, maps);
            for (int i = 0; i < ums.Length; i++)
            {
                GameObject childMesh = new GameObject();
                childMesh.transform.SetParent(gameObject.transform, false);
                MeshFilter meshFilter = childMesh.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = ums[i];
                MeshRenderer meshRenderer = childMesh.AddComponent<MeshRenderer>();
                meshRenderer.material = mat;
                meshes.Add(childMesh);
            }
            Debug.Log(string.Format("UpdateMesh time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        }
        public void UpdateField()
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            float[][] meshesNormedValues = Base.To.Unity.MPFieldToUMsNValuesByMaps(mpf, maps);
            for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].GetComponent<MeshFilter>().sharedMesh.colors =
                    Color.To.Unity.ColormapColorToUnityColorArray(
                        Color.Colormap.Get(colormap), meshesNormedValues[i]);
            }
            Debug.Log(string.Format("UpdateField time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        }


        [Serializable]
        public struct MeshPointData
        {
            public float[] coordinates;
            public int[] nCoordinates;
            public MeshPointData(Base.Point[] points)
            {
                List<float> coordinates = new List<float>();
                List<int> nCoordinates = new List<int>();
                foreach (Base.Point point in points)
                {
                    nCoordinates.Add(point.coordinates.Length);
                    coordinates.AddRange(point.coordinates);
                }
                this.coordinates = coordinates.ToArray();
                this.nCoordinates = nCoordinates.ToArray();
            }
            public Base.Point[] GetPoints()
            {
                List<Base.Point> points = new List<Base.Point>();
                int cnt = 0;
                for (int i = 0; i < nCoordinates.Length; i++)
                {
                    int ncs = nCoordinates[i];
                    float[] cs = new float[ncs];
                    Array.Copy(coordinates, cnt, cs, 0, ncs);
                    points.Add(new Base.Point(cs));
                    cnt += ncs;
                }
                return points.ToArray();
            }
        }
        [Serializable]
        public struct MeshEdgeData
        {
            public int[] points;
            public MeshEdgeData(Base.Edge[] edges)
            {
                List<int> points = new List<int>();
                foreach (Base.Edge face in edges)
                {
                    points.Add(face.pointsIndices.Length);
                    points.AddRange(face.pointsIndices);
                }
                this.points = points.ToArray();
            }
            public Base.Edge[] GetEdges()
            {
                List<int[]> edgesPoints = new List<int[]>();
                int cnt = 0;
                while (cnt < points.Length)
                {
                    int nPoints = points[cnt];
                    int[] edgePoints = new int[nPoints];
                    Array.Copy(points, cnt + 1, edgePoints, 0, nPoints);
                    edgesPoints.Add(edgePoints);
                    cnt += nPoints + 1;
                }
                List<Base.Edge> edges = new List<Base.Edge>();
                for (int i = 0; i < edgesPoints.Count; i++)
                {
                    edges.Add(new Base.Edge(edgesPoints[i]));
                }
                return edges.ToArray();
            }
        }
        [Serializable]
        public struct MeshFaceData
        {
            public int[] points;
            public int[] edges;
            public MeshFaceData(Base.Face[] faces)
            {
                List<int> points = new List<int>();
                List<int> edges = new List<int>();
                foreach (Base.Face face in faces)
                {
                    points.Add(face.pointsIndices.Length);
                    points.AddRange(face.pointsIndices);
                    edges.Add(face.edgesIndices.Length);
                    edges.AddRange(face.edgesIndices);
                }
                this.points = points.ToArray();
                this.edges = edges.ToArray();
            }
            public Base.Face[] GetFaces()
            {
                List<int[]> facesPoints = new List<int[]>();
                List<int[]> facesEdges = new List<int[]>();
                int cnt = 0;
                while (cnt < points.Length)
                {
                    int nPoints = points[cnt];
                    int[] facePoints = new int[nPoints];
                    Array.Copy(points, cnt + 1, facePoints, 0, nPoints);
                    facesPoints.Add(facePoints);
                    cnt += nPoints + 1;
                }
                cnt = 0;
                while (cnt < edges.Length)
                {
                    int nEdges = edges[cnt];
                    int[] faceEdges = new int[nEdges];
                    Array.Copy(edges, cnt + 1, faceEdges, 0, nEdges);
                    facesEdges.Add(faceEdges);
                    cnt += nEdges + 1;
                }
                List<Base.Face> faces = new List<Base.Face>();
                for (int i = 0; i < facesPoints.Count; i++)
                {
                    faces.Add(new Base.Face(facesPoints[i]));
                }
                for (int i = 0; i < facesEdges.Count; i++)
                {
                    faces[i].edgesIndices = facesEdges[i];
                }
                return faces.ToArray();
            }
        }
        [Serializable]
        public struct MeshCellData
        {
            public int[] points;
            public int[] faces;
            public MeshCellData(Base.Cell[] cells)
            {
                List<int> points = new List<int>();
                List<int> faces = new List<int>();
                foreach (Base.Cell cell in cells)
                {
                    points.Add(cell.pointsIndices.Length);
                    points.AddRange(cell.pointsIndices);
                    faces.Add(cell.facesIndices.Length);
                    faces.AddRange(cell.facesIndices);
                }
                this.points = points.ToArray();
                this.faces = faces.ToArray();
            }
            public Base.Cell[] GetCells()
            {
                List<int[]> cellsPoints = new List<int[]>();
                List<int[]> cellsFaces = new List<int[]>();
                int cnt = 0;
                while (cnt < points.Length)
                {
                    int nPoints = points[cnt];
                    int[] cellPoints = new int[nPoints];
                    Array.Copy(points, cnt + 1, cellPoints, 0, nPoints);
                    cellsPoints.Add(cellPoints);
                    cnt += nPoints + 1;
                }
                cnt = 0;
                while (cnt < faces.Length)
                {
                    int nFaces = faces[cnt];
                    int[] cellFaces = new int[nFaces];
                    Array.Copy(faces, cnt + 1, cellFaces, 0, nFaces);
                    cellsFaces.Add(cellFaces);
                    cnt += nFaces + 1;
                }
                List<Base.Cell> cells = new List<Base.Cell>();
                for (int i = 0; i < cellsPoints.Count; i++)
                {
                    cells.Add(new Base.Cell(cellsPoints[i]));
                }
                for (int i = 0; i < cellsFaces.Count; i++)
                {
                    cells[i].facesIndices = cellsFaces[i];
                }
                return cells.ToArray();
            }
        }
    }
}
