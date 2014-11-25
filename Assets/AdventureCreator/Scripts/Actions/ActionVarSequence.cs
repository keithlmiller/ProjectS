/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionVarSequence.cs"
 * 
 *	This action runs an Integer Variable through a sequence
 *	and performs different follow-up Actions accordingly.
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
	public class ActionVarSequence : ActionCheckMultiple
	{
		
		public int parameterID = -1;
		public int variableID;
		public int variableNumber;
		public bool doLoop = false;
		
		public VariableLocation location = VariableLocation.Global;
		
		private LocalVariables localVariables;
		private VariablesManager variablesManager;
		
		
		public ActionVarSequence ()
		{
			this.isDisplayed = true;
			title = "Variable: Run sequence";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			variableID = AssignVariableID (parameters, parameterID, variableID);
		}
		
		
		override public int End (List<Action> actions)
		{
			if (numSockets <= 0)
			{
				Debug.LogWarning ("Could not compute Random check because no values were possible!");
				return -1;
			}
			
			if (variableID == -1)
			{
				return 0;
			}
			
			GVar var = null;
			
			if (location == VariableLocation.Local && !isAssetFile)
			{
				var = LocalVariables.GetVariable (variableID);
			}
			else
			{
				var = RuntimeVariables.GetVariable (variableID);
			}
			
			if (var != null)
			{
				if (var.type == VariableType.Integer)
				{
					var.Download ();
					if (var.val < 1)
					{
						var.val = 1;
					}
					int originalValue = var.val-1;
					var.val ++;
					if (var.val > numSockets)
					{
						if (doLoop)
						{
							var.val = 1;
						}
						else
						{
							var.val = numSockets;
						}
					}
					var.Upload ();
					return ProcessResult (originalValue, actions);
				}
				else
				{
					Debug.LogWarning ("Variable: Run sequence Action is referencing a Variable that does not exist!");
				}
			}
			
			return 0;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			if (isAssetFile)
			{
				location = VariableLocation.Global;
			}
			else
			{
				location = (VariableLocation) EditorGUILayout.EnumPopup ("Source:", location);
			}
			
			if (location == VariableLocation.Global)
			{
				if (!variablesManager)
				{
					variablesManager = AdvGame.GetReferences ().variablesManager;
				}
				
				if (variablesManager)
				{
					parameterID = Action.ChooseParameterGUI ("Integer variable:", parameters, parameterID, ParameterType.GlobalVariable);
					if (parameterID >= 0)
					{
						variableID = ShowVarGUI (variablesManager.vars, variableID, false);
					}
					else
					{
						variableID = ShowVarGUI (variablesManager.vars, variableID, true);
					}
				}
			}
			
			else if (location == VariableLocation.Local)
			{
				if (!localVariables && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent<LocalVariables>())
				{
					localVariables = GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>();
				}
				
				if (localVariables)
				{
					parameterID = Action.ChooseParameterGUI ("Integer variable:", parameters, parameterID, ParameterType.LocalVariable);
					if (parameterID >= 0)
					{
						variableID = ShowVarGUI (localVariables.localVars, variableID, false);
					}
					else
					{
						variableID = ShowVarGUI (localVariables.localVars, variableID, true);
					}
				}
			}
			
			numSockets = EditorGUILayout.IntSlider ("# of possible values:", numSockets, 1, 10);
			doLoop = EditorGUILayout.Toggle ("Run on a loop?", doLoop);
		}
		
		
		private int ShowVarSelectorGUI (List<GVar> vars, int ID)
		{
			variableNumber = -1;
			
			List<string> labelList = new List<string>();
			foreach (GVar _var in vars)
			{
				labelList.Add (_var.label);
			}
			
			variableNumber = GetVarNumber (vars, ID);
			
			if (variableNumber == -1)
			{
				// Wasn't found (variable was deleted?), so revert to zero
				Debug.LogWarning ("Previously chosen variable no longer exists!");
				variableNumber = 0;
				ID = 0;
			}
			
			variableNumber = EditorGUILayout.Popup ("Variable:", variableNumber, labelList.ToArray());
			ID = vars[variableNumber].id;
			
			return ID;
		}
		
		
		private int ShowVarGUI (List<GVar> vars, int ID, bool changeID)
		{
			if (vars.Count > 0)
			{
				if (changeID)
				{
					ID = ShowVarSelectorGUI (vars, ID);
				}
				variableNumber = Mathf.Min (variableNumber, vars.Count-1);
				if (changeID)
				{
					if (vars[variableNumber].type != VariableType.Integer)
					{
						EditorGUILayout.HelpBox ("The selected Variable must be an Integer!", MessageType.Warning);
					}
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("No variables exist!", MessageType.Info);
				ID = -1;
				variableNumber = -1;
			}
			
			return ID;
		}
		
		
		private int GetVarNumber (List<GVar> vars, int ID)
		{
			int i = 0;
			foreach (GVar _var in vars)
			{
				if (_var.id == ID)
				{
					return i;
				}
				i++;
			}
			return -1;
		}
		
		
		override public string SetLabel ()
		{
			if (location == VariableLocation.Local && !isAssetFile)
			{
				if (!localVariables && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent<LocalVariables>())
				{
					localVariables = GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>();
				}
				
				if (localVariables)
				{
					return GetLabelString (localVariables.localVars);
				}
			}
			else
			{
				if (!variablesManager)
				{
					variablesManager = AdvGame.GetReferences ().variablesManager;
				}
				
				if (variablesManager)
				{
					return GetLabelString (variablesManager.vars);
				}
			}
			
			return "";
		}
		
		
		private string GetLabelString (List<GVar> vars)
		{
			string labelAdd = "";
			
			if (vars.Count > 0 && vars.Count > variableNumber && variableNumber > -1)
			{
				labelAdd = " (" + vars[variableNumber].label + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
	}
	
}