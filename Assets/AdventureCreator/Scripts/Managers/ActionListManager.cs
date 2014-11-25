/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionListManager.cs"
 * 
 *	This script keeps track of which ActionLists
 *	are running in a scene.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class ActionListManager : MonoBehaviour
	{
		
		public bool showActiveActionLists = false;
		
		private bool playCutsceneOnVarChange = false;
		private bool saveAfterCutscene = false;
		
		private Conversation conversationOnEnd;
		private List<ActionList> activeLists = new List<ActionList>();
		private PlayerMenus playerMenus;
		private StateHandler stateHandler;
		
		
		private void Awake ()
		{
			activeLists.Clear ();
		}
		
		
		private void Start ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
			{
				stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
				playerMenus = stateHandler.GetComponent <PlayerMenus>();
			}
		}
		
		
		public void UpdateActionListManager ()
		{
			if (saveAfterCutscene && !IsGameplayBlocked ())
			{
				saveAfterCutscene = false;
				SaveSystem.SaveGame (-1);
			}
			
			if (playCutsceneOnVarChange && stateHandler && (stateHandler.gameState == GameState.Normal || stateHandler.gameState == GameState.DialogOptions))
			{
				playCutsceneOnVarChange = false;
				
				if (GetComponent <SceneSettings>().cutsceneOnVarChange != null)
				{
					GetComponent <SceneSettings>().cutsceneOnVarChange.Interact ();
				}
			}
		}
		
		
		public void EndCutscene ()
		{
			if (!IsGameplayBlocked ())
			{
				return;
			}
			
			// Stop all non-looping sound
			Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
			foreach (Sound sound in sounds)
			{
				if (sound.GetComponent <AudioSource>())
				{
					if (sound.soundType != SoundType.Music && !sound.GetComponent <AudioSource>().loop)
					{
						sound.Stop ();
					}
				}
			}
			
			int iteration=0;
			while (IsGameplayBlocked () && iteration<20)
			{
				for (int i=0; i<activeLists.Count; i++)
				{
					if (activeLists[i].isSkippable && activeLists[i].actionListType == ActionListType.PauseGameplay)
					{
						activeLists[i].Skip ();
					}
				}
				iteration++;
			}
		}
		
		
		public bool AreActionListsRunning ()
		{
			if (activeLists.Count > 0)
			{
				return true;
			}
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		private void OnGUI ()
		{
			if (showActiveActionLists)
			{
				if (activeLists.Count > 0)
				{
					GUILayout.Label ("Current ActionLists running:", "Button");
					GUILayout.Space (10f);
					
					foreach (ActionList list in activeLists)
					{
						GUILayout.Label (list.gameObject.name, "Button");
					}
					
					GUILayout.Space (10f);
					GUILayout.Label ("Pausing gameplay: " + IsGameplayBlocked (), "Button");
				}
				else
				{
					GUILayout.Label ("No ActionLists are running", "Button");
				}
			}
		}
		
		#endif
		
		
		public void AddToList (ActionList _list)
		{
			if (!IsListRunning (_list))
			{
				activeLists.Add (_list);
			}
			
			if (_list.conversation)
			{
				conversationOnEnd = _list.conversation;
			}
			
			SetCorrectGameState ();
		}
		
		
		public void EndList (ActionList _list)
		{
			if (IsListRunning (_list))
			{
				activeLists.Remove (_list);
			}
			
			_list.Reset ();
			
			if (_list.conversation == conversationOnEnd && _list.conversation != null)
			{
				if (stateHandler)
				{
					stateHandler.gameState = GameState.Cutscene;
				}
				else
				{
					Debug.LogWarning ("Could not set correct GameState!");
				}
				
				conversationOnEnd.Interact ();
				conversationOnEnd = null;
			}
			else
			{
				if (_list is RuntimeActionList)
				{
					SetCorrectGameState_Menus ();
				}
				SetCorrectGameState ();
			}
			
			if (_list.autosaveAfter)
			{
				if (!IsGameplayBlocked ())
				{
					SaveSystem.SaveGame (-1);
				}
				else
				{
					saveAfterCutscene = true;
				}
			}
			
			if (_list is RuntimeActionList)
			{
				RuntimeActionList runtimeActionList = (RuntimeActionList) _list;
				runtimeActionList.DestroySelf ();
			}
		}
		
		
		public void EndAssetList (string assetName)
		{
			RuntimeActionList[] runtimeActionLists = FindObjectsOfType (typeof (RuntimeActionList)) as RuntimeActionList[];
			foreach (RuntimeActionList runtimeActionList in runtimeActionLists)
			{
				if (runtimeActionList.name == assetName)
				{
					EndList (runtimeActionList);
				}
			}
		}
		
		
		public void VariableChanged ()
		{
			playCutsceneOnVarChange = true;
		}
		
		
		public bool IsListRunning (ActionList _list)
		{
			foreach (ActionList list in activeLists)
			{
				if (list == _list)
				{
					return true;
				}
			}
			
			return false;
		}
		
		
		public void KillAllLists ()
		{
			foreach (ActionList _list in activeLists)
			{
				_list.Reset ();
				
				if (_list is RuntimeActionList)
				{
					RuntimeActionList runtimeActionList = (RuntimeActionList) _list;
					runtimeActionList.DestroySelf ();
				}
			}
			
			activeLists.Clear ();
		}
		
		
		private void SetCorrectGameState_Menus ()
		{
			bool foundBlockingMenu = false;
			
			foreach (Menu menu in PlayerMenus.GetMenus ())
			{
				if (menu.IsEnabled () && menu.IsBlocking ())
				{
					foundBlockingMenu = true;
				}
			}
			
			if (foundBlockingMenu)
			{
				stateHandler.gameState = GameState.Paused;
			}
			else
			{
				stateHandler.RestoreLastNonPausedState ();
			}
		}
		
		
		private void SetCorrectGameState ()
		{
			if (stateHandler == null)
			{
				Start ();
			}
			
			if (stateHandler != null)
			{
				if (playerMenus.ArePauseMenusOn ())
				{
					stateHandler.gameState = GameState.Paused;
				}
				else if (IsGameplayBlocked ())
				{
					stateHandler.gameState = GameState.Cutscene;
				}
				else if (stateHandler.gameState == GameState.Cutscene)
				{
					if (GetComponent <PlayerInput>().activeConversation != null)
					{
						stateHandler.gameState = GameState.DialogOptions;
					}
					else
					{
						stateHandler.gameState = GameState.Normal;
					}
				}
			}
			else
			{
				Debug.LogWarning ("Could not set correct GameState!");
			}
		}
		
		
		public bool IsGameplayBlocked ()
		{
			foreach (ActionList list in activeLists)
			{
				if (list.actionListType == AC.ActionListType.PauseGameplay)
				{
					return true;
				}
			}
			return false;
		}
		
		
		public bool IsInSkippableCutscene ()
		{
			foreach (ActionList list in activeLists)
			{
				if (list.actionListType == AC.ActionListType.PauseGameplay && list.isSkippable)
				{
					return true;
				}
			}
			
			return false;
		}
		
		
		private void OnDestroy ()
		{
			activeLists.Clear ();
			stateHandler = null;
		}
		
	}
	
}