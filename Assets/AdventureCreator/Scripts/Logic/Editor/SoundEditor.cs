using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using AC;

[CustomEditor (typeof (Sound))]
public class SoundEditor : Editor
{
	
	public override void OnInspectorGUI()
	{
		Sound _target = (Sound) target;
		
		_target.soundType = (SoundType) EditorGUILayout.EnumPopup ("Sound type:", _target.soundType);
		_target.playWhilePaused = EditorGUILayout.Toggle ("Play while game paused?", _target.playWhilePaused);
		_target.relativeVolume = EditorGUILayout.Slider ("Relative volume:", _target.relativeVolume, 0f, 1f);

		if (_target.soundType == SoundType.Music)
		{
			_target.surviveSceneChange = EditorGUILayout.Toggle ("Play music across scenes?", _target.surviveSceneChange);
			if (_target.surviveSceneChange && _target.transform.root != null && _target.transform.root != _target.gameObject.transform)
			{
				Debug.Log (_target.transform.root + " != " + _target.gameObject);
				EditorGUILayout.HelpBox ("For music to survive scene-changes, please move this object out of it's hierarchy, so that it has no parent GameObject.", MessageType.Warning);
			}
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
	
}
