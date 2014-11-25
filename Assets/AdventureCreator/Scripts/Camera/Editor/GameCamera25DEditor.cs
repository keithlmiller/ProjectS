using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (GameCamera25D))]

public class GameCamera25DEditor : Editor
{

	public override void OnInspectorGUI ()
	{
		GameCamera25D _target = (GameCamera25D) target;
		
		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Background image", EditorStyles.boldLabel);
		
			_target.backgroundImage = (BackgroundImage) EditorGUILayout.ObjectField ("Prefab:", _target.backgroundImage, typeof (BackgroundImage), true);
			
			if (_target.backgroundImage)
			{
				if (GUILayout.Button ("Set as active"))
				{
					Undo.RecordObject (_target, "Set active background");
					
					_target.SetActiveBackground ();
					_target.SnapCameraInEditor ();
				}
			}
		
		EditorGUILayout.EndVertical ();
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
	
}
