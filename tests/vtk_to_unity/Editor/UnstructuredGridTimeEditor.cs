using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnstructuredGridTime))]
public class UnstructuredGridTimeToUnityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UnstructuredGridTime myScript = (UnstructuredGridTime)target;

        if (GUILayout.Button("Clear"))
        {
            myScript.Clear();
        }
        if (GUILayout.Button("Read"))
        {
            myScript.Read();
        }
        if (GUILayout.Button("UpdateField"))
        {
            myScript.UpdateField();
        }
    }
}
