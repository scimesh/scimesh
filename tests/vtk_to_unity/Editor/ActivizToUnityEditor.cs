using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActivizToUnity))]
public class ActivizToUnityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ActivizToUnity myScript = (ActivizToUnity)target;

        if (GUILayout.Button("Clear"))
        {
            myScript.Clear();
        }
        if (GUILayout.Button("Polydata to Unity"))
        {
            myScript.PolydataToUnity();
        }
        if (GUILayout.Button("XmlMultiBlockData to Unity"))
        {
            myScript.XmlMultiBlockDataToUnity();
        }
    }
}