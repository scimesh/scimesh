using UnityEditor;
using UnityEngine;

namespace Scimesh.Unity
{
    [CustomEditor(typeof(UnstructuredGridTime2))]
    public class UnstructuredGridTime2Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            UnstructuredGridTime2 myScript = (UnstructuredGridTime2)target;

            if (GUILayout.Button("Read"))
            {
                myScript.Read();
                //EditorUtility.SetDirty(target);  // For save changes to play mode
            }
        }
    }
}
