/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerInteraction.cs"
 * 
 *	This script processes cursor clicks over hotspots and NPCs
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class PlayerInteraction : MonoBehaviour
	{
		
		private Hotspot hotspot;
		private Button button = null;
		private int interactionIndex = -1;

		private PlayerInput playerInput;
		private PlayerMenus playerMenus;
		private PlayerCursor playerCursor;
		private StateHandler stateHandler;
		private RuntimeInventory runtimeInventory;
		private SettingsManager settingsManager;
		private CursorManager cursorManager;

		
		private void Awake ()
		{
			playerInput = this.GetComponent <PlayerInput>();
			playerCursor = this.GetComponent <PlayerCursor>();

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
			if (GameObject.FindWithTag (Tags.persistentEngine))
			{
				if (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
				{
					stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
				}
				
				if (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
				{
					runtimeInventory = stateHandler.GetComponent <RuntimeInventory>();
				}
				
				if (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>())
				{
					playerMenus = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>();
				}
			}
		}
		
		
		public void UpdateInteraction ()
		{
			if (stateHandler && playerInput && settingsManager && runtimeInventory && stateHandler.gameState == GameState.Normal)			
			{
				if (playerInput.dragState == DragState.Moveable)
				{
					DisableHotspot (true);
					return;
				}

				if (playerInput.mouseState == MouseState.RightClick && runtimeInventory.selectedItem != null && !playerInput.mouseOverMenu && (settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive && settingsManager.inventoryInteractions == InventoryInteractions.Single))
				{
					if (settingsManager.rightClickInventory == RightClickInventory.ExaminesItem)
					{
						playerInput.mouseState = MouseState.Normal;
						runtimeInventory.Look (runtimeInventory.selectedItem);
					}
					else if (settingsManager.rightClickInventory == AC.RightClickInventory.DeselectsItem)
					{
						runtimeInventory.SetNull ();
					}
				}

				if (playerInput.IsCursorLocked () && settingsManager.hideLockedCursor)
				{
					DisableHotspot (true);
					return;
				}

				if (!playerInput.IsCursorReadable ())
				{
					return;
				}
				
				if (!playerInput.mouseOverMenu && Camera.main && !playerInput.ActiveArrowsDisablingHotspots ())
				{
					if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
					{
						if (settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
						{
							ContextSensitiveClick ();
						}
						else if (!playerInput.interactionMenuIsOn || settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot)
						{
							ChooseHotspotThenInteractionClick ();
						}
					}
					else
					{
						ContextSensitiveClick ();
					}
				}
				else 
				{
					DisableHotspot (false);
				}

				if (settingsManager.playerFacesHotspots)
				{
					if (hotspot)
					{
						KickStarter.player.SetHeadTurnTarget (hotspot.GetIconPosition (), false, HeadFacing.Hotspot);
					}
					else if (button == null)
					{
						KickStarter.player.ClearHeadTurnTarget (HeadFacing.Hotspot, false);
					}
				}
			}
		}


		public void UpdateInventory ()
		{
			if (hotspot == null && button == null && IsDroppingInventory ())
			{
				runtimeInventory.SetNull ();
			}
		}
		
		
		private Hotspot CheckForHotspots ()
		{
			if (settingsManager)
			{
				if (playerInput)
				{
					if (settingsManager.inventoryDragDrop && playerInput.GetMousePosition () == Vector2.zero)
					{
						return null;
					}
				}

				if (settingsManager.hotspotDetection == HotspotDetection.PlayerVicinity && KickStarter.player.hotspotDetector)
				{
					if (settingsManager.movementMethod == MovementMethod.Direct || settingsManager.movementMethod == MovementMethod.FirstPerson)
					{
						return (KickStarter.player.hotspotDetector.GetSelected ());
					}
					else
					{
						// Just highlight the nearest hotspot, but don't make it the "active" one
						KickStarter.player.hotspotDetector.HighlightAll ();
					}
				}

				if (settingsManager && settingsManager.IsUnity2D ())
				{
					RaycastHit2D hit;
					if (Camera.main.isOrthoGraphic)
					{
						hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (playerInput.GetMousePosition ()), Vector2.zero, settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer (settingsManager.hotspotLayer));
					}
					else
					{
						Vector3 pos = playerInput.GetMousePosition ();
						pos.z = -Camera.main.transform.position.z;
						hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (pos), Vector2.zero, settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer (settingsManager.hotspotLayer));
					}

					if (hit.collider != null && hit.collider.gameObject.GetComponent <Hotspot>())
					{
						Hotspot hitHotspot = hit.collider.gameObject.GetComponent <Hotspot>();
						if (settingsManager.hotspotDetection == HotspotDetection.MouseOver)
						{
							return (hitHotspot);
						}
						else if (settingsManager.hotspotDetection == HotspotDetection.PlayerVicinity && KickStarter.player.hotspotDetector && KickStarter.player.hotspotDetector.IsHotspotInTrigger (hitHotspot))
						{
							return (hitHotspot);
						}
					}
				}
				else
				{
					Ray ray = Camera.main.ScreenPointToRay (playerInput.GetMousePosition ());
					RaycastHit hit;
					
					if (Physics.Raycast (ray, out hit, settingsManager.hotspotRaycastLength, 1 << LayerMask.NameToLayer (settingsManager.hotspotLayer)))
					{
						if (hit.collider.gameObject.GetComponent <Hotspot>())
						{
							Hotspot hitHotspot = hit.collider.gameObject.GetComponent <Hotspot>();
							if (settingsManager.hotspotDetection == HotspotDetection.MouseOver)
							{
								return (hitHotspot);
							}
							else if (settingsManager.hotspotDetection == HotspotDetection.PlayerVicinity && KickStarter.player.hotspotDetector && KickStarter.player.hotspotDetector.IsHotspotInTrigger (hitHotspot))
							{
								return (hitHotspot);
							}
						}
					}
				}
			}
			
			return null;
		}


		private bool CanDoDoubleTap ()
		{
			if (runtimeInventory.selectedItem != null && settingsManager.inventoryDragDrop)
				return false;

			if (settingsManager.inputMethod == InputMethod.TouchScreen && settingsManager.doubleTapHotspots)
				return true;

			return false;
		}


		private void ChooseHotspotThenInteractionClick ()
		{
			if (CanDoDoubleTap ())
			{
				if (playerInput.mouseState == MouseState.SingleClick)
				{
					ChooseHotspotThenInteractionClick_Process (true);
				}
			}
			else
			{
				ChooseHotspotThenInteractionClick_Process (false);
			}
		}
		

		private void ChooseHotspotThenInteractionClick_Process (bool doubleTap)
		{
			Hotspot newHotspot = CheckForHotspots ();

			if (hotspot != null && newHotspot == null)
			{
				DisableHotspot (false);
			}
			else if (newHotspot != null)
			{
				if (newHotspot.IsSingleInteraction ())
				{
					ContextSensitiveClick ();
					return;
				}
				
				if (playerInput.mouseState == MouseState.HeldDown && playerInput.dragState == DragState.Player)
				{
					// Disable hotspots while dragging player
					DisableHotspot (false);
				}
				else
				{
					bool clickedNew = false;
					if (newHotspot != hotspot)
					{
						clickedNew = true;

						if (hotspot)
						{
							hotspot.Deselect ();
							playerMenus.DisableHotspotMenus ();
						}
						hotspot = newHotspot;
						hotspot.Select ();

						playerMenus.SetInteractionMenus (false);
					}

					if (hotspot)
					{
						if (playerInput.mouseState == MouseState.SingleClick ||
						    (settingsManager.inventoryDragDrop && IsDroppingInventory ()) ||
						    (settingsManager.MouseOverForInteractionMenu () && runtimeInventory.hoverItem == null && clickedNew && !IsDroppingInventory ()))
						{
							if (runtimeInventory.selectedItem != null && settingsManager.inventoryInteractions == InventoryInteractions.Single)
							{
								if (!settingsManager.inventoryDragDrop && clickedNew && doubleTap)
								{
									return;
								} 
								else
								{
									HandleInteraction ();
								}
							}
							else if (playerMenus)
							{
								if (playerInput.interactionMenuIsOn && settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot)
								{
									ClickHotspotToInteract ();
									return;
								}

								if (clickedNew && doubleTap)
								{
									return;
								}

								if (KickStarter.player)
								{
									KickStarter.player.Halt ();
								}

								playerInput.hotspotMovingTo = null;
								StopInteraction ();
								runtimeInventory.SetNull ();
								playerMenus.SetInteractionMenus (true);
							}
						}
						else if (playerInput.mouseState == MouseState.RightClick)
						{
							hotspot.Deselect ();
						}
					}
				}
			}
		}


		private void ContextSensitiveClick ()
		{
			if (CanDoDoubleTap ())
			{
				// Detect Hotspots only on mouse click
				if (playerInput.mouseState == MouseState.SingleClick)
				{
					// Check Hotspots only when click/tap
					ContextSensitiveClick_Process (true, CheckForHotspots ());
				}
				else if (playerInput.mouseState == MouseState.RightClick)
				{
					HandleInteraction ();
				}
			}
			else
			{
				// Always detect Hotspots
				ContextSensitiveClick_Process (false, CheckForHotspots ());

				if (!playerInput.mouseOverMenu && hotspot)
				{
					if (playerInput.mouseState == MouseState.SingleClick || playerInput.mouseState == MouseState.DoubleClick || playerInput.mouseState == MouseState.RightClick || IsDroppingInventory ())
					{
						if (settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
						{
							if (playerInput.mouseState != MouseState.RightClick)
							{
								ClickHotspotToInteract ();
							}
						}
						else
						{
							HandleInteraction ();
						}
					}
				}
			}

		}


		private void ContextSensitiveClick_Process (bool doubleTap, Hotspot newHotspot)
		{
			if (hotspot != null && newHotspot == null)
			{
				DisableHotspot (false);
			}
			else if (newHotspot != null)
			{
				if (playerInput.mouseState == MouseState.HeldDown && playerInput.dragState == DragState.Player)
				{
					// Disable hotspots while dragging player
					DisableHotspot (false); 
				}
				else if (newHotspot != hotspot)
				{
					if (hotspot)
					{
						hotspot.Deselect ();
					}
					
					hotspot = newHotspot;
					hotspot.Select ();

					if (settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						runtimeInventory.MatchInteractions ();
						ResetInteractionIndex ();
					}
				}
				else if (hotspot != null && doubleTap)
				{
					// Still work if not clicking on the active Hotspot
					HandleInteraction ();
				}
			}
		}

		
		public void DisableHotspot (bool isInstant)
		{
			if (hotspot)
			{
				if (isInstant)
				{
					hotspot.DeselectInstant ();
				}
				else
				{
					hotspot.Deselect ();
				}
				hotspot = null;
			}
		}
		
		
		public bool DoesHotspotHaveInventoryInteraction ()
		{
			if (hotspot && runtimeInventory && runtimeInventory.selectedItem != null)
			{
				foreach (Button _button in hotspot.invButtons)
				{
					if (_button.invID == runtimeInventory.selectedItem.id && !_button.isDisabled)
					{
						return true;
					}
				}
			}
			
			return false;
		}
		
		
		private void HandleInteraction ()
		{
			if (hotspot)
			{
				if (settingsManager == null || settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					if (playerInput.mouseState == MouseState.SingleClick || playerInput.mouseState == MouseState.DoubleClick)
					{
						if (runtimeInventory.selectedItem == null && hotspot.HasContextUse ())
						{
							// Perform "Use" interaction
							ClickButton (InteractionType.Use, -1, -1);
						}
						
						else if (runtimeInventory.selectedItem != null)
						{
							// Perform "Use Inventory" interaction
							ClickButton (InteractionType.Inventory, -1, runtimeInventory.selectedItem.id);

							if (settingsManager.inventoryDisableLeft)
							{
								runtimeInventory.SetNull ();
							}
						}
						
						else if (hotspot.HasContextLook () && cursorManager.leftClickExamine)
						{
							// Perform "Look" interaction
							ClickButton (InteractionType.Examine, -1, -1);
						}
					}
					else if (playerInput.mouseState == MouseState.RightClick)
					{
						if (hotspot.HasContextLook () && runtimeInventory.selectedItem == null)
						{
							// Perform "Look" interaction
							ClickButton (InteractionType.Examine, -1, -1);
						}
					}
					else if (settingsManager.inventoryDragDrop && IsDroppingInventory ())
					{
						// Perform "Use Inventory" interaction (Drag n' drop mode)
						ClickButton (InteractionType.Inventory, -1, runtimeInventory.selectedItem.id);
					}
				}
				
				else if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && playerCursor && cursorManager)
				{
					if (playerInput.mouseState == MouseState.SingleClick)
					{
						if (runtimeInventory.selectedItem == null && hotspot.provideUseInteraction && playerCursor.GetSelectedCursor () >= 0)
						{
							// Perform "Use" interaction
							ClickButton (InteractionType.Use, cursorManager.cursorIcons [playerCursor.GetSelectedCursor ()].id, -1);
						}
						
						else if (runtimeInventory.selectedItem != null && playerCursor.GetSelectedCursor () == -2)
						{
							// Perform "Use Inventory" interaction
							playerCursor.ResetSelectedCursor ();
							ClickButton (InteractionType.Inventory, -1, runtimeInventory.selectedItem.id);

							if (settingsManager.inventoryDisableLeft)
							{
								runtimeInventory.SetNull ();
							}
						}
					}
					else if (settingsManager.inventoryDragDrop && IsDroppingInventory ())
					{
						// Perform "Use Inventory" interaction (Drag n' drop mode)
						ClickButton (InteractionType.Inventory, -1, runtimeInventory.selectedItem.id);
					}
				}
				
				else if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (runtimeInventory.selectedItem != null && hotspot.provideUseInteraction && settingsManager.inventoryInteractions == InventoryInteractions.Single)
					{
						// Perform "Use Inventory" interaction
						ClickButton (InteractionType.Inventory, -1, runtimeInventory.selectedItem.id);
					}
					
					else if (runtimeInventory.selectedItem == null && hotspot.IsSingleInteraction ())
					{
						// Perform "Use" interaction
						ClickButton (InteractionType.Use, -1, -1);

						if (settingsManager.inventoryDisableLeft)
						{
							runtimeInventory.SetNull ();
						}
					}
				}
			}
		}
		
		
		public void ClickButton (InteractionType _interactionType, int selectedCursorID, int selectedItemID)
		{
			StopCoroutine ("UseObject");

			if (KickStarter.player)
			{
				KickStarter.player.EndPath ();
			}

			playerInput.mouseState = MouseState.Normal;
			playerInput.ResetClick ();
			button = null;

			if (_interactionType == InteractionType.Use)
			{
				if (selectedCursorID == -1)
				{
					button = hotspot.GetFirstUseButton ();
				}
				else
				{
					foreach (Button _button in hotspot.useButtons)
					{
						if (_button.iconID == selectedCursorID)
						{
							button = _button;
							break;
						}
					}

					if (button == null && settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
					{
						AdvGame.RunActionListAsset (cursorManager.GetUnhandledInteraction (selectedCursorID));
						runtimeInventory.SetNull ();
						KickStarter.player.ClearHeadTurnTarget (HeadFacing.Hotspot, false);
						return;
					}
				}
			}
			else if (_interactionType == InteractionType.Examine)
			{
				button = hotspot.lookButton;
			}
			else if (_interactionType == InteractionType.Inventory && selectedItemID >= 0)
			{
				foreach (Button invButton in hotspot.invButtons)
				{
					if (invButton.invID == selectedItemID)
					{
						button = invButton;
						break;
					}
				}
			}
			
			if (button != null && button.isDisabled)
			{
				button = null;
				
				if (_interactionType != InteractionType.Inventory)
				{
					KickStarter.player.ClearHeadTurnTarget (HeadFacing.Hotspot, false);
					return;
				}
			}

			StartCoroutine ("UseObject", selectedItemID);
		}
		
		
		private IEnumerator UseObject (int selectedItemID)
		{
			bool doRun = false;
			if (playerInput.hotspotMovingTo == hotspot)
			{
				doRun = true;
			}

			if (playerInput != null && playerInput.runLock == PlayerMoveLock.AlwaysWalk)
			{
				doRun = false;
			}
			
			if (KickStarter.player)
			{
				if (button != null && !button.isBlocking && (button.playerAction == PlayerAction.WalkToMarker || button.playerAction == PlayerAction.WalkTo) && settingsManager.movementMethod != MovementMethod.UltimateFPS)
				{
					stateHandler.gameState = GameState.Normal;
					playerInput.hotspotMovingTo = hotspot;
				}
				else
				{
					stateHandler.gameState = GameState.Cutscene;
					playerInput.hotspotMovingTo = null;
				}
			}
			
			Hotspot _hotspot = hotspot;
			hotspot.Deselect ();
			hotspot = null;
			
			if (KickStarter.player)
			{
				if (button != null && button.playerAction != PlayerAction.DoNothing)
				{
					Vector3 lookVector = Vector3.zero;
					Vector3 targetPos = _hotspot.transform.position;
					
					if (settingsManager.ActInScreenSpace ())
					{
						lookVector = AdvGame.GetScreenDirection (KickStarter.player.transform.position, _hotspot.transform.position);
					}
					else
					{
						lookVector = targetPos - KickStarter.player.transform.position;
						lookVector.y = 0;
					}
					
					KickStarter.player.SetLookDirection (lookVector, false);
					
					if (button.playerAction == PlayerAction.TurnToFace)
					{
						while (KickStarter.player.IsTurning ())
						{
							yield return new WaitForFixedUpdate ();			
						}
					}
					
					if (button.playerAction == PlayerAction.WalkToMarker && _hotspot.walkToMarker)
					{
						if (Vector3.Distance (KickStarter.player.transform.position, _hotspot.walkToMarker.transform.position) > (1.05f - settingsManager.destinationAccuracy))
						{
							if (GetComponent <NavigationManager>())
							{
								Vector3[] pointArray;
								Vector3 targetPosition = _hotspot.walkToMarker.transform.position;
								
								if (settingsManager.ActInScreenSpace ())
								{
									targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
								}
								
								pointArray = GetComponent <NavigationManager>().navigationEngine.GetPointsArray (KickStarter.player.transform.position, targetPosition);
								KickStarter.player.MoveAlongPoints (pointArray, doRun);
								targetPos = pointArray [pointArray.Length - 1];
							}
							
							while (KickStarter.player.activePath)
							{
								yield return new WaitForFixedUpdate ();
							}
						}
						
						if (button.faceAfter)
						{
							lookVector = _hotspot.walkToMarker.transform.forward;
							lookVector.y = 0;
							KickStarter.player.Halt ();
							KickStarter.player.SetLookDirection (lookVector, false);
							
							while (KickStarter.player.IsTurning ())
							{
								yield return new WaitForFixedUpdate ();			
							}
						}
					}
					
					else if (lookVector.magnitude > 2f && button.playerAction == PlayerAction.WalkTo)
					{
						if (GetComponent <NavigationManager>())
						{
							Vector3[] pointArray;
							Vector3 targetPosition = _hotspot.transform.position;
							if (_hotspot.walkToMarker)
							{
								targetPosition = _hotspot.walkToMarker.transform.position;
							}

							if (settingsManager.ActInScreenSpace ())
							{
								targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
							}
							
							pointArray = GetComponent <NavigationManager>().navigationEngine.GetPointsArray (KickStarter.player.transform.position, targetPosition);
							KickStarter.player.MoveAlongPoints (pointArray, doRun);
							targetPos = pointArray [pointArray.Length - 1];
						}
						
						if (button.setProximity)
						{
							button.proximity = Mathf.Max (button.proximity, 1f);
							targetPos.y = KickStarter.player.transform.position.y;
							
							while (Vector3.Distance (KickStarter.player.transform.position, targetPos) > button.proximity && KickStarter.player.activePath)
							{
								yield return new WaitForFixedUpdate ();
							}
						}
						else
						{
							yield return new WaitForSeconds (0.6f);
						}
						
						if (button.faceAfter)
						{
							if (settingsManager.ActInScreenSpace ())
							{
								lookVector = AdvGame.GetScreenDirection (KickStarter.player.transform.position, _hotspot.transform.position);
							}
							else
							{
								lookVector = _hotspot.transform.position - KickStarter.player.transform.position;
								lookVector.y = 0;
							}
							
							KickStarter.player.Halt ();
							KickStarter.player.SetLookDirection (lookVector, false);
							
							while (KickStarter.player.IsTurning ())
							{
								yield return new WaitForFixedUpdate ();			
							}
						}
					}
				}
				else
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
				
				KickStarter.player.EndPath ();
				playerInput.hotspotMovingTo = null;
				yield return new WaitForSeconds (0.1f);
				KickStarter.player.EndPath ();
				playerInput.hotspotMovingTo = null;
			}

			_hotspot.Deselect ();
			hotspot = null;

			if (KickStarter.player)
			{
				KickStarter.player.ClearHeadTurnTarget (HeadFacing.Hotspot, false);
			}

			if (button == null)
			{
				// Unhandled event
				if (selectedItemID >= 0 && runtimeInventory.GetItem (selectedItemID) != null && runtimeInventory.GetItem (selectedItemID).unhandledActionList)
				{
					ActionListAsset unhandledActionList = runtimeInventory.GetItem (selectedItemID).unhandledActionList;
					runtimeInventory.SetNull ();
					AdvGame.RunActionListAsset (unhandledActionList);	
				}
				else if (selectedItemID >= 0 && runtimeInventory.unhandledHotspot)
				{
					runtimeInventory.SetNull ();
					AdvGame.RunActionListAsset (runtimeInventory.unhandledHotspot);	
				}
				else
				{
					stateHandler.gameState = GameState.Normal;
				}
			}
			else
			{
				runtimeInventory.SetNull ();

				if (_hotspot.interactionSource == ActionListSource.AssetFile)
				{
					AdvGame.RunActionListAsset (button.assetFile);
				}
				else if (button.interaction)
				{
					button.interaction.Interact ();
				}
				else
				{
					//Debug.Log ("No interaction object found for " + _hotspot.name);
					stateHandler.gameState = GameState.Normal;
				}
			}

			button = null;
		}
		
		
		public string GetLabel ()
		{
			string label = "";
			
			if (cursorManager && cursorManager.inventoryHandling != InventoryHandling.ChangeCursor && settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction && runtimeInventory.selectedItem != null)
			{
				if (Options.GetLanguage () > 0)
				{
					label = SpeechManager.GetTranslation (cursorManager.hotspotPrefix1.lineID, Options.GetLanguage ()) + " " + runtimeInventory.selectedItem.label + " " + SpeechManager.GetTranslation (cursorManager.hotspotPrefix1.lineID, Options.GetLanguage ()) + " ";
				}
				else
				{
					label = cursorManager.hotspotPrefix1.label + " " + runtimeInventory.selectedItem.label + " " + cursorManager.hotspotPrefix2.label + " ";
				}
			}
			else if (cursorManager && cursorManager.addHotspotPrefix)
			{
				if (settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					if (hotspot && hotspot.provideUseInteraction)
					{
						label = cursorManager.GetLabelFromID (hotspot.GetFirstUseButton ().iconID);
					}
				}
				else if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					label = cursorManager.GetLabelFromID (playerCursor.GetSelectedCursorID ());
				}
			}
			
			if (hotspot)
			{
				if (Options.GetLanguage () > 0)
				{
					label += SpeechManager.GetTranslation (hotspot.lineID, Options.GetLanguage ());
				}
				else if (hotspot.hotspotName != "")
				{
					label += hotspot.hotspotName;
				}
				else
				{
					label += hotspot.name;
				}
			}

			return (label);		
		}
		
		
		public void StopInteraction ()
		{
			button = null;
			StopCoroutine ("UseObject");
		}
		
		
		public Vector2 GetHotspotScreenCentre ()
		{
			Vector3 screenPosition = Camera.main.WorldToViewportPoint (hotspot.transform.position);
			
			if (hotspot._collider && hotspot._collider is CapsuleCollider)
			{
				CapsuleCollider capsuleCollider = (CapsuleCollider) hotspot._collider;
				screenPosition.y += capsuleCollider.center.y / Screen.height * 100f;
			}
			else if (hotspot._collider2D && hotspot._collider2D is BoxCollider2D)
			{
				BoxCollider2D boxCollider = (BoxCollider2D) hotspot._collider2D;
				screenPosition.y += hotspot.transform.localScale.y * boxCollider.center.y / Screen.height * 100f;
			}
			
			return (new Vector2 (screenPosition.x, 1 - screenPosition.y));
		}
		
		
		public bool IsMouseOverHotspot ()
		{
			// Return false if we're in "Walk mode" anyway
			if (settingsManager && settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot
			    && playerCursor && playerCursor.GetSelectedCursor () == -1)
			{
				return false;
			}
			
			if (settingsManager && settingsManager.IsUnity2D ())
			{
				RaycastHit2D hit = new RaycastHit2D ();

				if (Camera.main.isOrthoGraphic)
				{
					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (playerInput.GetMousePosition ()), Vector2.zero, settingsManager.navMeshRaycastLength);
				}
				else
				{
					Vector3 pos = playerInput.GetMousePosition ();
					pos.z = -Camera.main.transform.position.z;
					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint(pos), Vector2.zero, settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer (settingsManager.hotspotLayer));
				}

				if (hit.collider != null && hit.collider.gameObject.GetComponent <Hotspot>())
				{
					return true;
				}
			}
			else
			{
				Ray ray = Camera.main.ScreenPointToRay (playerInput.GetMousePosition ());
				RaycastHit hit;
				
				if (Physics.Raycast (ray, out hit, settingsManager.hotspotRaycastLength, 1 << LayerMask.NameToLayer (settingsManager.hotspotLayer)))
				{
					if (hit.collider.gameObject.GetComponent <Hotspot>())
					{
						return true;
					}
				}

				// Include moveables in query
				if (Physics.Raycast (ray, out hit, settingsManager.moveableRaycastLength, 1 << LayerMask.NameToLayer (settingsManager.hotspotLayer)))
				{
					if (hit.collider.gameObject.GetComponent <DragBase>())
					{
						return true;
					}
				}
			}
			
			return false;
		}


		public bool IsDroppingInventory ()
		{
			if (stateHandler.gameState == GameState.Cutscene || stateHandler.gameState == GameState.DialogOptions)
			{
				return false;
			}

			if (runtimeInventory.selectedItem == null || !runtimeInventory.localItems.Contains (runtimeInventory.selectedItem))
			{
				return false;
			}

			if (settingsManager.inventoryDragDrop && playerInput.mouseState == MouseState.Normal && playerInput.dragState == DragState.Inventory)
			{
				return true;
			}

			if (settingsManager.inventoryDragDrop && playerInput.CanClick () && playerInput.mouseState == MouseState.Normal && playerInput.dragState == DragState.None)
			{
				return true;
			}

			if (playerInput.mouseState == MouseState.SingleClick && settingsManager.inventoryDisableLeft)
		    {
				return true;
			}

			if (playerInput.mouseState == MouseState.RightClick && settingsManager.rightClickInventory == RightClickInventory.DeselectsItem && (settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive || settingsManager.inventoryInteractions == InventoryInteractions.Single))
			{
				return true;
			}

			return false;
		}


		public Hotspot GetActiveHotspot ()
		{
			return hotspot;
		}


		public int GetActiveUseButtonIconID ()
		{
			if (settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				if (runtimeInventory.hoverItem != null && settingsManager.inventoryInteractions == AC.InventoryInteractions.Multiple)
				{
					if (interactionIndex == -1)
					{
						if (runtimeInventory.hoverItem.interactions.Count == 0)
						{
							return -1;
						}
						else
						{
							interactionIndex = 0;
							return 0;
						}
					}

					if (interactionIndex < runtimeInventory.hoverItem.interactions.Count)
					{
						return runtimeInventory.hoverItem.interactions [interactionIndex].icon.id;
					}
				}
				else if (GetActiveHotspot ())
				{
					if (interactionIndex == -1)
					{
						if (GetActiveHotspot ().GetFirstUseButton () == null)
						{
							return -1;
						}
						else
						{
							interactionIndex = 0;
							return 0;
						}
					}

					if (interactionIndex < GetActiveHotspot ().useButtons.Count)
					{
						return GetActiveHotspot ().useButtons [interactionIndex].iconID;
					}
				}
			}
			else
			{
				// Cycle menus

				if (runtimeInventory.selectedItem != null && settingsManager.inventoryInteractions == AC.InventoryInteractions.Multiple)
				{
					if (interactionIndex == -1)
					{
						return -1;
					}

					if (interactionIndex < runtimeInventory.selectedItem.interactions.Count)
					{
						return runtimeInventory.selectedItem.interactions [interactionIndex].icon.id;
					}
				}
				else if (GetActiveHotspot ())
				{
					if (interactionIndex == -1)
					{
						if (GetActiveHotspot ().GetFirstUseButton () == null)
						{
							return -1;
						}
						else
						{
							interactionIndex = 0;
							return 0;
						}
					}

					if (interactionIndex < GetActiveHotspot ().useButtons.Count)
					{
						return GetActiveHotspot ().useButtons [interactionIndex].iconID;
					}
				}
			}
			return -1;
		}


		public int GetActiveInvButtonID ()
		{
			if (settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				if (runtimeInventory.hoverItem != null && settingsManager.inventoryInteractions == AC.InventoryInteractions.Multiple)
				{
					if (interactionIndex >= runtimeInventory.hoverItem.interactions.Count && runtimeInventory.matchingInvInteractions.Count > 0)
					{
						return runtimeInventory.hoverItem.combineID [runtimeInventory.matchingInvInteractions [interactionIndex - runtimeInventory.hoverItem.interactions.Count]];
					}
				}
				else if (GetActiveHotspot ())
				{
					if (interactionIndex >= GetActiveHotspot ().useButtons.Count)
					{
						return GetActiveHotspot ().invButtons [runtimeInventory.matchingInvInteractions [interactionIndex - GetActiveHotspot ().useButtons.Count]].invID;
					}
				}
			}
			else
			{
				// Cycle menus

				if (runtimeInventory.selectedItem != null && settingsManager.inventoryInteractions == AC.InventoryInteractions.Multiple)
				{
					if (interactionIndex >= runtimeInventory.selectedItem.interactions.Count && runtimeInventory.matchingInvInteractions.Count > 0)
					{
						return runtimeInventory.selectedItem.combineID [runtimeInventory.matchingInvInteractions [interactionIndex - runtimeInventory.selectedItem.interactions.Count]];
					}
				}
				else if (GetActiveHotspot ())
				{
					if (interactionIndex >= GetActiveHotspot ().useButtons.Count)
					{
						return GetActiveHotspot ().invButtons [runtimeInventory.matchingInvInteractions [interactionIndex - GetActiveHotspot ().useButtons.Count]].invID;
					}
				}
			}
			return -1;
		}


		public void SetNextInteraction ()
		{
			if (settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				if (runtimeInventory.hoverItem != null)
				{
					interactionIndex = runtimeInventory.hoverItem.GetNextInteraction (interactionIndex, runtimeInventory.matchingInvInteractions.Count);
				}
				else if (GetActiveHotspot () != null)
				{
					interactionIndex = GetActiveHotspot ().GetNextInteraction (interactionIndex, runtimeInventory.matchingInvInteractions.Count);
				}

				if (!settingsManager.cycleInventoryCursors && GetActiveInvButtonID () >= 0)
				{
					interactionIndex = -1;
				}
				else
				{
					runtimeInventory.SelectItemByID (GetActiveInvButtonID ());
				}
			}
			else
			{
				// Cycle menus

				if (runtimeInventory.selectedItem != null)
				{
					interactionIndex = runtimeInventory.selectedItem.GetNextInteraction (interactionIndex, runtimeInventory.matchingInvInteractions.Count);
				}
				else if (GetActiveHotspot () != null)
				{
					interactionIndex = GetActiveHotspot ().GetNextInteraction (interactionIndex, runtimeInventory.matchingInvInteractions.Count);
				}
			}
		}


		public void SetPreviousInteraction ()
		{
			if (runtimeInventory.selectedItem != null)
			{
				interactionIndex = runtimeInventory.selectedItem.GetPreviousInteraction (interactionIndex, runtimeInventory.matchingInvInteractions.Count);
			}
			else if (GetActiveHotspot () != null)
			{
				interactionIndex = GetActiveHotspot ().GetPreviousInteraction (interactionIndex, runtimeInventory.matchingInvInteractions.Count);
			}
		}


		public void ResetInteractionIndex ()
		{
			interactionIndex = -1;
		}


		public int GetInteractionIndex ()
		{
			return interactionIndex;
		}


		private void ClickHotspotToInteract ()
		{
			int invID = GetActiveInvButtonID ();

			if (invID == -1)
			{
				ClickButton (InteractionType.Use, GetActiveUseButtonIconID (), -1);
			}
			else
			{
				ClickButton (InteractionType.Inventory, -1, invID);
			}
		}


		public void ClickInvItemToInteract ()
		{
			int invID = GetActiveInvButtonID ();
			if (invID == -1)
			{
				if (settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
				{
					runtimeInventory.RunInteraction (true, GetActiveUseButtonIconID ());
				}
				else
				{
					runtimeInventory.RunInteraction (false, GetActiveUseButtonIconID ());
				}
			}
			else
			{
				runtimeInventory.SelectItem (runtimeInventory.hoverItem);
				runtimeInventory.Combine (invID);
			}
		}


		private void OnDestroy ()
		{
			playerInput = null;
			stateHandler = null;
			runtimeInventory = null;
		}
	}
	
}