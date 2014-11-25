using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace AC
{
	
	[CustomEditor (typeof (Hotspot))]
	public class HotspotEditor : Editor
	{
		
		private Hotspot _target;
		
		private InventoryManager inventoryManager;
		private SettingsManager settingsManager;
		private CursorManager cursorManager;
		
		private static GUIContent
			deleteContent = new GUIContent("-", "Delete this interaction"),
			addContent = new GUIContent("+", "Create this interaction");
		
		private static GUILayoutOption
			autoWidth = GUILayout.MaxWidth (90f),
			buttonWidth = GUILayout.MaxWidth (20f);
		
		
		private void OnEnable ()
		{
			_target = (Hotspot) target;
		}
		
		
		public override void OnInspectorGUI()
		{
			if (AdvGame.GetReferences () == null)
			{
				Debug.LogError ("A References file is required - please use the Adventure Creator window to create one.");
				EditorGUILayout.LabelField ("No References file found!");
			}
			else
			{
				if (AdvGame.GetReferences ().inventoryManager)
				{
					inventoryManager = AdvGame.GetReferences ().inventoryManager;
				}
				if (AdvGame.GetReferences ().cursorManager)
				{
					cursorManager = AdvGame.GetReferences ().cursorManager;
				}
				if (AdvGame.GetReferences ().settingsManager)
				{
					settingsManager = AdvGame.GetReferences ().settingsManager;
				}
				
				if (_target.lineID > -1)
				{
					EditorGUILayout.LabelField ("Speech Manager ID:", _target.lineID.ToString ());
				}
				
				_target.interactionSource = (ActionListSource) EditorGUILayout.EnumPopup ("Interaction source:", _target.interactionSource);
				_target.hotspotName = EditorGUILayout.TextField ("Label (if not name):", _target.hotspotName);
				_target.highlight = (Highlight) EditorGUILayout.ObjectField ("Object to highlight:", _target.highlight, typeof (Highlight), true);
				_target.walkToMarker = (Marker) EditorGUILayout.ObjectField ("Walk-to marker:", _target.walkToMarker, typeof (Marker), true);
				_target.drawGizmos = EditorGUILayout.Toggle ("Draw yellow cube?", _target.drawGizmos);
				
				if (settingsManager != null && settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive)
				{
					_target.oneClick = EditorGUILayout.Toggle ("Single 'Use' Interaction?", _target.oneClick);
				}
				EditorGUILayout.Space ();
				
				UseInteractionGUI ();
				
				if (settingsManager == null || settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					EditorGUILayout.Space ();
					LookInteractionGUI ();
				}
				
				EditorGUILayout.Space ();
				InvInteractionGUI ();
			}
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (_target);
			}
		}
		
		
		private void LookInteractionGUI ()
		{
			EditorGUILayout.BeginVertical("Button");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Examine interaction", EditorStyles.boldLabel);
			
			if (!_target.provideLookInteraction)
			{
				if (GUILayout.Button (addContent, EditorStyles.miniButtonRight, buttonWidth))
				{
					Undo.RecordObject (_target, "Create examine interaction");
					_target.provideLookInteraction = true;
				}
			}
			else
			{
				if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
				{
					Undo.RecordObject (_target, "Delete examine interaction");
					_target.provideLookInteraction = false;
				}
			}
			
			EditorGUILayout.EndHorizontal ();
			if (_target.provideLookInteraction)
			{
				ButtonGUI (_target.lookButton, "Look", IsAssetBased (_target));
			}
			EditorGUILayout.EndVertical ();
		}
		
		
		private void UseInteractionGUI ()
		{
			if (settingsManager && settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				if (_target.UpgradeSelf ())
				{
					EditorUtility.SetDirty (_target);
				}
			}
			
			EditorGUILayout.BeginVertical("Button");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Use interactions", EditorStyles.boldLabel);
			
			if (GUILayout.Button (addContent, EditorStyles.miniButtonRight, buttonWidth))
			{
				Undo.RecordObject (_target, "Create use interaction");
				_target.useButtons.Add (new Button ());
				_target.provideUseInteraction = true;
			}
			EditorGUILayout.EndHorizontal();
			
			if (_target.provideUseInteraction)
			{
				if (cursorManager)
				{
					// Create a string List of the field's names (for the PopUp box)
					List<string> labelList = new List<string>();
					int iconNumber;
					
					if (cursorManager.cursorIcons.Count > 0)
					{
						foreach (CursorIcon _icon in cursorManager.cursorIcons)
						{
							labelList.Add (_icon.label);
						}
						
						foreach (Button useButton in _target.useButtons)
						{
							iconNumber = -1;
							
							int j = 0;
							foreach (CursorIcon _icon in cursorManager.cursorIcons)
							{
								// If an item has been removed, make sure selected variable is still valid
								if (_icon.id == useButton.iconID)
								{
									iconNumber = j;
									break;
								}
								j++;
							}
							
							if (iconNumber == -1)
							{
								// Wasn't found (item was deleted?), so revert to zero
								iconNumber = 0;
								useButton.iconID = 0;
							}
							
							EditorGUILayout.Space ();
							EditorGUILayout.BeginHorizontal ();
							
							iconNumber = EditorGUILayout.Popup ("Cursor:", iconNumber, labelList.ToArray());
							
							// Re-assign variableID based on PopUp selection
							useButton.iconID = cursorManager.cursorIcons[iconNumber].id;
							string iconLabel = cursorManager.cursorIcons[iconNumber].label;
							
							if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
							{
								Undo.RecordObject (_target, "Delete interaction");
								
								_target.useButtons.Remove (useButton);
								
								if (_target.useButtons.Count == 0)
								{
									_target.provideUseInteraction = false;
								}
								
								break;
							}
							
							EditorGUILayout.EndHorizontal ();
							ButtonGUI (useButton, iconLabel, IsAssetBased (_target));
							GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
						}
					}					
					else
					{
						EditorGUILayout.LabelField ("No cursor icons exist!");
						iconNumber = -1;
						
						for (int i=0; i<_target.useButtons.Count; i++)
						{
							_target.useButtons[i].iconID = -1;
						}
					}
				}
				else
				{
					Debug.LogWarning ("A CursorManager is required to run the game properly - please open the Adventure Creator wizard and set one.");
				}
			}
			
			EditorGUILayout.EndVertical ();
		}
		
		
		private void InvInteractionGUI ()
		{
			EditorGUILayout.BeginVertical("Button");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Inventory interactions", EditorStyles.boldLabel);
			
			if (GUILayout.Button (addContent, EditorStyles.miniButtonRight, buttonWidth))
			{
				Undo.RecordObject (_target, "Create inventory interaction");
				_target.invButtons.Add (new Button ());
				_target.provideInvInteraction = true;
			}
			EditorGUILayout.EndHorizontal();
			
			if (_target.provideInvInteraction)
			{
				if (inventoryManager)
				{
					// Create a string List of the field's names (for the PopUp box)
					List<string> labelList = new List<string>();
					int invNumber;
					
					if (inventoryManager.items.Count > 0)
					{
						
						foreach (InvItem _item in inventoryManager.items)
						{
							labelList.Add (_item.label);
						}
						
						foreach (Button invButton in _target.invButtons)
						{
							invNumber = -1;
							
							int j = 0;
							foreach (InvItem _item in inventoryManager.items)
							{
								// If an item has been removed, make sure selected variable is still valid
								if (_item.id == invButton.invID)
								{
									invNumber = j;
									break;
								}
								
								j++;
							}
							
							if (invNumber == -1)
							{
								// Wasn't found (item was deleted?), so revert to zero
								Debug.Log ("Previously chosen item no longer exists!");
								invNumber = 0;
								invButton.invID = 0;
							}
							
							EditorGUILayout.Space ();
							EditorGUILayout.BeginHorizontal ();
							
							invNumber = EditorGUILayout.Popup ("Inventory item:", invNumber, labelList.ToArray());
							
							// Re-assign variableID based on PopUp selection
							invButton.invID = inventoryManager.items[invNumber].id;
							
							if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
							{
								Undo.RecordObject (_target, "Delete inventory interaction");
								
								_target.invButtons.Remove (invButton);
								
								if (_target.invButtons.Count == 0)
								{
									_target.provideInvInteraction = false;
								}
								
								break;
							}
							
							EditorGUILayout.EndHorizontal ();
							ButtonGUI (invButton, "Inv", IsAssetBased (_target));
							GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
						}
						
					}					
					else
					{
						EditorGUILayout.LabelField ("No inventory items exist!");
						invNumber = -1;
						
						for (int i=0; i<_target.invButtons.Count; i++)
						{
							_target.invButtons[i].invID = -1;
						}
					}
				}
				else
				{
					Debug.LogWarning ("An InventoryManager is required to run the game properly - please open the Adventure Creator wizard and set one.");
				}
			}
			
			EditorGUILayout.EndVertical ();
		}
		
		
		private void ButtonGUI (Button button, string suffix, bool useAssetFile)
		{
			bool isEnabled = !button.isDisabled;
			isEnabled = EditorGUILayout.Toggle ("Enabled:", isEnabled);
			button.isDisabled = !isEnabled;
			
			if (useAssetFile)
			{
				button.assetFile = (ActionListAsset) EditorGUILayout.ObjectField ("Interaction:", button.assetFile, typeof (ActionListAsset), false);
			}
			else
			{
				EditorGUILayout.BeginHorizontal ();
				button.interaction = (Interaction) EditorGUILayout.ObjectField ("Interaction:", button.interaction, typeof (Interaction), true);
				
				if (button.interaction == null)
				{
					if (GUILayout.Button ("Auto-create", autoWidth))
					{
						Undo.RecordObject (_target, "Create Interaction");
						Interaction newInteraction = AdvGame.GetReferences ().sceneManager.AddPrefab ("Logic", "Interaction", true, false, true).GetComponent <Interaction>();
						
						string hotspotName = _target.gameObject.name;
						if (_target.hotspotName != "")
						{
							hotspotName = _target.hotspotName;
						}
						
						
						newInteraction.gameObject.name = AdvGame.UniqueName (suffix + ": " + hotspotName);
						button.interaction = newInteraction;
					}
				}
				EditorGUILayout.EndHorizontal ();
			}
			
			button.playerAction = (PlayerAction) EditorGUILayout.EnumPopup ("Player action:", button.playerAction);
			
			if (button.playerAction == PlayerAction.WalkTo || button.playerAction == PlayerAction.WalkToMarker)
			{
				button.isBlocking = EditorGUILayout.Toggle ("Cutscene while moving?", button.isBlocking);
				button.faceAfter = EditorGUILayout.Toggle ("Face after moving?", button.faceAfter);
				
				if (button.playerAction == PlayerAction.WalkTo)
				{
					button.setProximity = EditorGUILayout.Toggle ("Set minimum distance?", button.setProximity);
					if (button.setProximity)
					{
						button.proximity = EditorGUILayout.FloatField ("Proximity:", button.proximity);
					}
				}
			}
		}
		
		
		private bool IsAssetBased (Hotspot _target)
		{
			if (_target.interactionSource == ActionListSource.AssetFile)
			{
				return true;
			}
			return false;
		}
		
	}
	
}
