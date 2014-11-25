using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (AC.Char))]

public class CharEditor : Editor
{

	public override void OnInspectorGUI ()
	{
		AC.Char _target = (AC.Char) target;

		if (_target.animEngine == null || !_target.animEngine.ToString ().Contains (_target.animationEngine.ToString ()))
		{
			_target.ResetAnimationEngine ();
		}
		_target.animationEngine = (AnimationEngine) EditorGUILayout.EnumPopup ("Animation engine:", _target.animationEngine);
		if (_target.animationEngine == AnimationEngine.Sprites2DToolkit && !tk2DIntegration.IsDefinePresent ())
		{
			EditorGUILayout.HelpBox ("The 'tk2DIsPresent' preprocessor define must be declared in the\ntk2DIntegration.cs script. Please open it and follow instructions.", MessageType.Warning);
		}

		EditorGUILayout.BeginVertical ("Button");
			_target.animEngine.CharSettingsGUI ();
		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Movement settings:", EditorStyles.boldLabel);
		
			_target.walkSpeedScale = EditorGUILayout.FloatField ("Walk speed scale:", _target.walkSpeedScale);
			_target.runSpeedScale = EditorGUILayout.FloatField ("Run speed scale:", _target.runSpeedScale);
			_target.turnSpeed = EditorGUILayout.FloatField ("Turn speed:", _target.turnSpeed);
			_target.acceleration = EditorGUILayout.FloatField ("Acceleration:", _target.acceleration);
			_target.deceleration = EditorGUILayout.FloatField ("Deceleration:", _target.deceleration);
			_target.turnBeforeWalking = EditorGUILayout.Toggle ("Turn before walking?", _target.turnBeforeWalking);
		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Rigidbody settings:", EditorStyles.boldLabel);
			_target.ignoreGravity = EditorGUILayout.Toggle ("Ignore gravity?", _target.ignoreGravity);
			_target.freezeRigidbodyWhenIdle = EditorGUILayout.Toggle ("Freeze when Idle?", _target.freezeRigidbodyWhenIdle);
		EditorGUILayout.EndVertical ();
		
		
		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Audio clips:", EditorStyles.boldLabel);
		
			_target.walkSound = (AudioClip) EditorGUILayout.ObjectField ("Walk sound:", _target.walkSound, typeof (AudioClip), false);
			_target.runSound = (AudioClip) EditorGUILayout.ObjectField ("Run sound:", _target.runSound, typeof (AudioClip), false);
			_target.soundChild = (Sound) EditorGUILayout.ObjectField ("Sound child:", _target.soundChild, typeof (Sound), true);
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Dialogue settings:", EditorStyles.boldLabel);

			EditorGUILayout.LabelField ("Portrait graphic:");
			_target.portraitIcon.ShowGUI (false);
			_target.speechColor = EditorGUILayout.ColorField ("Speech text colour:", _target.speechColor);
			_target.speechLabel = EditorGUILayout.TextField ("Speaker label:", _target.speechLabel);
		EditorGUILayout.EndVertical ();
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}

}
