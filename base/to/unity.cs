using UnityEngine;
using System.Collections.Generic;
using System;

namespace Scimesh.Base.To
{
    /// <summary>
    /// Base to Unity functions
    /// </summary>
    public static class Unity
    {
        /// <summary>
        /// Unity restriction on max vertices in the mesh.
        /// </summary>
        public const int MAX_MESH_VERTICES = 65534; // 21844 separated (with no shared vertices) triangles

        /// <summary>
        /// Scimesh Mesh to Unity Mesh.
        /// </summary>
        public static readonly Func<Mesh, MeshFilter, UnityEngine.Mesh[]> MeshToUnityMesh = (m, mf) =>
        {
            // Unity Meshes vertices and triangles (with max vertices[i].Length == MAX_MESH_VERTICES)
            List<Vector3[]> vs = new List<Vector3[]>();
            List<int[]> ts = new List<int[]>();
            // Convert Cells
            List<int> meshTriangles = new List<int>();
            List<Vector3> meshVertices = new List<Vector3>();
            int vertexCnt = 0;
            foreach (int cellIndex in mf.cellsIndices)
            {
                for (int j = 0; j < m.cells[cellIndex].facesIndices.Length; j++)
                {
                    if (vertexCnt <= MAX_MESH_VERTICES - 2)
                    {
                        int[] facePointsIndices = m.faces[m.cells[cellIndex].facesIndices[j]].pointsIndices;
                        for (int k = 0; k < facePointsIndices.Length; k++)
                        {
                            float[] coordinates = m.points[facePointsIndices[k]].coordinates;
                            meshVertices.Add(new Vector3(coordinates[0], coordinates[1], coordinates[2]));
                            meshTriangles.Add(vertexCnt);
                            vertexCnt += 1;
                        }
                    }
                    else
                    {
                        vs.Add(meshVertices.ToArray());
                        ts.Add(meshTriangles.ToArray());
                        meshVertices = new List<Vector3>();
                        meshTriangles = new List<int>();
                        vertexCnt = 0;
                    }
                }
            }
            // Remaining vertices and triangles
            if (vertexCnt > 0)
            {
                vs.Add(meshVertices.ToArray());
                ts.Add(meshTriangles.ToArray());
            }
            // Convert Faces
            meshTriangles = new List<int>();
            meshVertices = new List<Vector3>();
            vertexCnt = 0;
            foreach (int faceIndex in mf.facesIndices)
            {
                if (vertexCnt <= MAX_MESH_VERTICES - 2)
                {
                    int[] facePointsIndices = m.faces[faceIndex].pointsIndices;
                    for (int k = 0; k < facePointsIndices.Length; k++)
                    {
                        float[] coordinates = m.points[facePointsIndices[k]].coordinates;
                        meshVertices.Add(new Vector3(coordinates[0], coordinates[1], coordinates[2]));
                        meshTriangles.Add(vertexCnt);
                        vertexCnt += 1;
                    }
                }
                else
                {
                    vs.Add(meshVertices.ToArray());
                    ts.Add(meshTriangles.ToArray());
                    meshVertices = new List<Vector3>();
                    meshTriangles = new List<int>();
                    vertexCnt = 0;
                }
            }
            // Remaining vertices and triangles
            if (vertexCnt > 0)
            {
                vs.Add(meshVertices.ToArray());
                ts.Add(meshTriangles.ToArray());
            }
            // Create and return Unity Meshes
            UnityEngine.Mesh[] unityMeshes = new UnityEngine.Mesh[vs.Count];
            for (int i = 0; i < unityMeshes.Length; i++)
            {
                unityMeshes[i] = new UnityEngine.Mesh();
                unityMeshes[i].vertices = vs[i];
                unityMeshes[i].triangles = ts[i];
                unityMeshes[i].RecalculateBounds();
                unityMeshes[i].RecalculateNormals();
                unityMeshes[i].RecalculateTangents();
            }
            return unityMeshes;
        };

