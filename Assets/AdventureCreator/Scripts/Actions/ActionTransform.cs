/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionTransform.cs"
 * 
 *	This action modifies a GameObject position, rotation or scale over a set time.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionTransform : Action
	{
		
		public Marker marker;
		public int markerParameterID = -1;
		public int markerID = 0;
		public bool doEulerRotation = false;
		
		public int parameterID = -1;
		public int constantID = 0;
		public Moveable linkedProp;
		public float transitionTime;
		public Vector3 newVector;
		
		public TransformType transformType;
		public MoveMethod moveMethod;
		
		public enum ToBy { To, By };
		public ToBy toBy;
		
		
		public ActionTransform ()
		{
			this.isDisplayed = true;
			title = "Object: Transform";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			linkedProp = AssignFile <Moveable> (parameters, parameterID, constantID, linkedProp);
			marker = AssignFile <Marker> (parameters, markerParameterID, markerID, marker);
		}
		
		
		override public float Run ()	
		{
			if (!isRunning)
			{
				isRunning = true;
				
				if (linkedProp)
				{
					RunToTime (transitionTime);
					
					if (willWait && transitionTime > 0f)
					{
						return (defaultPauseTime);
					}
				}
			}
			else
			{
				if (linkedProp)
				{
					if (!linkedProp.isMoving)
					{
						isRunning = false;
					}
					else
					{
						return defaultPauseTime;
					}
				}
			}
			
			return 0f;
		}
		
		
		override public void Skip ()	
		{
			if (linkedProp)
			{
				RunToTime (0f);
			}
		}
		
		
		
		private void RunToTime (float _time)	
		{
			if (transformType == TransformType.CopyMarker)
			{
				if (marker)
				{
					linkedProp.Move (marker, moveMethod, _time);
				}
			}
			else
			{
				Vector3 targetVector = newVector;
				
				if (transformType == TransformType.Translate)
				{
					if (toBy == ToBy.By)
					{
						targetVector += linkedProp.transform.localPosition;
					}
				}
				
				else if (transformType == TransformType.Rotate)
				{
					if (toBy == ToBy.By)
					{
						targetVector += linkedProp.transform.localEulerAngles;
					}
				}
				
				else if (transformType == TransformType.Scale)
				{
					if (toBy == ToBy.By)
					{
						targetVector += linkedProp.transform.localScale;
					}
				}

				if (transformType == TransformType.Rotate)
				{
					linkedProp.Move (targetVector, moveMethod, _time, transformType, doEulerRotation);
				}
				else
				{
					linkedProp.Move (targetVector, moveMethod, _time, transformType, false);
				}
			}
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Moveable object:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				linkedProp = null;
			}
			else
			{
				linkedProp = (Moveable) EditorGUILayout.ObjectField ("Moveable object:", linkedProp, typeof (Moveable), true);
				
				constantID = FieldToID <Moveable> (linkedProp, constantID);
				linkedProp = IDToField <Moveable> (linkedProp, constantID, false);
			}
			
			EditorGUILayout.BeginHorizontal ();
			transformType = (TransformType) EditorGUILayout.EnumPopup (transformType);
			if (transformType != TransformType.CopyMarker)
			{
				toBy = (ToBy) EditorGUILayout.EnumPopup (toBy);
			}
			EditorGUILayout.EndHorizontal ();
			
			if (transformType == TransformType.CopyMarker)
			{
				markerID = Action.ChooseParameterGUI ("Marker:", parameters, markerParameterID, ParameterType.GameObject);
				if (markerParameterID >= 0)
				{
					markerID = 0;
					marker = null;
				}
				else
				{
					marker = (Marker) EditorGUILayout.ObjectField ("Marker:", marker, typeof (Marker), true);
					
					markerID = FieldToID <Marker> (marker, markerID);
					marker = IDToField <Marker> (marker, markerID, false);
				}
				
			}
			else
			{
				newVector = EditorGUILayout.Vector3Field ("Vector:", newVector);
			}
			transitionTime = EditorGUILayout.Slider ("Transition time:", transitionTime, 0, 10f);
			
			if (transitionTime > 0f)
			{
				if (transformType == TransformType.Rotate)
				{
					doEulerRotation = EditorGUILayout.Toggle ("Euler rotation?", doEulerRotation);
				}
				moveMethod = (MoveMethod) EditorGUILayout.EnumPopup ("Move method", moveMethod);
				willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);
			}
			
			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			if (linkedProp)
			{
				labelAdd = " (" + linkedProp.name + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
	}

}