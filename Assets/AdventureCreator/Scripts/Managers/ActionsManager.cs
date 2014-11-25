/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionsManager.cs"
 * 
 *	This script handles the "Actions" tab of the main wizard.
 *	Custom actions can be added and removed by selecting them with this.
 * 
 */

using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionsManager : ScriptableObject
	{

		#if UNITY_EDITOR && !UNITY_WEBPLAYER
		public string customFolderPath = "AdventureCreator/Scripts/Actions";
		private string folderPath = "AdventureCreator/Scripts/Actions";
		#endif

		public int defaultClass;
		
		public List<ActionType> AllActions = new List<ActionType>();
		public List<ActionType> EnabledActions = new List<ActionType>();
		
		//private Vector2 scrollPos;
		private string[] categories = { "Camera", "Character", "Container", "Dialogue", "Engine", "Hotspot", "Input", "Inventory", "Menu", "Moveable", "Object", "Player", "Third-Party", "Variable", "Custom" };
		
		
		public string GetDefaultAction ()
		{
			if (EnabledActions.Count > 0 && EnabledActions.Count > defaultClass)
			{
				return EnabledActions[defaultClass].fileName;
			}
			
			return "";
		}
		
		
		#if UNITY_EDITOR
		
		private static GUILayoutOption
			titleWidth = GUILayout.MaxWidth (160),
			nameWidth = GUILayout.MaxWidth (130);
		
		
		public void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
			GUILayout.Label ("Custom Action scripts", EditorStyles.boldLabel);
			#if UNITY_WEBPLAYER
			EditorGUILayout.HelpBox ("Locating Actions requires a non-WebPlayer platform.  Please switch platform and try again.", MessageType.Warning);
			GUILayout.Space (10);
			#else
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Folder to search:", GUILayout.Width (110f));
			GUILayout.Label (customFolderPath, EditorStyles.textField);
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Set directory"))
			{
				string path = EditorUtility.OpenFolderPanel("Set custom Actions directory", "Assets", "");
				string dataPath = Application.dataPath;
				if (path.Contains (dataPath))
				{
					if (path == dataPath)
					{
						customFolderPath = "";
					}
					else
					{
						customFolderPath = path.Replace (dataPath + "/", "");
					}
				}
				else
				{
					Debug.LogError ("Cannot set new directory - be sure to select within the Assets directory.");
				}
			}
			if (GUILayout.Button ("Refresh list"))
			{
				RefreshList ();
			}
			GUILayout.EndHorizontal ();
			#endif
			EditorGUILayout.EndVertical ();
			
			if (AllActions.Count > 0)
			{
				GUILayout.Space (10);

				EditorGUILayout.BeginVertical ("Button");
				defaultClass = EditorGUILayout.Popup ("Default action:", defaultClass, GetDefaultPopUp ());
				GUILayout.Space (10);
				
				GUILayout.BeginHorizontal ();
				
				GUILayout.Label ("Title", EditorStyles.boldLabel, titleWidth);
				GUILayout.Label ("Filename", EditorStyles.boldLabel, nameWidth);
				GUILayout.Label ("Enabled?", EditorStyles.boldLabel, GUILayout.MaxWidth (50));
				
				GUILayout.EndHorizontal ();
				
				foreach (ActionType subclass in AllActions)
				{
					GUILayout.BeginHorizontal ();
					
					GUILayout.Label (subclass.title, titleWidth);
					GUILayout.Label (subclass.fileName, nameWidth);
					
					subclass.isEnabled = GUILayout.Toggle (subclass.isEnabled, "");
					
					GUILayout.EndHorizontal();
					GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				}
				EditorGUILayout.EndVertical ();
				
				SetEnabled ();
				
				if (defaultClass > EnabledActions.Count - 1)
				{
					defaultClass = EnabledActions.Count - 1;
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("No Action subclass files found.", MessageType.Warning);
			}
			
			EditorUtility.SetDirty (this);
		}
		
		
		public void RefreshList ()
		{
			#if !UNITY_WEBPLAYER
			
			Undo.RecordObject (this, "Refresh list");
			
			// Load default Actions
			DirectoryInfo dir = new DirectoryInfo ("Assets/" + folderPath);
			FileInfo[] info = dir.GetFiles ("*.cs");

			AllActions.Clear ();
			
			foreach (FileInfo f in info) 
			{
				int extentionPosition = f.Name.IndexOf (".cs");
				string className = f.Name.Substring (0, extentionPosition);
				Action tempAction = (Action) CreateInstance (className);
				string title = tempAction.title;
				AllActions.Add (new ActionType (className, title));
			}
			
			// Load custom Actions
			if (customFolderPath != folderPath)
			{
				dir = new DirectoryInfo ("Assets/" + customFolderPath);
				info = dir.GetFiles ("*.cs");
				
				foreach (FileInfo f in info) 
				{
					try
					{
						int extentionPosition = f.Name.IndexOf (".cs");
						string className = f.Name.Substring (0, extentionPosition);
						Action tempAction = (Action) CreateInstance (className);
						if (tempAction is Action)
						{
							string title = tempAction.title;
							AllActions.Add (new ActionType (className, title));
						}
					}
					catch {}
				}
			}
			
			AllActions.Sort (delegate(ActionType i1, ActionType i2) { return i1.title.CompareTo(i2.title); });
			
			#endif
		}
		
		#endif
		
		
		public void SetEnabled ()
		{
			EnabledActions.Clear ();
			
			foreach (ActionType subclass in AllActions)
			{
				if (subclass.isEnabled)
				{
					EnabledActions.Add (subclass);
				}
			}
		}
		
		
		private string[] GetDefaultPopUp ()
		{
			List<string> defaultPopUp = new List<string> ();
			
			foreach (ActionType subclass in EnabledActions)
			{
				defaultPopUp.Add (subclass.title);
			}
			
			return (defaultPopUp.ToArray ());
		}
		
		
		public string GetActionName (int i)
		{
			return (EnabledActions [i].fileName);
		}
		
		
		public bool DoesActionExist (string _name)
		{
			foreach (ActionType actionType in EnabledActions)
			{
				if (_name == actionType.fileName || _name == ("AC." + actionType.fileName))
				{
					return true;
				}
			}
			return false;
		}
		
		
		public int GetActionsSize ()
		{
			return (EnabledActions.Count);
		}
		
		
		public string[] GetActionTitles ()
		{
			List<string> titles = new List<string>();
			
			foreach (ActionType type in EnabledActions)
			{
				titles.Add (type.title);
			}
			
			return (titles.ToArray ());
		}
		
		
		public string[] GetActionCategories ()
		{
			return categories;
		}
		
		
		public string[] GetActionSubCategories (int categoryNumber)
		{
			List<string> titles = new List<string>();
			string category = categories[categoryNumber];
			bool found = false;
			
			foreach (ActionType type in EnabledActions)
			{
				if (type.title.Contains (category + ": "))
				{
					found = true;
					string newTitle = type.title.Replace (category + ": ", "");
					titles.Add (newTitle);
				}
			}
			
			if (found)
			{
				return (titles.ToArray ());
			}
			
			// Try custom
			foreach (ActionType type in EnabledActions)
			{
				found = false;
				foreach (string _category in categories)
				{
					if (type.title.Contains (_category + ": "))
					{
						found = true;
					}
				}
				
				if (!found)
				{
					string newTitle = type.title.Replace (category + ": ", "");
					titles.Add (newTitle);
				}
			}
			
			return (titles.ToArray ());
		}
		
		
		public int GetActionCategory (int number)
		{
			if (EnabledActions.Count == 0)
			{
				return 0;
			}
			
			int index = EnabledActions[number].title.IndexOf (":");
			string category = EnabledActions[number].title.Substring (0, index);
			
			for (int i=0; i<categories.Length; i++)
			{
				if (categories[i] == category)
				{
					return i;
				}
			}
			
			// Try Custom
			for (int i=0; i<categories.Length; i++)
			{
				if (categories[i] == "Custom")
				{
					return i;
				}
			}
			
			return 0;
		}
		
		
		public int GetActionSubCategory (string title, int categoryNumber)
		{
			int i=0;
			string category = categories[categoryNumber];
			
			foreach (ActionType type in EnabledActions)
			{
				if (type.title.Contains (category + ": "))
				{
					if (type.title == title)
					{
						return i;
					}
					i++;
				}
			}
			
			// Try custom
			foreach (ActionType type in EnabledActions)
			{
				bool found = false;
				foreach (string _category in categories)
				{
					if (type.title.Contains (_category + ": "))
					{
						found = true;
					}
				}
				
				if (!found)
				{
					if (type.title == title)
					{
						return i;
					}
					i++;
				}
			}
			
			return 0;
		}
		
		
		public int GetTypeNumber (int categoryNumber, int subCategoryNumber)
		{
			if (GetActionSubCategories (categoryNumber).Length == 0)
			{
				Debug.LogWarning ("No Actions found in selected category");
				return defaultClass;
			}
			
			if (GetActionSubCategories (categoryNumber).Length <= subCategoryNumber)
			{
				subCategoryNumber = 0;
				//return 0;
			}
			
			string subCategory = GetActionSubCategories (categoryNumber)[subCategoryNumber];
			
			foreach (ActionType type in EnabledActions)
			{
				if (type.title.Contains (categories[categoryNumber] + ": "))
				{
					if (type.title.Contains (subCategory))
					{
						return EnabledActions.IndexOf (type);
					}
				}
			}
			
			// Not found in Category, try Custom
			foreach (ActionType type in EnabledActions)
			{
				if (type.title.Contains (subCategory))
				{
					return EnabledActions.IndexOf (type);
				}
			}
			
			return 0;
		}
		
	}
	
}
