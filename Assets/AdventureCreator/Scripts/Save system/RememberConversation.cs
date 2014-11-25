/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberConversation.cs"
 * 
 *	This script is attached to conversation objects in the scene
 *	with DialogOption states we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class RememberConversation : ConstantID
	{

		public ConversationData SaveData ()
		{
			ConversationData conversationData = new ConversationData();
			conversationData.objectID = constantID;

			if (GetComponent <Conversation>())
			{
				conversationData.optionStates = GetComponent <Conversation>().GetOptionStates ();
				conversationData.optionLocks = GetComponent <Conversation>().GetOptionLocks ();
			}

			return (conversationData);
		}


		public void LoadData (ConversationData data)
		{
			if (GetComponent <Conversation>())
			{
				GetComponent <Conversation>().SetOptionStates (data.optionStates);
				GetComponent <Conversation>().SetOptionLocks (data.optionLocks);
			}
		}

	}


	[System.Serializable]
	public class ConversationData
	{
		public int objectID;
		public List<bool> optionStates = new List<bool>();
		public List<bool> optionLocks = new List<bool>();
		
		public ConversationData () { }
	}

}