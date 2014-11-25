/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RuntimeActionList.cs"
 * 
 *	This is a special derivative of ActionList, attached to the GameEngine.
 *	It is used to run ActionList assets, which are assets defined outside of the scene.
 *	This type of asset's actions are copied here and run locally.
 *	When a ActionList asset is copied is copied from a menu, the menu it is called from is recorded, so that the game returns
 *	to the appropriate state after running.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class RuntimeActionList : ActionList
	{

		public ActionListAsset assetSource;


		public void DownloadActions (ActionListAsset actionListAsset, Conversation endConversation, int i)
		{
			this.name = actionListAsset.name;
			assetSource = actionListAsset;

			useParameters = actionListAsset.useParameters;
			parameters = actionListAsset.parameters;

			actionListType = actionListAsset.actionListType;
			if (actionListAsset.actionListType == ActionListType.PauseGameplay)
			{
				isSkippable = actionListAsset.isSkippable;
			}
			else
			{
				isSkippable = false;
			}

			conversation = endConversation;
			actions.Clear ();
			
			foreach (AC.Action action in actionListAsset.actions)
			{
				actions.Add (action);
			}

			if (!useParameters)
			{
				foreach (Action action in actions)
				{
					action.AssignValues (null);
				}
			}
			
			Interact (i);
		}


		public void DestroySelf ()
		{
			Destroy (this.gameObject);
		}
	
	}

}
