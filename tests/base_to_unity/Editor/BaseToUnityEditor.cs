using UnityEngine;
using UnityEditor;

namespace Scimesh.Unity
{
    [CustomEditor(typeof(BaseToUnity))]
    public class BaseToUnityEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BaseToUnity myScript = (BaseToUnity)target;

            if (GUILayout.Button("Clear"))
            {
                myScript.Clear();
            }

            if (GUILayout.Button("TestMeshPointField to Unity"))
            {
                myScript.TestMeshPointFieldToUnity();
            }
        }
    }
}