using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(VtkToUnity))]
public class TestEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		VtkToUnity myScript = (VtkToUnity)target;

		if (GUILayout.Button ("VTK Polydata to Unity mesh")) {
			myScript.PolyDataToMesh ();
		}
	}
}
