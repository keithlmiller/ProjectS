/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionRunActionList.cs"
 * 
 *	This is a blank action template.
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
	public class ActionRunActionList : Action
	{
		
		public enum ListSource { InScene, AssetFile };
		public ListSource listSource = ListSource.InScene;

		public ActionList actionList;
		public ActionListAsset invActionList;
		public int constantID = 0;
		public int parameterID = -1;

		public bool runFromStart = true;
		public int jumpToAction;
		public AC.Action jumpToActionActual;
		public bool runInParallel = false;

		public List<ActionParameter> localParameters = new List<ActionParameter>();

		private RuntimeActionList runtimeActionList = null;


		public ActionRunActionList ()
		{
			this.isDisplayed = true;
			title = "Engine: Run ActionList";
			numSockets = 0;
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			if (listSource == ListSource.InScene)
			{
				actionList = AssignFile <ActionList> (parameters, parameterID, constantID, actionList);
			}
		}


		override public float Run ()
		{
			ActionListManager actionListManager = GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>();

			if (!isRunning)
			{
				isRunning = true;
				runtimeActionList = null;

				if (listSource == ListSource.InScene)
				{
					if (actionList != null)
					{
						actionListManager.EndList (actionList);

						if (actionList.useParameters)
						{
							SendParameters (actionList.parameters, false);
						}

						if (runFromStart)
						{
							actionList.Interact ();
						}
						else
						{
							actionList.Interact (GetSkipIndex (actionList.actions));
						}
					}
				}
				else if (listSource == ListSource.AssetFile && invActionList != null)
				{
					if (invActionList.useParameters)
					{
						SendParameters (invActionList.parameters, true);
					}

					if (runFromStart)
					{
						runtimeActionList = AdvGame.RunActionListAsset (invActionList);
					}
					else
					{
						runtimeActionList = AdvGame.RunActionListAsset (invActionList, GetSkipIndex (invActionList.actions));
					}
				}

				if (!runInParallel || (runInParallel && willWait))
				{
					return defaultPauseTime;
				}
			}
			else
			{
				if (listSource == ListSource.InScene && actionList != null)
				{
					if (actionListManager.IsListRunning (actionList))
					{
						return defaultPauseTime;
					}
					else
					{
						isRunning = false;
					}
				}
				else if (listSource == ListSource.AssetFile && invActionList != null)
				{
					if (actionListManager.IsListRunning (runtimeActionList))
					{
						return defaultPauseTime;
					}
					else
					{
						isRunning = false;
					}
				}
			}

			return 0f;
		}


		override public void Skip ()
		{
			ActionListManager actionListManager = GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>();
			
			if (listSource == ListSource.InScene && actionList != null)
			{
				actionListManager.EndList (actionList);

				if (actionList.useParameters)
				{
					SendParameters (actionList.parameters, false);
				}

				if (runFromStart)
				{
					actionList.Interact ();
				}
				else
				{
					actionList.Interact (GetSkipIndex (actionList.actions));
				}
			}
			else if (listSource == ListSource.AssetFile && invActionList != null)
			{
				actionListManager.EndAssetList (invActionList.name);

				if (invActionList.useParameters)
				{
					SendParameters (invActionList.parameters, true);
				}

				if (runFromStart)
				{
					runtimeActionList = AdvGame.RunActionListAsset (invActionList);
				}
				else
				{
					runtimeActionList = AdvGame.RunActionListAsset (invActionList, GetSkipIndex (invActionList.actions));
				}
			}
		}


		private int GetSkipIndex (List<Action> _actions)
		{
			int skip = jumpToAction;
			if (jumpToActionActual && _actions.IndexOf (jumpToActionActual) > 0)
			{
				skip = _actions.IndexOf (jumpToActionActual);
			}
			return skip;
		}


		override public int End (List<AC.Action> actions)
		{
			if (runInParallel)
			{
				return (base.End (actions));
			}

			return -1;
		}


		private void SendParameters (List<ActionParameter> externalParameters, bool sendingToAsset)
		{
			for (int i=0; i<externalParameters.Count; i++)
			{
				if (localParameters.Count > i)
				{
					if (externalParameters[i].parameterType == ParameterType.String)
					{
						externalParameters[i].SetValue (localParameters[i].stringValue);
					}
					else if (externalParameters[i].parameterType == ParameterType.Float)
					{
						externalParameters[i].SetValue (localParameters[i].floatValue);
					}
					else if (externalParameters[i].parameterType != ParameterType.GameObject)
					{
						externalParameters[i].SetValue (localParameters[i].intValue);
					}
					else
					{
						if (sendingToAsset)
						{
							if (isAssetFile)
							{
								externalParameters[i].SetValue (localParameters[i].intValue);
							}
							else if (localParameters[i].gameObject != null)
							{
								int idToSend = 0;
								if (localParameters[i].gameObject && localParameters[i].gameObject.GetComponent <ConstantID>())
								{
									idToSend = localParameters[i].gameObject.GetComponent <ConstantID>().constantID;
								}
								else
								{
									Debug.LogWarning (localParameters[i].gameObject.name + " requires a ConstantID script component!");
								}
								externalParameters[i].SetValue (idToSend);
							}
							else
							{
								externalParameters[i].SetValue (localParameters[i].intValue);
							}
						}
						else if (localParameters[i].gameObject != null)
						{
							externalParameters[i].SetValue (localParameters[i].gameObject);
						}
						else
						{
							externalParameters[i].SetValue (localParameters[i].intValue);
						}
					}
				}
			}
		}


		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			listSource = (ListSource) EditorGUILayout.EnumPopup ("Source:", listSource);
			if (listSource == ListSource.InScene)
			{
				parameterID = Action.ChooseParameterGUI ("ActionList:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					localParameters.Clear ();
					constantID = 0;
					actionList = null;
				}
				else
				{
					actionList = (ActionList) EditorGUILayout.ObjectField ("ActionList:", actionList, typeof (ActionList), true);
					
					constantID = FieldToID <ActionList> (actionList, constantID);
					actionList = IDToField <ActionList> (actionList, constantID, true);
				}

				if (actionList != null && actionList.useParameters)
				{
					SetParametersGUI (actionList.parameters);
				}

				runFromStart = EditorGUILayout.Toggle ("Run from start?", runFromStart);

				if (!runFromStart && actionList != null && actionList.actions.Count > 1)
				{
					JumpToActionGUI (actionList.actions);
				}
			}
			else if (listSource == ListSource.AssetFile)
			{
				invActionList = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList asset:", invActionList, typeof (ActionListAsset), true);

				if (invActionList != null && invActionList.useParameters)
				{
					SetParametersGUI (invActionList.parameters);
				}

				runFromStart = EditorGUILayout.Toggle ("Run from start?", runFromStart);
				
				if (!runFromStart && invActionList != null && invActionList.actions.Count > 1)
				{
					JumpToActionGUI (invActionList.actions);
				}
			}

			runInParallel = EditorGUILayout.Toggle ("Run in parallel?", runInParallel);

			if (runInParallel)
			{
				willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);
				numSockets = 1;
				AfterRunningOption ();
			}
			else
			{
				numSockets = 0;
			}
		}


		private void JumpToActionGUI (List<Action> actions)
		{
			int tempSkipAction = jumpToAction;
			List<string> labelList = new List<string>();
			
			if (jumpToActionActual)
			{
				bool found = false;
				
				for (int i = 0; i < actions.Count; i++)
				{
					labelList.Add (i.ToString () + ": " + actions [i].title);
					
					if (jumpToActionActual == actions [i])
					{
						jumpToAction = i;
						found = true;
					}
				}

				if (!found)
				{
					jumpToAction = tempSkipAction;
				}
			}
			
			if (jumpToAction < 0)
			{
				jumpToAction = 0;
			}
			
			if (jumpToAction >= actions.Count)
			{
				jumpToAction = actions.Count - 1;
			}
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField ("  Action to skip to:");
			tempSkipAction = EditorGUILayout.Popup (jumpToAction, labelList.ToArray());
			jumpToAction = tempSkipAction;
			EditorGUILayout.EndHorizontal();
			jumpToActionActual = actions [jumpToAction];
		}


		private int ShowVarSelectorGUI (string label, List<GVar> vars, int ID)
		{
			int variableNumber = -1;
			
			List<string> labelList = new List<string>();
			labelList.Add (" (None)");
			foreach (GVar _var in vars)
			{
				labelList.Add (_var.label);
			}
			
			variableNumber = GetVarNumber (vars, ID) + 1;
			variableNumber = EditorGUILayout.Popup (label, variableNumber, labelList.ToArray()) - 1;

			if (variableNumber >= 0)
			{
				return vars[variableNumber].id;
			}

			return -1;
		}


		private int ShowInvItemSelectorGUI (string label, List<InvItem> items, int ID)
		{
			int invNumber = -1;
			
			List<string> labelList = new List<string>();
			labelList.Add (" (None)");
			foreach (InvItem _item in items)
			{
				labelList.Add (_item.label);
			}
			
			invNumber = GetInvNumber (items, ID) + 1;
			invNumber = EditorGUILayout.Popup (label, invNumber, labelList.ToArray()) - 1;

			if (invNumber >= 0)
			{
				return items[invNumber].id;
			}
			return -1;
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


		private int GetInvNumber (List<InvItem> items, int ID)
		{
			int i = 0;
			foreach (InvItem _item in items)
			{
				if (_item.id == ID)
				{
					return i;
				}
				i++;
			}
			return -1;
		}


		private void SetParametersGUI (List<ActionParameter> externalParameters)
		{
			// Ensure target and local parameter lists match
			
			int numParameters = externalParameters.Count;
			if (numParameters < localParameters.Count)
			{
				localParameters.RemoveRange (numParameters, localParameters.Count - numParameters);
			}
			else if (numParameters > localParameters.Count)
			{
				if (numParameters > localParameters.Capacity)
				{
					localParameters.Capacity = numParameters;
				}
				for (int i=localParameters.Count; i<numParameters; i++)
				{
					ActionParameter newParameter = new ActionParameter (externalParameters [i].ID);
					localParameters.Add (newParameter);
				}
			}

			EditorGUILayout.BeginVertical ("Button");
			for (int i=0; i<externalParameters.Count; i++)
			{
				string label = externalParameters[i].label;
				
				if (externalParameters[i].parameterType == ParameterType.GameObject)
				{
					if (isAssetFile)
					{
						// ID
						localParameters[i].intValue = EditorGUILayout.IntField (label + " (ID):", localParameters[i].intValue);
						localParameters[i].gameObject = null;
					}
					else
					{
						/// Gameobject
						localParameters[i].gameObject = (GameObject) EditorGUILayout.ObjectField (label + ":", localParameters[i].gameObject, typeof (GameObject), true);
						localParameters[i].intValue = 0;
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.GlobalVariable)
				{
					if (AdvGame.GetReferences () && AdvGame.GetReferences ().variablesManager)
					{
						VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;
						localParameters[i].intValue = ShowVarSelectorGUI (label + ":", variablesManager.vars, localParameters[i].intValue);
					}
					else
					{
						EditorGUILayout.HelpBox ("A Variables Manager is required to pass Global Variables.", MessageType.Warning);
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.InventoryItem)
				{
					if (AdvGame.GetReferences () && AdvGame.GetReferences ().inventoryManager)
					{
						InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
						localParameters[i].intValue = ShowInvItemSelectorGUI (label + ":", inventoryManager.items, localParameters[i].intValue);
					}
					else
					{
						EditorGUILayout.HelpBox ("An Inventory Manager is required to pass Inventory items.", MessageType.Warning);
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.LocalVariable)
				{
					if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>())
					{
						LocalVariables localVariables = GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>();
						localParameters[i].intValue = ShowVarSelectorGUI (label + ":", localVariables.localVars, localParameters[i].intValue);
					}
					else
					{
						EditorGUILayout.HelpBox ("A GameEngine prefab is required to pass Local Variables.", MessageType.Warning);
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.String)
				{
					localParameters[i].stringValue = EditorGUILayout.TextField (label + ":", localParameters[i].stringValue);
				}
				else if (externalParameters[i].parameterType == ParameterType.Float)
				{
					localParameters[i].floatValue = EditorGUILayout.FloatField (label + ":", localParameters[i].floatValue);
				}
			}
			EditorGUILayout.EndVertical ();
		}


		public override string SetLabel ()
		{
			string labelAdd = "";
			
			if (listSource == ListSource.InScene && actionList != null)
			{
				labelAdd += " (" + actionList.name + ")";
			}
			else if (listSource == ListSource.AssetFile && invActionList != null)
			{
				labelAdd += " (" + invActionList.name + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
	}

}