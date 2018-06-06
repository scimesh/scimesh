using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

namespace Scimesh.Unity
{
    public class UnstructuredGridTime : MonoBehaviour
    {
        public enum FieldType
        {
            Point,
            Cell
        };
        [Tooltip("Type of field to import")]
        public FieldType fieldType;
        public enum StructureType
        {
            Single,
            Multiblock
        }
        [Tooltip("Type of files structure to import")]
        public StructureType structureType;
        public string dirpath;
        public string filename;
        public int timeIndex;
        public int fieldIndex;
        public int arrayIndex;
        public Color.Colormap.Name colormap;
        public Material mat;
        [HideInInspector]
        public List<float> times;
        [HideInInspector]
        public List<string> fields;
        [HideInInspector]
        public List<GameObject> meshes;
        [HideInInspector]
        public List<MeshesNormedValues> timesMeshesNormedValues;

        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }
            meshes.Clear();
            timesMeshesNormedValues.Clear();
            times.Clear();
            fields.Clear();
        }
        public void Read()
        {
            string relPath = Path.Combine(dirpath, filename);
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
            Clear();
            Base.Mesh m = Scimesh.Third.Activiz.To.Base.rXmlUGridToMesh(relPath);
            Base.MeshFilter mf = Base.To.Base.boundaryFacesMeshFilter2(m);
            Base.MeshPointField mpf;
            if (fieldType == FieldType.Point)
            {
                mpf = Scimesh.Third.Activiz.To.Base.rXmlUGridPDArrayToMPFieldNoMesh(relPath, arrayIndex, m);

            }
            else
            {
                Base.MeshCellField mcf = Scimesh.Third.Activiz.To.Base.rXmlUGridCDArrayToMCFieldNoMesh(relPath, arrayIndex, m);
                mpf = Base.To.Base.cellFieldToPointField(mcf);
            }
            int[][] maps = Base.To.Unity.UMsVerticesToMPointsMaps(m, mf);
            Mesh[] ums = Base.To.Unity.MeshToUMeshesByMaps(m, maps);
            SetMeshes(ums);
            float[][] meshesNormedValues = Base.To.Unity.MPFieldToUMsNValuesByMaps(mpf, maps);
            times = new List<float>();
            times.Add(0);
            times.Add(1);
            fields = new List<string>();
            fields.Add(mpf.Name);
            fields.Add(mpf.Name);
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
                meshes[i].GetComponent<MeshFilter>().sharedMesh.colors =
                    Color.To.Unity.ColormapColorToUnityColorArray(
                        Color.Colormap.Get(colormap), mnvs.meshesValues[i].values);
            }
        }
        void SetMeshes(Mesh[] ums)
        {
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
        }

        /// <summary>
        /// Workaround of Unity serialization problems:
        /// Needs to serialize 3D List<float[][]>() array, but Unity allows to serialize only 1D arrays...
        /// Addressing this problem by using two additional container-classes MeshNormedValues and MeshesNormedValues.
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
}
