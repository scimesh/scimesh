using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class UnstructuredGridTime : MonoBehaviour {
    public bool multiblock;
    public string dirpath;
    public string filename;
    public int timeIndex;
    public int arrayIndex;
    public Scimesh.Color.GetColormap.Name colormapName;
    public Material mat;
    public List<GameObject> meshes;
    public List<MeshesNormedValues> timesMeshesNormedValues;
    public List<float> times;

    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
        }
        meshes.Clear();
        timesMeshesNormedValues.Clear();
        times.Clear();
    }

    public void Read()
    {
        string relPath = Path.Combine(dirpath, filename);
        UnityEngine.Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
        Clear();
        Scimesh.Base.Mesh m = Scimesh.Third.Activiz.To.Base.rXmlUGridToMesh(relPath);
        Scimesh.Base.MeshFilter mf = Scimesh.Base.To.Base.boundaryFacesMeshFilter2(m);
        Scimesh.Base.MeshPointField mpf = Scimesh.Third.Activiz.To.Base.rXmlUGridPDArrayToMPFieldNoMesh(relPath, arrayIndex, m);
        int[][] maps = Scimesh.Base.To.Unity.UMsVerticesToMPointsMaps(m, mf);
        Mesh[] ums = Scimesh.Base.To.Unity.MeshToUMeshesByMaps(m, maps);
        SetMeshes(ums);
        float[][] meshesNormedValues = Scimesh.Base.To.Unity.MPFieldToUMsNValuesByMaps(mpf, maps);
        times = new List<float>();
        times.Add(0);
        times.Add(1);
        List<float[][]> timesMeshesNormedValues = new List<float[][]>();
        timesMeshesNormedValues.Add(meshesNormedValues);
        timesMeshesNormedValues.Add(meshesNormedValues);
        SetNormedValues(timesMeshesNormedValues); // Workaround of a serialization problem
    }

    public void UpdateField()
    {
        MeshesNormedValues mnvs = timesMeshesNormedValues[timeIndex];
        for (int i = 0; i < meshes.Count; i++)
        {
            meshes[i].GetComponent<MeshFilter>().sharedMesh.colors = Scimesh.Color.To.Unity.ColormapColorToUnityColorArray(
                    Scimesh.Color.GetColormap.byName[colormapName], mnvs.meshesValues[i].values);
        }
    }

    void SetMeshes(Mesh[] ums)
    {
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
    }

    /// <summary>
    /// Workaround of Unity serialization problems:
    /// Need to serialize 3D List<float[][]>() array, but Unity allows to serialize only 1D arrays...
    /// To address this problem two additional container-classes 
    /// (MeshNormedValues and MeshesNormedValues) are created.
    /// </summary>
    void SetNormedValues(List<float[][]> timesMeshesNormedValues)
    {
        for (int i = 0; i < timesMeshesNormedValues.Count; i++)
        {
            List<MeshNormedValues> vs = new List<MeshNormedValues>();
            for (int j = 0; j < timesMeshesNormedValues[i].Length; j++)
            {
                vs.Add(new MeshNormedValues(timesMeshesNormedValues[i][j]));
            }
            this.timesMeshesNormedValues.Add(new MeshesNormedValues(vs.ToArray()));
        }
    }

    /// </summary>
    /// Additional container-class
    /// </summary>
    [Serializable]
    public class MeshNormedValues
    {
        public float[] values;

        public MeshNormedValues(float[] values)
        {
            this.values = values;
        }
    }

    /// </summary>
    /// Additional container-class
    /// </summary>
    [Serializable]
    public class MeshesNormedValues
    {
        public MeshNormedValues[] meshesValues;

        public MeshesNormedValues(MeshNormedValues[] meshesValues)
        {
            this.meshesValues = meshesValues;
        }
    }
}
