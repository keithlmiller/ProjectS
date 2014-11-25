/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionPlayerLock.cs"
 * 
 *	This action constrains the player in various ways (movement, saving etc)
 *	In Direct control mode, the player can be assigned a path,
 *	and will only be able to move along that path during gameplay.
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
	public class ActionPlayerLock : Action
	{
		
		public LockType doUpLock = LockType.NoChange;
		public LockType doDownLock = LockType.NoChange;
		public LockType doLeftLock = LockType.NoChange;
		public LockType doRightLock = LockType.NoChange;
		
		public PlayerMoveLock doRunLock = PlayerMoveLock.NoChange;
		public LockType doGravityLock = LockType.NoChange;
		public Paths movePath;

		
		public ActionPlayerLock ()
		{
			this.isDisplayed = true;
			title = "Player: Constrain";
		}
		
		
		override public float Run ()
		{
			PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
			Player player = KickStarter.player;

			if (playerInput)
			{
				if (IsSingleLockMovement ())
				{
					doLeftLock = doUpLock;
					doRightLock = doUpLock;
					doDownLock = doUpLock;
				}

				if (doUpLock == LockType.Disabled)
				{
					playerInput.SetUpLock (true);
				}
				else if (doUpLock == LockType.Enabled)
				{
					playerInput.SetUpLock (false);
				}
		
				if (doDownLock == LockType.Disabled)
				{
					playerInput.isDownLocked = true;
				}
				else if (doDownLock == LockType.Enabled)
				{
					playerInput.isDownLocked = false;
				}
				
				if (doLeftLock == LockType.Disabled)
				{
					playerInput.isLeftLocked = true;
				}
				else if (doLeftLock == LockType.Enabled)
				{
					playerInput.isLeftLocked = false;
				}
		
				if (doRightLock == LockType.Disabled)
				{
					playerInput.isRightLocked = true;
				}
				else if (doRightLock == LockType.Enabled)
				{
					playerInput.isRightLocked = false;
				}

				if (IsUltimateFPS ())
				{
					return 0f;
				}
				
				if (doRunLock != PlayerMoveLock.NoChange)
				{
					playerInput.runLock = doRunLock;
				}
			}
			
			if (player)
			{
				if (movePath)
				{
					player.SetLockedPath (movePath);
					player.SetMoveDirectionAsForward ();
				}
				else if (player.activePath)
				{
					player.EndPath ();
				}

				if (doGravityLock == LockType.Enabled)
				{
					player.ignoreGravity = false;
				}
				else if (doGravityLock == LockType.Disabled)
				{
					player.ignoreGravity = true;
				}
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			if (IsSingleLockMovement ())
			{
				doUpLock = (LockType) EditorGUILayout.EnumPopup ("Movement:", doUpLock);
			}
			else
			{
				doUpLock = (LockType) EditorGUILayout.EnumPopup ("Up movement:", doUpLock);
				doDownLock = (LockType) EditorGUILayout.EnumPopup ("Down movement:", doDownLock);
				doLeftLock = (LockType) EditorGUILayout.EnumPopup ("Left movement:", doLeftLock);
				doRightLock = (LockType) EditorGUILayout.EnumPopup ("Right movement:", doRightLock);
			}

			if (!IsUltimateFPS ())
			{
				doRunLock = (PlayerMoveLock) EditorGUILayout.EnumPopup ("Walk / run:", doRunLock);
				doGravityLock = (LockType) EditorGUILayout.EnumPopup ("Affected by gravity?", doGravityLock);
				movePath = (Paths) EditorGUILayout.ObjectField ("Move path:", movePath, typeof (Paths), true);
			}
			
			AfterRunningOption ();
		}
		
		#endif


		private bool IsSingleLockMovement ()
		{
			if (AdvGame.GetReferences ().settingsManager)
			{
				SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
				if (settingsManager.movementMethod == MovementMethod.PointAndClick || settingsManager.movementMethod == MovementMethod.Drag || settingsManager.movementMethod == MovementMethod.UltimateFPS || settingsManager.movementMethod == MovementMethod.StraightToCursor)
				{
					return true;
				}
			}
			return false;
		}


		private bool IsUltimateFPS ()
		{
			if (AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.movementMethod == MovementMethod.UltimateFPS)
			{
				return true;
			}
			return false;
		}
	}

}