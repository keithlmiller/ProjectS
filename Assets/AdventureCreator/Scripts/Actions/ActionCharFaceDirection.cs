/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCharDirection.cs"
 * 
 *	This action is used to make characters turn to face fixed directions relative to the camera.
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
	public class ActionCharFaceDirection : Action
	{
		
		public int charToMoveParameterID = -1;

		public int charToMoveID = 0;

		public bool isInstant;
		public CharDirection direction;
		public Char charToMove;

		public bool isPlayer;

		
		public ActionCharFaceDirection ()
		{
			this.isDisplayed = true;
			title = "Character: Face direction";
		}
		
		
		public override void AssignValues (List<ActionParameter> parameters)
		{
			charToMove = AssignFile <Char> (parameters, charToMoveParameterID, charToMoveID, charToMove);

			if (isPlayer)
			{
				charToMove = KickStarter.player;
			}
		}


		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				
				if (charToMove)
				{
					charToMove.SetLookDirection (GetLookVector (), isInstant);

					if (!isInstant)
					{
						charToMove.Halt ();

						if (willWait)
						{
							return (defaultPauseTime);
						}
					}
				}
				
				return 0f;
			}
			else
			{
				if (charToMove.IsTurning ())
				{
					return defaultPauseTime;
				}
				else
				{
					isRunning = false;
					return 0f;
				}
			}
		}
		
		
		override public void Skip ()
		{
			if (charToMove)
			{
				charToMove.SetLookDirection (GetLookVector (), true);
			}
		}


		private Vector3 GetLookVector ()
		{
			Transform camTransform = Camera.main.transform;

			Vector3 lookVector = Vector3.zero;
			if (direction == CharDirection.Down)
			{
				lookVector = -camTransform.up;
			}
			else if (direction == CharDirection.Left)
			{
				lookVector = -camTransform.right;
			}
			else if (direction == CharDirection.Right)
			{
				lookVector = camTransform.right - new Vector3 (0f, 0.01f); // Angle slightly so that left->right rotations face camera
			}
			else if (direction == CharDirection.Up)
			{
				lookVector = camTransform.up;
			}
			else if (direction == CharDirection.DownLeft)
			{
				lookVector = (-camTransform.up - camTransform.right).normalized;
			}
			else if (direction == CharDirection.DownRight)
			{
				lookVector = (-camTransform.up + camTransform.right).normalized;
			}
			else if (direction == CharDirection.UpLeft)
			{
				lookVector = (camTransform.up - camTransform.right).normalized;
			}
			else if (direction == CharDirection.UpRight)
			{
				lookVector = (camTransform.up + camTransform.right).normalized;
			}

			lookVector = new Vector3 (lookVector.x, 0f, lookVector.y);
			return lookVector;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Affect Player?", isPlayer);
			if (!isPlayer)
			{
				charToMoveParameterID = Action.ChooseParameterGUI ("Character to turn:", parameters, charToMoveParameterID, ParameterType.GameObject);
				if (charToMoveParameterID >= 0)
				{
					charToMoveID = 0;
					charToMove = null;
				}
				else
				{
					charToMove = (Char) EditorGUILayout.ObjectField ("Character to turn:", charToMove, typeof(Char), true);
					
					charToMoveID = FieldToID <Char> (charToMove, charToMoveID);
					charToMove = IDToField <Char> (charToMove, charToMoveID, false);
				}
			}

			direction = (CharDirection) EditorGUILayout.EnumPopup ("Direction to face:", direction);
			isInstant = EditorGUILayout.Toggle ("Is instant?", isInstant);
			if (!isInstant)
			{
				willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);
			}
			
			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (charToMove)
			{
				labelAdd = " (" + charToMove.name + " - " + direction + ")";
			}

			return labelAdd;
		}
		
		#endif
		
	}

}