using UnityEngine;
using UnityEditor;

namespace Scimesh.Unity
{
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
                EditorUtility.SetDirty(target);  // For save changes to play mode
            }
            if (GUILayout.Button("Read"))
            {
                myScript.Read();
                EditorUtility.SetDirty(target);  // For save changes to play mode
            }
            if (GUILayout.Button("UpdateField"))
            {
                myScript.UpdateField();
                EditorUtility.SetDirty(target);  // For save changes to play mode
            }
        }
    }
}
