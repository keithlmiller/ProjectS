/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SceneHandler.cs"
 * 
 *	This script stores the gameState variable, which is used by
 *	other scripts to determine if the game is running normal gameplay,
 *	in a cutscene, paused, or displaying conversation options.
 * 
 */

using UnityEngine;
namespace AC
{

	public class StateHandler : MonoBehaviour
	{
		
		public GameState gameState = GameState.Normal;

		private GameState previousUpdateState = GameState.Normal;
		private GameState lastNonPausedState = GameState.Normal;


		public bool cursorIsOff;
		public bool inputIsOff;
		public bool interactionIsOff;
		public bool menuIsOff;
		public bool movementIsOff;
		public bool cameraIsOff;
		public bool triggerIsOff;
		public bool playerIsOff;

		public bool playedGlobalOnStart = false;


		private Dialog dialog;
		private ArrowPrompt[] arrowPrompts;
		private DragBase[] dragBases;
		private Parallax2D[] parallax2Ds;
		private Hotspot[] hotspots;
		private PlayerInput playerInput;
		private PlayerCursor playerCursor;
		private PlayerInteraction playerInteraction;
		private PlayerMovement playerMovement;
		private PlayerMenus playerMenus;
		private SettingsManager settingsManager;
		private ActionListManager actionListManager;

		
		private void Awake ()
		{
			Time.timeScale = 1f;
			DontDestroyOnLoad (this);
			GetReferences ();
		}


		private void OnLevelWasLoaded ()
		{
			GetReferences ();
		}


		public bool PlayGlobalOnStart ()
		{
			if (playedGlobalOnStart)
			{
				return false;
			}

			if (settingsManager.actionListOnStart)
			{
				AdvGame.RunActionListAsset (settingsManager.actionListOnStart);
				playedGlobalOnStart = true;
				return true;
			}

			return false;
		}


		private void GetReferences ()
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;

			if (settingsManager != null && settingsManager.IsInLoadingScene ())
			{
				return;
			}

			playerCursor = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerCursor>();
			playerInput = playerCursor.GetComponent <PlayerInput>();
			playerInteraction = playerCursor.GetComponent <PlayerInteraction>();
			playerMovement = playerCursor.GetComponent <PlayerMovement>();
			dialog = playerCursor.GetComponent <Dialog>();

			playerMenus = GetComponent <PlayerMenus>();

			actionListManager = playerCursor.GetComponent <ActionListManager>();

			dragBases = FindObjectsOfType (typeof (DragBase)) as DragBase[];
			hotspots = FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			arrowPrompts = FindObjectsOfType (typeof (ArrowPrompt)) as ArrowPrompt[];
			parallax2Ds = FindObjectsOfType (typeof (Parallax2D)) as Parallax2D[];
		}


		private void Update ()
		{
			if (settingsManager != null && settingsManager.IsInLoadingScene ())
			{
				return;
			}

			if (gameState != GameState.Paused)
			{
				lastNonPausedState = gameState;
			}

			if (!inputIsOff)
			{
				playerInput.UpdateInput ();

				if (gameState == GameState.Normal)
				{
					playerInput.UpdateDirectInput ();
				}

				dialog.UpdateSkipDialogue ();
			}

			if (!cursorIsOff)
			{
				playerCursor.UpdateCursor ();
			}

			if (!menuIsOff)
			{
				playerMenus.UpdateAllMenus ();
			}

			if (!interactionIsOff)
			{
				playerInteraction.UpdateInteraction ();
			}

			actionListManager.UpdateActionListManager ();

			if (!movementIsOff)
			{
				foreach (DragBase dragBase in dragBases)
				{
					dragBase.UpdateMovement ();
				}

				if (gameState == GameState.Normal && settingsManager && settingsManager.movementMethod != MovementMethod.None)
				{
					playerMovement.UpdatePlayerMovement ();
				}
			}

			if (!interactionIsOff)
			{
				playerInteraction.UpdateInventory ();
			}

			if (HasGameStateChanged ())
			{
				if (settingsManager.movementMethod == MovementMethod.UltimateFPS)
				{
					UltimateFPSIntegration.Update (gameState);
				}

				if (gameState != GameState.Paused)
				{
					AudioListener.pause = false;
				}
			}

			previousUpdateState = gameState;
		}


		private void LateUpdate ()
		{
			foreach (Parallax2D parallax2D in parallax2Ds)
			{
				parallax2D.UpdateOffset ();
			}
		}


		private bool HasGameStateChanged ()
		{
			if (previousUpdateState != gameState)
			{
				return true;
			}
			return false;
		}


		private void OnGUI ()
		{
			if (settingsManager != null && settingsManager.IsInLoadingScene ())
			{
				return;
			}

			if (!cursorIsOff && gameState == GameState.Normal && settingsManager)
			{
				if (settingsManager.hotspotIconDisplay != HotspotIconDisplay.Never)
				{
					foreach (Hotspot hotspot in hotspots)
					{
						hotspot.DrawHotspotIcon ();
					}
				}

				foreach (DragBase dragBase in dragBases)
				{
					dragBase.DrawGrabIcon ();
				}
			}

			if (!inputIsOff)
			{
				playerInput.DrawDragLine ();
				
				foreach (ArrowPrompt arrowPrompt in arrowPrompts)
				{
					arrowPrompt.DrawArrows ();
				}
			}

			if (!menuIsOff)
			{
				playerMenus.DrawMenus ();
			}

			if (!cursorIsOff)
			{
				playerCursor.DrawCursor ();
			}

			if (!cameraIsOff)
			{
				KickStarter.mainCamera.DrawCameraFade ();
			}
		}


		public GameState GetLastNonPausedState ()
		{
			return lastNonPausedState;
		}
		
		
		public void RestoreLastNonPausedState ()
		{
			if (actionListManager.IsGameplayBlocked ())
			{
				gameState = GameState.Cutscene;
			}
			else if (playerInput.activeConversation != null)
			{
				gameState = GameState.DialogOptions;
			}
			else
			{
				gameState = GameState.Normal;
			}
		}
		

		public void TurnOnAC ()
		{
			gameState = GameState.Normal;
		}
		
		
		public void TurnOffAC ()
		{
			if (GameObject.FindWithTag (Tags.gameEngine))
			{
				if (GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>())
				{
					GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>().KillAllLists ();
				}

				if (GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>())
				{
					GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>().KillDialog (true);
				}
			}
			
			Moveable[] moveables = FindObjectsOfType (typeof (Moveable)) as Moveable[];
			foreach (Moveable moveable in moveables)
			{
				moveable.Kill ();
			}

			Char[] chars = FindObjectsOfType (typeof (Char)) as Char[];
			foreach (Char _char in chars)
			{
				_char.EndPath ();
			}
			
			gameState = GameState.Cutscene;
		}


		private void OnDestroy ()
		{
			playerInput = null;
			playerCursor = null;
			playerMenus = null;
			playerInteraction = null;
			playerMovement = null;
			settingsManager = null;
			arrowPrompts = null;
			hotspots = null;
			actionListManager = null;
			dialog = null;
		}

	}

}