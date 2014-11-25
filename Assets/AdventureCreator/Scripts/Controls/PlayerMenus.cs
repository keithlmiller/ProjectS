/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerMenus.cs"
 * 
 *	This script handles the displaying of each of the menus defined in MenuSystem.
 *	It avoids referencing specific menus and menu elements as much as possible,
 *	so that the menu can be completely altered using just the MenuSystem script.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class PlayerMenus : MonoBehaviour
	{
		
		[HideInInspector] public bool lockSave = false;

		private bool isPaused;
		private string hotspotLabel = "";
		private float pauseAlpha = 0f;
		private List<Menu> menus = new List<Menu>();
		private Texture2D pauseTexture;
		private string elementIdentifier;
		private string lastElementIdentifier;
		private MenuInput selectedInputBox;
		private string selectedInputBoxMenuName;
		private Vector2 activeInventoryBoxCentre = Vector2.zero;
		private InvItem oldHoverItem;

		private Menu crossFadeTo;
		private Menu crossFadeFrom;
		
		private GUIStyle normalStyle = new GUIStyle ();
		private GUIStyle highlightedStyle = new GUIStyle();
		
		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		private TouchScreenKeyboard keyboard;
		#endif
		
		private PlayerCursor playerCursor;
		private ActionListManager actionListManager;
		private Dialog dialog;
		private PlayerInput playerInput;
		private PlayerInteraction playerInteraction;
		private MenuSystem menuSystem;
		private StateHandler stateHandler;
		private Options options;
		private SettingsManager settingsManager;
		private CursorManager cursorManager;
		private SpeechManager speechManager;
		private MenuManager menuManager;
		private RuntimeInventory runtimeInventory;
		private SceneSettings sceneSettings;
		
		
		private void Start ()
		{
			menus = new List<Menu>();
			GetReferences ();
			
			if (menuManager)
			{
				pauseTexture = menuManager.pauseTexture;

				foreach (Menu _menu in menuManager.menus)
				{
					Menu newMenu = ScriptableObject.CreateInstance <Menu>();
					newMenu.Copy (_menu);
					menus.Add (newMenu);
				}
			}
			
			foreach (Menu menu in menus)
			{
				menu.Recalculate ();
			}

			#if UNITY_WEBPLAYER && !UNITY_EDITOR
			// WebPlayer takes another second to get the correct screen dimensions
			foreach (Menu menu in menus)
			{
				menu.Recalculate ();
			}
			#endif
		}
		
		
		
		private void OnLevelWasLoaded ()
		{
			GetReferences ();
		}
		
		
		private void GetReferences ()
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;
			if (settingsManager.IsInLoadingScene ())
			{
				return;
			}
			
			speechManager = AdvGame.GetReferences ().speechManager;
			cursorManager = AdvGame.GetReferences ().cursorManager;
			menuManager = AdvGame.GetReferences ().menuManager;
			
			playerCursor = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerCursor>();
			actionListManager = playerCursor.GetComponent <ActionListManager>();
			playerInput = playerCursor.GetComponent <PlayerInput>();
			playerInteraction = playerCursor.GetComponent <PlayerInteraction>();
			menuSystem = playerCursor.GetComponent <MenuSystem>();
			dialog = playerCursor.GetComponent <Dialog>();
			sceneSettings = playerCursor.GetComponent <SceneSettings>();
			
			stateHandler = this.GetComponent <StateHandler>();
			options = this.GetComponent <Options>();
			runtimeInventory = this.GetComponent <RuntimeInventory>();
		}
		
		
		public void ShowPauseBackground (bool fadeIn)
		{
			float fadeSpeed = 0.5f;
			if (fadeIn)
			{
				if (pauseAlpha < 1f)
				{
					pauseAlpha += (0.2f * fadeSpeed);
				}				
				else
				{
					pauseAlpha = 1f;
				}
			}
			
			else
			{
				if (pauseAlpha > 0f)
				{
					pauseAlpha -= (0.2f * fadeSpeed);
				}
				else
				{
					pauseAlpha = 0f;
				}
			}
			
			Color tempColor = GUI.color;
			tempColor.a = pauseAlpha;
			GUI.color = tempColor;
			GUI.DrawTexture (AdvGame.GUIRect (0.5f, 0.5f, 1f, 1f), pauseTexture, ScaleMode.ScaleToFit, true, 0f);
		}
		
		
		public void DrawMenus ()
		{
			if (playerInteraction && playerInput && menuSystem && stateHandler && settingsManager)
			{
				GUI.depth = menuManager.globalDepth;

				if (pauseTexture)
				{
					isPaused = false;
					foreach (Menu menu in menus)
					{
						if (menu.IsEnabled ())
						{
							if ((menu.appearType == AppearType.Manual || menu.appearType == AppearType.OnInputKey) && menu.pauseWhenEnabled)
							{
								isPaused = true;
							}
						}
					}
					
					if (isPaused)
					{
						ShowPauseBackground (true);
					}
					else
					{
						ShowPauseBackground (false);
					}
				}
				
				if (selectedInputBox)
				{
					Event currentEvent = Event.current;
					if (currentEvent.isKey && currentEvent.type == EventType.KeyDown)
					{
						selectedInputBox.CheckForInput (currentEvent.keyCode.ToString (), currentEvent.shift, selectedInputBoxMenuName);
					}
				}
				
				foreach (Menu menu in menus)
				{
					Color tempColor = GUI.color;
					
					if (menu.IsEnabled ())
					{
						if (menu.transitionType == MenuTransition.None && menu.IsFading ())
						{
							// Stop until no longer "fading" so that it appears in right place
							break;
						}
						
						if (menu.transitionType == MenuTransition.Fade || menu.transitionType == MenuTransition.FadeAndPan)
						{
							tempColor.a = menu.transitionProgress;
							GUI.color = tempColor;
						}
						else
						{
							tempColor.a = 1f;
							GUI.color = tempColor;
						}
						
						menu.StartDisplay ();
						
						foreach (MenuElement element in menu.elements)
						{
							if (element.isVisible)
							{
								SetStyles (element);
								
								for (int i=0; i<element.GetNumSlots (); i++)
								{
									if (menu.IsEnabled () && stateHandler.gameState != GameState.Cutscene && settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot && menu.appearType == AppearType.OnInteraction)
									{
										if (element is MenuInteraction)
										{
											MenuInteraction menuInteraction = (MenuInteraction) element;
											if (menuInteraction.iconID == playerInteraction.GetActiveUseButtonIconID ())
											{
												if (cursorManager.addHotspotPrefix)
												{
													if (runtimeInventory.selectedItem != null)
													{
														hotspotLabel = cursorManager.GetLabelFromID (menuInteraction.iconID) + runtimeInventory.selectedItem.GetLabel ();
													}
													else
													{
														hotspotLabel = cursorManager.GetLabelFromID (menuInteraction.iconID) + playerInteraction.GetLabel ();
													}
												}
												element.Display (highlightedStyle, i, menu.GetZoom (), true);
											}
											else
											{
												element.Display (normalStyle, i, menu.GetZoom (), false);
											}
										}
										else if (element is MenuInventoryBox)
										{
											MenuInventoryBox menuInventoryBox = (MenuInventoryBox) element;
											if (menuInventoryBox.inventoryBoxType == AC_InventoryBoxType.HostpotBased && menuInventoryBox.items[i].id == playerInteraction.GetActiveInvButtonID ())
											{
												if (cursorManager.addHotspotPrefix)
												{
													if (runtimeInventory.selectedItem != null)
													{
														hotspotLabel = cursorManager.hotspotPrefix1.label + " " + menuInventoryBox.GetLabel (i) + " " + cursorManager.hotspotPrefix2.label + " " + runtimeInventory.selectedItem.GetLabel ();
													}
													else
													{
														hotspotLabel = cursorManager.hotspotPrefix1.label + " " + menuInventoryBox.GetLabel (i) + " " + cursorManager.hotspotPrefix2.label + " " + playerInteraction.GetLabel ();
													}
												}
												element.Display (highlightedStyle, i, menu.GetZoom (), true);
											}
											else
											{
												element.Display (normalStyle, i, menu.GetZoom (), false);
											}
										}
										else
										{
											element.Display (normalStyle, i, menu.GetZoom (), false);
										}
									}

									else if (menu.IsVisible () && !menu.ignoreMouseClicks && element.isClickable && playerInput.IsCursorReadable () && stateHandler.gameState != GameState.Cutscene && 
									 ((settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.IsPointerOverSlot (element, i, playerInput.GetInvertedMouse ())) ||
									 (settingsManager.inputMethod == InputMethod.TouchScreen && menu.IsPointerOverSlot (element, i, playerInput.GetInvertedMouse ())) ||
									 (settingsManager.inputMethod == InputMethod.KeyboardOrController && stateHandler.gameState == GameState.Normal && menu.IsPointerOverSlot (element, i, playerInput.GetInvertedMouse ())) ||
									 ((settingsManager.inputMethod == InputMethod.KeyboardOrController && stateHandler.gameState != GameState.Normal && menu.selected_element == element && menu.selected_slot == i))))
									{
										float zoom = 1;
										if (menu.transitionType == MenuTransition.Zoom)
										{
											zoom = menu.GetZoom ();
										}
										
										if ((!playerInput.interactionMenuIsOn || menu.appearType == AppearType.OnInteraction)
										    && (playerInput.dragState == DragState.None || (playerInput.dragState == DragState.Inventory && CanElementBeDroppedOnto (element))))
										{
											element.Display (highlightedStyle, i, zoom, true);
										}
										else
										{
											element.Display (normalStyle, i, zoom, false);
										}
									}

									else
									{
										element.Display (normalStyle, i, menu.GetZoom (), false);
									}
								}
								
								if (element is MenuInput)
								{
									if (selectedInputBox == null)
									{
										MenuInput input = (MenuInput) element;
										SelectInputBox (input);
										
										selectedInputBoxMenuName = menu.title;
									}
								}
							}
						}
						
						menu.EndDisplay ();
					}
					
					tempColor.a = 1f;
					GUI.color = tempColor;
				}
			}
		}


		public void UpdateMenuPosition (Menu menu, Vector2 invertedMouse)
		{
			if (invertedMouse == Vector2.zero)
			{
				invertedMouse = playerInput.GetInvertedMouse ();
			}

			if (menu.positionType == AC_PositionType.FollowCursor)
			{
				menu.SetCentre (new Vector2 ((invertedMouse.x / Screen.width) + (menu.manualPosition.x / 100f) - 0.5f,
				                             (invertedMouse.y / Screen.height) + (menu.manualPosition.y / 100f) - 0.5f));
			}
			else if (menu.positionType == AC_PositionType.OnHotspot)
			{
				if (activeInventoryBoxCentre != Vector2.zero)
				{
					Vector2 screenPosition = new Vector2 (activeInventoryBoxCentre.x / Screen.width, activeInventoryBoxCentre.y / Screen.height);
					menu.SetCentre (new Vector2 (screenPosition.x + (menu.manualPosition.x / 100f) - 0.5f,
					                             screenPosition.y + (menu.manualPosition.y / 100f) - 0.5f));
				}
				else if (playerInteraction.GetActiveHotspot ())
				{
					Vector2 screenPosition = playerInteraction.GetHotspotScreenCentre ();
					menu.SetCentre (new Vector2 (screenPosition.x + (menu.manualPosition.x / 100f) - 0.5f,
					                             screenPosition.y + (menu.manualPosition.y / 100f) - 0.5f));
				}
			}
			else if (menu.positionType == AC_PositionType.AboveSpeakingCharacter)
			{
				if (dialog.GetSpeakingCharacter () != null && dialog.isMessageAlive)
				{
					Vector2 screenPosition = dialog.GetSpeakingCharacter ().GetScreenCentre ();
					menu.SetCentre (new Vector2 (screenPosition.x + (menu.manualPosition.x / 100f) - 0.5f,
					                             screenPosition.y + (menu.manualPosition.y / 100f) - 0.5f));
				}
			}
			else if (menu.positionType == AC_PositionType.AbovePlayer)
			{
				if (KickStarter.player)
				{
					Vector2 screenPosition = KickStarter.player.GetScreenCentre ();
					menu.SetCentre (new Vector2 (screenPosition.x + (menu.manualPosition.x / 100f) - 0.5f,
					                             screenPosition.y + (menu.manualPosition.y / 100f) - 0.5f));
				}
			}
		}


		private void UpdateMenu (Menu menu)
		{
			Vector2 invertedMouse = playerInput.GetInvertedMouse ();
			UpdateMenuPosition (menu, invertedMouse);
			
			menu.HandleTransition ();
			
			if (settingsManager)
			{
				if (settingsManager.inputMethod == InputMethod.KeyboardOrController && menu.IsEnabled () &&
				    ((stateHandler.gameState == GameState.Paused && menu.pauseWhenEnabled) || (stateHandler.gameState == GameState.DialogOptions && menu.appearType == AppearType.DuringConversation)))
				{
					playerInput.selected_option = menu.ControlSelected (playerInput.selected_option);
				}
			}
			else
			{
				Debug.LogWarning ("A settings manager is not present.");
			}
			
			if (menu.appearType == AppearType.Manual)
			{
				if (menu.IsVisible () && !menu.isLocked && menu.GetRect ().Contains (invertedMouse) && !menu.ignoreMouseClicks)
				{
					playerInput.mouseOverMenu = true;
				}
			}
			
			else if (menu.appearType == AppearType.DuringGameplay)
			{
				if (stateHandler.gameState == GameState.Normal && !menu.isLocked && menu.IsOff ())
				{
					menu.TurnOn (true);
					
					if (menu.GetRect ().Contains (invertedMouse) && !menu.ignoreMouseClicks)
					{
						playerInput.mouseOverMenu = true;
					}
				}
				else if (stateHandler.gameState == GameState.Paused)
				{
					menu.TurnOff (true);
				}
				else if (stateHandler.gameState != GameState.Normal && menu.IsOn () && actionListManager.AreActionListsRunning ())
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.MouseOver)
			{
				if (stateHandler.gameState == GameState.Normal && !menu.isLocked && menu.GetRect ().Contains (invertedMouse))
				{
					if (menu.IsOff ())
					{
						menu.TurnOn (true);
					}

					if (!menu.ignoreMouseClicks)
					{
						playerInput.mouseOverMenu = true;
					}
				}
				else if (stateHandler.gameState == GameState.Paused)
				{
					menu.ForceOff ();
				}
				else
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.OnContainer)
			{
				if (playerInput.activeContainer != null && !menu.isLocked && (stateHandler.gameState == GameState.Normal || (stateHandler.gameState == AC.GameState.Paused && menu.pauseWhenEnabled)))
				{
					if (menu.IsVisible () && menu.GetRect ().Contains (invertedMouse) && !menu.ignoreMouseClicks)
					{
						playerInput.mouseOverMenu = true;
					}
					menu.TurnOn (true);
				}
				else
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.DuringConversation)
			{
				if (playerInput.activeConversation != null && stateHandler.gameState == GameState.DialogOptions)
				{
					menu.TurnOn (true);
				}
				else if (stateHandler.gameState == GameState.Paused)
				{
					menu.ForceOff ();
				}
				else
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.OnInputKey)
			{
				if (menu.IsEnabled () && !menu.isLocked && menu.GetRect ().Contains (invertedMouse) && !menu.ignoreMouseClicks)
				{
					playerInput.mouseOverMenu = true;
				}
				
				try
				{
					if (Input.GetButtonDown (menu.toggleKey))
					{
						if (!menu.IsEnabled ())
						{
							if (stateHandler.gameState == GameState.Paused)
							{
								CrossFade (menu);
							}
							else
							{
								menu.TurnOn (true);
							}
						}
						else
						{
							menu.TurnOff (true);
						}
					}
				}
				catch
				{
					if (settingsManager.inputMethod != InputMethod.TouchScreen)
					{
						Debug.LogWarning ("No '" + menu.toggleKey + "' button exists - please define one in the Input Manager.");
					}
				}
			}
			
			else if (menu.appearType == AppearType.OnHotspot)
			{
				if (settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive && !menu.isLocked && runtimeInventory.selectedItem == null)
				{
					Hotspot hotspot = playerInteraction.GetActiveHotspot ();
					if (hotspot != null)
					{
						menu.HideInteractions ();
						
						if (hotspot.HasContextUse ())
						{
							menu.MatchUseInteraction (hotspot.GetFirstUseButton ());
						}
						
						if (hotspot.HasContextLook ())
						{
							menu.MatchLookInteraction (hotspot.lookButton);
						}
						
						menu.Recalculate ();
					}
				}
				
				if (hotspotLabel != "" && !menu.isLocked && (stateHandler.gameState == GameState.Normal || stateHandler.gameState == GameState.DialogOptions))
				{
					menu.TurnOn (true);
				}
				else if (stateHandler.gameState == GameState.Paused)
				{
					menu.ForceOff ();
				}
				else
				{
					menu.TurnOff (true);
				}
			}
			
			else if (menu.appearType == AppearType.OnInteraction)
			{
				if (settingsManager.CanClickOffInteractionMenu ())
				{
					if (menu.IsEnabled () && (stateHandler.gameState == GameState.Normal || menu.pauseWhenEnabled))
					{
						playerInput.interactionMenuIsOn = true;
						
						if (playerInput.mouseState == MouseState.SingleClick && !menu.GetRect ().Contains (invertedMouse))
						{
							playerInput.mouseState = MouseState.Normal;
							playerInput.interactionMenuIsOn = false;
							menu.ForceOff ();
						}
					}
					else if (stateHandler.gameState == GameState.Paused)
					{
						playerInput.interactionMenuIsOn = false;
						menu.ForceOff ();
					}
					else if (playerInteraction.GetActiveHotspot () == null)
					{
						playerInput.interactionMenuIsOn = false;
						menu.TurnOff (true);
					}
				}
				else
				{
					if (menu.IsEnabled () && (stateHandler.gameState == GameState.Normal || menu.pauseWhenEnabled))
					{
						if (menu.GetRect ().Contains (invertedMouse) && !menu.ignoreMouseClicks && (playerInteraction.GetActiveHotspot () != null || runtimeInventory.selectedItem != null))
						{
							playerInput.interactionMenuIsOn = true;
						}
						else if (settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot && playerInteraction.GetActiveHotspot () != null)
						{
							playerInput.interactionMenuIsOn = true;
						}
						else if (settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot && (runtimeInventory.selectedItem != null && runtimeInventory.selectedItem == runtimeInventory.hoverItem))
						{
							playerInput.interactionMenuIsOn = true;
						}
						else if (playerInteraction.GetActiveHotspot () == null || settingsManager.inputMethod == InputMethod.TouchScreen)
						{
							menu.TurnOff (true);
						}
						else if (runtimeInventory.selectedItem == null && playerInteraction.GetActiveHotspot () != null && runtimeInventory.hoverItem != null)
						{
							menu.TurnOff (true);
						}
						else if (runtimeInventory.selectedItem != null && runtimeInventory.selectedItem != runtimeInventory.hoverItem)
						{
							menu.TurnOff (true);
						}
					}
					else if (stateHandler.gameState == GameState.Paused)
					{
						menu.ForceOff ();
					}
					else if (playerInteraction.GetActiveHotspot () == null)
					{
						menu.TurnOff (true);
					}
				}
			}
			
			else if (menu.appearType == AppearType.WhenSpeechPlays)
			{
				if (stateHandler.gameState != GameState.Paused)
				{
					if (dialog.isMessageAlive && stateHandler.gameState != GameState.DialogOptions &&
					    (menu.speechMenuType == SpeechMenuType.All ||
					 (menu.speechMenuType == SpeechMenuType.CharactersOnly && dialog.GetSpeakingCharacter () != null) ||
					 (menu.speechMenuType == SpeechMenuType.NarrationOnly && dialog.GetSpeakingCharacter () == null)))
					{
						if (options.optionsData == null || (options.optionsData != null && options.optionsData.showSubtitles) || (speechManager && speechManager.forceSubtitles && !dialog.foundAudio)) 
						{
							menu.TurnOn (true);
						}
						else
						{
							menu.TurnOff (true);	
						}
					}
					else
					{
						menu.TurnOff (true);
					}
				}
				else
				{
					menu.ForceOff ();
				}
			}
		}
		
		
		private void UpdateElements (Menu menu)
		{
			if (menu.transitionType == MenuTransition.None && menu.IsFading ())
			{
				// Stop until no longer "fading" so that it appears in right place
				return;
			}
			
			activeInventoryBoxCentre = Vector2.zero;
			
			if (settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.GetRect ().Contains (playerInput.GetInvertedMouse ()))
			{
				elementIdentifier = menu.id.ToString ();
			}
			
			foreach (MenuElement element in menu.elements)
			{
				if (element.isVisible)
				{
					for (int i=0; i<element.GetNumSlots (); i++)
					{
						if (SlotIsInteractive (menu, element, i))
						{
							if ((!playerInput.interactionMenuIsOn || menu.appearType == AppearType.OnInteraction)
							    && (playerInput.dragState == DragState.None || (playerInput.dragState == DragState.Inventory && CanElementBeDroppedOnto (element))))
							{
								if (sceneSettings && element.hoverSound && lastElementIdentifier != (menu.id.ToString () + element.ID.ToString () + i.ToString ()))
								{
									sceneSettings.PlayDefaultSound (element.hoverSound, false);
								}
								
								elementIdentifier = menu.id.ToString () + element.ID.ToString () + i.ToString ();
							}
							
							if (stateHandler.gameState != GameState.Cutscene)
							{
								if (element is MenuInventoryBox)
								{
									if (stateHandler.gameState == GameState.Normal)
									{
										if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && settingsManager.inventoryInteractions == InventoryInteractions.Single && runtimeInventory.selectedItem == null)
										{
											playerCursor.ResetSelectedCursor ();
										}
										
										MenuInventoryBox inventoryBox = (MenuInventoryBox) element;
										if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.HostpotBased)
										{
											if (cursorManager.addHotspotPrefix)
											{
												if (runtimeInventory.selectedItem != null)
												{
													hotspotLabel = runtimeInventory.selectedItem.GetLabel ();
												}
												else
												{
													hotspotLabel = playerInteraction.GetLabel ();
												}
												
												if ((runtimeInventory.selectedItem == null && !playerInput.interactionMenuIsOn) || playerInput.interactionMenuIsOn)
												{
													hotspotLabel = cursorManager.hotspotPrefix1.label + " " + inventoryBox.GetLabel (i) + " " + cursorManager.hotspotPrefix2.label + " " + hotspotLabel;
												}
											}
										}
										else
										{
											activeInventoryBoxCentre = menu.GetSlotCentre (inventoryBox, i);

											InvItem newHoverItem = inventoryBox.GetItem (i);
											if (oldHoverItem != newHoverItem)
											{
												runtimeInventory.MatchInteractions ();
												playerInteraction.ResetInteractionIndex ();
											}
											runtimeInventory.hoverItem = newHoverItem;
											
											if (!playerInput.interactionMenuIsOn)
											{
												if (inventoryBox.displayType == ConversationDisplayType.IconOnly)
												{
													hotspotLabel = inventoryBox.GetLabel (i);
												}
											}
											else if (runtimeInventory.selectedItem != null)
											{
												hotspotLabel = runtimeInventory.selectedItem.GetLabel ();
											}
										}
									}
								}
								else if (element is MenuCrafting)
								{
									if (stateHandler.gameState == GameState.Normal)
									{
										MenuCrafting crafting = (MenuCrafting) element;
										runtimeInventory.hoverItem = crafting.GetItem (i);
										
										if (runtimeInventory.hoverItem != null)
										{
											if (!playerInput.interactionMenuIsOn)
											{
												hotspotLabel = crafting.GetLabel (i);
											}
											else if (runtimeInventory.selectedItem != null)
											{
												hotspotLabel = runtimeInventory.selectedItem.GetLabel ();
											}
										}
									}
								}
								else if (element is MenuInteraction)
								{
									if (runtimeInventory.selectedItem != null)
									{
										hotspotLabel = runtimeInventory.selectedItem.GetLabel ();
									}
									else
									{
										hotspotLabel = playerInteraction.GetLabel ();
									}
									
									if (cursorManager.addHotspotPrefix && playerInput.interactionMenuIsOn && settingsManager.SelectInteractionMethod () == SelectInteractions.ClickingMenu)
									{
										MenuInteraction interaction = (MenuInteraction) element;
										hotspotLabel = cursorManager.GetLabelFromID (interaction.iconID) + hotspotLabel;
									}
								}
								else if (element is MenuDialogList)
								{
									if (stateHandler.gameState == GameState.DialogOptions)
									{
										MenuDialogList dialogList = (MenuDialogList) element;
										if (dialogList.displayType == ConversationDisplayType.IconOnly)
										{
											hotspotLabel = dialogList.GetLabel (i);
										}
									}
								}
								else if (element is MenuButton)
								{
									MenuButton button = (MenuButton) element;
									if (button.hotspotLabel != "")
									{
										hotspotLabel = button.GetHotspotLabel ();
									}
								}
							}
						}
					}
				}
			}
		}


		private bool SlotIsInteractive (Menu menu, MenuElement element, int i)
		{
			if (menu.IsVisible () && element.isClickable && 
			    ((settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.IsPointerOverSlot (element, i, playerInput.GetInvertedMouse ())) ||
			 (settingsManager.inputMethod == InputMethod.TouchScreen && menu.IsPointerOverSlot (element, i, playerInput.GetInvertedMouse ())) ||
			 (settingsManager.inputMethod == InputMethod.KeyboardOrController && stateHandler.gameState == GameState.Normal && menu.IsPointerOverSlot (element, i, playerInput.GetInvertedMouse ())) ||
			 ((settingsManager.inputMethod == InputMethod.KeyboardOrController && stateHandler.gameState != GameState.Normal && menu.selected_element == element && menu.selected_slot == i))))
			{
				return true;
			}
			return false;
		}


		private void CheckClicks (Menu menu)
		{
			if (menu.transitionType == MenuTransition.None && menu.IsFading ())
			{
				// Stop until no longer "fading" so that it appears in right place
				return;
			}
			
			activeInventoryBoxCentre = Vector2.zero;
			
			if (settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.GetRect ().Contains (playerInput.GetInvertedMouse ()))
			{
				elementIdentifier = menu.id.ToString ();
			}
			
			foreach (MenuElement element in menu.elements)
			{
				if (element.isVisible)
				{
					for (int i=0; i<element.GetNumSlots (); i++)
					{
						if (SlotIsInteractive (menu, element, i))
						{
							if (playerInput.mouseState != MouseState.Normal && (playerInput.dragState == DragState.None || playerInput.dragState == DragState.Menu))
							{
								if (playerInput.mouseState == MouseState.SingleClick || playerInput.mouseState == MouseState.LetGo || playerInput.mouseState == MouseState.RightClick)// && (!playerInput.interactionMenuIsOn || menu.appearType == AppearType.OnInteraction))
								{
									if (element is MenuInput) {}
									else DeselectInputBox ();
									
									CheckClick (menu, element, i, playerInput.mouseState);
								}
								else if (playerInput.mouseState == MouseState.HeldDown)
								{
									CheckContinuousClick (menu, element, i, playerInput.mouseState);
								}
							}
							else if (playerInteraction.IsDroppingInventory () && CanElementBeDroppedOnto (element))
							{
								DeselectInputBox ();
								CheckClick (menu, element, i, MouseState.SingleClick);
							}
						}
					}
				}
			}
		}
		
		
		public void UpdateAllMenus ()
		{
			#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
			if (keyboard != null && selectedInputBox != null)
			{
				selectedInputBox.label = keyboard.text;
			}
			#endif
			
			if (stateHandler && settingsManager && playerInput && playerInteraction && options && dialog && menuSystem && Time.time > 0f)
			{
				hotspotLabel = playerInteraction.GetLabel ();
				oldHoverItem = runtimeInventory.hoverItem;
				runtimeInventory.hoverItem = null;
				
				if (stateHandler.gameState == GameState.Paused)
				{
					if (Time.timeScale != 0f)
					{
						//Time.timeScale = 0f;
						sceneSettings.PauseGame ();
					}
				}
				else
				{
					Time.timeScale = playerInput.timeScale;
				}
				
				playerInput.mouseOverMenu = false;
				
				if (!settingsManager.CanClickOffInteractionMenu ())
				{
					playerInput.interactionMenuIsOn = false;
				}

				foreach (Menu menu in menus)
				{
					UpdateMenu (menu);
					if (menu.IsEnabled ())
					{
						UpdateElements (menu);
					}
				}

				lastElementIdentifier = elementIdentifier;

				// Check clicks in reverse order
				for (int i=menus.Count-1; i>=0; i--)
				{
					if (menus[i].IsEnabled () && !menus[i].ignoreMouseClicks)
					{
						CheckClicks (menus[i]);
					}
				}
			}
		}
		
		
		public void CheckCrossfade (Menu _menu)
		{
			if (crossFadeFrom == _menu && crossFadeTo != null)
			{
				crossFadeFrom.ForceOff ();
				crossFadeTo.TurnOn (true);
				crossFadeTo = null;
			}
		}
		
		
		private void SelectInputBox (MenuInput input)
		{
			selectedInputBox = input;
			
			// Mobile keyboard
			#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
			if (input.inputType == AC_InputType.NumbericOnly)
			{
				keyboard = TouchScreenKeyboard.Open (input.label, TouchScreenKeyboardType.NumberPad, false, false, false, false, "");
			}
			else
			{
				keyboard = TouchScreenKeyboard.Open (input.label, TouchScreenKeyboardType.ASCIICapable, false, false, false, false, "");
			}
			#endif
		}
		
		
		private void DeselectInputBox ()
		{
			if (selectedInputBox)
			{
				selectedInputBox.Deselect ();
				selectedInputBox = null;
				
				// Mobile keyboard
				#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
				if (keyboard != null)
				{
					keyboard.active = false;
					keyboard = null;
				}
				#endif
			}
		}
		
		
		private void CheckClick (Menu _menu, MenuElement _element, int _slot, MouseState _mouseState)
		{
			playerInput.mouseState = MouseState.Normal;

			if (_mouseState == MouseState.LetGo)
			{
				if (settingsManager.inputMethod == InputMethod.TouchScreen && !settingsManager.offsetTouchCursor && runtimeInventory.selectedItem == null && !(_element is MenuInventoryBox) && !(_element is MenuCrafting))
				{
					_mouseState = MouseState.SingleClick;
				}
				else
				{
					_mouseState = MouseState.Normal;
					return;
				}
			}
			
			if (_mouseState != MouseState.Normal)
			{
				if (_element.clickSound != null && sceneSettings != null)
				{
					sceneSettings.PlayDefaultSound (_element.clickSound, false);
				}
				
				if (_element is MenuDialogList)
				{
					MenuDialogList dialogList = (MenuDialogList) _element;
					dialogList.RunOption (_slot);
				}
				
				else if (_element is MenuSavesList)
				{
					MenuSavesList savesList = (MenuSavesList) _element;
					
					if (savesList.saveListType == AC_SaveListType.Save)
					{
						_menu.TurnOff (true);
						SaveSystem.SaveGame (_slot);
						
						if (savesList.actionListOnSave)
						{
							AdvGame.RunActionListAsset (savesList.actionListOnSave);
						}
					}
					else if (savesList.saveListType == AC_SaveListType.Load)
					{
						_menu.TurnOff (false);
						SaveSystem.LoadGame (_slot);
					}
				}
				
				else if (_element is MenuButton)
				{
					MenuButton button = (MenuButton) _element;
					button.ShowClick ();
					
					if (button.buttonClickType == AC_ButtonClickType.TurnOffMenu)
					{
						_menu.TurnOff (button.doFade);
					}
					else if (button.buttonClickType == AC_ButtonClickType.Crossfade)
					{
						Menu menuToSwitchTo = GetMenuWithName (button.switchMenuTitle);
						
						if (menuToSwitchTo != null)
						{
							CrossFade (menuToSwitchTo);
						}
						else
						{
							Debug.LogWarning ("Cannot find any menu of name '" + button.switchMenuTitle + "'");
						}
					}
					else if (button.buttonClickType == AC_ButtonClickType.OffsetInventoryOrDialogue)
					{
						MenuElement elementToShift = GetElementWithName (_menu.title, button.inventoryBoxTitle);
						
						if (elementToShift != null)
						{
							elementToShift.Shift (button.shiftInventory, button.shiftAmount);
							elementToShift.RecalculateSize ();
						}
						else
						{
							Debug.LogWarning ("Cannot find '" + button.inventoryBoxTitle + "' inside '" + _menu.title + "'");
						}
					}
					else if (button.buttonClickType == AC_ButtonClickType.OffsetJournal)
					{
						MenuJournal journalToShift = (MenuJournal) GetElementWithName (_menu.title, button.inventoryBoxTitle);
						
						if (journalToShift != null)
						{
							journalToShift.Shift (button.shiftInventory, button.loopJournal);
							journalToShift.RecalculateSize ();
						}
						else
						{
							Debug.LogWarning ("Cannot find '" + button.inventoryBoxTitle + "' inside '" + _menu.title + "'");
						}
					}
					else if (button.buttonClickType == AC_ButtonClickType.RunActionList)
					{
						if (button.actionList)
						{
							AdvGame.RunActionListAsset (button.actionList);
						}
					}
					else if (button.buttonClickType == AC_ButtonClickType.CustomScript)
					{
						MenuSystem.OnElementClick (_menu, _element, _slot, (int) _mouseState);
					}
					else if (button.buttonClickType == AC_ButtonClickType.SimulateInput)
					{
						playerInput.SimulateInput (button.simulateInput, button.inputAxis, button.simulateValue);
					}
				}
				
				else if (_element is MenuSlider)
				{
					MenuSlider slider = (MenuSlider) _element;
					if (settingsManager.inputMethod == InputMethod.KeyboardOrController)
					{
						slider.Change ();
					}
					else
					{
						slider.Change (playerInput.GetMousePosition ().x - _menu.GetRect ().x);
					}
					if (slider.sliderType == AC_SliderType.CustomScript)
					{
						MenuSystem.OnElementClick (_menu, _element, _slot, (int) _mouseState);
					}
				}
				
				else if (_element is MenuCycle)
				{
					MenuCycle cycle = (MenuCycle) _element;
					cycle.Cycle ();
					
					if (cycle.cycleType == AC_CycleType.CustomScript)
					{
						MenuSystem.OnElementClick (_menu, _element, _slot, (int) _mouseState);
					}
				}
				
				else if (_element is MenuToggle)
				{
					MenuToggle toggle = (MenuToggle) _element;
					toggle.Toggle ();
					
					if (toggle.toggleType == AC_ToggleType.CustomScript)
					{
						MenuSystem.OnElementClick (_menu, _element, _slot, (int) _mouseState);
					}
				}
				
				else if (_element is MenuInventoryBox)
				{
					MenuInventoryBox inventoryBox = (MenuInventoryBox) _element;
					if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.CustomScript)
					{
						MenuSystem.OnElementClick (_menu, _element, _slot, (int) _mouseState);
					}
					else if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.Default || inventoryBox.inventoryBoxType == AC_InventoryBoxType.DisplayLastSelected)
					{
						if (settingsManager.inventoryInteractions == InventoryInteractions.Multiple && settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
						{
							if (_mouseState == MouseState.SingleClick)
							{
								playerInteraction.ClickInvItemToInteract ();
							}
						}
						else if (settingsManager.inventoryInteractions == InventoryInteractions.Multiple && playerInput.interactionMenuIsOn && settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot && runtimeInventory.selectedItem == runtimeInventory.hoverItem)
						{
							SetInteractionMenus (false);
							playerInteraction.ClickInvItemToInteract ();
						}
						else if (settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive && settingsManager.inventoryInteractions == InventoryInteractions.Single)
						{
							inventoryBox.HandleDefaultClick (_mouseState, _slot, AC_InteractionMethod.ContextSensitive);
						}
						else
						{
							inventoryBox.HandleDefaultClick (_mouseState, _slot, settingsManager.interactionMethod);
						}
						_menu.Recalculate ();
					}
					else if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.Container)
					{
						inventoryBox.ClickContainer (_mouseState, _slot, playerInput.activeContainer);
						_menu.Recalculate ();
					}
					else if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.HostpotBased)
					{
						if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
						{
							if (runtimeInventory.selectedItem != null)
							{
								runtimeInventory.Combine (inventoryBox.items [_slot]);
							}
							else if (playerInteraction.GetActiveHotspot ())
							{
								InvItem _item = inventoryBox.items [_slot];
								if (_item != null)
								{
									runtimeInventory.SelectItem (_item);
									_menu.TurnOff (false);
									playerInteraction.ClickButton (InteractionType.Inventory, -2, _item.id);
									playerCursor.ResetSelectedCursor ();
								}
							}
							else
							{
								Debug.LogWarning ("Cannot handle inventory click since there is no active Hotspot.");
							}
						}
						else
						{
							Debug.LogWarning ("This type of InventoryBox only works with the Choose Hotspot Then Interaction method of interaction.");
						}
					}
				}
				
				else if (_element is MenuCrafting)
				{
					MenuCrafting recipe = (MenuCrafting) _element;
					
					if (recipe.craftingType == CraftingElementType.Ingredients)
					{
						recipe.HandleDefaultClick (_mouseState, _slot);
					}
					else if (recipe.craftingType == CraftingElementType.Output)
					{
						recipe.ClickOutput (_menu, _mouseState);
					}
					
					_menu.Recalculate ();
				}
				
				else if (_element is MenuInteraction)
				{
					MenuInteraction interaction = (MenuInteraction) _element;
					
					if (settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
					{
						Debug.LogWarning ("This element is not compatible with the Context-Sensitive interaction method.");
					}
					else if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
					{
						playerCursor.SetCursorFromID (interaction.iconID);
					}
					else if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
					{
						if (runtimeInventory.selectedItem != null)
						{
							_menu.ForceOff ();
							runtimeInventory.RunInteraction (false, interaction.iconID);
						}
						else if (playerInteraction.GetActiveHotspot ())
						{
							_menu.ForceOff ();
							playerInteraction.ClickButton (InteractionType.Use, interaction.iconID, -1);
						}
					}
				}
				
				else if (_element is MenuInput)
				{
					SelectInputBox ((MenuInput) _element);
				}
				
				else if (_element is MenuDrag)
				{
					if (_mouseState == MouseState.SingleClick)
					{
						MenuDrag drag = (MenuDrag) _element;
						drag.StartDrag (_menu);
						playerInput.activeDragElement = drag;
					}
				}
				
				PlayerMenus.ResetInventoryBoxes ();
			}
		}
		
		
		private void CheckContinuousClick (Menu _menu, MenuElement _element, int _slot, MouseState _mouseState)
		{
			if (_element is MenuButton)
			{
				MenuButton button = (MenuButton) _element;
				if (button.buttonClickType == AC_ButtonClickType.SimulateInput)
				{
					playerInput.SimulateInput (button.simulateInput, button.inputAxis, button.simulateValue);
				}
				else if (button.buttonClickType == AC_ButtonClickType.CustomScript && button.allowContinuousClick)
				{
					MenuSystem.OnElementClick (_menu, _element, _slot, (int) _mouseState);
				}
			}
			else if (_element is MenuSlider)
			{
				MenuSlider slider = (MenuSlider) _element;
				if (settingsManager.inputMethod == InputMethod.KeyboardOrController)
				{
					slider.Change ();
				}
				else
				{
					slider.Change (playerInput.GetMousePosition ().x - _menu.GetRect ().x);
				}
				if (slider.sliderType == AC_SliderType.CustomScript)
				{
					MenuSystem.OnElementClick (_menu, _element, _slot, (int) _mouseState);
				}
			}
		}
		
		
		public void CrossFade (Menu _menuTo)
		{
			if (_menuTo.isLocked)
			{
				Debug.Log ("Cannot crossfade to menu " + _menuTo.title + " as it is locked.");
			}
			else if (!_menuTo.IsEnabled())
			{
				// Turn off all other menus
				crossFadeFrom = null;
				
				foreach (Menu menu in menus)
				{
					if (menu.IsVisible ())
					{
						if (menu.appearType == AppearType.OnHotspot)
						{
							menu.ForceOff ();
						}
						else
						{
							menu.TurnOff (true);
							crossFadeFrom = menu;
						}
					}
					else
					{
						menu.ForceOff ();
					}
				}
				
				if (crossFadeFrom != null)
				{
					crossFadeTo = _menuTo;
				}
				else
				{
					_menuTo.TurnOn (true);
				}
			}
		}
		
		
		public void SetInteractionMenus (bool turnOn)
		{
			playerInput.interactionMenuIsOn = turnOn;

			foreach (Menu _menu in menus)
			{
				if (_menu.appearType == AppearType.OnInteraction)
				{
					if (turnOn)
					{
						playerInteraction.ResetInteractionIndex ();

						if (runtimeInventory.selectedItem != null)
						{
							_menu.MatchInteractions (runtimeInventory.selectedItem);
						}
						else if (playerInteraction.GetActiveHotspot ())
						{
							_menu.MatchInteractions (playerInteraction.GetActiveHotspot ().useButtons);
						}

						_menu.ForceOff ();
						_menu.TurnOn (true);
					}
					else
					{
						_menu.TurnOff (true);
					}
				}
			}
		}


		public void DisableHotspotMenus ()
		{
			foreach (Menu _menu in menus)
			{
				if (_menu.appearType == AppearType.OnHotspot)
				{
					_menu.ForceOff ();
				}
			}
		}
		
		
		public string GetHotspotLabel ()
		{
			return hotspotLabel;
		}
		
		
		private void SetStyles (MenuElement element)
		{
			normalStyle.normal.textColor = element.fontColor;
			normalStyle.font = element.font;
			normalStyle.fontSize = element.GetFontSize ();
			normalStyle.alignment = TextAnchor.MiddleCenter;
			
			highlightedStyle.font = element.font;
			highlightedStyle.fontSize = element.GetFontSize ();
			highlightedStyle.normal.textColor = element.fontHighlightColor;
			highlightedStyle.normal.background = element.highlightTexture;
			highlightedStyle.alignment = TextAnchor.MiddleCenter;
		}
		
		
		private bool CanElementBeDroppedOnto (MenuElement element)
		{
			if (element is MenuInventoryBox)
			{
				MenuInventoryBox inventoryBox = (MenuInventoryBox) element;
				if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.Default || inventoryBox.inventoryBoxType == AC_InventoryBoxType.CustomScript)
				{
					return true;
				}
			}
			else if (element is MenuCrafting)
			{
				MenuCrafting crafting = (MenuCrafting) element;
				if (crafting.craftingType == CraftingElementType.Ingredients)
				{
					return true;
				}
			}
			
			return false;
		}
		
		
		private void OnDestroy ()
		{
			actionListManager = null;
			dialog = null;
			playerInput = null;
			playerInteraction = null;
			menuSystem = null;
			stateHandler = null;
			options = null;
			menus = null;
			runtimeInventory = null;
			settingsManager = null;
			cursorManager = null;
			speechManager = null;
			menuManager = null;
			sceneSettings = null;
		}
		
		
		public static List<Menu> GetMenus ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>())
			{
				return GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>().menus;
			}
			return null;
		}
		
		
		public static Menu GetMenuWithName (string menuName)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>())
			{
				PlayerMenus playerMenus = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>();
				
				foreach (Menu menu in playerMenus.menus)
				{
					if (menu.title == menuName)
					{
						return menu;
					}
				}
			}
			
			return null;
		}
		
		
		public static MenuElement GetElementWithName (string menuName, string menuElementName)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>())
			{
				PlayerMenus playerMenus = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>();
				
				foreach (Menu menu in playerMenus.menus)
				{
					if (menu.title == menuName)
					{
						foreach (MenuElement menuElement in menu.elements)
						{
							if (menuElement.title == menuElementName)
							{
								return menuElement;
							}
						}
					}
				}
			}
			
			return null;
		}
		
		
		public static bool IsSavingLocked ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
			{
				StateHandler stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
				if (stateHandler.GetLastNonPausedState () != GameState.Normal)
				{
					return true;
				}
			}
			
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>())
			{
				return (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>().lockSave);
			}
			
			return false;
		}
		
		
		public static void ResetInventoryBoxes ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>())
			{
				PlayerMenus playerMenus = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>();
				
				foreach (Menu menu in playerMenus.menus)
				{
					foreach (MenuElement menuElement in menu.elements)
					{
						if (menuElement is MenuInventoryBox)
						{
							menuElement.RecalculateSize ();
						}
					}
				}
			}
		}
		
		
		public static void CreateRecipe ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>())
			{
				PlayerMenus playerMenus = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>();
				foreach (Menu menu in playerMenus.menus)
				{
					foreach (MenuElement menuElement in menu.elements)
					{
						if (menuElement is MenuCrafting)
						{
							MenuCrafting crafting = (MenuCrafting) menuElement;
							if (crafting.craftingType == CraftingElementType.Output)
							{
								crafting.SetOutput (false, playerMenus.GetComponent <RuntimeInventory>());
							}
						}
					}
				}
			}
		}
		
		
		public static void ForceOffAllMenus (bool onlyPausing)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>())
			{
				PlayerMenus playerMenus = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>();
				
				foreach (Menu menu in playerMenus.menus)
				{
					if (menu.IsEnabled ())
					{
						if (!onlyPausing || (onlyPausing && menu.pauseWhenEnabled))
						{
							menu.ForceOff ();
						}
					}
				}
			}
		}
		
		
		public static void SimulateClick (string menuName, MenuElement _element, int _slot)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>())
			{
				Menu menu = PlayerMenus.GetMenuWithName (menuName);
				GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>().CheckClick (menu, _element, _slot, MouseState.SingleClick);
			}
		}
		
		
		public bool ArePauseMenusOn ()
		{
			foreach (Menu menu in menus)
			{
				if (menu.IsOn () && menu.pauseWhenEnabled)
				{
					return true;
				}
			}
			return false;
		}
		
		
		public void ForceOffSubtitles ()
		{
			foreach (Menu menu in menus)
			{
				if (menu.IsEnabled () && menu.appearType == AppearType.WhenSpeechPlays)
				{
					menu.ForceOff ();
				}
			}
		}
		
	}
	
}


