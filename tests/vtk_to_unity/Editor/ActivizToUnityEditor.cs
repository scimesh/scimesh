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
        if (GUILayout.Button("ReadPolydata to Unity"))
        {
            myScript.ReadPolydataToUnity();
        }
        if (GUILayout.Button("ReadXmlMultiBlockData to Unity"))
        {
            myScript.ReadXmlMultiBlockDataToUnity();
        }
        if (GUILayout.Button("ReadXmlMultiBlockMetaDataToUnity to Unity"))
        {
            myScript.ReadXmlMultiBlockMetaDataToUnity();
        }
    }
}