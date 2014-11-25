/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ManagerPackage.cs"
 * 
 *	This script is used to store references to Manager assets,
 *	so that they can be quickly loaded into the game engine in bulk.
 * 
 */

using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ManagerPackage : ScriptableObject
	{

		public ActionsManager actionsManager;
		public SceneManager sceneManager;
		public SettingsManager settingsManager;
		public InventoryManager inventoryManager;
		public VariablesManager variablesManager;
		public SpeechManager speechManager;
		public CursorManager cursorManager;
		public MenuManager menuManager;


		public void AssignManagers ()
		{
			if (AdvGame.GetReferences () != null)
			{
				if (actionsManager)
				{
					AdvGame.GetReferences ().sceneManager = sceneManager;
				}
				
				if (actionsManager)
				{
					AdvGame.GetReferences ().settingsManager = settingsManager;
				}
				
				if (actionsManager)
				{
					AdvGame.GetReferences ().actionsManager = actionsManager;
				}
				
				if (actionsManager)
				{
					AdvGame.GetReferences ().variablesManager = variablesManager;
				}
				
				if (actionsManager)
				{
					AdvGame.GetReferences ().inventoryManager = inventoryManager;
				}
				
				if (actionsManager)
				{
					AdvGame.GetReferences ().speechManager = speechManager;
				}
				
				if (actionsManager)
				{
					AdvGame.GetReferences ().cursorManager = cursorManager;
				}
				
				if (actionsManager)
				{
					AdvGame.GetReferences ().menuManager = menuManager;
				}

				#if UNITY_EDITOR
				if (AdvGame.GetReferences ().sceneManager && GameObject.FindWithTag (Tags.gameEngine))
				{
					AdvGame.GetReferences ().sceneManager.GetPrefabsInScene ();
				}

				AssetDatabase.SaveAssets ();
				#endif
				
				Debug.Log ("Managers assigned.");
			}
			else
			{
				Debug.LogError ("Can't assign managers - no References file found in Resources folder.");
			}
		}

	}

}