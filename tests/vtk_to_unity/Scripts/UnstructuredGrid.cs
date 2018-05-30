using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class UnstructuredGrid : MonoBehaviour
{
    [Tooltip("Relative to Assets folder")]
    public string path;
    [Tooltip("Material for Unity Mesh")]
    public Material mat;
    [Tooltip("Point/Cell DataArray index at UnstructuredGrid")]
    public int index;

    public List<GameObject> meshes;

    public void Clear()
    {
        UnityEngine.Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
        }
        meshes.Clear();
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Clearing time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
    }

    public void ReadXmlUGridToUnity()
    {
        UnityEngine.Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
        Clear();
        // Read XML file
        Stopwatch stopwatch = Stopwatch.StartNew();
        Scimesh.Base.Mesh m = Scimesh.Third.Activiz.To.Base.rXmlUGridToMesh(path);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Reading time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        // Create MeshFilter
        stopwatch = Stopwatch.StartNew();
        Scimesh.Base.MeshFilter mf = Scimesh.Base.To.Base.boundaryFacesMeshFilter2(m);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("MeshFilter creating time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        // Scimesh Mesh to Unity Mesh
        stopwatch = Stopwatch.StartNew();
        Mesh[] ums = Scimesh.Base.To.Unity.MeshToUnityMesh(m, mf);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Scimesh to UnityMesh time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        // Unity
        stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < ums.Length; i++)
        {
            GameObject childMesh = new GameObject();
            childMesh.transform.parent = gameObject.transform;
            MeshFilter meshFilter = childMesh.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = ums[i];
            MeshRenderer meshRenderer = childMesh.AddComponent<MeshRenderer>();
            meshRenderer.material = mat;
            meshes.Add(childMesh);
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Unity time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
    }

    public void ReadXmlUGridPArrayToUnity()
    {
        UnityEngine.Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
        Clear();
        // Read XML file
        Stopwatch stopwatch = Stopwatch.StartNew();
        Scimesh.Base.MeshPointField mpf = Scimesh.Third.Activiz.To.Base.rXmlUGridPDArrayToMPField(path, index);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Reading time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        // Create MeshFilter
        stopwatch = Stopwatch.StartNew();
        Scimesh.Base.MeshFilter mf = Scimesh.Base.To.Base.boundaryFacesMeshFilter2(mpf.Mesh);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("MeshFilter creating time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        // Scimesh Mesh to Unity Mesh
        stopwatch = Stopwatch.StartNew();
        Mesh[] ums = Scimesh.Base.To.Unity.MeshPointFieldToUnityMesh(mpf, mf, Scimesh.Color.GetColormap.byName[Scimesh.Color.GetColormap.Name.RainbowAlpha]);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Scimesh to UnityMesh time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        // Unity
        stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < ums.Length; i++)
        {
            GameObject childMesh = new GameObject();
            childMesh.transform.parent = gameObject.transform;
            MeshFilter meshFilter = childMesh.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = ums[i];
            MeshRenderer meshRenderer = childMesh.AddComponent<MeshRenderer>();
            meshRenderer.material = mat;
            meshes.Add(childMesh);
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Unity time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
    }

    public void ReadXmlUGridCArrayToUnity()
    {
        UnityEngine.Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
        Clear();
        // Read XML file
        Stopwatch stopwatch = Stopwatch.StartNew();
        Scimesh.Base.MeshCellField mcf = Scimesh.Third.Activiz.To.Base.rXmlUGridCDArrayToMCField(path, index);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Reading time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        //UnityEngine.Debug.Log(mcf);
        // Convert Cell Field to Point Field
        stopwatch = Stopwatch.StartNew();
        Scimesh.Base.MeshPointField mpf = Scimesh.Base.To.Base.cellFieldToPointField(mcf);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("MeshCellField to MeshPointField converting time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        //UnityEngine.Debug.Log(mpf);
        // Create MeshFilter
        stopwatch = Stopwatch.StartNew();
        Scimesh.Base.MeshFilter mf = Scimesh.Base.To.Base.boundaryFacesMeshFilter2(mpf.Mesh);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("MeshFilter creating time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        // Scimesh Mesh to Unity Mesh
        stopwatch = Stopwatch.StartNew();
        Mesh[] ums = Scimesh.Base.To.Unity.MeshPointFieldToUnityMesh(mpf, mf, Scimesh.Color.GetColormap.byName[Scimesh.Color.GetColormap.Name.RainbowAlpha]);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Scimesh to UnityMesh time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        // Unity
        stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < ums.Length; i++)
        {
            GameObject childMesh = new GameObject();
            childMesh.transform.parent = gameObject.transform;
            MeshFilter meshFilter = childMesh.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = ums[i];
            MeshRenderer meshRenderer = childMesh.AddComponent<MeshRenderer>();
            meshRenderer.material = mat;
            meshes.Add(childMesh);
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Unity time: {0} ms, {1} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
    }
}
