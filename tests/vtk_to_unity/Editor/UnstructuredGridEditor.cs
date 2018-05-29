using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnstructuredGrid))]
public class UnstructuredGridToUnityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UnstructuredGrid myScript = (UnstructuredGrid)target;

        if (GUILayout.Button("Clear"))
        {
            myScript.Clear();
        }
        if (GUILayout.Button("ReadXml"))
        {
            myScript.ReadXmlUGridToUnity();
        }
        if (GUILayout.Button("ReadXmlPointArray"))
        {
            myScript.ReadXmlUGridPArrayToUnity();
        }
        if (GUILayout.Button("ReadXmlCellArray"))
        {
            myScript.ReadXmlUGridCArrayToUnity();
        }
    }
}
