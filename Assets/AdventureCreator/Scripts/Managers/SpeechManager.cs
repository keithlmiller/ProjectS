/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SpeechManager.cs"
 * 
 *	This script handles the "Speech" tab of the main wizard.
 *	It is used to auto-number lines for audio files, and handle translations.
 * 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class SpeechManager : ScriptableObject
	{
		
		public float textScrollSpeed = 50;
		public AudioClip textScrollCLip = null;
		public bool displayForever = false;
		public float screenTimeFactor = 0.1f;
		public bool allowSpeechSkipping = false;
		public bool allowGameplaySpeechSkipping = false;
		public bool searchAudioFiles = true;
		public bool forceSubtitles = true;
		public bool translateAudio = true;
		public bool scrollSubtitles = true;
		public bool endScrollBeforeSkip = false;
		public bool keepTextInBuffer = false;

		public bool separateLines = false;
		public float separateLinePause = 0.1f;

		public List<SpeechLine> lines = new List<SpeechLine>();
		public List<string> languages = new List<string>();
		public string[] sceneFiles;

		public bool ignoreOriginalText = false;
		
	#if UNITY_EDITOR

		private List<SpeechLine> tempLines = new List<SpeechLine>();
		private string sceneLabel;

		private bool showSceneLines = false;
		private bool showHotspotLines = false;
		private bool showInventoryLines = false;
		private bool showAssetLines = false;
		private bool showCursorLines = false;
		private bool showMenuLines = false;


		private static GUIContent
			deleteContent = new GUIContent("-", "Delete translation");
		
		
		public void ShowGUI ()
		{
			#if UNITY_WEBPLAYER
			EditorGUILayout.HelpBox ("Exporting game text cannot be performed in WebPlayer mode - please switch platform to do so.", MessageType.Warning);
			GUILayout.Space (10);
			#endif

			EditorGUILayout.LabelField ("Subtitles", EditorStyles.boldLabel);

			separateLines = EditorGUILayout.Toggle ("Separate carriage returns?", separateLines);
			if (separateLines)
			{
				separateLinePause = EditorGUILayout.Slider ("Split line delay (s):", separateLinePause, 0.1f, 1f);
			}
			scrollSubtitles = EditorGUILayout.Toggle ("Scroll text?", scrollSubtitles);
			if (scrollSubtitles)
			{
				textScrollSpeed = EditorGUILayout.FloatField ("Text scroll speed:", textScrollSpeed);
				textScrollCLip = (AudioClip) EditorGUILayout.ObjectField ("Text scroll audio clip:", textScrollCLip, typeof (AudioClip), false);
			}

			displayForever = EditorGUILayout.Toggle ("Display indefinitely?", displayForever);
			if (displayForever)
			{
				endScrollBeforeSkip = EditorGUILayout.Toggle ("End scrolling before skip?", endScrollBeforeSkip);
			}
			else
			{
				screenTimeFactor = EditorGUILayout.FloatField ("Display time factor:", screenTimeFactor);
				allowSpeechSkipping = EditorGUILayout.Toggle ("Allow subtitle skipping?", allowSpeechSkipping);
				if (allowSpeechSkipping)
				{
					allowGameplaySpeechSkipping = EditorGUILayout.Toggle ("Skip background speech?", allowGameplaySpeechSkipping);
					if (scrollSubtitles)
					{
						endScrollBeforeSkip = EditorGUILayout.Toggle ("End scrolling before skip?", endScrollBeforeSkip);
					}
				}
			}

			keepTextInBuffer = EditorGUILayout.Toggle ("Retain text after playing?", keepTextInBuffer);

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Audio", EditorStyles.boldLabel);

			forceSubtitles = EditorGUILayout.Toggle ("Force subtitles if no audio?", forceSubtitles);
			searchAudioFiles = EditorGUILayout.Toggle ("Auto-play audio files?", searchAudioFiles);
			translateAudio = EditorGUILayout.Toggle ("Audio translations?", translateAudio);

			EditorGUILayout.Space ();

			LanguagesGUI ();

			EditorGUILayout.Space ();
			
			GUILayout.Label ("Game text", EditorStyles.boldLabel);
			GUILayout.Label ("Speech audio files must be placed in: /Resources/Speech");

			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Gather text", EditorStyles.miniButtonLeft))
			{
				PopulateList ();
				
				if (sceneFiles.Length > 0)
				{
					Array.Sort (sceneFiles);
				}
			}
			if (GUILayout.Button ("Reset text", EditorStyles.miniButtonMid))
			{
				ClearList ();
			}
			if (GUILayout.Button ("Create script sheet", EditorStyles.miniButtonRight))
			{
				if (lines.Count > 0)
				{
					CreateScript ();
				}
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Import all translations", EditorStyles.miniButtonLeft))
			{
				ImportGameText ();
			}
			if (GUILayout.Button ("Export all translations", EditorStyles.miniButtonRight))
			{
				ExportGameText ();
			}
			EditorGUILayout.EndHorizontal ();

			if (lines.Count > 0)
			{
				ListLines ();
			}
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}


		private void ListLines ()
		{
			EditorGUILayout.BeginVertical ("Button");		

			showSceneLines = EditorGUILayout.Foldout (showSceneLines, "Scene text");
			if (showSceneLines)
			{
				if (sceneFiles.Length > 0)
				{
					foreach (string scene in sceneFiles)
					{
						int extentionPos = scene.IndexOf (".unity");
						sceneLabel = scene.Substring (7, extentionPos-7);
						GUILayout.Label ("Scene: " + sceneLabel, EditorStyles.largeLabel);
						
						GUILayout.Label ("Speech text: ", EditorStyles.boldLabel);
						foreach (SpeechLine line in lines)
						{
							if (line.scene == scene && line.textType == AC_TextType.Speech)
							{
								line.ShowGUI ();
							}
						}

						GUILayout.Label ("Hotspots: ", EditorStyles.boldLabel);
						foreach (SpeechLine line in lines)
						{
							if (line.scene == scene && line.textType == AC_TextType.Hotspot)
							{
								line.ShowGUI ();
							}
						}

						GUILayout.Label ("Dialogue options: ", EditorStyles.boldLabel);
						foreach (SpeechLine line in lines)
						{
							if (line.scene == scene && line.textType == AC_TextType.DialogueOption)
							{
								line.ShowGUI ();
							}
						}

						GUILayout.Label ("Journal pages: ", EditorStyles.boldLabel);
						foreach (SpeechLine line in lines)
						{
							if (line.scene == scene && line.textType == AC_TextType.JournalEntry)
							{
								line.ShowGUI ();
							}
						}
					}
				}
			}

			showInventoryLines = EditorGUILayout.Foldout (showInventoryLines, "Inventory items");
			if (showInventoryLines)
			{
				foreach (SpeechLine line in lines)
				{
					if (line.textType == AC_TextType.InventoryItem)
					{
						line.ShowGUI ();
					}
				}
			}

			showAssetLines = EditorGUILayout.Foldout (showAssetLines, "Scene-independent text");
			if (showAssetLines)
			{
				foreach (SpeechLine line in lines)
				{
					if (line.textType == AC_TextType.Speech && line.scene == "")
					{
						line.ShowGUI ();
					}
				}
			}

			showCursorLines = EditorGUILayout.Foldout (showCursorLines, "Cursor icons");
			if (showCursorLines)
			{
				foreach (SpeechLine line in lines)
				{
					if (line.textType == AC_TextType.CursorIcon)
					{
						line.ShowGUI ();
					}
				}
			}

			showHotspotLines = EditorGUILayout.Foldout (showHotspotLines, "Hotspot prefixes");
			if (showHotspotLines)
			{
				foreach (SpeechLine line in lines)
				{
					if (line.textType == AC_TextType.HotspotPrefix)
					{
						line.ShowGUI ();
					}
				}
			}

			showMenuLines = EditorGUILayout.Foldout (showMenuLines, "Menu element");
			if (showMenuLines)
			{
				foreach (SpeechLine line in lines)
				{
					if ((line.textType == AC_TextType.MenuElement) || (line.textType == AC_TextType.JournalEntry && line.scene == ""))
					{
						line.ShowGUI ();
					}
				}
			}
			EditorGUILayout.EndVertical ();
		}


		private void LanguagesGUI ()
		{
			GUILayout.Label ("Translations", EditorStyles.boldLabel);
			
			if (languages.Count == 0)
			{
				ClearLanguages ();
			}
			else
			{
				if (languages.Count > 1)
				{
					ignoreOriginalText = EditorGUILayout.Toggle ("Disallow original language?", ignoreOriginalText);

					for (int i=1; i<languages.Count; i++)
					{
						EditorGUILayout.BeginHorizontal ();
						languages[i] = EditorGUILayout.TextField (languages[i]);
						
						if (GUILayout.Button ("Import translation", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(110f)))
						{
							ImportTranslation (i);
						}
						
						if (GUILayout.Button ("Export translation", EditorStyles.miniButtonRight, GUILayout.MaxWidth(110f)))
						{
							ExportTranslation (i);
						}
						
						if (GUILayout.Button (deleteContent, EditorStyles.miniButton, GUILayout.MaxWidth(20f)))
						{
							Undo.RecordObject (this, "Delete translation: " + languages[i]);
							DeleteLanguage (i);
						}
						EditorGUILayout.EndHorizontal ();
					}
				}
				
				if (GUILayout.Button ("Create new translation"))
				{
					Undo.RecordObject (this, "Add translation");
					CreateLanguage ("New " + languages.Count.ToString ());
				}
			}
		}

		
		private void CreateLanguage (string name)
		{
			languages.Add (name);
			
			foreach (SpeechLine line in lines)
			{
				line.translationText.Add (line.text);
			}
		}
		
		
		private void DeleteLanguage (int i)
		{
			languages.RemoveAt (i);
			
			foreach (SpeechLine line in lines)
			{
				line.translationText.RemoveAt (i-1);
			}
			
		}
		
		
		private void ClearLanguages ()
		{
			languages.Clear ();
			
			foreach (SpeechLine line in lines)
			{
				line.translationText.Clear ();
			}
			
			languages.Add ("Original");	
			
		}


		private void PopulateList ()
		{
			string originalScene = EditorApplication.currentScene;
			
			if (EditorApplication.SaveCurrentSceneIfUserWantsTo ())
			{
				Undo.RecordObject (this, "Update speech list");
				
				// Store the lines temporarily, so that we can update the translations afterwards
				BackupTranslations ();
				
				lines.Clear ();
				
				sceneFiles = AdvGame.GetSceneFiles ();
				
				// First look for lines that already have an assigned lineID
				foreach (string sceneFile in sceneFiles)
				{
					GetLinesInScene (sceneFile, false);
				}
				
				GetLinesFromInventory (false);
				GetLinesFromCursors (false);
				GetLinesFromMenus (false);
				
				// Now look for new lines, which don't have a unique lineID
				foreach (string sceneFile in sceneFiles)
				{
					GetLinesInScene (sceneFile, true);
				}
				
				GetLinesFromInventory (true);
				GetLinesFromCursors (true);
				GetLinesFromMenus (true);
				
				RestoreTranslations ();
				
				if (EditorApplication.currentScene != originalScene)
				{
					EditorApplication.OpenScene (originalScene);
				}
			}
		}


		private string RemoveLineBreaks (string text)
		{
			return (text.Replace ("\n", "[break]"));
		}
		
		
		private string AddLineBreaks (string text)
		{
			return (text.Replace ("[break]", "\n"));
		}
		

		private void ExtractHotspot (Hotspot hotspot, bool onlySeekNew)
		{
			if (hotspot.interactionSource == ActionListSource.AssetFile)
			{
				if (hotspot.useButton.IsButtonModified () && hotspot.useButton.assetFile)
				{
					ProcessActionList (hotspot.useButton.assetFile.actions, onlySeekNew, true);
					EditorUtility.SetDirty (hotspot.useButton.assetFile);
				}

				if (hotspot.lookButton.assetFile)
				{
					ProcessActionList (hotspot.lookButton.assetFile.actions, onlySeekNew, true);
					EditorUtility.SetDirty (hotspot.lookButton.assetFile);
				}

				foreach (Button _button in hotspot.useButtons)
				{
					if (_button.assetFile)
					{
						ProcessActionList (_button.assetFile.actions, onlySeekNew, true);
						EditorUtility.SetDirty (_button.assetFile);
					}
				}

				foreach (Button _button in hotspot.invButtons)
				{
					if (_button.assetFile)
					{
						ProcessActionList (_button.assetFile.actions, onlySeekNew, true);
						EditorUtility.SetDirty (_button.assetFile);
					}
				}
			}

			string hotspotName = hotspot.name;
			if (hotspot.hotspotName != "")
			{
				hotspotName = hotspot.hotspotName;
			}

			if (onlySeekNew && hotspot.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, hotspotName, languages.Count - 1, AC_TextType.Hotspot);
				hotspot.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && hotspot.lineID > -1)
			{
				// Already has an ID, so don't replace
				lines.Add (new SpeechLine (hotspot.lineID, EditorApplication.currentScene, hotspotName, languages.Count - 1, AC_TextType.Hotspot));
			}
		}
		
		
		private void ExtractDialogOption (ButtonDialog dialogOption, bool onlySeekNew)
		{
			if (dialogOption.assetFile != null)
			{
				ProcessActionList (dialogOption.assetFile.actions, onlySeekNew, true);
				EditorUtility.SetDirty (dialogOption.assetFile);
			}

			if (onlySeekNew && dialogOption.lineID < 1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, dialogOption.label, languages.Count - 1, AC_TextType.DialogueOption);
				dialogOption.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && dialogOption.lineID > 0)
			{
				// Already has an ID, so don't replace
				lines.Add (new SpeechLine (dialogOption.lineID, EditorApplication.currentScene, dialogOption.label, languages.Count - 1, AC_TextType.DialogueOption));
			}
		}


		private void ExtractInventory (InvItem invItem, bool onlySeekNew)
		{
			if (onlySeekNew && invItem.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				if (invItem.altLabel != "")
				{
					newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, invItem.altLabel, languages.Count - 1, AC_TextType.InventoryItem);
				}
				else
				{
					newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, invItem.label, languages.Count - 1, AC_TextType.InventoryItem);
				}
				invItem.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && invItem.lineID > -1)
			{
				// Already has an ID, so don't replace
				if (invItem.altLabel != "")
				{
					lines.Add (new SpeechLine (invItem.lineID, EditorApplication.currentScene, invItem.altLabel, languages.Count - 1, AC_TextType.InventoryItem));
				}
				else
				{
					lines.Add (new SpeechLine (invItem.lineID, EditorApplication.currentScene, invItem.label, languages.Count - 1, AC_TextType.InventoryItem));
				}
			}
		}


		private void ExtractPrefix (HotspotPrefix prefix, bool onlySeekNew)
		{
			if (onlySeekNew && prefix.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, prefix.label, languages.Count - 1, AC_TextType.HotspotPrefix);
				prefix.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && prefix.lineID > -1)
			{
				// Already has an ID, so don't replace
				lines.Add (new SpeechLine (prefix.lineID, EditorApplication.currentScene, prefix.label, languages.Count - 1, AC_TextType.HotspotPrefix));
			}
		}


		private void ExtractIcon (CursorIcon icon, bool onlySeekNew)
		{
			if (onlySeekNew && icon.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetIDArray(), "", icon.label, languages.Count - 1, AC_TextType.CursorIcon);
				icon.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && icon.lineID > -1)
			{
				// Already has an ID, so don't replace
				lines.Add (new SpeechLine (icon.lineID, "", icon.label, languages.Count - 1, AC_TextType.CursorIcon));
			}
		}


		private void ExtractElement (MenuElement element, string elementLabel, bool onlySeekNew)
		{
			if (onlySeekNew && element.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetIDArray(), "", element.title, elementLabel, languages.Count - 1, AC_TextType.MenuElement);
				element.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && element.lineID > -1)
			{
				// Already has an ID, so don't replace
				lines.Add (new SpeechLine (element.lineID, "", element.title, elementLabel, languages.Count - 1, AC_TextType.MenuElement));
			}
		}


		private void ExtractHotspotOverride (MenuButton button, string hotspotLabel, bool onlySeekNew)
		{
			if (hotspotLabel == "")
			{
				button.hotspotLabelID = -1;
				return;
			}

			if (onlySeekNew && button.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetIDArray(), "", button.title, hotspotLabel, languages.Count - 1, AC_TextType.MenuElement);
				button.hotspotLabelID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && button.hotspotLabelID > -1)
			{
				// Already has an ID, so don't replace
				lines.Add (new SpeechLine (button.hotspotLabelID, "", button.title, hotspotLabel, languages.Count - 1, AC_TextType.MenuElement));
			}
		}


		private void ExtractJournalElement (MenuJournal journal, List<JournalPage> pages, bool onlySeekNew)
		{
			foreach (JournalPage page in pages)
			{
				if (onlySeekNew && page.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine;
					newLine = new SpeechLine (GetIDArray(), "", journal.title, page.text, languages.Count - 1, AC_TextType.JournalEntry);
					page.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && page.lineID > -1)
				{
					// Already has an ID, so don't replace
					lines.Add (new SpeechLine (page.lineID, "", journal.title, page.text, languages.Count - 1, AC_TextType.JournalEntry));
				}
			}
		}


		private void ExtractSpeech (ActionSpeech action, bool onlySeekNew, bool isInScene)
		{
			string speaker = "";
			
			if (action.isPlayer)
			{
				speaker = "Player";
			}
			else if (action.speaker)
			{
				speaker = action.speaker.name;
			}
			else
			{
				speaker = "Narrator";
			}
			
			if (speaker != "" && action.messageText != "")
			{
				if (onlySeekNew && action.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine;
					if (isInScene)
					{
						newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, speaker, action.messageText, languages.Count - 1, AC_TextType.Speech);
					}
					else
					{
						newLine = new SpeechLine (GetIDArray(), "", speaker, action.messageText, languages.Count - 1, AC_TextType.Speech);
					}
					action.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && action.lineID > -1)
				{
					// Already has an ID, so don't replace
					if (isInScene)
					{
						lines.Add (new SpeechLine (action.lineID, EditorApplication.currentScene, speaker, action.messageText, languages.Count - 1, AC_TextType.Speech));
					}
					else
					{
						lines.Add (new SpeechLine (action.lineID, "", speaker, action.messageText, languages.Count - 1, AC_TextType.Speech));
					}
				}
			}
			else
			{
				// Remove from SpeechManager
				action.lineID = -1;
			}
		}


		private void ExtractJournalEntry (ActionMenuState action, bool onlySeekNew, bool isInScene)
		{
			if (action.changeType == ActionMenuState.MenuChangeType.AddJournalPage && action.journalText != "")
			{
				if (onlySeekNew && action.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine;
					if (isInScene)
					{
						newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					else
					{
						newLine = new SpeechLine (GetIDArray(), "", action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					action.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && action.lineID > -1)
				{
					// Already has an ID, so don't replace
					if (isInScene)
					{
						lines.Add (new SpeechLine (action.lineID, EditorApplication.currentScene, action.journalText, languages.Count - 1, AC_TextType.JournalEntry));
					}
					else
					{
						lines.Add (new SpeechLine (action.lineID, "", action.journalText, languages.Count - 1, AC_TextType.JournalEntry));
					}
				}
			}
			else
			{
				// Remove from SpeechManager
				action.lineID = -1;
			}
		}


		private void GetLinesFromInventory (bool onlySeekNew)
		{
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
			
			if (inventoryManager)
			{
				// Unhandled combine
				if (inventoryManager.unhandledCombine != null)
				{
					ProcessActionList (inventoryManager.unhandledCombine.actions, onlySeekNew, false);
					EditorUtility.SetDirty (inventoryManager.unhandledCombine);
				}
				
				// Unhandled hotspot
				if (inventoryManager.unhandledHotspot != null)
				{
					ProcessActionList (inventoryManager.unhandledHotspot.actions, onlySeekNew, false);
					EditorUtility.SetDirty (inventoryManager.unhandledHotspot);
				}
				
				// Item-specific events
				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem item in inventoryManager.items)
					{
						// Label
						ExtractInventory (item, onlySeekNew);
						
						// Use
						if (item.useActionList != null)
						{
							ProcessActionList (item.useActionList.actions, onlySeekNew, false);
							EditorUtility.SetDirty (item.useActionList);
						}
						
						// Look
						if (item.lookActionList)
						{
							ProcessActionList (item.lookActionList.actions, onlySeekNew, false);
							EditorUtility.SetDirty (item.lookActionList);
						}

						// Unhandled
						if (item.unhandledActionList)
						{
							ProcessActionList (item.unhandledActionList.actions, onlySeekNew, false);
							EditorUtility.SetDirty (item.unhandledActionList);
						}
						if (item.unhandledCombineActionList)
						{
							ProcessActionList (item.unhandledCombineActionList.actions, onlySeekNew, false);
							EditorUtility.SetDirty (item.unhandledCombineActionList);
						}
						
						// Combines
						foreach (ActionListAsset actionList in item.combineActionList)
						{
							if (actionList != null)
							{
								ProcessActionList (actionList.actions, onlySeekNew, false);
								EditorUtility.SetDirty (actionList);
							}
						}
					}
				}

				EditorUtility.SetDirty (inventoryManager);
			}
		}


		private void GetLinesFromMenus (bool onlySeekNew)
		{
			MenuManager menuManager = AdvGame.GetReferences ().menuManager;

			if (menuManager)
			{
				// Gather elements
				if (menuManager.menus.Count > 0)
				{
					foreach (Menu menu in menuManager.menus)
					{
						foreach (MenuElement element in menu.elements)
						{
							if (element is MenuButton)
							{
								MenuButton button = (MenuButton) element;
								ExtractElement (element, button.label, onlySeekNew);
								ExtractHotspotOverride (button, button.hotspotLabel, onlySeekNew);

								if (button.buttonClickType == AC_ButtonClickType.RunActionList && button.actionList != null)
								{
									ProcessActionList (button.actionList.actions, onlySeekNew, false);
									EditorUtility.SetDirty (button.actionList);
								}
							}
							else if (element is MenuCycle)
							{
								MenuCycle button = (MenuCycle) element;
								ExtractElement (element, button.label, onlySeekNew);
							}
							else if (element is MenuDrag)
							{
								MenuDrag button = (MenuDrag) element;
								ExtractElement (element, button.label, onlySeekNew);
							}
							else if (element is MenuInput)
							{
								MenuInput button = (MenuInput) element;
								ExtractElement (element, button.label, onlySeekNew);
							}
							else if (element is MenuLabel)
							{
								MenuLabel button = (MenuLabel) element;
								ExtractElement (element, button.label, onlySeekNew);
							}
							else if (element is MenuSavesList)
							{
								MenuSavesList button = (MenuSavesList) element;
								if (button.saveListType == AC_SaveListType.Save)
								{
									ExtractElement (element, button.newSaveText, onlySeekNew);
								}
							}
							else if (element is MenuSlider)
							{
								MenuSlider button = (MenuSlider) element;
								ExtractElement (element, button.label, onlySeekNew);
							}
							else if (element is MenuToggle)
							{
								MenuToggle button = (MenuToggle) element;
								ExtractElement (element, button.label, onlySeekNew);
							}
							else if (element is MenuJournal)
							{
								MenuJournal journal = (MenuJournal) element;
								ExtractJournalElement (journal, journal.pages, onlySeekNew);
							}
						}
					}
				}

				EditorUtility.SetDirty (menuManager);
			}
		}


		private void GetLinesFromCursors (bool onlySeekNew)
		{
			CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
			
			if (cursorManager)
			{
				// Prefixes
				ExtractPrefix (cursorManager.hotspotPrefix1, onlySeekNew);
				ExtractPrefix (cursorManager.hotspotPrefix2, onlySeekNew);

				// Gather icons
				if (cursorManager.cursorIcons.Count > 0)
				{
					foreach (CursorIcon icon in cursorManager.cursorIcons)
					{
						ExtractIcon (icon, onlySeekNew);
					}
				}

				EditorUtility.SetDirty (cursorManager);
			}
		}
		
		
		private void GetLinesInScene (string sceneFile, bool onlySeekNew)
		{
			if (EditorApplication.currentScene != sceneFile)
			{
				EditorApplication.OpenScene (sceneFile);
			}
			
			// Speech lines and journal entries
			ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
			foreach (ActionList list in actionLists)
			{
				if (list.source == ActionListSource.AssetFile)
				{
					if (list.assetFile != null)
					{
						ProcessActionList (list.assetFile.actions, onlySeekNew, false);
						EditorUtility.SetDirty (list.assetFile);
					}
				}
				else
				{
					ProcessActionList (list.actions, onlySeekNew, true);
					EditorUtility.SetDirty (list);
				}
			}

			// Hotspots
			Hotspot[] hotspots = GameObject.FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			foreach (Hotspot hotspot in hotspots)
			{
				ExtractHotspot (hotspot, onlySeekNew);
				EditorUtility.SetDirty (hotspot);
			}
			
			// Dialogue options
			Conversation[] conversations = GameObject.FindObjectsOfType (typeof (Conversation)) as Conversation[];
			foreach (Conversation conversation in conversations)
			{
				foreach (ButtonDialog dialogOption in conversation.options)
				{
					ExtractDialogOption (dialogOption, onlySeekNew);
				}
				EditorUtility.SetDirty (conversation);
			}
			
			// Save the scene
			EditorApplication.SaveScene ();
			EditorUtility.SetDirty (this);
		}
		
		
		private int[] GetIDArray ()
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (SpeechLine line in lines)
			{
				idArray.Add (line.lineID);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		private void RestoreTranslations ()
		{
			// Match IDs for each entry in lines and tempLines, send over translation data
			foreach (SpeechLine tempLine in tempLines)
			{
				foreach (SpeechLine line in lines)
				{
					if (tempLine.lineID == line.lineID)
					{
						line.translationText = tempLine.translationText;
						break;
					}
				}
			}
			
			tempLines = null;
		}
		
		
		private void BackupTranslations ()
		{
			tempLines = new List<SpeechLine>();
			foreach (SpeechLine line in lines)
			{
				tempLines.Add (line);
			}
		}


		private void ImportTranslation (int i)
		{
			string fileName = EditorUtility.OpenFilePanel ("Import " + languages[i] + " translation", "Assets", "csv");
			if (fileName.Length == 0)
			{
				return;
			}

			if (File.Exists (fileName))
			{
				string csvText = Serializer.LoadSaveFile (fileName);
				string [,] csvOutput = CSVReader.SplitCsvGrid (csvText);

				int lineID = 0;
				string translationText = "";
				string owner = "";

				for (int y = 1; y < csvOutput.Length; y++)
				{
					try
					{
						lineID = int.Parse (csvOutput [0, y]);
						translationText = csvOutput [3, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);
						string typeText = csvOutput [1, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);

						if (typeText.Contains ("JournalEntry (Page "))
						{
							owner = typeText.Replace ("JournalEntry (", "");
							owner = owner.Replace (")", "");
						}
						else
						{
							owner = "";
						}

						UpdateTranslation (i, lineID, owner, AddLineBreaks (translationText));
					}
					catch
					{}
				}

				EditorUtility.SetDirty (this);
			}
			else
			{
				Debug.LogWarning ("No CSV file found.  Looking for: " + fileName);
			}
		}


		private void UpdateTranslation (int i, int _lineID, string _owner, string translationText)
		{
			foreach (SpeechLine line in lines)
			{
				if (line.lineID == _lineID)
				{
					line.translationText [i-1] = translationText;
				}
			}
		}


		private void ImportGameText ()
		{
			string fileName = EditorUtility.OpenFilePanel ("Import game text", "Assets", "csv");
			if (fileName.Length == 0)
			{
				return;
			}
			
			if (File.Exists (fileName))
			{
				string csvText = Serializer.LoadSaveFile (fileName);
				string [,] csvOutput = CSVReader.SplitCsvGrid (csvText);
				
				int lineID = 0;
				string translationText = "";
				string owner = "";
				
				for (int y = 1; y < csvOutput.Length; y++)
				{
					try
					{
						lineID = int.Parse (csvOutput [0, y]);
						string typeText = csvOutput [1, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);
						
						if (typeText.Contains ("JournalEntry (Page "))
						{
							owner = typeText.Replace ("JournalEntry (", "");
							owner = owner.Replace (")", "");
						}
						else
						{
							owner = "";
						}

						if (languages.Count > 1)
						{
							for (int i=1; i<languages.Count; i++)
							{
								translationText = csvOutput [i+2, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);
								UpdateTranslation (i, lineID, owner, AddLineBreaks (translationText));
							}
						}
					}
					catch
					{}
				}
				
				EditorUtility.SetDirty (this);
			}
			else
			{
				Debug.LogWarning ("No CSV file found.  Looking for: " + fileName);
			}
		}


		private void ExportGameText ()
		{
			#if UNITY_WEBPLAYER
			Debug.LogWarning ("Game text cannot be exported in WebPlayer mode - please switch platform and try again.");
			return;
			#else

			string suggestedFilename = "";
			if (AdvGame.GetReferences ().settingsManager)
			{
				suggestedFilename = AdvGame.GetReferences ().settingsManager.saveFileName + " - ";
			}
			suggestedFilename += "AllGameText.csv";

			string fileName = EditorUtility.SaveFilePanel ("Export game text", "Assets", suggestedFilename, "csv");
			if (fileName.Length == 0)
			{
				return;
			}
			
			bool fail = false;
			List<string[]> output = new List<string[]>();

			List<string> headerList = new List<string>();
			headerList.AddRange (new [] { "ID", "Type", "Original text" });
			if (languages.Count > 1)
			{
				for (int i=1; i<languages.Count; i++)
				{
					headerList.Add (languages [i]);
				}
			}
			output.Add (headerList.ToArray ());
			
			foreach (SpeechLine line in lines)
			{
				List<string> rowList = new List<string>();
				rowList.AddRange (new [] { 
					line.lineID.ToString (), 
					line.GetInfo (),
					RemoveLineBreaks (line.text)
				});

				foreach (string translationText in line.translationText)
				{
					rowList.Add (RemoveLineBreaks (translationText));

					if (translationText.Contains (CSVReader.csvDelimiter))
					{
						fail = true;
					}
				}

				output.Add (rowList.ToArray ());

				if (line.textType != AC_TextType.JournalEntry && line.text.Contains (CSVReader.csvDelimiter))
				{
					fail = true;
				}

				if (fail)
				{
					Debug.LogError ("Cannot export translation since line " + line.lineID.ToString () + " contains the character '" + CSVReader.csvDelimiter + "'.");
				}
			}
			
			if (!fail)
			{
				int length = output.Count;
				
				StringBuilder sb = new StringBuilder();
				for (int j=0; j<length; j++)
				{
					sb.AppendLine (string.Join (CSVReader.csvDelimiter, output[j]));
				}
				
				Serializer.CreateSaveFile (fileName, sb.ToString ());
			}

			#endif
		}


		private void ExportTranslation (int i)
		{
			#if UNITY_WEBPLAYER
			Debug.LogWarning ("Game text cannot be exported in WebPlayer mode - please switch platform and try again.");
			return;
			#else

			string suggestedFilename = "";
			if (AdvGame.GetReferences ().settingsManager)
			{
				suggestedFilename = AdvGame.GetReferences ().settingsManager.saveFileName + " - ";
			}
			suggestedFilename += languages[i].ToString () + ".csv";

			string fileName = EditorUtility.SaveFilePanel ("Export " + languages[i] + " translation", "Assets", suggestedFilename, "csv");
			if (fileName.Length == 0)
			{
				return;
			}

			bool fail = false;
			List<string[]> output = new List<string[]>();
			output.Add (new string[] {"ID", "Type", "Original line", languages[i] + " translation"});

			foreach (SpeechLine line in lines)
			{
				output.Add (new string[] 
				{
					line.lineID.ToString (),
					line.GetInfo (),
					RemoveLineBreaks (line.text),
					RemoveLineBreaks (line.translationText [i-1])
				});

				if (line.textType != AC_TextType.JournalEntry && (line.text.Contains (CSVReader.csvDelimiter) || line.translationText [i-1].Contains (CSVReader.csvDelimiter)))
				{
					fail = true;
					Debug.LogError ("Cannot export translation since line " + line.lineID.ToString () + " contains the character '" + CSVReader.csvDelimiter + "'.");
				}
			}

			if (!fail)
			{
				int length = output.Count;

				StringBuilder sb = new StringBuilder();
				for (int j=0; j<length; j++)
				{
					sb.AppendLine (string.Join (CSVReader.csvDelimiter, output[j]));
				}

				Serializer.CreateSaveFile (fileName, sb.ToString ());
			}

			#endif
		}


		private void CreateScript ()
		{
			#if UNITY_WEBPLAYER
			Debug.LogWarning ("Game text cannot be exported in WebPlayer mode - please switch platform and try again.");
			#else

			string suggestedFilename = "";
			if (AdvGame.GetReferences ().settingsManager)
			{
				suggestedFilename = AdvGame.GetReferences ().settingsManager.saveFileName + " - ";
			}
			suggestedFilename += "script.txt";

			string fileName = EditorUtility.SaveFilePanel ("Save script file", "Assets", suggestedFilename, "txt");
			if (fileName.Length == 0)
			{
				return;
			}

			string script = "Script file created " + DateTime.UtcNow.ToString("HH:mm dd MMMM, yyyy");

			// By scene
			foreach (string scene in sceneFiles)
			{
				bool foundLinesInScene = false;
				
				foreach (SpeechLine line in lines)
				{
					if (line.scene == scene && line.textType == AC_TextType.Speech)
					{
						if (!foundLinesInScene)
						{
							script += "\n";
							script += "\n";
							script += "Scene: " + scene;
							foundLinesInScene = true;
						}
						
						script += "\n";
						script += "\n";
						script += line.Print ();
					}
				}
			}
			
			// No scene
			bool foundLinesInInventory = false;
			
			foreach (SpeechLine line in lines)
			{
				if (line.scene == "" && line.textType == AC_TextType.Speech)
				{
					if (!foundLinesInInventory)
					{
						script += "\n";
						script += "\n";
						script += "Scene-independent lines: ";
						foundLinesInInventory = true;
					}
					
					script += "\n";
					script += "\n";
					script += line.Print ();
				}
			}
			
			Serializer.CreateSaveFile (fileName, script);

			#endif
		}
		

		private void ClearList ()
		{
			if (EditorUtility.DisplayDialog ("Reset all translation lines?", "This will completely reset the IDs of every text line in your game, removing any supplied translations and invalidaing speech audio filenames. Continue?", "OK", "Cancel"))
			{
				string originalScene = EditorApplication.currentScene;
				
				if (EditorApplication.SaveCurrentSceneIfUserWantsTo ())
				{
					lines.Clear ();
					
					sceneFiles = AdvGame.GetSceneFiles ();
					
					// First look for lines that already have an assigned lineID
					foreach (string sceneFile in sceneFiles)
					{
						ClearLinesInScene (sceneFile);
					}
					
					ClearLinesFromInventory ();
					ClearLinesFromCursors ();
					ClearLinesFromMenus ();
					
					if (EditorApplication.currentScene != originalScene)
					{
						EditorApplication.OpenScene (originalScene);
					}
				}
			}
		}


		private void ClearLinesInScene (string sceneFile)
		{
			if (EditorApplication.currentScene != sceneFile)
			{
				EditorApplication.OpenScene (sceneFile);
			}
			
			// Speech lines and journal entries
			ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
			foreach (ActionList list in actionLists)
			{
				if (list.source == ActionListSource.AssetFile)
				{
					if (list.assetFile != null)
					{
						ClearLinesFromActionList (list.assetFile.actions);
						EditorUtility.SetDirty (list.assetFile);
					}
				}
				else
				{
					ClearLinesFromActionList (list.actions);
					EditorUtility.SetDirty (list);
				}
			}
			
			// Hotspots
			Hotspot[] hotspots = GameObject.FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			foreach (Hotspot hotspot in hotspots)
			{
				if (hotspot.useButton.IsButtonModified () && hotspot.useButton.assetFile)
				{
					ClearLinesFromActionList (hotspot.useButton.assetFile.actions);
					EditorUtility.SetDirty (hotspot.useButton.assetFile);
				}

				if (hotspot.lookButton.assetFile)
				{
					ClearLinesFromActionList (hotspot.lookButton.assetFile.actions);
					EditorUtility.SetDirty (hotspot.lookButton.assetFile);
				}

				foreach (Button _button in hotspot.useButtons)
				{
					if (_button.assetFile)
					{
						ClearLinesFromActionList (_button.assetFile.actions);
						EditorUtility.SetDirty (_button.assetFile);
					}
				}

				foreach (Button _button in hotspot.invButtons)
				{
					if (_button.assetFile)
					{
						ClearLinesFromActionList (_button.assetFile.actions);
						EditorUtility.SetDirty (_button.assetFile);
					}
				}

				hotspot.lineID = -1;
				EditorUtility.SetDirty (hotspot);
			}
			
			// Dialogue options
			Conversation[] conversations = GameObject.FindObjectsOfType (typeof (Conversation)) as Conversation[];
			foreach (Conversation conversation in conversations)
			{
				foreach (ButtonDialog dialogOption in conversation.options)
				{
					if (dialogOption.assetFile)
					{
						ClearLinesFromActionList (dialogOption.assetFile.actions);
						EditorUtility.SetDirty (dialogOption.assetFile);
					}
					dialogOption.lineID = -1;
				}
				EditorUtility.SetDirty (conversation);
			}
			
			// Save the scene
			EditorApplication.SaveScene ();
			EditorUtility.SetDirty (this);
		}


		private void ClearLinesFromActionList (List<Action> actions)
		{
			if (actions == null)
			{
				return;
			}

			foreach (Action action in actions)
			{
				if (action is ActionSpeech)
				{
					ActionSpeech actionSpeech = (ActionSpeech) action;
					actionSpeech.lineID = -1;
				}
				else if (action is ActionMenuState)
				{
					ActionMenuState actionMenuState = (ActionMenuState) action;
					actionMenuState.lineID = -1;
				}
				else if (action is ActionRunActionList)
				{
					ActionRunActionList runActionList = (ActionRunActionList) action;
					if (runActionList.listSource == ActionRunActionList.ListSource.AssetFile && runActionList.invActionList != null)
					{
						ClearLinesFromActionList (runActionList.invActionList.actions);
						EditorUtility.SetDirty (runActionList.invActionList);
					}
				}
			}

		}


		private void ClearLinesFromInventory ()
		{
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
			
			if (inventoryManager)
			{
				if (inventoryManager.unhandledCombine)
				{
					ClearLinesFromActionList (inventoryManager.unhandledCombine.actions);
					EditorUtility.SetDirty (inventoryManager.unhandledCombine);
				}

				if (inventoryManager.unhandledHotspot)
				{
					ClearLinesFromActionList (inventoryManager.unhandledHotspot.actions);
					EditorUtility.SetDirty (inventoryManager.unhandledHotspot);
				}

				// Item-specific events
				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem item in inventoryManager.items)
					{
						// Label
						item.lineID = -1;

						if (item.useActionList)
						{
							ClearLinesFromActionList (item.useActionList.actions);
							EditorUtility.SetDirty (item.useActionList);
						}
						if (item.lookActionList)
						{
							ClearLinesFromActionList (item.lookActionList.actions);
							EditorUtility.SetDirty (item.lookActionList);
						}

						foreach (ActionListAsset actionList in item.combineActionList)
						{
							ClearLinesFromActionList (actionList.actions);
							EditorUtility.SetDirty (actionList);
						}
					}
				}
				
				EditorUtility.SetDirty (inventoryManager);
			}
		}


		private void ClearLinesFromCursors ()
		{
			CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
			
			if (cursorManager)
			{
				// Prefixes
				cursorManager.hotspotPrefix1.lineID = -1;
				cursorManager.hotspotPrefix2.lineID = -1;

				// Gather icons
				if (cursorManager.cursorIcons.Count > 0)
				{
					foreach (CursorIcon icon in cursorManager.cursorIcons)
					{
						icon.lineID = -1;
					}
				}
				
				EditorUtility.SetDirty (cursorManager);
			}
		}


		private void ClearLinesFromMenus ()
		{
			MenuManager menuManager = AdvGame.GetReferences ().menuManager;
			
			if (menuManager)
			{
				// Gather elements
				if (menuManager.menus.Count > 0)
				{
					foreach (Menu menu in menuManager.menus)
					{
						foreach (MenuElement element in menu.elements)
						{
							if (element is MenuButton)
							{
								MenuButton button = (MenuButton) element;
								button.lineID = -1;
								button.hotspotLabelID = -1;

								if (button.buttonClickType == AC_ButtonClickType.RunActionList && button.actionList != null)
								{
									ClearLinesFromActionList (button.actionList.actions);
									EditorUtility.SetDirty (button.actionList);
								}
							}
							else if (element is MenuCycle)
							{
								MenuCycle button = (MenuCycle) element;
								button.lineID = -1;
							}
							else if (element is MenuDrag)
							{
								MenuDrag button = (MenuDrag) element;
								button.lineID = -1;
							}
							else if (element is MenuInput)
							{
								MenuInput button = (MenuInput) element;
								button.lineID = -1;
							}
							else if (element is MenuLabel)
							{
								MenuLabel button = (MenuLabel) element;
								button.lineID = -1;
							}
							else if (element is MenuSavesList)
							{
								MenuSavesList button = (MenuSavesList) element;
								button.lineID = -1;
							}
							else if (element is MenuSlider)
							{
								MenuSlider button = (MenuSlider) element;
								button.lineID = -1;
							}
							else if (element is MenuToggle)
							{
								MenuToggle button = (MenuToggle) element;
								button.lineID = -1;
							}
							else if (element is MenuJournal)
							{
								MenuJournal journal = (MenuJournal) element;
								journal.lineID = -1;
							}
						}
					}
				}
				
				EditorUtility.SetDirty (menuManager);
			}		
		}


		private void ProcessActionList (List<Action> actions, bool onlySeekNew, bool isInScene)
		{
			foreach (Action action in actions)
			{
				if (action is ActionSpeech)
				{
					ExtractSpeech (action as ActionSpeech, onlySeekNew, isInScene);
				}
				else if (action is ActionMenuState)
				{
					ExtractJournalEntry (action as ActionMenuState, onlySeekNew, isInScene);
				}
				else if (action is ActionRunActionList)
				{
					ActionRunActionList runActionList = (ActionRunActionList) action;
					if (runActionList.listSource == ActionRunActionList.ListSource.AssetFile && runActionList.invActionList != null)
					{
						ProcessActionList (runActionList.invActionList.actions, onlySeekNew, false);
						EditorUtility.SetDirty (runActionList.invActionList);
					}
				}
			}
		}


		#endif
		
		
		public static string GetTranslation (int _lineID, int i)
		{
			SpeechManager speechManager = AdvGame.GetReferences ().speechManager;
			
			if (_lineID == -1)
			{
				Debug.Log ("Cannot find translation because the text has not been added to the Speech Manager.");
			}
			else
			{
				foreach (SpeechLine line in speechManager.lines)
				{
					if (line.lineID == _lineID)
					{
						if (line.translationText.Count > (i-1))
						{
							return line.translationText [i-1];
						}
						else
						{
							Debug.LogWarning ("A translation is being requested that does not exist!");
						}
					}
				}
			}
			return "";
		}

	}

}