/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionVarCheck.cs"
 * 
 *	This action checks to see if a Variable has been assigned a certain value,
 *	and performs something accordingly.
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
	public class ActionVarCheck : ActionCheck
	{

		public int parameterID = -1;
		public int variableID;
		public int variableNumber;

		public GetVarMethod getVarMethod = GetVarMethod.EnteredValue;
		public int compareVariableID;

		public int intValue;
		public float floatValue;
		public IntCondition intCondition;
		public bool isAdditive = false;
		
		public BoolValue boolValue = BoolValue.True;
		public enum BoolCondition { EqualTo, NotEqualTo };
		public BoolCondition boolCondition;

		public string stringValue;

		public VariableLocation location = VariableLocation.Global;

		private LocalVariables localVariables;
		private VariablesManager variablesManager;
		
		
		public ActionVarCheck ()
		{
			this.isDisplayed = true;
			title = "Variable: Check";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			variableID = AssignVariableID (parameters, parameterID, variableID);
		}

		
		override public int End (List<AC.Action> actions)
		{
			if (variableID == -1)
			{
				return 0;
			}

			GVar compareVar = null;

			if (getVarMethod == GetVarMethod.GlobalVariable || getVarMethod == GetVarMethod.LocalVariable)
			{
				if (compareVariableID == -1)
				{
					return 0;
				}

				if (getVarMethod == GetVarMethod.GlobalVariable)
				{
					compareVar = RuntimeVariables.GetVariable (compareVariableID);
					compareVar.Download ();
				}
				else if (getVarMethod == GetVarMethod.LocalVariable && !isAssetFile)
				{
					compareVar = LocalVariables.GetVariable (compareVariableID);
				}
			}

			if (location == VariableLocation.Local && !isAssetFile)
			{
				return ProcessResult (CheckCondition (LocalVariables.GetVariable (variableID), compareVar), actions);
			}

			else
			{
				GVar var = RuntimeVariables.GetVariable (variableID);
				if (var != null)
				{
					var.Download ();
					return ProcessResult (CheckCondition (var, compareVar), actions);
				}
				return -1;
			}
		}
		
		
		private bool CheckCondition (GVar _var, GVar _compareVar)
		{
			if (_compareVar != null && _var != null && _compareVar.type != _var.type)
			{
				Debug.LogWarning ("Cannot compare " + _var.label + " and " + _compareVar.label + " as they are not the same type!");
				return false;
			}

			if (_var.type == VariableType.Boolean)
			{
				int fieldValue = _var.val;
				int compareValue = (int) boolValue;
				if (_compareVar != null)
				{
					compareValue = _compareVar.val;
				}

				if (boolCondition == BoolCondition.EqualTo)
				{
					if (fieldValue == compareValue)
					{
						return true;
					}
				}
				else
				{
					if (fieldValue != compareValue)
					{
						return true;
					}
				}
			}

			else if (_var.type == VariableType.Integer)
			{
				int fieldValue = _var.val;
				int compareValue = intValue;
				if (_compareVar != null)
				{
					compareValue = _compareVar.val;
				}

				if (intCondition == IntCondition.EqualTo)
				{
					if (fieldValue == compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.NotEqualTo)
				{
					if (fieldValue != compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.LessThan)
				{
					if (fieldValue < compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.MoreThan)
				{
					if (fieldValue > compareValue)
					{
						return true;
					}
				}
			}

			else if (_var.type == VariableType.Float)
			{
				float fieldValue = _var.floatVal;
				float compareValue = floatValue;
				if (_compareVar != null)
				{
					compareValue = _compareVar.floatVal;
				}

				if (intCondition == IntCondition.EqualTo)
				{
					if (fieldValue == compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.NotEqualTo)
				{
					if (fieldValue != compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.LessThan)
				{
					if (fieldValue < compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.MoreThan)
				{
					if (fieldValue > compareValue)
					{
						return true;
					}
				}
			}

			else if (_var.type == VariableType.String)
			{
				string fieldValue = _var.textVal;
				string compareValue = AdvGame.ConvertTokens (stringValue);
				if (_compareVar != null)
				{
					compareValue = _compareVar.textVal;
				}

				if (boolCondition == BoolCondition.EqualTo)
				{
					if (fieldValue == compareValue)
					{
						return true;
					}
				}
				else
				{
					if (fieldValue != compareValue)
					{
						return true;
					}
				}
			}
			
			return false;
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

			if (isAssetFile && getVarMethod == GetVarMethod.LocalVariable)
			{
				EditorGUILayout.HelpBox ("Local Variables cannot be referenced by Asset-based Actions.", MessageType.Warning);
			}

			if (location == VariableLocation.Global)
			{
				if (!variablesManager)
				{
					variablesManager = AdvGame.GetReferences ().variablesManager;
				}
				
				if (variablesManager)
				{
					parameterID = Action.ChooseParameterGUI ("Variable:", parameters, parameterID, ParameterType.GlobalVariable);
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
					parameterID = Action.ChooseParameterGUI ("Variable:", parameters, parameterID, ParameterType.LocalVariable);
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

		}


		private int ShowVarSelectorGUI (List<GVar> vars, int ID, bool showLabel)
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

			if (showLabel)
			{
				variableNumber = EditorGUILayout.Popup ("Variable:", variableNumber, labelList.ToArray());
			}
			else
			{
				variableNumber = EditorGUILayout.Popup (variableNumber, labelList.ToArray());
			}
			ID = vars[variableNumber].id;

			return ID;
		}


		private int ShowVarGUI (List<GVar> vars, int ID, bool changeID)
		{
			if (vars.Count > 0)
			{
				if (changeID)
				{
					ID = ShowVarSelectorGUI (vars, ID, true);
				}
				variableNumber = Mathf.Min (variableNumber, vars.Count-1);

				getVarMethod = (GetVarMethod) EditorGUILayout.EnumPopup ("Compare with:", getVarMethod);

				EditorGUILayout.BeginHorizontal ();

				if (vars [variableNumber].type == VariableType.Boolean)
				{
					boolCondition = (BoolCondition) EditorGUILayout.EnumPopup ("Statement:", boolCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						boolValue = (BoolValue) EditorGUILayout.EnumPopup (boolValue);
					}
				}
				else if (vars [variableNumber].type == VariableType.Integer)
				{
					intCondition = (IntCondition) EditorGUILayout.EnumPopup ("Statement:", intCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						intValue = EditorGUILayout.IntField (intValue);
					}
				}
				else if (vars [variableNumber].type == VariableType.Float)
				{
					intCondition = (IntCondition) EditorGUILayout.EnumPopup ("Statement:", intCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						floatValue = EditorGUILayout.FloatField (floatValue);
					}
				}
				else if (vars [variableNumber].type == VariableType.String)
				{
					boolCondition = (BoolCondition) EditorGUILayout.EnumPopup ("Statement:", boolCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						stringValue = EditorGUILayout.TextField (stringValue);
					}
				}

				if (getVarMethod == GetVarMethod.GlobalVariable)
				{
					if (!variablesManager)
					{
						variablesManager = AdvGame.GetReferences ().variablesManager;
					}

					if (variablesManager == null || variablesManager.vars == null || variablesManager.vars.Count == 0)
					{
						EditorGUILayout.HelpBox ("No Global variables exist!", MessageType.Info);
					}
					else
					{
						compareVariableID = ShowVarSelectorGUI (variablesManager.vars, compareVariableID, false);
					}
				}
				else if (getVarMethod == GetVarMethod.LocalVariable)
				{
					if (!localVariables && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent<LocalVariables>())
					{
						localVariables = GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>();
					}
					
					if (localVariables == null || localVariables.localVars == null || localVariables.localVars.Count == 0)
					{
						EditorGUILayout.HelpBox ("No Local variables exist!", MessageType.Info);
					}
					else
					{
						compareVariableID = ShowVarSelectorGUI (localVariables.localVars, compareVariableID, false);
					}
				}

				EditorGUILayout.EndHorizontal ();
			}
			else
			{
				EditorGUILayout.HelpBox ("No variables exist!", MessageType.Info);
				ID = -1;
				variableNumber = -1;
			}

			return ID;
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
				labelAdd = " (" + vars[variableNumber].label;
				
				if (vars [variableNumber].type == VariableType.Boolean)
				{
					labelAdd += " " + boolCondition.ToString () + " " + boolValue.ToString ();
				}
				else if (vars [variableNumber].type == VariableType.Integer)
				{
					labelAdd += " " + intCondition.ToString () + " " + intValue.ToString ();
				}
				else if (vars [variableNumber].type == VariableType.Float)
				{
					labelAdd += " " + intCondition.ToString () + " " + floatValue.ToString ();
				}
				else if (vars [variableNumber].type == VariableType.String)
				{
					labelAdd += " " + boolCondition.ToString () + " " + stringValue;
				}
				
				labelAdd += ")";
			}

			return labelAdd;
		}
		
		#endif


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

	}

}