/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerCursor.cs"
 * 
 *	This script displays a cursor graphic on the screen.
 *	PlayerInput decides if this should be at the mouse position,
 *	or a position based on controller input.
 *	The cursor graphic changes based on what hotspot is underneath it.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{
	
	public class PlayerCursor : MonoBehaviour
	{
		
		private int selectedCursor = -1; // -2 = inventory, -1 = pointer, 0+ = cursor array
		private bool showCursor = false;
		private bool canShowHardwareCursor = false;
		private float pulse = 0f;
		private int pulseDirection = 0; // 0 = none, 1 = in, -1 = out
		
		// Animation variables
		private CursorIconBase activeIcon = null;
		private CursorIconBase activeLookIcon = null;
		
		private SettingsManager settingsManager;
		private CursorManager cursorManager;
		private StateHandler stateHandler;
		private RuntimeInventory runtimeInventory;
		private PlayerInput playerInput;
		private PlayerMovement playerMovement;
		private PlayerInteraction playerInteraction;
		private PlayerMenus playerMenus;
		
		
		
		private void Awake ()
		{
			playerInput = this.GetComponent <PlayerInput>();
			playerInteraction = this.GetComponent <PlayerInteraction>();
			playerMovement = this.GetComponent <PlayerMovement>();
			
			if (AdvGame.GetReferences () == null)
			{
				Debug.LogError ("A References file is required - please use the Adventure Creator window to create one.");
			}
			else
			{
				if (AdvGame.GetReferences ().settingsManager)
				{
					settingsManager = AdvGame.GetReferences ().settingsManager;
				}
				if (AdvGame.GetReferences ().cursorManager)
				{
					cursorManager = AdvGame.GetReferences ().cursorManager;
				}
			}
		}
		
		
		private void Start ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
			{
				stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
			}
			
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
			{
				runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
			}
			
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>())
			{
				playerMenus = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>();
			}
		}
		
		
		public void UpdateCursor ()
		{
			if (cursorManager.cursorRendering == CursorRendering.Software)
			{
				if (!canShowHardwareCursor)
				{
					Screen.showCursor = false;
				}
				else if (playerInput.dragState == DragState.Moveable)
				{
					Screen.showCursor = false;
				}
				else if (settingsManager && cursorManager && (!cursorManager.allowMainCursor || cursorManager.pointerIcon.texture == null) && runtimeInventory.selectedItem == null && settingsManager.inputMethod == InputMethod.MouseAndKeyboard && stateHandler.gameState != GameState.Cutscene)
				{
					Screen.showCursor = true;
				}
				else if (cursorManager == null)
				{
					Screen.showCursor = true;
				}
				else
				{
					Screen.showCursor = false;
				}
			}

			if (settingsManager && stateHandler)
			{
				if (stateHandler.gameState == GameState.Cutscene)
				{
					if (cursorManager.waitIcon.texture != null)
					{
						showCursor = true;
					}
					else
					{
						showCursor = false;
					}
				}
				else if (stateHandler.gameState != GameState.Normal && settingsManager.inputMethod == InputMethod.KeyboardOrController)
				{
					showCursor = false;
				}
				else if (cursorManager)
				{
					if (stateHandler.gameState == GameState.Paused && (cursorManager.cursorDisplay == CursorDisplay.OnlyWhenPaused || cursorManager.cursorDisplay == CursorDisplay.Always))
					{
						showCursor = true;
					}
					else if (playerInput.dragState == DragState.Moveable)
					{
						showCursor = false;
					}
					else if ((stateHandler.gameState == GameState.Normal || stateHandler.gameState == GameState.DialogOptions) && (cursorManager.cursorDisplay == CursorDisplay.Always))
					{
						showCursor = true;
					}
					else
					{
						showCursor = false;
					}
				}
				else
				{
					showCursor = true;
				}
				
				if (stateHandler.gameState == GameState.Normal && settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && cursorManager != null && cursorManager.cycleCursors &&
				    (playerInput.mouseState == MouseState.RightClick || playerInput.InputGetButtonDown ("CycleCursors")))
				{
					CycleCursors ();
				}

				else if (stateHandler.gameState == GameState.Normal && settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot &&
				    (playerInput.mouseState == MouseState.RightClick || playerInput.InputGetButtonDown ("CycleCursors")))
				{
					playerInteraction.SetNextInteraction ();
				}
			}

			if (cursorManager.cursorRendering == CursorRendering.Hardware)
			{
				Screen.showCursor = showCursor;
			}
		}
		
		
		public void DrawCursor ()
		{
			if (playerInput && playerInteraction && stateHandler && settingsManager && cursorManager && runtimeInventory && showCursor)
			{
				if (playerInput.IsCursorLocked () && settingsManager.hideLockedCursor)
				{
					canShowHardwareCursor = false;
					return;
				}
				
				GUI.depth = -1;
				canShowHardwareCursor = true;
				
				if (runtimeInventory.selectedItem != null && cursorManager.inventoryHandling != InventoryHandling.ChangeHotspotLabel)
				{
					// Cursor becomes selected inventory
					selectedCursor = -2;
					canShowHardwareCursor = false;
				}
				else if (settingsManager.interactionMethod != AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					if (settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive && cursorManager.allowInteractionCursorForInventory && runtimeInventory.hoverItem != null)
					{
						ShowContextIcons (runtimeInventory.hoverItem);
						return;
					}
					else if (playerInteraction.GetActiveHotspot () != null && stateHandler.gameState == GameState.Normal && (playerInteraction.GetActiveHotspot ().HasContextUse () || playerInteraction.GetActiveHotspot ().HasContextLook ()) && cursorManager.allowInteractionCursor)
					{
						selectedCursor = 0;
						
						if (settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
						{
							canShowHardwareCursor = false;
							ShowContextIcons ();
						}
					}
					else
					{
						selectedCursor = -1;
					}
				}
				else if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && (stateHandler.gameState == GameState.DialogOptions || stateHandler.gameState == GameState.Paused))
				{
					selectedCursor = -1;
				}
				
				if (stateHandler.gameState == GameState.Cutscene && cursorManager.waitIcon.texture != null)
				{
					// Wait
					DrawIcon (cursorManager.waitIcon, false);
				}
				else if (selectedCursor == -2 && runtimeInventory.selectedItem != null && (settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || settingsManager.inventoryInteractions == InventoryInteractions.Single))
				{
					// Inventory
					canShowHardwareCursor = false;

					if (settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						if (playerInteraction.GetActiveHotspot () == null && runtimeInventory.hoverItem == null)
						{
							if (playerInteraction.GetInteractionIndex () >= 0)
							{
								// Item was selected due to cycling icons
								playerInteraction.ResetInteractionIndex ();
								runtimeInventory.SetNull ();
								return;
							}
						}
					}
					
					if (settingsManager.inventoryActiveEffect != InventoryActiveEffect.None && runtimeInventory.selectedItem.activeTex && playerMenus.GetHotspotLabel () != "" &&
					    (settingsManager.activeWhenUnhandled || playerInteraction.DoesHotspotHaveInventoryInteraction () || (runtimeInventory.hoverItem != null && runtimeInventory.hoverItem.DoesHaveInventoryInteraction (runtimeInventory.selectedItem))))
					{
						DrawActiveInventoryCursor ();
					}
					else if (runtimeInventory.selectedItem.tex)
					{
						DrawIcon (AdvGame.GUIBox (playerInput.GetMousePosition (), cursorManager.inventoryCursorSize), runtimeInventory.selectedItem.tex);
						pulseDirection = 0;
					}
					else
					{
						selectedCursor = -1;
						runtimeInventory.SetNull ();
						pulseDirection = 0;
					}
					
					if (runtimeInventory.selectedItem != null && runtimeInventory.selectedItem.canCarryMultiple)
					{
						runtimeInventory.DrawInventoryCount (playerInput.GetMousePosition (), cursorManager.inventoryCursorSize, runtimeInventory.selectedItem.count);
					}
				}
				else if (selectedCursor >= 0 && settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && cursorManager.allowInteractionCursor)
				{
					//	Custom icon
					
					pulseDirection = 0;
					canShowHardwareCursor = false;
					
					DrawIcon (cursorManager.cursorIcons [selectedCursor], false);
				}
				else if (settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
				{
					ShowCycleCursor (playerInteraction.GetActiveUseButtonIconID (), playerInteraction.GetActiveInvButtonID ());
				}
				else if (cursorManager.allowMainCursor || settingsManager.inputMethod == InputMethod.KeyboardOrController)
				{
					// Pointer
					
					pulseDirection = 0;
					
					if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && runtimeInventory.hoverItem == null && playerInteraction.GetActiveHotspot () != null && (!playerInput.interactionMenuIsOn || settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot))
					{
						if (playerInteraction.GetActiveHotspot ().IsSingleInteraction ())
						{
							ShowContextIcons ();
						}
						else if (cursorManager.mouseOverIcon.texture != null)
						{
							DrawIcon (cursorManager.mouseOverIcon, false);
						}
						else
						{
							DrawMainCursor ();
						}
					}
					else if (selectedCursor == -1 || settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
					{
						DrawMainCursor ();
					}
					else if (selectedCursor == -2 && runtimeInventory.selectedItem == null)
					{
						selectedCursor = -1;
					}
				}
				
			}
		}
		
		
		private void DrawMainCursor ()
		{
			bool showWalkCursor = false;

			if (cursorManager.walkIcon.texture && playerInput && !playerInput.mouseOverMenu && !playerInput.interactionMenuIsOn && stateHandler && stateHandler.gameState == GameState.Normal)
			{
				if (cursorManager.onlyWalkWhenOverNavMesh)
				{
					if (playerMovement.ClickPoint (playerInput.GetMousePosition ()) != Vector3.zero)
					{
						showWalkCursor = true;
					}
				}
				else
				{
					showWalkCursor = true;
				}
			}

			if (showWalkCursor)
			{
				DrawIcon (cursorManager.walkIcon, false);
			}
			else if (cursorManager.pointerIcon.texture)
			{
				DrawIcon (cursorManager.pointerIcon, false);
			}
			else
			{
				Debug.LogWarning ("No 'main' texture defined - please set in SettingsManager.");
			}
		}
		
		
		private void ShowContextIcons ()
		{
			Hotspot hotspot = playerInteraction.GetActiveHotspot ();
			if (hotspot == null)
			{
				return;
			}
			
			if (hotspot.HasContextUse ())
			{
				if (!hotspot.HasContextLook ())
				{
					DrawIcon (cursorManager.GetCursorIconFromID (hotspot.GetFirstUseButton ().iconID), false);
					return;
				}
				else
				{
					Button _button = hotspot.GetFirstUseButton ();
					
					if (hotspot.HasContextUse () && hotspot.HasContextLook () && cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide)
					{
						CursorIcon icon = cursorManager.GetCursorIconFromID (_button.iconID);
						DrawIcon (new Vector2 (-icon.size * Screen.width / 2f, 0f), icon, false);
					}
					else
					{
						DrawIcon (cursorManager.GetCursorIconFromID (_button.iconID), false);
					}
				}
			}
			
			if (hotspot.HasContextLook () &&
			    (!hotspot.HasContextUse () ||
			 (hotspot.HasContextUse () && cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide)))
			{
				if (cursorManager.cursorIcons.Count > 0)
				{
					CursorIcon icon = cursorManager.GetCursorIconFromID (cursorManager.lookCursor_ID);
					
					if (hotspot.HasContextUse () && hotspot.HasContextLook () && cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide)
					{
						DrawIcon (new Vector2 (icon.size * Screen.width / 2f, 0f), icon, true);
					}
					else
					{
						DrawIcon (icon, true);
					}
				}
			}	
		}


		private void ShowContextIcons (InvItem invItem)
		{
			if (cursorManager.cursorIcons.Count > 0)
			{
				if (invItem.lookActionList != null && cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide)
				{
					CursorIcon icon = cursorManager.GetCursorIconFromID (invItem.useIconID);
					DrawIcon (new Vector2 (-icon.size * Screen.width / 2f, 0f), icon, false);
				}
				else
				{
					DrawIcon (cursorManager.GetCursorIconFromID (invItem.useIconID), false);
				}
				
				if (invItem.lookActionList != null)
				{
					CursorIcon icon = cursorManager.GetCursorIconFromID (cursorManager.lookCursor_ID);
					
					if (invItem.lookActionList != null && cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide)
					{
						DrawIcon (new Vector2 (icon.size * Screen.width / 2f, 0f), icon, true);
					}
					else
					{
						DrawIcon (icon, true);
					}
				}
			}	
		}


		private void ShowCycleCursor (int useCursorID, int invCursorID)
		{
			if (invCursorID >= 0)
			{
				selectedCursor = -2;
				DrawActiveInventoryCursor ();
			}
			else if (useCursorID >= 0)
			{
				selectedCursor = useCursorID;
				DrawIcon (cursorManager.GetCursorIconFromID (useCursorID), false);
			}
			else if (useCursorID == -1)
			{
				selectedCursor = -1;
				DrawMainCursor ();
			}
		}
		
		
		private void DrawActiveInventoryCursor ()
		{
			if (runtimeInventory.selectedItem == null)
			{
				return;
			}

			if (runtimeInventory.selectedItem.activeTex == null)
			{
				DrawIcon (AdvGame.GUIBox (playerInput.GetMousePosition (), cursorManager.inventoryCursorSize), runtimeInventory.selectedItem.tex);
				pulseDirection = 0;
			}
			if (settingsManager.inventoryActiveEffect == InventoryActiveEffect.Simple)
			{
				DrawIcon (AdvGame.GUIBox (playerInput.GetMousePosition (), cursorManager.inventoryCursorSize), runtimeInventory.selectedItem.activeTex);
			}
			else if (settingsManager.inventoryActiveEffect == InventoryActiveEffect.Pulse && runtimeInventory.selectedItem.tex)
			{
				if (pulseDirection == 0)
				{
					pulse = 0f;
					pulseDirection = 1;
				}
				else if (pulse > 1f)
				{
					pulse = 1f;
					pulseDirection = -1;
				}
				else if (pulse < 0f)
				{
					pulse = 0f;
					pulseDirection = 1;
				}
				else if (pulseDirection == 1)
				{
					pulse += settingsManager.inventoryPulseSpeed * Time.deltaTime;
				}
				else if (pulseDirection == -1)
				{
					pulse -= settingsManager.inventoryPulseSpeed * Time.deltaTime;
				}
				
				Color backupColor = GUI.color;
				Color tempColor = GUI.color;
				
				tempColor.a = pulse;
				GUI.color = tempColor;
				DrawIcon (AdvGame.GUIBox (playerInput.GetMousePosition (), cursorManager.inventoryCursorSize), runtimeInventory.selectedItem.activeTex);
				GUI.color = backupColor;
				DrawIcon (AdvGame.GUIBox (playerInput.GetMousePosition (), cursorManager.inventoryCursorSize), runtimeInventory.selectedItem.tex);
			}
		}
		
		
		private void DrawIcon (Rect _rect, Texture2D _tex)
		{
			if (_tex != null)
			{
				if (cursorManager.cursorRendering == CursorRendering.Hardware)
				{
					Cursor.SetCursor (_tex, Vector2.zero, CursorMode.Auto);
				}
				else
				{
					GUI.DrawTexture (_rect, _tex, ScaleMode.ScaleToFit, true, 0f);
				}
			}
		}
		
		
		private void DrawIcon (Vector2 offset, CursorIconBase icon, bool isLook)
		{
			if (icon != null)
			{
				bool isNew = false;
				if (isLook && activeLookIcon != icon)
				{
					activeLookIcon = icon;
					icon.Reset ();
				}
				else if (!isLook && activeIcon != icon)
				{
					activeIcon = icon;
					isNew = true;
					icon.Reset ();
				}

				if (cursorManager.cursorRendering == CursorRendering.Hardware)
				{
					if (isNew)
					{
						Cursor.SetCursor (icon.texture, icon.clickOffset, CursorMode.Auto);
					}
				}
				else
				{
					icon.Draw (playerInput.GetMousePosition () + offset);
				}
			}
		}
		
		
		private void DrawIcon (CursorIconBase icon, bool isLook)
		{
			if (icon != null)
			{
				DrawIcon (new Vector2 (0f, 0f), icon, isLook);
			}
		}


		private void CycleCursors ()
		{
			if (cursorManager.cursorIcons.Count > 0)
			{
				selectedCursor ++;
				if (selectedCursor >= cursorManager.cursorIcons.Count)
				{
					selectedCursor = -1;
				}
			}
			else
			{
				// Pointer
				selectedCursor = -1;
			}
		}
		
		
		public int GetSelectedCursor ()
		{
			return selectedCursor;
		}
		
		
		public int GetSelectedCursorID ()
		{
			if (cursorManager && cursorManager.cursorIcons.Count > 0 && selectedCursor > -1)
			{
				return cursorManager.cursorIcons [selectedCursor].id;
			}
			
			return -1;
		}
		
		
		public void ResetSelectedCursor ()
		{
			selectedCursor = -1;
		}
		
		
		public void SetCursorFromID (int ID)
		{
			if (cursorManager && cursorManager.cursorIcons.Count > 0)
			{
				foreach (CursorIcon cursor in cursorManager.cursorIcons)
				{
					if (cursor.id == ID)
					{
						selectedCursor = cursorManager.cursorIcons.IndexOf (cursor);
					}
				}
			}
		}
		
		
		private void OnDestroy ()
		{
			stateHandler = null;
			runtimeInventory = null;
			playerInput = null;
			playerInteraction = null;
			playerMenus = null;
		}
		
	}
	
}