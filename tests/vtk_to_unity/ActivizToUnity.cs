using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ActivizToUnity : MonoBehaviour
{
    [HideInInspector]
    public Scimesh.Base.Mesh mesh;
    [HideInInspector]
    public Scimesh.Base.MeshFilter mf;
    public string filename;
    public Material meshMaterial;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ActivizToMesh()
    {
        // Activiz to Scimesh
        Stopwatch stopwatch = Stopwatch.StartNew();
        mesh = Scimesh.Third.Activiz.To.Base.activizToMesh(filename);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format(
               "Elapsed time: {0:E0} ms, {1:E0} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks));
        // Serialize Scimesh
        stopwatch = Stopwatch.StartNew();
        long size = 0;
        using (Stream stream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, mesh);
            size = stream.Length;
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log("Scimesh size: " + size.ToString() + " bytes");
        UnityEngine.Debug.Log("Scimesh serializing time: " + stopwatch.ElapsedMilliseconds);
        // Scimesh procedures
        stopwatch = Stopwatch.StartNew();
        mesh.EvaluateCellsNeighbourCells();
        stopwatch.Stop();
        UnityEngine.Debug.Log("EvaluateCellsNeighbourCells time: " + stopwatch.ElapsedMilliseconds + " ms");
        stopwatch = Stopwatch.StartNew();
        mesh.EvaluatePointsNeighbourPoints(Scimesh.Base.Mesh.Neighbours.InFaces);
        stopwatch.Stop();
        UnityEngine.Debug.Log("EvaluatePointsNeighbourPoints time: " + stopwatch.ElapsedMilliseconds + " ms");
        // Set Scimesh MeshFilter
        stopwatch = Stopwatch.StartNew();
        int[] cellIndices = new int[mesh.cells.Length];
        for (int i = 0; i < cellIndices.Length; i++)
        {
            cellIndices[i] = i;
        }
        mf = new Scimesh.Base.MeshFilter(cellIndices);
        stopwatch.Stop();
        UnityEngine.Debug.Log("Scimesh to UnityMesh " + stopwatch.ElapsedMilliseconds + " ms");
        // Scimesh to UnityMesh
        stopwatch = Stopwatch.StartNew();
        Mesh[] unityMeshes = Scimesh.Base.To.Unity.MeshToUnityMesh(mesh, mf);
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
            meshRenderer.material = meshMaterial;
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log("Scimesh to UnityMesh " + stopwatch.ElapsedMilliseconds + " ms");
    }
}
