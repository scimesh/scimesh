using System.Diagnostics;
using UnityEngine;

namespace Scimesh.Unity
{
    public class BaseToUnity : MonoBehaviour
    {
        public Material mat;
        public int meshPointFieldType;

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

        public void TestMeshPointFieldToUnity()
        {
            UnityEngine.Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
            Clear();
            // Create MeshPointField
            Stopwatch stopwatch = Stopwatch.StartNew();
            Scimesh.Base.MeshPointFieldNullable mpf = Scimesh.Base.To.Base.testMeshPointField(meshPointFieldType);
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
            Mesh[] ms = Base.To.Unity.MeshPointFieldToUnityMesh(
                mpf,
                Base.To.Base.boundaryFacesMeshFilter2(mpf.Mesh),
                //Scimesh.Base.To.Base.boundaryFacesMeshFilter(mpf.Mesh),
                //Scimesh.Base.To.Base.allFacesMeshFilter(mpf.Mesh),
                Color.Colormap.Get(Color.Colormap.Name.RainbowAlphaBlendedTransparent));
            stopwatch.Stop();
            UnityEngine.Debug.Log("Scimesh to UnityMesh " + stopwatch.ElapsedMilliseconds + " ms");
            // Scimesh Unity
            stopwatch = Stopwatch.StartNew();
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
            UnityEngine.Debug.Log("Unity " + stopwatch.ElapsedMilliseconds + " ms");
        }
    }
}

