/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCheckMultiple.cs"
 * 
 *	This is an intermediate class for "checking" Actions,
 *	that have MULTIPLE endings.
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
	public class ActionCheckMultiple : Action
	{
		
		public List<ActionEnd> endings = new List<ActionEnd>();
		
		
		public ActionCheckMultiple ()
		{
			numSockets = 2;
		}
		
		
		override public int End (List<Action> actions)
		{
			return ProcessResult (-1, actions);
		}
		
		
		protected int ProcessResult (int i, List<Action> actions)
		{
			if (endings.Count > i && i >= 0 && endings[i] != null)
			{
				ActionEnd ending = endings[i];
				if (ending.resultAction == ResultAction.Continue)
				{
					return -3;
				}
				
				else if (ending.resultAction == ResultAction.Stop)
				{
					return -1;
				}
				
				else if (ending.resultAction == ResultAction.Skip)
				{
					int skip = ending.skipAction;
					if (ending.skipActionActual && actions.Contains (ending.skipActionActual))
					{
						skip = actions.IndexOf (ending.skipActionActual);
					}
					else if (skip == -1)
					{
						skip = 0;
					}
					return (skip);
				}
				
				else if (ending.resultAction == ResultAction.RunCutscene)
				{
					if (isAssetFile && ending.linkedAsset != null)
					{
						AdvGame.RunActionListAsset (ending.linkedAsset);
					}
					else if (!isAssetFile && ending.linkedCutscene != null)
					{
						ending.linkedCutscene.SendMessage ("Interact");
					}
					return -2;
				}
			}

			return 0;
		}
		
		
		public virtual bool CheckCondition ()
		{
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		override public void SkipActionGUI (List<Action> actions, bool showGUI)
		{
			if (numSockets < 0)
			{
				numSockets = 0;
			}
			
			if (numSockets < endings.Count)
			{
				endings.RemoveRange (numSockets, endings.Count - numSockets);
			}
			else if (numSockets > endings.Count)
			{
				if (numSockets > endings.Capacity)
				{
					endings.Capacity = numSockets;
				}
				for (int i=endings.Count; i<numSockets; i++)
				{
					ActionEnd newEnd = new ActionEnd ();
					endings.Add (newEnd);
				}
			}

			foreach (ActionEnd ending in endings)
			{
				if (showGUI)
				{
					EditorGUILayout.Space ();
					int i = endings.IndexOf (ending) +1;
					ending.resultAction = (ResultAction) EditorGUILayout.EnumPopup ("If result is " + i.ToString () + ":", (ResultAction) ending.resultAction);
				}

				if (ending.resultAction == ResultAction.RunCutscene && showGUI)
				{
					if (isAssetFile)
					{
						ending.linkedAsset = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList to run:", ending.linkedAsset, typeof (ActionListAsset), false);
					}
					else
					{
						ending.linkedCutscene = (Cutscene) EditorGUILayout.ObjectField ("Cutscene to run:", ending.linkedCutscene, typeof (Cutscene), true);
					}
				}
				else if (ending.resultAction == ResultAction.Skip)
				{
					SkipActionGUI (ending, actions, showGUI);
				}
				else
				{
					EditorGUILayout.Space ();
					EditorGUILayout.Space ();
					EditorGUILayout.Space ();
				}
			}
		}
		
		
		private void SkipActionGUI (ActionEnd ending, List<Action> actions, bool showGUI)
		{
			if (ending.skipAction == -1)
			{
				// Set default
				int i = actions.IndexOf (this);
				if (actions.Count > i+1)
				{
					ending.skipAction = i+1;
				}
				else
				{
					ending.skipAction = i;
				}
			}
			
			int tempSkipAction = ending.skipAction;
			List<string> labelList = new List<string>();
			
			if (ending.skipActionActual)
			{
				bool found = false;
				
				for (int i = 0; i < actions.Count; i++)
				{
					labelList.Add (i.ToString () + ": " + actions [i].title);
					
					if (ending.skipActionActual == actions [i])
					{
						ending.skipAction = i;
						found = true;
					}
				}
				
				if (!found)
				{
					ending.skipAction = tempSkipAction;
				}
			}
			
			if (ending.skipAction >= actions.Count)
			{
				ending.skipAction = actions.Count - 1;
			}
			
			if (showGUI)
			{
				if (actions.Count > 1)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField ("  Action to skip to:");
					tempSkipAction = EditorGUILayout.Popup (ending.skipAction, labelList.ToArray());
					ending.skipAction = tempSkipAction;
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					EditorGUILayout.HelpBox ("Cannot skip action - no further Actions available", MessageType.Warning);
					return;
				}
			}
			
			ending.skipActionActual = actions [ending.skipAction];
		}

		
		override public void DrawOutWires (List<Action> actions, int i, int offset)
		{
			foreach (ActionEnd ending in endings)
			{
				int k = endings.Count - endings.IndexOf (ending);
				float j = ((float) k) / endings.Count;
				Color wireColor = new Color (1f-j, j, 0f);

				if (ending.resultAction == ResultAction.Continue)
				{
					if (actions.Count > i+1)
					{
						AdvGame.DrawNodeCurve (nodeRect, actions[i+1].nodeRect, wireColor, k * 43 -13);
					}
				}
				else if (ending.resultAction == ResultAction.Skip)
				{
					if (actions.Contains (ending.skipActionActual))
					{
						AdvGame.DrawNodeCurve (nodeRect, ending.skipActionActual.nodeRect, wireColor, k * 43 - 13);
					}
				}
			}
		}
		
		#endif
		
	}
	
}