using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseToBase : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        Scimesh.Base.Mesh m = Scimesh.Base.To.Base.mesh(0);
        //foreach (Scimesh.Base.Point p in m.points)
        //{
        //    UnityEngine.Debug.Log(p);
        //    foreach (int f in p.facesIndices)
        //    {
        //        UnityEngine.Debug.Log(f);
        //    }
        //}
        Scimesh.Base.MeshFilter mf = Scimesh.Base.To.Base.boundaryFacesMeshFilter(m);
        foreach (int f in mf.facesIndices)
        {
            UnityEngine.Debug.Log(f);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