        /// <summary>
        /// Scimesh MeshPointField to Unity Mesh.
        /// </summary>
        public static readonly Func<MeshPointField, MeshFilter, Scimesh.Color.Colormap, UnityEngine.Mesh[]> MeshPointFieldToUnityMesh = (pf, mf, cm) =>
        {
            // Unity Meshes vertices and triangles (with max vertices[i].Length == MAX_MESH_VERTICES)
            List<Vector3[]> vs = new List<Vector3[]>();
            List<int[]> ts = new List<int[]>();
            List<UnityEngine.Color[]> cs = new List<UnityEngine.Color[]>();
            // Convert Cells
            List<int> meshTriangles = new List<int>();
            List<Vector3> meshVertices = new List<Vector3>();
            List<UnityEngine.Color> verticesColors = new List<UnityEngine.Color>();
            int vertexCnt = 0;
            foreach (int cellIndex in mf.cellsIndices)
            {
                for (int j = 0; j < pf.Mesh.cells[cellIndex].facesIndices.Length; j++)
                {
                    if (vertexCnt <= MAX_MESH_VERTICES - 2)
                    {
                        int[] facePointsIndices = pf.Mesh.faces[pf.Mesh.cells[cellIndex].facesIndices[j]].pointsIndices;
                        for (int k = 0; k < facePointsIndices.Length; k++)
                        {
                            float[] coordinates = pf.Mesh.points[facePointsIndices[k]].coordinates;
                            meshVertices.Add(new Vector3(coordinates[0], coordinates[1], coordinates[2]));
                            meshTriangles.Add(vertexCnt);
                            verticesColors.Add(Scimesh.Color.To.Unity.ColormapColorToUnityColorNullable(cm, pf.GetNormedValue(facePointsIndices[k])));
                            vertexCnt += 1;
                        }
                    }
                    else
                    {
                        vs.Add(meshVertices.ToArray());
                        ts.Add(meshTriangles.ToArray());
                        cs.Add(verticesColors.ToArray());
                        meshVertices = new List<Vector3>();
                        meshTriangles = new List<int>();
                        verticesColors = new List<UnityEngine.Color>();
                        vertexCnt = 0;
                    }
                }
            }
            // Remaining vertices and triangles
            if (vertexCnt > 0)
            {
                vs.Add(meshVertices.ToArray());
                ts.Add(meshTriangles.ToArray());
                cs.Add(verticesColors.ToArray());
            }
            // Convert Faces
            meshTriangles = new List<int>();
            meshVertices = new List<Vector3>();
            verticesColors = new List<UnityEngine.Color>();
            vertexCnt = 0;
            foreach (int faceIndex in mf.facesIndices)
            {
                // Face triangulation
                int nVertices = (pf.Mesh.faces[faceIndex].pointsIndices.Length - 2) * 3;
                if (vertexCnt <= MAX_MESH_VERTICES - nVertices + 1)
                {
                    Face[] fs = Base.triangulateFace(pf.Mesh.faces[faceIndex]);
                    foreach (Face f in fs)
                    {
                        int[] facePointsIndices = f.pointsIndices;
                        for (int k = 0; k < facePointsIndices.Length; k++)
                        {
                            float[] coordinates = pf.Mesh.points[facePointsIndices[k]].coordinates;
                            meshVertices.Add(new Vector3(coordinates[0], coordinates[1], coordinates[2]));
                            meshTriangles.Add(vertexCnt);
                            verticesColors.Add(Scimesh.Color.To.Unity.ColormapColorToUnityColorNullable(cm, pf.GetNormedValue(facePointsIndices[k])));
                            vertexCnt += 1;
                        }
                    }
                }
                else
                {
                    vs.Add(meshVertices.ToArray());
                    ts.Add(meshTriangles.ToArray());
                    cs.Add(verticesColors.ToArray());
                    meshVertices = new List<Vector3>();
                    meshTriangles = new List<int>();
                    verticesColors = new List<UnityEngine.Color>();
                    vertexCnt = 0;
                    Face[] fs = Base.triangulateFace(pf.Mesh.faces[faceIndex]);
                    foreach (Face f in fs)
                    {
                        int[] facePointsIndices = f.pointsIndices;
                        for (int k = 0; k < facePointsIndices.Length; k++)
                        {
                            float[] coordinates = pf.Mesh.points[facePointsIndices[k]].coordinates;
                            meshVertices.Add(new Vector3(coordinates[0], coordinates[1], coordinates[2]));
                            meshTriangles.Add(vertexCnt);
                            verticesColors.Add(Scimesh.Color.To.Unity.ColormapColorToUnityColorNullable(cm, pf.GetNormedValue(facePointsIndices[k])));
                            vertexCnt += 1;
                        }
                    }
                }
            }
            // Remaining vertices and triangles
            if (vertexCnt > 0)
            {
                vs.Add(meshVertices.ToArray());
                ts.Add(meshTriangles.ToArray());
                cs.Add(verticesColors.ToArray());
            }
            // Create and return Unity Meshes
            UnityEngine.Mesh[] unityMeshes = new UnityEngine.Mesh[vs.Count];
            for (int i = 0; i < unityMeshes.Length; i++)
            {
                unityMeshes[i] = new UnityEngine.Mesh();
                unityMeshes[i].vertices = vs[i];
                unityMeshes[i].triangles = ts[i];
                unityMeshes[i].colors = cs[i];
                unityMeshes[i].RecalculateBounds();
                unityMeshes[i].RecalculateNormals();
                unityMeshes[i].RecalculateTangents();
            }
            return unityMeshes;
        };

