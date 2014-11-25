/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionList.cs"
 * 
 *	This script stores, and handles the sequentual triggering of, actions.
 *	It is derived by Cutscene, Hotspot, Trigger, and DialogOption.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	[System.Serializable]
	public class ActionList : MonoBehaviour
	{
		
		[HideInInspector] public bool isSkippable = true;
		[HideInInspector] public float triggerTime = 0f;
		[HideInInspector] public bool autosaveAfter = false;
		[HideInInspector] public ActionListType actionListType = ActionListType.PauseGameplay;
		[HideInInspector] public List<AC.Action> actions = new List<AC.Action>();
		[HideInInspector] public Conversation conversation = null;
		[HideInInspector] public ActionListAsset assetFile;
		[HideInInspector] public ActionListSource source;
		[HideInInspector] public bool useParameters = false;
		[HideInInspector] public List<ActionParameter> parameters = new List<ActionParameter>();
		
		protected bool isSkipping = false;
		protected int nextActionNumber = -1; 	// Set as -1 to stop running
		protected LayerMask LayerHotspot;
		protected LayerMask LayerOff;
		protected StateHandler stateHandler;
		protected ActionListManager actionListManager;
		
		
		private void Awake ()
		{
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			LayerHotspot = LayerMask.NameToLayer (settingsManager.hotspotLayer);
			LayerOff = LayerMask.NameToLayer (settingsManager.deactivatedLayer);
			
			if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>())
			{
				actionListManager = GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>();
			}
			
			// If asset-based, download actions
			if (source == ActionListSource.AssetFile)
			{
				actions.Clear ();
				if (assetFile != null && assetFile.actions.Count > 0)
				{
					foreach (AC.Action action in assetFile.actions)
					{
						actions.Add (action);
					}
					useParameters = assetFile.useParameters;
					parameters = assetFile.parameters;
				}
			}
			
			if (useParameters)
			{
				// Reset all parameters
				foreach (ActionParameter _parameter in parameters)
				{
					_parameter.Reset ();
				}
			}
		}
		
		
		private void Start ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
			{
				stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
			}
			
			/*if (!useParameters)
			{
				foreach (Action action in actions)
				{
					if (action != null)
					{
						action.AssignValues (null);
					}
				}
			} */
		}
		
		
		public virtual void Interact ()
		{
			if (actions.Count > 0)
			{
				if (triggerTime > 0f)
				{
					StartCoroutine ("PauseUntilStart");
				}
				else
				{
					ResetSkips ();
					BeginActionList (0);
				}
			}
		}
		
		
		public void Interact (int i)
		{
			if (actions.Count > 0 && actions.Count > i)
			{
				ResetSkips ();
				BeginActionList (i);
			}
		}
		
		
		private IEnumerator PauseUntilStart ()
		{
			if (triggerTime > 0f)
			{
				yield return new WaitForSeconds (triggerTime);
			}
			
			ResetSkips ();
			BeginActionList (0);
		}
		
		
		private void ResetSkips ()
		{
			// "lastResult" is used to backup Check results when skipping
			foreach (Action action in actions)
			{
				if (action != null)
				{
					action.lastResult = -10;
				}
			}
		}
		
		
		private void BeginActionList (int i)
		{
			if (actionListManager == null)
			{
				actionListManager = GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>();
			}
			actionListManager.AddToList (this);
			
			nextActionNumber = i;
			ProcessAction (i);
		}
		
		
		private void ProcessAction (int thisActionNumber)
		{
			if (nextActionNumber > -1 && nextActionNumber < actions.Count && actions [thisActionNumber] is AC.Action)
			{
				if (!actions [thisActionNumber].isEnabled)
				{
					if (actions.Count > (thisActionNumber+1))
					{
						ProcessAction (thisActionNumber + 1);
					}
					else
					{
						EndCutscene ();
					}
				}
				else
				{
					nextActionNumber = thisActionNumber + 1;
					StartCoroutine ("RunAction", actions [thisActionNumber]);
				}
			}
			else
			{
				EndCutscene ();
			}
		}
		
		
		private IEnumerator RunAction (AC.Action action)
		{
			if (useParameters)
			{
				action.AssignValues (parameters);
			}
			else
			{
				action.AssignValues (null);
			}
			
			if (isSkipping)
			{
				action.Skip ();
			}
			else
			{
				action.isRunning = false;
				float waitTime = action.Run ();		
				if (waitTime > 0f)
				{
					while (action.isRunning)
					{
						yield return new WaitForSeconds (waitTime);
						waitTime = action.Run ();
					}
				}
			}
			action.isRunning = false;
			
			int actionEnd = 0;
			if (isSkipping && action.lastResult != -10 && (action is ActionCheck || action is ActionCheckMultiple))
			{
				// When skipping an ActionCheck that has already run, revert to previous result
				actionEnd = action.lastResult;
			}
			else
			{
				actionEnd = action.End (this.actions);
				action.lastResult = actionEnd;
			}
			if (actionEnd >= 0)
			{
				nextActionNumber = actionEnd;
			}
			
			if (action.endAction == ResultAction.RunCutscene)
			{
				if (action.isAssetFile && action.linkedAsset != null)
				{
					AdvGame.RunActionListAsset (action.linkedAsset);
				}
				else if (!action.isAssetFile && action.linkedCutscene != null && action.linkedCutscene != this)
				{
					action.linkedCutscene.SendMessage ("Interact");
				}
			}
			if (actionEnd == -1 || actionEnd == -2)
			{
				EndCutscene ();
			}
			else if (nextActionNumber >= 0)
			{
				ProcessAction (nextActionNumber);
			}
			
			if (action.endAction == ResultAction.RunCutscene && !action.isAssetFile && action.linkedCutscene != null && action.linkedCutscene == this)
			{
				action.linkedCutscene.SendMessage ("Interact");
			}
		}
		
		
		protected virtual void EndCutscene ()
		{
			actionListManager.EndList (this);
		}
		
		
		private void TurnOn ()
		{
			gameObject.layer = LayerHotspot;
		}
		
		
		private void TurnOff ()
		{
			gameObject.layer = LayerOff;
		}
		
		
		public void Reset ()
		{
			isSkipping = false;
			nextActionNumber = -1;
			StopCoroutine ("RunAction");
			StopCoroutine ("InteractCoroutine");
		}
		
		
		public void Kill ()
		{
			StopCoroutine ("PauseUntilStart");
			actionListManager.EndList (this);
		}
		
		
		public void Skip ()
		{
			isSkipping = true;
			StopCoroutine ("RunAction");
			
			BeginActionList (0);
		}
		
		
		private void OnDestroy ()
		{
			actionListManager = null;
			stateHandler = null;
		}
		
	}
	
}
