/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCharPortrait.cs"
 * 
 *	This action picks a new portrait for the chosen Character.
 *	Written for the AC community by Guran.
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
	public class ActionCharPortrait : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public bool isPlayer;
		public Char _char;
		public Texture2D newPortraitGraphic;


		public ActionCharPortrait ()
		{
			this.isDisplayed = true;
			title = "Character: Switch Portrait";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			_char = AssignFile <Char> (parameters, parameterID, constantID, _char);

			if (isPlayer)
			{
				_char = KickStarter.player;
			}
		}

		
		override public float Run ()
		{
			if (_char)
			{
				_char.portraitIcon.texture = newPortraitGraphic;
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			// Action-specific Inspector GUI code here
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
				parameterID = Action.ChooseParameterGUI ("Character:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					_char = null;
				}
				else
				{
					_char = (Char) EditorGUILayout.ObjectField ("Character:", _char, typeof (Char), true);
					
					constantID = FieldToID <Char> (_char, constantID);
					_char = IDToField <Char> (_char, constantID, false);
				}
			}
			
			newPortraitGraphic = (Texture2D) EditorGUILayout.ObjectField ("New Portrait graphic:", newPortraitGraphic, typeof (Texture2D), true);
			AfterRunningOption ();
		}
		

		public override string SetLabel ()
		{
			string labelAdd = "";

			if (isPlayer)
			{
				labelAdd = " (Player)";
			}
			else if (_char)
			{
				labelAdd = " (" + _char.name + ")";
			}
			
			return labelAdd;
		}

		#endif
		
	}

}