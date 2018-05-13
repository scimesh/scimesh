using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ActivizToUnity : MonoBehaviour
{
    //[HideInInspector]
    //public Scimesh.Base.Mesh[] ms;
    //[HideInInspector]
    //public Scimesh.Base.MeshFilter[] mfs;
    public string polydataFilename;
    public string xmlMultiBlockDataFilename;
    public Material mat;

    public void Clear()
    {
        UnityEngine.Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Clearing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
    }

    public void PolydataToUnity()
    {
        UnityEngine.Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
        Clear();
        Stopwatch stopwatch = Stopwatch.StartNew();
        Scimesh.Base.Mesh[] ms = new Scimesh.Base.Mesh[1];
        Scimesh.Base.MeshFilter[] mfs = new Scimesh.Base.MeshFilter[1];
        ms[0] = Scimesh.Third.Activiz.To.Base.polydataToMesh(polydataFilename);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Reading time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        UnityEngine.Debug.Log("Serialising");
        stopwatch = Stopwatch.StartNew();
        long size = 0;
        using (Stream stream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, ms[0]);
            size = stream.Length;
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log("Scimesh size: " + size.ToString() + " bytes");
        UnityEngine.Debug.Log(string.Format("Elapsed time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        UnityEngine.Debug.Log("Scimesh evaluations");
        stopwatch = Stopwatch.StartNew();
        ms[0].EvaluateCellsNeighbourCells();
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Elapsed time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        stopwatch = Stopwatch.StartNew();
        ms[0].EvaluatePointsNeighbourPoints(Scimesh.Base.Mesh.Neighbours.InFaces);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Elapsed time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        UnityEngine.Debug.Log("Scimesh MeshFilter initialisation");
        stopwatch = Stopwatch.StartNew();
        int[] cellIndices = new int[ms[0].cells.Length];
        for (int i = 0; i < cellIndices.Length; i++)
        {
            cellIndices[i] = i;
        }
        mfs[0] = new Scimesh.Base.MeshFilter(new int[0], new int[0], new int[0], cellIndices);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Elapsed time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        UnityEngine.Debug.Log("Unity mesh initialisation");
        stopwatch = Stopwatch.StartNew();
        Mesh[] unityMeshes = Scimesh.Base.To.Unity.MeshToUnityMesh(ms[0], mfs[0]);
        foreach (Transform child in gameObject.transform)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }
        for (int i = 0; i < unityMeshes.Length; i++)
        {
            GameObject childMesh = new GameObject();
            childMesh.transform.parent = gameObject.transform;
            MeshFilter meshFilter = childMesh.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = unityMeshes[i];
            MeshRenderer meshRenderer = childMesh.AddComponent<MeshRenderer>();
            meshRenderer.material = mat;
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Elapsed time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
    }

    public void XmlMultiBlockDataToUnity()
    {
        UnityEngine.Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
        Clear();
        Stopwatch stopwatch = Stopwatch.StartNew();
        Scimesh.Base.Mesh[] ms = Scimesh.Third.Activiz.To.Base.xmlMultiBlockDataToMesh(xmlMultiBlockDataFilename);
        Scimesh.Base.MeshFilter[] mfs = new Scimesh.Base.MeshFilter[ms.Length];
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Reading time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        for (int i = 0; i < ms.Length; i++)
        {
            // Scimesh to UnityMesh
            stopwatch = Stopwatch.StartNew();
            //// Serialize Scimesh
            //long size = 0;
            //using (Stream stream = new MemoryStream())
            //{
            //    BinaryFormatter formatter = new BinaryFormatter();
            //    formatter.Serialize(stream, ms[i]);
            //    size = stream.Length;
            //}
            //stopwatch.Stop();
            //UnityEngine.Debug.Log("Scimesh size: " + size.ToString() + " bytes");
            //UnityEngine.Debug.Log("Scimesh serializing time: " + stopwatch.ElapsedMilliseconds);
            //// Scimesh procedures
            //stopwatch = Stopwatch.StartNew();
            //ms[i].EvaluateCellsNeighbourCells();
            //stopwatch.Stop();
            //UnityEngine.Debug.Log("EvaluateCellsNeighbourCells time: " + stopwatch.ElapsedMilliseconds + " ms");
            //stopwatch = Stopwatch.StartNew();
            //ms[i].EvaluatePointsNeighbourPoints(Scimesh.Base.Mesh.Neighbours.InFaces);
            //stopwatch.Stop();
            //UnityEngine.Debug.Log("EvaluatePointsNeighbourPoints time: " + stopwatch.ElapsedMilliseconds + " ms");
            //// Set Scimesh MeshFilter
            //stopwatch = Stopwatch.StartNew();
            ////int[] cellIndices = new int[ms[i].cells.Length];
            ////for (int j = 0; j < cellIndices.Length; j++)
            ////{
            ////    cellIndices[j] = j;
            ////}
            ////mfs[i] = new Scimesh.Base.MeshFilter(cellIndices);
            //stopwatch.Stop();
            //UnityEngine.Debug.Log("Scimesh to UnityMesh " + stopwatch.ElapsedMilliseconds + " ms");
            // Scimesh to UnityMesh
            //stopwatch = Stopwatch.StartNew();
            mfs[i] = Scimesh.Base.To.Base.boundaryFacesMeshFilter2(ms[i]);
            Mesh[] ums = Scimesh.Base.To.Unity.MeshToUnityMesh(ms[i], mfs[i]);
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format("Scimesh to UnityMesh time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
            // Unity
            stopwatch = Stopwatch.StartNew();
            GameObject blockMesh = new GameObject();
            blockMesh.transform.parent = gameObject.transform;
            for (int j = 0; j < ums.Length; j++)
            {
                GameObject childMesh = new GameObject();
                childMesh.transform.parent = blockMesh.gameObject.transform;
                MeshFilter meshFilter = childMesh.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = ums[j];
                MeshRenderer meshRenderer = childMesh.AddComponent<MeshRenderer>();
                meshRenderer.material = mat;
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format("Unity time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        }
    }

}
