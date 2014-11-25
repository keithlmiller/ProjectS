/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionConversation.cs"
 * 
 *	This action turns on a conversation.
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
	public class ActionConversation : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public Conversation conversation;
		
		
		public ActionConversation ()
		{
			this.isDisplayed = true;
			title = "Dialogue: Start conversation";
			numSockets = 0;
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			conversation = AssignFile <Conversation> (parameters, parameterID, constantID, conversation);
		}
		
		
		override public float Run ()
		{
			if (conversation)
			{
				conversation.Interact ();
			}
			
			return 0f;
		}
		
		
		override public int End (List<AC.Action> actions)
		{
			return -1;
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Conversation:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				conversation = null;
			}
			else
			{
				conversation = (Conversation) EditorGUILayout.ObjectField ("Conversation:", conversation, typeof (Conversation), true);
				
				constantID = FieldToID <Conversation> (conversation, constantID);
				conversation = IDToField <Conversation> (conversation, constantID, false);
			}
		}
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (conversation)
			{
				labelAdd = " (" + conversation + ")";
			}
			
			return labelAdd;
		}

		#endif
		
	}

}