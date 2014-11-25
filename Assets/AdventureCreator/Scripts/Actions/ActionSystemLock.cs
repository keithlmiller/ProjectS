/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionSystemLock.cs"
 * 
 *	This action handles the enabling / disabling
 *	of individual AC systems, allowing for
 *	minigames or other non-adventure elements
 *	to be run.
 * 
 */

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionSystemLock : Action
	{

		public bool changeMovementMethod = false;
		public MovementMethod newMovementMethod;

		public LockType cursorLock = LockType.NoChange;
		public LockType inputLock = LockType.NoChange;
		public LockType interactionLock = LockType.NoChange;
		public LockType menuLock = LockType.NoChange;
		public LockType movementLock = LockType.NoChange;
		public LockType cameraLock = LockType.NoChange;
		public LockType triggerLock = LockType.NoChange;
		public LockType playerLock = LockType.NoChange;
		public LockType saveLock = LockType.NoChange;

		
		public ActionSystemLock ()
		{
			this.isDisplayed = true;
			title = "Engine: Manage systems";
		}
		
		
		override public float Run ()
		{
			if (changeMovementMethod)
			{
				AdvGame.GetReferences ().settingsManager.movementMethod = newMovementMethod;
			}

			StateHandler stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();

			if (cursorLock == LockType.Enabled)
			{
				stateHandler.cursorIsOff = false;
			}
			else if (cursorLock == LockType.Disabled)
			{
				stateHandler.cursorIsOff = true;
			}

			if (inputLock == LockType.Enabled)
			{
				stateHandler.inputIsOff = false;
			}
			else if (inputLock == LockType.Disabled)
			{
				stateHandler.inputIsOff = true;
			}

			if (interactionLock == LockType.Enabled)
			{
				stateHandler.interactionIsOff = false;
			}
			else if (interactionLock == LockType.Disabled)
			{
				stateHandler.interactionIsOff = true;
			}

			if (menuLock == LockType.Enabled)
			{
				stateHandler.menuIsOff = false;
			}
			else if (menuLock == LockType.Disabled)
			{
				stateHandler.menuIsOff = true;
			}

			if (movementLock == LockType.Enabled)
			{
				stateHandler.movementIsOff = false;
			}
			else if (movementLock == LockType.Disabled)
			{
				stateHandler.movementIsOff = true;
			}

			if (cameraLock == LockType.Enabled)
			{
				stateHandler.cameraIsOff = false;
			}
			else if (cameraLock == LockType.Disabled)
			{
				stateHandler.cameraIsOff = true;
			}

			if (triggerLock == LockType.Enabled)
			{
				stateHandler.triggerIsOff = false;
			}
			else if (triggerLock == LockType.Disabled)
			{
				stateHandler.triggerIsOff = true;
			}

			if (playerLock == LockType.Enabled)
			{
				stateHandler.playerIsOff = false;
			}
			else if (playerLock == LockType.Disabled)
			{
				stateHandler.playerIsOff = true;
			}

			if (saveLock == LockType.Disabled)
			{
				GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>().lockSave = true;
			}
			else if (saveLock == LockType.Enabled)
			{
				GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>().lockSave = false;
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			changeMovementMethod = EditorGUILayout.BeginToggleGroup ("Change movement method?", changeMovementMethod);
			newMovementMethod = (MovementMethod) EditorGUILayout.EnumPopup ("Movement method:", newMovementMethod);
			EditorGUILayout.EndToggleGroup ();

			EditorGUILayout.Space ();

			cursorLock = (LockType) EditorGUILayout.EnumPopup ("Cursor:", cursorLock);
			inputLock = (LockType) EditorGUILayout.EnumPopup ("Input:", inputLock);
			interactionLock = (LockType) EditorGUILayout.EnumPopup ("Interactions:", interactionLock);
			menuLock = (LockType) EditorGUILayout.EnumPopup ("Menus:", menuLock);
			movementLock = (LockType) EditorGUILayout.EnumPopup ("Movement:", movementLock);
			cameraLock = (LockType) EditorGUILayout.EnumPopup ("Camera:", cameraLock);
			triggerLock = (LockType) EditorGUILayout.EnumPopup ("Triggers:", triggerLock);
			playerLock = (LockType) EditorGUILayout.EnumPopup ("Player:", playerLock);
			saveLock = (LockType) EditorGUILayout.EnumPopup ("Saving:", saveLock);

			AfterRunningOption ();
		}
		
		#endif
		
	}

}