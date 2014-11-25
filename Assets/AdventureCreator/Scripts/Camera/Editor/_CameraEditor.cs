using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(_Camera))]

public class _CameraEditor : Editor
{
	
	public override void OnInspectorGUI()
	{
		EditorGUILayout.HelpBox ("Attach this script to a custom Camera type to integrate it with Adventure Creator.", MessageType.Info);
	}

}