        /// <summary>
        /// Maps from Unity Meshes vertices to Scimesh Mesh points.
        /// Each Unity Mesh's number of vertices limited by MAX_MESH_VERTICES,
        /// so number of Unity Meshes for one Scimesh Mesh can be greater than 1.
        /// Scimesh Mesh pointIndex = maps[i][j], where i - index of Unity Mesh, j - index of Unity Mesh vertex.
        /// </summary>
        public static readonly Func<Mesh, MeshFilter, int[][]> UMsVerticesToMPointsMaps = (m, mf) =>
        {
            List<int[]> maps = new List<int[]>();
            // Convert Cells
            List<int> map = new List<int>();
            int vertexCnt = 0;
            foreach (int cellIndex in mf.cellsIndices)
            {
                foreach (int faceIndex in m.cells[cellIndex].facesIndices)
                {
                    int nVertices = (m.faces[faceIndex].pointsIndices.Length - 2) * 3;
                    if (vertexCnt <= MAX_MESH_VERTICES - nVertices + 1)
                    {
                        Face[] fs = Base.triangulateFace(m.faces[faceIndex]);
                        foreach (Face f in fs)
                        {
                            foreach (int pointIndex in f.pointsIndices)
                            {
                                map.Add(pointIndex);
                                vertexCnt += 1;
                            }
                        }
                    }
                    else
                    {
                        maps.Add(map.ToArray());
                        map = new List<int>();
                        vertexCnt = 0;
                        Face[] fs = Base.triangulateFace(m.faces[faceIndex]);
                        foreach (Face f in fs)
                        {
                            foreach (int pointIndex in f.pointsIndices)
                            {
                                map.Add(pointIndex);
                                vertexCnt += 1;
                            }
                        }
                    }
                }
            }
            if (vertexCnt > 0)
            {
                maps.Add(map.ToArray());
            }
            // Convert Faces
            map = new List<int>();
            vertexCnt = 0;
            foreach (int faceIndex in mf.facesIndices)
            {
                int nVertices = (m.faces[faceIndex].pointsIndices.Length - 2) * 3;
                if (vertexCnt <= MAX_MESH_VERTICES - nVertices + 1)
                {
                    Face[] fs = Base.triangulateFace(m.faces[faceIndex]);
                    foreach (Face f in fs)
                    {
                        foreach (int pointIndex in f.pointsIndices)
                        {
                            map.Add(pointIndex);
                            vertexCnt += 1;
                        }
                    }
                }
                else
                {
                    maps.Add(map.ToArray());
                    map = new List<int>();
                    vertexCnt = 0;
                    Face[] fs = Base.triangulateFace(m.faces[faceIndex]);
                    foreach (Face f in fs)
                    {
                        foreach (int pointIndex in f.pointsIndices)
                        {
                            map.Add(pointIndex);
                            vertexCnt += 1;
                        }
                    }
                }
            }
            if (vertexCnt > 0)
            {
                maps.Add(map.ToArray());
            }
            return maps.ToArray();
        };

        /// <summary>
        /// Scimesh Mesh to Unity Mesh by maps from Unity Meshes vertices to Scimesh Mesh points.
        /// </summary>
        public static readonly Func<Mesh, int[][], UnityEngine.Mesh[]> MeshToUMeshesByMaps = (m, maps) =>
        {
            UnityEngine.Mesh[] ums = new UnityEngine.Mesh[maps.Length];
            for (int i = 0; i < maps.Length; i++)
            {
                List<Vector3> vs = new List<Vector3>();
                List<int> ts = new List<int>();
                for (int j = 0; j < maps[i].Length; j++)
                {
                    float[] coordinates = m.points[maps[i][j]].coordinates;
                    vs.Add(new Vector3(coordinates[0], coordinates[1], coordinates[2]));
                    ts.Add(j);
                }
                ums[i] = new UnityEngine.Mesh();
                ums[i].vertices = vs.ToArray();
                ums[i].triangles = ts.ToArray();
                ums[i].RecalculateBounds();
                ums[i].RecalculateNormals();
                ums[i].RecalculateTangents();
            }
            return ums;
        };

        /// <summary>
        /// MeshPointField to normed nullable values arrays corresponding to Unity Mesh vertices/colors arrays,
        /// by maps from Unity Meshes vertices to Scimesh Mesh points.
        /// </summary>
        public static readonly Func<MeshPointField, int[][], float?[][]> MPFieldToUMsNValuesByMapsNullable = (mpf, maps) =>
        {
            List<float?[]> umsNormdedValues = new List<float?[]>();
            for (int i = 0; i < maps.Length; i++)
            {
                List<float?> normedValues = new List<float?>();
                for (int j = 0; j < maps[i].Length; j++)
                {
                    normedValues.Add(mpf.GetNormedValue(maps[i][j]));
                }
                umsNormdedValues.Add(normedValues.ToArray());
            }
            return umsNormdedValues.ToArray();
        };

        /// <summary>
        /// MeshPointField to normed  values arrays corresponding to Unity Mesh vertices/colors arrays,
        /// by maps from Unity Meshes vertices to Scimesh Mesh points.
        /// </summary>
        public static readonly Func<MeshPointField, int[][], float[][]> MPFieldToUMsNValuesByMaps = (mpf, maps) =>
        {
            List<float[]> umsNormdedValues = new List<float[]>();
            for (int i = 0; i < maps.Length; i++)
            {
                List<float> normedValues = new List<float>();
                for (int j = 0; j < maps[i].Length; j++)
                {
                    normedValues.Add(mpf.GetNormedValue(maps[i][j]) ?? 0);
                }
                umsNormdedValues.Add(normedValues.ToArray());
            }
            return umsNormdedValues.ToArray();
        };
    }
}
