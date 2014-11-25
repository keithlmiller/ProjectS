/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"VariablesManager.cs"
 * 
 *	This script handles the "Variables" tab of the main wizard.
 *	Boolean and integer, which can be used regardless of scene, are defined here.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class VariablesManager : ScriptableObject
	{

		public List<GVar> vars = new List<GVar>();
		public bool updateRuntime = false;
		
		#if UNITY_EDITOR

		private GVar selectedVar;
		private Texture2D icon;
		private int sideVar = -1;
		private VariableLocation sideVarLocation = VariableLocation.Global;
		private string[] boolType = {"False", "True"};
		private string filter = "";
		

		public void ShowGUI ()
		{
			LocalVariables localVariables = null;
			RuntimeVariables runtimeVariables = null;
			if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>())
			{
				localVariables = GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>();
			}
			if (Application.isPlaying && GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>())
			{
				runtimeVariables = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>();
			}

			if (icon == null)
			{
				icon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/inspector-use.png", typeof (Texture2D));
			}

			updateRuntime = EditorGUILayout.Toggle ("Show realtime values?", updateRuntime);
			filter = EditorGUILayout.TextField ("Filter by name:", filter);
			EditorGUILayout.Space ();

			// List global variables
			if (Application.isPlaying && updateRuntime && runtimeVariables != null)
			{
				ShowVarList (runtimeVariables.globalVars, VariableLocation.Global, false);
			}
			else
			{
				ShowVarList (vars, VariableLocation.Global, true);
			}

			EditorGUILayout.Space ();
			if (localVariables != null)
			{
				if (Application.isPlaying && updateRuntime)
				{
					ShowVarList (localVariables.localVars, VariableLocation.Local, false);
				}
				else
				{
					ShowVarList (localVariables.localVars, VariableLocation.Local, true);
				}
			}
			else
			{
				EditorGUILayout.LabelField ("Local variables", EditorStyles.boldLabel);
				EditorGUILayout.HelpBox ("A GameEngine prefab must be present in the scene before Local variables can be defined", MessageType.Info);
			}

			EditorGUILayout.Space ();
			if (selectedVar != null && (!Application.isPlaying || !updateRuntime))
			{
				if (vars.Contains (selectedVar))
				{
					ShowVarGUI (VariableLocation.Global);
				}
				else if (localVariables != null && localVariables.localVars.Contains (selectedVar))
				{
					ShowVarGUI (VariableLocation.Local);
				}
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);

				if (localVariables != null)
				{
					EditorUtility.SetDirty (localVariables);
				}
			}
		}


		private void ResetFilter ()
		{
			filter = "";
		}


		private void SideMenu (GVar _var, List<GVar> _vars, VariableLocation location)
		{
			GenericMenu menu = new GenericMenu ();
			sideVar = _vars.IndexOf (_var);
			sideVarLocation = location;

			menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
			if (_vars.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
			}
			if (sideVar > 0 || sideVar < _vars.Count-1)
			{
				menu.AddSeparator ("");
			}
			if (sideVar > 0)
			{
				menu.AddItem (new GUIContent ("Move up"), false, Callback, "Move up");
			}
			if (sideVar < _vars.Count-1)
			{
				menu.AddItem (new GUIContent ("Move down"), false, Callback, "Move down");
			}
			
			menu.ShowAsContext ();
		}
		
		
		private void Callback (object obj)
		{
			if (sideVar >= 0)
			{
				ResetFilter ();
				List<GVar> _vars = new List<GVar>();

				if (sideVarLocation == VariableLocation.Global)
				{
					_vars = vars;
				}
				else
				{
					_vars = GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>().localVars;
				}
				GVar tempVar = _vars[sideVar];

				switch (obj.ToString ())
				{
				case "Insert after":
					Undo.RecordObject (this, "Insert variable");
					_vars.Insert (sideVar+1, new GVar (GetIDArray (_vars)));
					DeactivateAllVars ();
					break;
					
				case "Delete":
					Undo.RecordObject (this, "Delete variable");
					_vars.RemoveAt (sideVar);
					DeactivateAllVars ();
					break;

				case "Move up":
					Undo.RecordObject (this, "Move variable up");
					_vars.RemoveAt (sideVar);
					_vars.Insert (sideVar-1, tempVar);
					break;

				case "Move down":
					Undo.RecordObject (this, "Move variable down");
					_vars.RemoveAt (sideVar);
					_vars.Insert (sideVar+1, tempVar);
					break;
				}
			}

			sideVar = -1;

			if (sideVarLocation == AC.VariableLocation.Global)
			{
				EditorUtility.SetDirty (this);
				AssetDatabase.SaveAssets ();
			}
			else
			{
				EditorUtility.SetDirty (GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>());
			}
		}


		private void ActivateVar (GVar var)
		{
			var.isEditing = true;
			selectedVar = var;
		}
		
		
		private void DeactivateAllVars ()
		{
			if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>())
			{
				foreach (GVar var in GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>().localVars)
				{
					var.isEditing = false;
				}
			}

			foreach (GVar var in vars)
			{
				var.isEditing = false;
			}
			selectedVar = null;
		}


		private int[] GetIDArray (List<GVar> _vars)
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (GVar variable in _vars)
			{
				idArray.Add (variable.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}


		private string GetVarValue (GVar _var)
		{
			if (_var.type == VariableType.Integer)
			{
				return _var.val.ToString ();
			}
			else if (_var.type == VariableType.String)
			{
				return _var.textVal;
			}
			else if (_var.type == VariableType.Float)
			{
				return _var.floatVal.ToString ();
			}
			else
			{
				if (_var.val == 0)
				{
					return "False";
				}
				else
				{
					return "True";
				}
			}
		}


		private void ShowVarList (List<GVar> _vars, VariableLocation location, bool allowEditing)
		{
			EditorGUILayout.LabelField (location + " variables", EditorStyles.boldLabel);

			foreach (GVar _var in _vars)
			{
				if (filter == "" || _var.label.ToLower ().Contains (filter.ToLower ()))
				{
					//EditorGUILayout.BeginVertical("Button");
					EditorGUILayout.BeginHorizontal ();
					
					string buttonLabel = _var.id + ": ";
					if (buttonLabel == "")
					{
						buttonLabel += "(Untitled)";	
					}
					else
					{
						buttonLabel += _var.label;
					}
					
					buttonLabel += " (" + _var.type.ToString () + " - " + GetVarValue (_var) + ")";

					if (allowEditing)
					{
						if (GUILayout.Toggle (_var.isEditing, buttonLabel, "Button"))
						{
							if (selectedVar != _var)
							{
								DeactivateAllVars ();
								ActivateVar (_var);
							}
						}
						
						if (GUILayout.Button (icon, GUILayout.Width (20f), GUILayout.Height (15f)))
						{
							SideMenu (_var, _vars, location);
						}
					}
					else
					{
						GUILayout.Label (buttonLabel, "Button");
					}
					
					EditorGUILayout.EndHorizontal ();
					//EditorGUILayout.EndVertical();
				}
			}

			if (allowEditing)
			{
				EditorGUILayout.Space ();
				if (GUILayout.Button("Create new " + location + " variable"))
				{
					ResetFilter ();
					Undo.RecordObject (this, "Add " + location + " variable");
					_vars.Add (new GVar (GetIDArray (_vars)));
					DeactivateAllVars ();
					ActivateVar (_vars [_vars.Count-1]);
				}
			}
		}


		private void ShowVarGUI (VariableLocation location)
		{
			EditorGUILayout.LabelField (location + " variable '" + selectedVar.label + "' properties", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical("Button");
			
			selectedVar.label = EditorGUILayout.TextField ("Label:", selectedVar.label);
			selectedVar.type = (VariableType) EditorGUILayout.EnumPopup ("Type:", selectedVar.type);

			if (location == VariableLocation.Local)
			{
				EditorGUILayout.LabelField ("Replacement token:", "[localvar:" + selectedVar.id.ToString () + "]");
			}
			else
			{
				EditorGUILayout.LabelField ("Replacement token:", "[var:" + selectedVar.id.ToString () + "]");
			}
			
			if (selectedVar.type == VariableType.Boolean)
			{
				if (selectedVar.val != 1)
				{
					selectedVar.val = 0;
				}
				selectedVar.val = EditorGUILayout.Popup ("Initial value:", selectedVar.val, boolType);
			}
			else if (selectedVar.type == VariableType.Integer)
			{
				selectedVar.val = EditorGUILayout.IntField ("Initial value:", selectedVar.val);
			}
			else if (selectedVar.type == VariableType.String)
			{
				selectedVar.textVal = EditorGUILayout.TextField ("Initial value:", selectedVar.textVal);
			}
			else if (selectedVar.type == VariableType.Float)
			{
				selectedVar.floatVal = EditorGUILayout.FloatField ("Initial value:", selectedVar.floatVal);
			}

			if (location == VariableLocation.Local)
			{
				selectedVar.link = VarLink.None;
			}
			else
			{
				EditorGUILayout.Space ();
				selectedVar.link = (VarLink) EditorGUILayout.EnumPopup ("Link to:", selectedVar.link);
				if (selectedVar.link == VarLink.PlaymakerGlobalVariable)
				{
					if (PlayMakerIntegration.IsDefinePresent ())
					{
						selectedVar.pmVar = EditorGUILayout.TextField ("Playmaker Global Variable:", selectedVar.pmVar);
						selectedVar.updateLinkOnStart = EditorGUILayout.Toggle ("Use PM for initial value?", selectedVar.updateLinkOnStart);
					}
					else
					{
						EditorGUILayout.HelpBox ("The 'PlayMakerIsPresent' Scripting Define Symbol must be listed in the\nPlayer Settings. Please set it from Edit -> Project Settings -> Player", MessageType.Warning);
					}
				}
				else if (selectedVar.link == VarLink.OptionsData)
				{
					EditorGUILayout.HelpBox ("This Variable will be stored in PlayerPrefs, and not in saved game files.", MessageType.Info);
				}
			}
			EditorGUILayout.EndVertical ();
		}

		#endif

	}

}