using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (NavigationMesh))]
public class NavigationMeshEditor : Editor
{
	
	public override void OnInspectorGUI ()
	{
		NavigationMesh _target = (NavigationMesh) target;

		_target.disableRenderer = EditorGUILayout.Toggle ("Disable mesh renderer?", _target.disableRenderer);
		
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.IsUnity2D ())
		{
			int numOptions = _target.polygonColliderHoles.Count;
			numOptions = EditorGUILayout.IntField ("Number of holes:", _target.polygonColliderHoles.Count);
			if (numOptions < 0)
			{
				numOptions = 0;
			}
			
			if (numOptions < _target.polygonColliderHoles.Count)
			{
				_target.polygonColliderHoles.RemoveRange (numOptions, _target.polygonColliderHoles.Count - numOptions);
			}
			else if (numOptions > _target.polygonColliderHoles.Count)
			{
				if (numOptions > _target.polygonColliderHoles.Capacity)
				{
					_target.polygonColliderHoles.Capacity = numOptions;
				}
				for (int i=_target.polygonColliderHoles.Count; i<numOptions; i++)
				{
					_target.polygonColliderHoles.Add (null);
				}
			}
			
			for (int i=0; i<_target.polygonColliderHoles.Count; i++)
			{
				_target.polygonColliderHoles [i] = (PolygonCollider2D) EditorGUILayout.ObjectField ("Hole #" + i.ToString () + ":", _target.polygonColliderHoles [i], typeof (PolygonCollider2D), true);
			}
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
}
