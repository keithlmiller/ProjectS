/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SettingsManager.cs"
 * 
 *	This script handles the "Settings" tab of the main wizard.
 *	It is used to define the player, and control methods of the game.
 * 
 */

using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class SettingsManager : ScriptableObject
	{
		
		#if UNITY_EDITOR
		private static GUIContent
			deleteContent = new GUIContent("-", "Delete item");
		
		private static GUILayoutOption
			buttonWidth = GUILayout.MaxWidth (20f);
		#endif
		
		// Save settings
		public string saveFileName = "";
		public SaveTimeDisplay saveTimeDisplay = SaveTimeDisplay.DateOnly;
		public bool takeSaveScreenshots;

		// ActionListAsset settings
		public ActionListAsset actionListOnStart;

		// Character settings
		public PlayerSwitching playerSwitching = PlayerSwitching.DoNotAllow;
		public Player player;
		public List<PlayerPrefab> players = new List<PlayerPrefab>();
		public bool shareInventory = false;
		
		// Interface settings
		public MovementMethod movementMethod = MovementMethod.PointAndClick;
		public InputMethod inputMethod = InputMethod.MouseAndKeyboard;
		public AC_InteractionMethod interactionMethod = AC_InteractionMethod.ContextSensitive;
		public CancelInteractions cancelInteractions = CancelInteractions.CursorLeavesMenus;
		public SeeInteractions seeInteractions = SeeInteractions.ClickOnHotspot;
		public SelectInteractions selectInteractions = SelectInteractions.ClickingMenu;
		public bool cycleInventoryCursors = true;
		public bool hideLockedCursor = false;
		public bool lockCursorOnStart = false;
		
		// Inventory settings
		public bool inventoryDragDrop = false;
		public bool inventoryDropLook = false;
		public bool useOuya = false;
		public InventoryInteractions inventoryInteractions = InventoryInteractions.Single;
		public InventoryActiveEffect inventoryActiveEffect = InventoryActiveEffect.Simple;
		public bool activeWhenHover = false;
		public bool inventoryDisableLeft = true;
		public float inventoryPulseSpeed = 1f;
		public bool activeWhenUnhandled = true;
		public bool canReorderItems = false;
		public bool hideSelectedFromMenu = false;
		public RightClickInventory rightClickInventory = RightClickInventory.DeselectsItem;
		public bool reverseInventoryCombinations = false;
		public bool canMoveWhenActive = true;

		// Movement settings
		public Transform clickPrefab;
		public DirectMovementType directMovementType = DirectMovementType.RelativeToCamera;
		public float destinationAccuracy = 0.9f;
		public float walkableClickRange = 0.5f;
		public DragAffects dragAffects = DragAffects.Movement;
		public float verticalReductionFactor = 0.7f;
		public bool doubleClickMovement = false;
		public float jumpSpeed = 4f;
		public bool singleTapStraight = false;

		// Drag settings
		public float dragRayLength = 100f;
		public float cameraInfluence = 200f;

		public float freeAimTouchSpeed = 0.01f;
		public float dragWalkThreshold = 5f;
		public float dragRunThreshold = 20f;
		public bool drawDragLine = false;
		public float dragLineWidth = 3f;
		public Color dragLineColor = Color.white;

		// Touch Screen settings
		public bool offsetTouchCursor = false;
		public bool doubleTapHotspots = true;
		
		// Camera settings
		public bool forceAspectRatio = false;
		public float wantedAspectRatio = 1.5f;
		public bool landscapeModeOnly = true;
		public CameraPerspective cameraPerspective = CameraPerspective.ThreeD;
		private int cameraPerspective_int;
		#if UNITY_EDITOR
		private string[] cameraPerspective_list = { "2D", "2.5D", "3D" };
		#endif
		public MovingTurning movingTurning = MovingTurning.Unity2D;
		
		// Hotspot settings
		public HotspotDetection hotspotDetection = HotspotDetection.MouseOver;
		public HotspotsInVicinity hotspotsInVicinity = HotspotsInVicinity.NearestOnly;
		public HotspotIconDisplay hotspotIconDisplay = HotspotIconDisplay.Never;
		public HotspotIcon hotspotIcon;
		public Texture2D hotspotIconTexture = null;
		public float hotspotIconSize = 0.04f;
		public bool playerFacesHotspots = false;
		
		// Raycast settings
		public float navMeshRaycastLength = 100f;
		public float hotspotRaycastLength = 100f;
		public float moveableRaycastLength = 30f;
		
		// Layer names
		public string hotspotLayer = "Default";
		public string navMeshLayer = "NavMesh";
		public string backgroundImageLayer = "BackgroundImage";
		public string deactivatedLayer = "Ignore Raycast";

		// Loading screen
		public bool useLoadingScreen = false;
		public int loadingScene = 0;
		
		// Options data
		#if UNITY_EDITOR
		private OptionsData optionsData = new OptionsData ();
		private string ppKey = "Options";
		private string optionsBinary = "";
		
		
		public void ShowGUI ()
		{
			EditorGUILayout.LabelField ("Save game settings", EditorStyles.boldLabel);
			
			if (saveFileName == "")
			{
				saveFileName = SaveSystem.SetProjectName ();
			}
			saveFileName = EditorGUILayout.TextField ("Save filename:", saveFileName);
			#if !UNITY_WEBPLAYER
			saveTimeDisplay = (SaveTimeDisplay) EditorGUILayout.EnumPopup ("Time display:", saveTimeDisplay);
			takeSaveScreenshots = EditorGUILayout.ToggleLeft ("Take screenshot when saving?", takeSaveScreenshots);
			#else
			takeSaveScreenshots = false;
			#endif

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Cutscenes settings:", EditorStyles.boldLabel);

			actionListOnStart = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList on start game:", actionListOnStart, typeof (ActionListAsset), false);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Character settings:", EditorStyles.boldLabel);
			
			CreatePlayersGUI ();
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Interface settings", EditorStyles.boldLabel);
			
			movementMethod = (MovementMethod) EditorGUILayout.EnumPopup ("Movement method:", movementMethod);
			if (movementMethod == MovementMethod.UltimateFPS && !UltimateFPSIntegration.IsDefinePresent ())
			{
				EditorGUILayout.HelpBox ("The 'UltimateFPSIsPresent' preprocessor define must be declared in the Player Settings.", MessageType.Warning);
			}
			inputMethod = (InputMethod) EditorGUILayout.EnumPopup ("Input method:", inputMethod);
			interactionMethod = (AC_InteractionMethod) EditorGUILayout.EnumPopup ("Interaction method:", interactionMethod);

			if (inputMethod != InputMethod.TouchScreen)
			{
				useOuya = EditorGUILayout.ToggleLeft ("Playing on OUYA platform?", useOuya);
				if (useOuya && !OuyaIntegration.IsDefinePresent ())
				{
					EditorGUILayout.HelpBox ("The 'OUYAIsPresent' preprocessor define must be declared in the Player Settings.", MessageType.Warning);
				}
				if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					selectInteractions = (SelectInteractions) EditorGUILayout.EnumPopup ("Select Interactions by:", selectInteractions);
					if (selectInteractions != SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						seeInteractions = (SeeInteractions) EditorGUILayout.EnumPopup ("See Interactions with:", seeInteractions);
					}
					if (selectInteractions == SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						cycleInventoryCursors = EditorGUILayout.ToggleLeft ("Cycle through Inventory items too?", cycleInventoryCursors);
					}
				}

				if (SelectInteractionMethod () == SelectInteractions.ClickingMenu)
				{
					cancelInteractions = (CancelInteractions) EditorGUILayout.EnumPopup ("Cancel interactions with:", cancelInteractions);
				}
				else
				{
					cancelInteractions = CancelInteractions.CursorLeavesMenus;
				}
			}
			lockCursorOnStart = EditorGUILayout.ToggleLeft ("Lock cursor in screen's centre when game begins?", lockCursorOnStart);
			hideLockedCursor = EditorGUILayout.ToggleLeft ("Hide cursor when locked in screen's centre?", hideLockedCursor);

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Inventory settings", EditorStyles.boldLabel);
			
			reverseInventoryCombinations = EditorGUILayout.ToggleLeft ("Combine interactions work in reverse?", reverseInventoryCombinations);
			if (interactionMethod != AC_InteractionMethod.ContextSensitive)
			{
				inventoryInteractions = (InventoryInteractions) EditorGUILayout.EnumPopup ("Inventory interactions:", inventoryInteractions);
			}
			if (interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || inventoryInteractions == InventoryInteractions.Single)
			{
				inventoryDragDrop = EditorGUILayout.ToggleLeft ("Drag and drop Inventory interface?", inventoryDragDrop);
				if (!inventoryDragDrop)
				{
					inventoryDisableLeft = EditorGUILayout.ToggleLeft ("Left-click deselects active item?", inventoryDisableLeft);
					if (interactionMethod == AC_InteractionMethod.ContextSensitive || inventoryInteractions == InventoryInteractions.Single)
					{
						rightClickInventory = (RightClickInventory) EditorGUILayout.EnumPopup ("Right-click active item:", rightClickInventory);
					}

					if (movementMethod == MovementMethod.PointAndClick)
					{
						canMoveWhenActive = EditorGUILayout.ToggleLeft ("Can move player if an Item is active?", canMoveWhenActive);
					}
				}
				else
				{
					inventoryDropLook = EditorGUILayout.ToggleLeft ("Can drop an Item onto itself to Examine it?", inventoryDropLook);
				}
				inventoryActiveEffect = (InventoryActiveEffect) EditorGUILayout.EnumPopup ("Active cursor FX:", inventoryActiveEffect);
				if (inventoryActiveEffect == InventoryActiveEffect.Pulse)
				{
					inventoryPulseSpeed = EditorGUILayout.Slider ("Active FX pulse speed:", inventoryPulseSpeed, 0.5f, 2f);
				}
				activeWhenUnhandled = EditorGUILayout.ToggleLeft ("Show Active FX when an Interaction is unhandled?", activeWhenUnhandled);
				canReorderItems = EditorGUILayout.ToggleLeft ("Items can be re-ordered in Menu?", canReorderItems);
				hideSelectedFromMenu = EditorGUILayout.ToggleLeft ("Hide currently active Item in Menu?", hideSelectedFromMenu);
			}
			activeWhenHover = EditorGUILayout.ToggleLeft ("Show Active FX when Cursor hovers over Item in Menu?", activeWhenHover);

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Required inputs:", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox ("The following inputs are available for the chosen interface settings:" + GetInputList (), MessageType.Info);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Movement settings", EditorStyles.boldLabel);
			
			if ((inputMethod == InputMethod.TouchScreen && movementMethod != MovementMethod.PointAndClick) || movementMethod == MovementMethod.Drag)
			{
				dragWalkThreshold = EditorGUILayout.FloatField ("Walk threshold:", dragWalkThreshold);
				dragRunThreshold = EditorGUILayout.FloatField ("Run threshold:", dragRunThreshold);
				
				if (inputMethod == InputMethod.TouchScreen && movementMethod == MovementMethod.FirstPerson)
				{
					freeAimTouchSpeed = EditorGUILayout.FloatField ("Freelook speed:", freeAimTouchSpeed);
				}
				
				drawDragLine = EditorGUILayout.Toggle ("Draw drag line?", drawDragLine);
				if (drawDragLine)
				{
					dragLineWidth = EditorGUILayout.FloatField ("Drag line width:", dragLineWidth);
					dragLineColor = EditorGUILayout.ColorField ("Drag line colour:", dragLineColor);
				}
			}
			else if (movementMethod == MovementMethod.Direct)
			{
				directMovementType = (DirectMovementType) EditorGUILayout.EnumPopup ("Direct-movement type:", directMovementType);
			}
			else if (movementMethod == MovementMethod.PointAndClick)
			{
				clickPrefab = (Transform) EditorGUILayout.ObjectField ("Click marker:", clickPrefab, typeof (Transform), false);
				walkableClickRange = EditorGUILayout.Slider ("NavMesh search %:", walkableClickRange, 0f, 1f);
				doubleClickMovement = EditorGUILayout.Toggle ("Double-click to move?", doubleClickMovement);
			}
			if (movementMethod == MovementMethod.StraightToCursor)
			{
				dragRunThreshold = EditorGUILayout.FloatField ("Run threshold:", dragRunThreshold);
				singleTapStraight = EditorGUILayout.Toggle ("Single-click works too?", singleTapStraight);
			}
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen)
			{
				dragAffects = (DragAffects) EditorGUILayout.EnumPopup ("Touch-drag affects:", dragAffects);
			}
			if ((movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson) && inputMethod != InputMethod.TouchScreen)
			{
				jumpSpeed = EditorGUILayout.Slider ("Jump speed:", jumpSpeed, 1f, 10f);
			}
			
			destinationAccuracy = EditorGUILayout.Slider ("Destination accuracy:", destinationAccuracy, 0f, 1f);
			
			if (inputMethod == InputMethod.TouchScreen)
			{
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Touch Screen settings", EditorStyles.boldLabel);

				offsetTouchCursor = EditorGUILayout.Toggle ("Drag cursor with touch?", offsetTouchCursor);
				doubleTapHotspots = EditorGUILayout.Toggle ("Double-tap Hotspots?", doubleTapHotspots);
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Camera settings", EditorStyles.boldLabel);
			
			cameraPerspective_int = (int) cameraPerspective;
			cameraPerspective_int = EditorGUILayout.Popup ("Camera perspective:", cameraPerspective_int, cameraPerspective_list);
			cameraPerspective = (CameraPerspective) cameraPerspective_int;
			if (movementMethod == MovementMethod.FirstPerson)
			{
				cameraPerspective = CameraPerspective.ThreeD;
			}
			if (cameraPerspective == CameraPerspective.TwoD)
			{
				movingTurning = (MovingTurning) EditorGUILayout.EnumPopup ("Moving and turning:", movingTurning);
				if (movingTurning == MovingTurning.TopDown || movingTurning == MovingTurning.Unity2D)
				{
					verticalReductionFactor = EditorGUILayout.Slider ("Vertical movement factor:", verticalReductionFactor, 0.1f, 1f);
				}
			}
			
			forceAspectRatio = EditorGUILayout.Toggle ("Force aspect ratio?", forceAspectRatio);
			if (forceAspectRatio)
			{
				wantedAspectRatio = EditorGUILayout.FloatField ("Aspect ratio:", wantedAspectRatio);
				#if UNITY_IPHONE
				landscapeModeOnly = EditorGUILayout.Toggle ("Landscape-mode only?", landscapeModeOnly);
				#endif
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Hotpot settings", EditorStyles.boldLabel);

			hotspotDetection = (HotspotDetection) EditorGUILayout.EnumPopup ("Hotspot detection method:", hotspotDetection);
			
			if (hotspotDetection == HotspotDetection.PlayerVicinity && (movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson))
			{
				hotspotsInVicinity = (HotspotsInVicinity) EditorGUILayout.EnumPopup ("Hotspots in vicinity:", hotspotsInVicinity);
			}

			if (cameraPerspective != CameraPerspective.TwoD)
			{
				playerFacesHotspots = EditorGUILayout.Toggle ("Player turns head to active?", playerFacesHotspots);
			}

			hotspotIconDisplay = (HotspotIconDisplay) EditorGUILayout.EnumPopup ("Display Hotspot icon:", hotspotIconDisplay);
			if (hotspotIconDisplay != HotspotIconDisplay.Never)
			{
				hotspotIcon = (HotspotIcon) EditorGUILayout.EnumPopup ("Hotspot icon type:", hotspotIcon);
				if (hotspotIcon == HotspotIcon.Texture)
				{
					hotspotIconTexture = (Texture2D) EditorGUILayout.ObjectField ("Hotspot icon texture:", hotspotIconTexture, typeof (Texture2D), false);
				}
				hotspotIconSize = EditorGUILayout.FloatField ("Hotspot icon size:", hotspotIconSize);
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Raycast settings", EditorStyles.boldLabel);
			navMeshRaycastLength = EditorGUILayout.FloatField ("NavMesh ray length:", navMeshRaycastLength);
			hotspotRaycastLength = EditorGUILayout.FloatField ("Hotspot ray length:", hotspotRaycastLength);
			moveableRaycastLength = EditorGUILayout.FloatField ("Moveable ray length:", moveableRaycastLength);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Layer names", EditorStyles.boldLabel);
			
			hotspotLayer = EditorGUILayout.TextField ("Hotspot:", hotspotLayer);
			navMeshLayer = EditorGUILayout.TextField ("Nav mesh:", navMeshLayer);
			if (cameraPerspective == CameraPerspective.TwoPointFiveD)
			{
				backgroundImageLayer = EditorGUILayout.TextField ("Background image:", backgroundImageLayer);
			}
			deactivatedLayer = EditorGUILayout.TextField ("Deactivated:", deactivatedLayer);

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Loading scene", EditorStyles.boldLabel);
			useLoadingScreen = EditorGUILayout.Toggle ("Use loading screen?", useLoadingScreen);
			if (useLoadingScreen)
			{
				loadingScene = EditorGUILayout.IntField ("Loading screen scene:", loadingScene);
			}

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Options data", EditorStyles.boldLabel);
			
			if (!PlayerPrefs.HasKey (ppKey))
			{
				optionsData = new OptionsData ();
				optionsBinary = Serializer.SerializeObjectBinary (optionsData);
				PlayerPrefs.SetString (ppKey, optionsBinary);
			}
			
			optionsBinary = PlayerPrefs.GetString (ppKey);
			optionsData = Serializer.DeserializeObjectBinary <OptionsData> (optionsBinary);
			
			optionsData.speechVolume = EditorGUILayout.Slider ("Speech volume:", optionsData.speechVolume, 0f, 1f);
			optionsData.musicVolume = EditorGUILayout.Slider ("Music volume:", optionsData.musicVolume, 0f, 1f);
			optionsData.sfxVolume = EditorGUILayout.Slider ("SFX volume:", optionsData.sfxVolume, 0f, 1f);
			optionsData.showSubtitles = EditorGUILayout.Toggle ("Show subtitles?", optionsData.showSubtitles);
			optionsData.language = EditorGUILayout.IntField ("Language:", optionsData.language);
			
			optionsBinary = Serializer.SerializeObjectBinary (optionsData);
			PlayerPrefs.SetString (ppKey, optionsBinary);
			
			if (GUILayout.Button ("Reset options data"))
			{
				PlayerPrefs.DeleteKey ("Options");
				optionsData = new OptionsData ();
				Debug.Log ("PlayerPrefs cleared");
			}
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}
		
		#endif
		
		
		private string GetInputList ()
		{
			string result = "";
			
			if (inputMethod == InputMethod.KeyboardOrController)
			{
				result += "\n";
				result += "- InteractionA (Button)";
				result += "\n";
				result += "- InteractionB (Button)";
				result += "\n";
				result += "- CursorHorizontal (Axis)";
				result += "\n";
				result += "- CursorVertical (Axis)";
			}
			
			if (movementMethod != MovementMethod.PointAndClick && movementMethod != MovementMethod.StraightToCursor)
			{
				result += "\n";
				result += "- ToggleCursor (Button)";
			}
			
			if (movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson || inputMethod == InputMethod.KeyboardOrController)
			{
				if (inputMethod != InputMethod.TouchScreen)
				{
					result += "\n";
					result += "- Horizontal (Axis)";
					result += "\n";
					result += "- Vertical (Axis)";

					if (movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson)
					{
						result += "\n";
						result += "- Run (Button)";
						result += "\n";
						result += "- ToggleRun (Button)";
						result += "\n";
						result += "- Jump (Button)";
					}
				}
				
				if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.MouseAndKeyboard)
				{
					result += "\n";
					result += "- MouseScrollWheel (Axis)";
					result += "\n";
					result += "- CursorHorizontal (Axis)";
					result += "\n";
					result += "- CursorVertical (Axis)";
				}

				if ((movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson)
					&& (hotspotDetection == HotspotDetection.PlayerVicinity && hotspotsInVicinity == HotspotsInVicinity.CycleMultiple))
				{
					result += "\n";
					result += "- CycleHotspotsLeft (Button)";
					result += "\n";
					result += "- CycleHotspotsRight (Button)";
					result += "\n";
					result += "- CycleHotspots (Axis)";
				}
			}

			if (SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot)
			{
				result += "\n";
				result += "- CycleInteractionsLeft (Button)";
				result += "\n";
				result += "- CycleInteractionsRight (Button)";
				result += "\n";
				result += "- CycleInteractions (Axis)";
			}
			
			result += "\n";
			result += "- FlashHotspots (Button)";
			result += "\n";
			result += "- Menu (Button)";
			result += "\n";
			result += "- EndCutscene (Button)";
			result += "\n";
			result += "- ThrowMoveable (Button)";
			result += "\n";
			result += "- RotateMoveable (Button)";
			result += "\n";
			result += "- RotateMoveableToggle (Button)";
			result += "\n";
			result += "- ZoomMoveable (Axis)";
			
			return result;
		}
		
		
		public bool ActInScreenSpace ()
		{
			if ((movingTurning == MovingTurning.ScreenSpace || movingTurning == MovingTurning.Unity2D) && cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsUnity2D ()
		{
			if (movingTurning == MovingTurning.Unity2D && cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsTopDown ()
		{
			if (movingTurning == MovingTurning.TopDown && cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsFirstPersonDragRotation ()
		{
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen && dragAffects == DragAffects.Rotation)
			{
				return true;
			}
			return false;
		}


		public bool IsFirstPersonDragMovement ()
		{
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen && dragAffects == DragAffects.Movement)
			{
				return true;
			}
			return false;
		}

		
		
		#if UNITY_EDITOR
		
		private void CreatePlayersGUI ()
		{
			playerSwitching = (PlayerSwitching) EditorGUILayout.EnumPopup ("Player switching:", playerSwitching);
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				player = (Player) EditorGUILayout.ObjectField ("Player:", player, typeof (Player), false);
			}
			else
			{
				shareInventory = EditorGUILayout.Toggle ("Share same Inventory?", shareInventory);
				
				foreach (PlayerPrefab _player in players)
				{
					EditorGUILayout.BeginHorizontal ();
					
					_player.playerOb = (Player) EditorGUILayout.ObjectField ("Player " + _player.ID + ":", _player.playerOb, typeof (Player), false);
					
					if (_player.isDefault)
					{
						GUILayout.Label ("DEFAULT", EditorStyles.boldLabel, GUILayout.Width (80f));
					}
					else
					{
						if (GUILayout.Button ("Make default", GUILayout.Width (80f)))
						{
							SetDefaultPlayer (_player);
						}
					}
					
					if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
					{
						Undo.RecordObject (this, "Delete player reference");
						players.Remove (_player);
						break;
					}
					
					EditorGUILayout.EndHorizontal ();
				}
				
				if (GUILayout.Button("Add new player"))
				{
					Undo.RecordObject (this, "Add player");
					
					PlayerPrefab newPlayer = new PlayerPrefab (GetPlayerIDArray ());
					players.Add (newPlayer);
				}
			}
		}
		
		#endif
		
		
		private int[] GetPlayerIDArray ()
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (PlayerPrefab player in players)
			{
				idArray.Add (player.ID);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		public int GetDefaultPlayerID ()
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return 0;
			}
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.isDefault)
				{
					return _player.ID;
				}
			}
			
			return 0;
		}


		public bool CanClickOffInteractionMenu ()
		{
			if (cancelInteractions == CancelInteractions.ClickOffMenu || inputMethod == InputMethod.TouchScreen)
			{
				return true;
			}
			return false;
		}


		public bool MouseOverForInteractionMenu ()
		{
			if (seeInteractions == SeeInteractions.CursorOverHotspot && inputMethod != InputMethod.TouchScreen)
			{
				return true;
			}
			return false;
		}
		
		
		public Player GetDefaultPlayer ()
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return player;
			}
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.isDefault)
				{
					if (_player.playerOb != null)
					{
						return _player.playerOb;
					}
					
					Debug.LogWarning ("Default Player has no prefab!");
					return null;
				}
			}
			
			Debug.LogWarning ("Cannot find default player!");
			return null;
		}
		
		
		private void SetDefaultPlayer (PlayerPrefab defaultPlayer)
		{
			foreach (PlayerPrefab _player in players)
			{
				if (_player == defaultPlayer)
				{
					_player.isDefault = true;
				}
				else
				{
					_player.isDefault = false;
				}
			}
		}
		
		
		private bool DoPlayerAnimEnginesMatch ()
		{
			AnimationEngine animationEngine = AnimationEngine.Legacy;
			bool foundFirst = false;
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.playerOb != null)
				{
					if (!foundFirst)
					{
						foundFirst = true;
						animationEngine = _player.playerOb.animationEngine;
					}
					else
					{
						if (_player.playerOb.animationEngine != animationEngine)
						{
							return false;
						}
					}
				}
			}
			
			return true;
		}


		public SelectInteractions SelectInteractionMethod ()
		{
			if (inputMethod != InputMethod.TouchScreen && interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
			{
				return selectInteractions;
			}
			return SelectInteractions.ClickingMenu;
		}


		public bool IsInLoadingScene ()
		{
			if (useLoadingScreen && Application.loadedLevel == loadingScene && Application.loadedLevelName != "")
			{
				return true;
			}
			return false;
		}


		private bool IsOnOuya ()
		{
			if (inputMethod != InputMethod.TouchScreen && useOuya)
			{
				return true;
			}
			return false;
		}

	}
	
}