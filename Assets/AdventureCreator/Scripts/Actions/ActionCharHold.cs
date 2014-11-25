/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCharHold.cs"
 * 
 *	This action parents a GameObject to a character's hand.
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
	public class ActionCharHold : Action
	{

		public int objectToHoldParameterID = -1;

		public int _charID = 0;
		public int objectToHoldID = 0;

		public GameObject objectToHold;
		public bool isPlayer;
		public Char _char;
		public bool rotate90;
		
		public enum Hand { Left, Right };
		public Hand hand;
		
		
		public ActionCharHold ()
		{
			this.isDisplayed = true;
			title = "Character: Hold object";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			_char = AssignFile <Char> (_charID, _char);
			objectToHold = AssignFile (parameters, objectToHoldParameterID, objectToHoldID, objectToHold);

			if (isPlayer)
			{
				_char = KickStarter.player;
			}
		}

		
		override public float Run ()
		{
			if (_char)
			{
				if (_char.animEngine == null)
				{
					_char.ResetAnimationEngine ();
				}
				
				if (_char.animEngine != null)
				{
					_char.animEngine.ActionCharHoldRun (this);
				}
			}
			else
			{
				Debug.LogWarning ("Could not create animation engine!");
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
			if (isPlayer)
			{
				if (Application.isPlaying)
				{
					_char = KickStarter.player;
				}
				else
				{
					_char = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
				}
			}
			else
			{
				_char = (Char) EditorGUILayout.ObjectField ("Character:", _char, typeof (Char), true);
					
				_charID = FieldToID <Char> (_char, _charID);
				_char = IDToField <Char> (_char, _charID, true);
			}
			
			if (_char)
			{
				if (_char.animEngine == null)
				{
					_char.ResetAnimationEngine ();
				}
				if (_char.animEngine)
				{
					_char.animEngine.ActionCharHoldGUI (this, parameters);
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("This Action requires a Character before more options will show.", MessageType.Info);
			}
			
			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			string labelAdd = "";
			
			if (_char && objectToHold)
			{
				labelAdd = "(" + _char.name + " hold " + objectToHold.name + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
		
	}

}