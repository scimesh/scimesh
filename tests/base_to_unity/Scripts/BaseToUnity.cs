using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BaseToUnity : MonoBehaviour
{
    public Material mat;
    public int meshPointFieldType;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TestMeshPointFieldToUnity()
    {
        // Clear
        Stopwatch stopwatch = Stopwatch.StartNew();
        foreach (Transform child in transform)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log("Clear " + stopwatch.ElapsedMilliseconds + " ms");
        // Create MeshPointField
        stopwatch = Stopwatch.StartNew();
        Scimesh.Base.MeshPointField mpf = Scimesh.Base.To.Base.testMeshPointField(meshPointFieldType);
        //UnityEngine.Debug.Log(mpf.MaxValueIndex);
        //UnityEngine.Debug.Log(mpf.MinValueIndex);
        //UnityEngine.Debug.Log(mpf.MaxValueMagnitude);
        //UnityEngine.Debug.Log(mpf.MinValueMagnitude);
        //for (int i = 0; i < mpf.NValues; i++)
        //{
        //    foreach (float? comp in mpf[i])
        //    {
        //        UnityEngine.Debug.Log(comp);
        //    }
        //    UnityEngine.Debug.Log(mpf.GetNormedValue(i));
        //}
        //UnityEngine.Debug.Log(mpf.MaxValue);
        //UnityEngine.Debug.Log(mpf.MinValue);
        stopwatch.Stop();
        UnityEngine.Debug.Log("Create MeshPointField " + stopwatch.ElapsedMilliseconds + " ms");
        // Scimesh to UnityMesh
        stopwatch = Stopwatch.StartNew();
        Mesh[] ms = Scimesh.Base.To.Unity.MeshPointFieldToUnityMesh(
            mpf,
            Scimesh.Base.To.Base.allFacesMeshFilter(mpf.Mesh),
            Scimesh.Color.Colormaps.dictionary[Scimesh.Color.Colormaps.Name.RainbowAlphaBlendedTransparent]);
        for (int i = 0; i < ms.Length; i++)
        {
            GameObject childMesh = new GameObject();
            childMesh.transform.parent = gameObject.transform;
            MeshFilter meshFilter = childMesh.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = ms[i];
            MeshRenderer meshRenderer = childMesh.AddComponent<MeshRenderer>();
            meshRenderer.material = mat;
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log("Scimesh to UnityMesh " + stopwatch.ElapsedMilliseconds + " ms");
    }
}
