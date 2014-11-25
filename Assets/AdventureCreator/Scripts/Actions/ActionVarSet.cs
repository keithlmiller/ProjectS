/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionInventorySet.cs"
 * 
 *	This action is used to set the value of integer and boolean Variables, defined in the Variables Manager.
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
	public class ActionVarSet : Action
	{
		
		public SetVarMethod setVarMethod;
		public SetVarMethodString setVarMethodString = SetVarMethodString.EnteredHere;
		public SetVarMethodIntBool setVarMethodIntBool = SetVarMethodIntBool.EnteredHere;

		public int parameterID = -1;
		public int variableID;
		public int variableNumber;
		
		public int intValue;
		public float floatValue;
		public BoolValue boolValue;
		public string stringValue;
		public string formula;

		public VariableLocation location;

		public string menuName;
		public string elementName;

		public Animator animator;
		public string parameterName;

		private LocalVariables localVariables;
		private VariablesManager variablesManager;

		#if UNITY_EDITOR
		private static GUILayoutOption
			intWidth = GUILayout.MaxWidth (40);
		#endif
		
		
		public ActionVarSet ()
		{
			this.isDisplayed = true;
			title = "Variable: Set";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			variableID = AssignVariableID (parameters, parameterID, variableID);
		}


		override public float Run ()
		{
			if (variableID != -1)
			{
				if (location == VariableLocation.Local && !isAssetFile)
				{
					SetVariable (LocalVariables.GetVariable (variableID), VariableLocation.Local);
				}
				else
				{
					SetVariable (RuntimeVariables.GetVariable (variableID), VariableLocation.Global);
				}
			}

			return 0f;
		}

		
		private void SetVariable (GVar var, VariableLocation location)
		{
			if (var == null)
			{
				return;
			}

			if (location == VariableLocation.Global)
			{
				var.Download ();
			}

			if (var.type == VariableType.Integer)
			{
				int _value = 0;

				if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
				{
					if (setVarMethod == SetVarMethod.Formula)
					{
						_value = (int) AdvGame.CalculateFormula (AdvGame.ConvertTokens (formula));
					}
					else
					{
						_value = intValue;
					}
				}
				else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
				{
					if (animator && parameterName != "")
					{
						_value = animator.GetInteger (parameterName);
						setVarMethod = SetVarMethod.SetValue;
					}	
				}

				var.SetValue (_value, setVarMethod);
			}
			if (var.type == VariableType.Float)
			{
				float _value = 0;
				
				if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
				{
					if (setVarMethod == SetVarMethod.Formula)
					{
						_value = (float) AdvGame.CalculateFormula (AdvGame.ConvertTokens (formula));
					}
					else
					{
						_value = floatValue;
					}
				}
				else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
				{
					if (animator && parameterName != "")
					{
						_value = animator.GetFloat (parameterName);
						setVarMethod = SetVarMethod.SetValue;
					}	
				}
				
				var.SetValue (_value, setVarMethod);
			}
			else if (var.type == VariableType.Boolean)
			{
				int _value = 0;

				if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
				{
					_value = (int) boolValue;
				}
				else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
				{
					if (animator && parameterName != "")
					{
						if (animator.GetBool (parameterName))
						{
							_value = 1;
						}
					}
				}

				var.SetValue (_value, SetVarMethod.SetValue);
			}
			else if (var.type == VariableType.String)
			{
				string _value = "";

				if (setVarMethodString == SetVarMethodString.EnteredHere)
				{
					_value = AdvGame.ConvertTokens (stringValue);
				}
				else if (setVarMethodString == SetVarMethodString.SetAsMenuInputLabel)
				{
					if (PlayerMenus.GetElementWithName (menuName, elementName) != null)
					{
						MenuInput menuInput = (MenuInput) PlayerMenus.GetElementWithName (menuName, elementName);
						_value = menuInput.label;

						if (Options.GetLanguageName () == "Arabic" && _value.Length > 0)
						{
							// Invert
							char[] charArray = _value.ToCharArray ();
							_value = "";
							for (int i = charArray.Length-1; i >= 0; i --)
							{
								_value += charArray[i];
							}
						}
					}
					else
					{
						Debug.LogWarning ("Could not find MenuInput '" + elementName + "' in Menu '" + menuName + "'");
					}
				}

				var.SetValue (_value);
			}

			if (location == VariableLocation.Global)
			{
				var.Upload ();
			}

			GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>().VariableChanged ();
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
					ShowVarGUI (variablesManager.vars, parameters, ParameterType.GlobalVariable);
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
					ShowVarGUI (localVariables.localVars, parameters, ParameterType.LocalVariable);
				}
			}
		}


		private void ShowVarGUI (List<GVar> vars, List<ActionParameter> parameters, ParameterType parameterType)
		{
			// Create a string List of the field's names (for the PopUp box)
			List<string> labelList = new List<string>();
			
			int i = 0;
			if (parameterID == -1)
			{
				variableNumber = -1;
			}
			
			if (vars.Count > 0)
			{
				foreach (GVar _var in vars)
				{
					labelList.Add (_var.label);
					
					// If a GlobalVar variable has been removed, make sure selected variable is still valid
					if (_var.id == variableID)
					{
						variableNumber = i;
					}
					
					i ++;
				}
				
				if (variableNumber == -1 && (parameters == null || parameters.Count == 0 || parameterID == -1))
				{
					// Wasn't found (variable was deleted?), so revert to zero
					Debug.LogWarning ("Previously chosen variable no longer exists!");
					variableNumber = 0;
					variableID = 0;
				}
				
				
				parameterID = Action.ChooseParameterGUI ("Variable:", parameters, parameterID, parameterType);
				if (parameterID >= 0)
				{
					//variableNumber = 0;
					variableNumber = Mathf.Min (variableNumber, vars.Count-1);
					variableID = -1;
				}
				else
				{
					variableNumber = EditorGUILayout.Popup ("Variable:", variableNumber, labelList.ToArray());
					variableID = vars [variableNumber].id;
				}

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Statement:");

				if (vars [variableNumber].type == VariableType.Boolean)
				{
					EditorGUILayout.LabelField ("=", intWidth);
					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						boolValue = (BoolValue) EditorGUILayout.EnumPopup (boolValue);
					}
					EditorGUILayout.EndHorizontal();
					
					setVarMethodIntBool = (SetVarMethodIntBool) EditorGUILayout.EnumPopup ("New value is:", setVarMethodIntBool);
					
					if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
					{
						ShowMecanimGUI ();
					}
				}
				else if (vars [variableNumber].type == VariableType.Integer)
				{
					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						if (setVarMethod == SetVarMethod.IncreaseByValue)
						{
							EditorGUILayout.LabelField ("+=", intWidth);
							intValue = EditorGUILayout.IntField (intValue);
						}
						else if (setVarMethod == SetVarMethod.SetValue)
						{
							EditorGUILayout.LabelField ("=", intWidth);
							intValue = EditorGUILayout.IntField (intValue);
						}
						else if (setVarMethod == SetVarMethod.Formula)
						{
							EditorGUILayout.LabelField ("=", intWidth);
							formula = EditorGUILayout.TextField (formula);

							#if UNITY_WP8
							EditorGUILayout.HelpBox ("This feature is not available for Windows Phone 8.", MessageType.Warning);
							#endif
						}
						else if (setVarMethod == SetVarMethod.SetAsRandom)
						{
							EditorGUILayout.LabelField ("= 0 to", intWidth);
							intValue = EditorGUILayout.IntField (intValue);
						}
					}
					else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
					{
						EditorGUILayout.LabelField ("=", intWidth);
					}
					
					EditorGUILayout.EndHorizontal();
					
					setVarMethodIntBool = (SetVarMethodIntBool) EditorGUILayout.EnumPopup ("New value is:", setVarMethodIntBool);
					
					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						setVarMethod = (SetVarMethod) EditorGUILayout.EnumPopup ("Method:", setVarMethod);
						
						if (setVarMethod == SetVarMethod.SetAsRandom && intValue < 0)
						{
							intValue = 0;
						}
					}
					else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
					{
						ShowMecanimGUI ();
					}
				}
				else if (vars [variableNumber].type == VariableType.Float)
				{
					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						if (setVarMethod == SetVarMethod.IncreaseByValue)
						{
							EditorGUILayout.LabelField ("+=", intWidth);
							floatValue = EditorGUILayout.FloatField (floatValue);
						}
						else if (setVarMethod == SetVarMethod.SetValue)
						{
							EditorGUILayout.LabelField ("=", intWidth);
							floatValue = EditorGUILayout.FloatField (floatValue);
						}
						else if (setVarMethod == SetVarMethod.Formula)
						{
							EditorGUILayout.LabelField ("=", intWidth);
							formula = EditorGUILayout.TextField (formula);

							#if UNITY_WP8
							EditorGUILayout.HelpBox ("This feature is not available for Windows Phone 8.", MessageType.Warning);
							#endif
						}
						else if (setVarMethod == SetVarMethod.SetAsRandom)
						{
							EditorGUILayout.LabelField ("= 0 to", intWidth);
							floatValue = EditorGUILayout.FloatField (floatValue);
						}
					}
					else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
					{
						EditorGUILayout.LabelField ("=", intWidth);
					}
					
					EditorGUILayout.EndHorizontal();
					
					setVarMethodIntBool = (SetVarMethodIntBool) EditorGUILayout.EnumPopup ("New value is:", setVarMethodIntBool);
					
					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						setVarMethod = (SetVarMethod) EditorGUILayout.EnumPopup ("Method:", setVarMethod);
						
						if (setVarMethod == SetVarMethod.SetAsRandom && floatValue < 0f)
						{
							floatValue = 0f;
						}
					}
					else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
					{
						ShowMecanimGUI ();
					}
				}
				else if (vars [variableNumber].type == VariableType.String)
				{
					EditorGUILayout.LabelField ("=", intWidth);
					if (setVarMethodString == SetVarMethodString.EnteredHere)
					{
						stringValue = EditorGUILayout.TextField (stringValue);
					}
					EditorGUILayout.EndHorizontal();
					
					setVarMethodString = (SetVarMethodString) EditorGUILayout.EnumPopup ("New value is:", setVarMethodString);
					
					if (setVarMethodString == SetVarMethodString.SetAsMenuInputLabel)
					{
						menuName = EditorGUILayout.TextField ("Menu name:", menuName);
						elementName = EditorGUILayout.TextField ("Input element name:", elementName);
					}
				}
				else
				{
					EditorGUILayout.EndHorizontal();
				}
				
				AfterRunningOption ();
			}
			else
			{
				EditorGUILayout.HelpBox ("No variables exist!", MessageType.Info);
				variableID = -1;
				variableNumber = -1;
			}
		}


		private void ShowMecanimGUI ()
		{
			animator = (Animator) EditorGUILayout.ObjectField ("Animator:", animator, typeof (Animator), true);
			parameterName = EditorGUILayout.TextField ("Parameter name:", parameterName);
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

			if (vars.Count > 0 && variableNumber > -1 && vars.Count > variableNumber)
			{
				labelAdd = " (" + vars [variableNumber].label;

				if (vars[variableNumber].type == VariableType.Integer)
				{
					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						if (setVarMethod == SetVarMethod.IncreaseByValue)
						{
							labelAdd += " += " + intValue;
						}
						else if (setVarMethod == SetVarMethod.SetValue)
						{
							labelAdd += " = " + intValue;
						}
						else if (setVarMethod == SetVarMethod.SetAsRandom)
						{
							labelAdd += " = 0 to " + intValue;
						}
						else if (setVarMethod == SetVarMethod.Formula)
						{
							labelAdd += " = " + formula;
						}
					}
					else
					{
						labelAdd += " = " + parameterName;
					}
				}
				else if (vars[variableNumber].type == VariableType.Boolean)
				{
					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						labelAdd += " = " + boolValue;
					}
					else
					{
						labelAdd += " = " + parameterName;
					}
				}
				else if (vars[variableNumber].type == VariableType.Float)
				{
					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						if (setVarMethod == SetVarMethod.IncreaseByValue)
						{
							labelAdd += " += " + floatValue;
						}
						else if (setVarMethod == SetVarMethod.SetValue)
						{
							labelAdd += " = " + floatValue;
						}
						else if (setVarMethod == SetVarMethod.SetAsRandom)
						{
							labelAdd += " = 0 to " + floatValue;
						}
						else if (setVarMethod == SetVarMethod.Formula)
						{
							labelAdd += " = " + formula;
						}
					}
					else
					{
						labelAdd += " = " + parameterName;
					}
				}
				else if (vars[variableNumber].type == VariableType.String)
				{
					if (setVarMethodString == SetVarMethodString.EnteredHere)
					{
						labelAdd += " = " + stringValue;
					}
					else
					{
						labelAdd += " = " + elementName;
					}
				}

				labelAdd += ")";
			}

			return labelAdd;
		}

		#endif

	}

}