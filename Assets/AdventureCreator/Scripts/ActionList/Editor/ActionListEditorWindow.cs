using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using AC;

public class ActionListEditorWindow : EditorWindow
{
	
	private bool isMarquee = false;
	private Rect marqueeRect = new Rect (0f, 0f, 0f, 0f);
	private bool canMarquee = true;
	
	private float zoom = 1f;
	private float zoomMin = 0.5f;
	private float zoomMax = 1f;
	
	private Action actionChanging = null;
	private bool resultType;
	private int multipleResultType;
	private int offsetChanging = 0;
	private int numActions = 0;
	private int deleteSkips = 0;
	
	private ActionList _target;
	private ActionListAsset _targetAsset;
	private Vector2 scrollPosition = Vector2.zero;
	private Vector2 maxScroll;
	private Vector2 menuPosition;
	
	private Texture2D socketIn = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/pointer-active.png", typeof (Texture2D));
	private Texture2D socketOut = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/pointer.png", typeof (Texture2D));
	private Texture2D grey = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/grey.png", typeof (Texture2D));
	private ActionsManager actionsManager;
	
	
	[MenuItem ("Adventure Creator/Editors/ActionList Editor")]
	static void Init ()
	{
		ActionListEditorWindow window = (ActionListEditorWindow) EditorWindow.GetWindow (typeof (ActionListEditorWindow));
		window.Repaint ();
		window.title = "ActionList Editor";
	}
	
	
	private void OnEnable ()
	{
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().actionsManager)
		{
			actionsManager = AdvGame.GetReferences ().actionsManager;
		}

		if (_targetAsset != null)
		{
			UnmarkAll (true);
		}
		else
		{
			UnmarkAll (false);
		}
	}
	
	
	private void PanAndZoomWindow ()
	{
		if (actionChanging)
		{
			return;
		}
		
		if (Event.current.type == EventType.ScrollWheel)
		{
			Vector2 screenCoordsMousePos = Event.current.mousePosition;
			Vector2 delta = Event.current.delta;
			float zoomDelta = -delta.y / 80.0f;
			float oldZoom = zoom;
			zoom += zoomDelta;
			zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
			scrollPosition += (screenCoordsMousePos - scrollPosition) - (oldZoom / zoom) * (screenCoordsMousePos - scrollPosition);
			
			Event.current.Use();
		}
		
		if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
		{
			Vector2 delta = Event.current.delta;
			delta /= zoom;
			scrollPosition -= delta;
			
			Event.current.Use();
		}
	}
	
	
	private void DrawMarquee (bool isAsset)
	{
		if (actionChanging)
		{
			return;
		}
		
		if (!canMarquee)
		{
			isMarquee = false;
			return;
		}
		
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !isMarquee)
		{
			isMarquee = true;
			marqueeRect = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y, 0f, 0f);
		}
		else if (Event.current.type == EventType.MouseUp)
		{
			if (isMarquee)
			{
				MarqueeSelect (isAsset);
			}
			isMarquee = false;
		}
		
		if (isMarquee)
		{
			marqueeRect.width = Event.current.mousePosition.x - marqueeRect.x;
			marqueeRect.height = Event.current.mousePosition.y - marqueeRect.y;
			GUI.DrawTexture (marqueeRect, grey);
		}
	}
	
	
	private void MarqueeSelect (bool isAsset)
	{
		List<Action> actionList = new List<Action>();
		if (isAsset)
		{
			actionList = _targetAsset.actions;
		}
		else
		{
			actionList = _target.actions;
		}

		if (marqueeRect.width < 0f)
		{
			marqueeRect.x += marqueeRect.width;
			marqueeRect.width *= -1f;
		}
		if (marqueeRect.height < 0f)
		{
			marqueeRect.y += marqueeRect.height;
			marqueeRect.height *= -1f;
		}
		
		// Correct for panning
		marqueeRect.x += scrollPosition.x;
		marqueeRect.y += scrollPosition.y;
		
		// Correct for zooming
		marqueeRect.x /= zoom;
		marqueeRect.y /= zoom;
		marqueeRect.width /= zoom;
		marqueeRect.height /= zoom;
		
		foreach (Action action in actionList)
		{
			action.isMarked = false;
			if (action.nodeRect.x < (marqueeRect.x + marqueeRect.width) && (action.nodeRect.x + action.nodeRect.width) > marqueeRect.x &&
			    action.nodeRect.y < (marqueeRect.y + marqueeRect.height) && (action.nodeRect.y + action.nodeRect.height) > marqueeRect.y)
			{
				action.isMarked = true;
			}
		}
	}
	
	
	private void OnGUI ()
	{
		if (Selection.activeObject && Selection.activeObject is ActionListAsset)
		{
			_targetAsset = (ActionListAsset) Selection.activeObject;
			_target = null;
		}
		else if (Selection.activeGameObject && Selection.activeGameObject.GetComponent <ActionList>())
		{
			_targetAsset = null;
			_target = Selection.activeGameObject.GetComponent<ActionList>();
		}
		/*else
		{
			_targetAsset = null;
			_target = null;
		}*/
		
		if (_targetAsset != null)
		{
			ActionListAssetEditor.ResetList (_targetAsset);

			PanAndZoomWindow ();
			NodesGUI (true);
			DrawMarquee (true);

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Editing ActionList asset: " + _targetAsset.name);
			EditorGUILayout.EndVertical ();

			if (GUI.changed)
			{
				EditorUtility.SetDirty (_targetAsset);
			}
		}
		else if (_target != null)
		{
			ActionListEditor.ResetList (_target);

			if (_target.source == ActionListSource.AssetFile)
			{
				EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Cannot view Actions, since this object references an Asset file.");
				EditorGUILayout.EndVertical ();
			}
			else
			{
				PanAndZoomWindow ();
				NodesGUI (false);
				DrawMarquee (false);

				EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Editing scene ActionList: " + _target.gameObject.name);
				EditorGUILayout.EndVertical ();
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty (_target);
			}
		}
		else
		{
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("No ActionList selected");
			EditorGUILayout.EndVertical ();
		}
	}
	
	
	private void OnInspectorUpdate ()
	{
		Repaint();
	}
	
	
	private void PositionNode (Action action, bool isAsset)
	{
		if (!isAsset && _target == null)
		{
			return;
		}

		if (isAsset && _targetAsset == null)
		{
			return;
		}

		int i = 0;
		Vector2 offset = Vector2.zero;
		if (isAsset)
		{
			i = _targetAsset.actions.IndexOf (action);
			if (i>0)
			{
				offset = new Vector2 (_targetAsset.actions [i-1].nodeRect.x, _targetAsset.actions [i-1].nodeRect.y);
			}
		}
		else
		{
			i = _target.actions.IndexOf (action);
			if (i>0)
			{
				offset = new Vector2 (_target.actions [i-1].nodeRect.x, _target.actions [i-1].nodeRect.y);
			}
		}
		
		if (i == 0)
		{
			action.nodeRect.x = 50;
			action.nodeRect.y = 50;
		}
		else
		{
			action.nodeRect.x = offset.x + 350;
			action.nodeRect.y = offset.y;
		}
	}
	
	
	private void PositionAllNodes (bool isAsset)
	{
		if (isAsset)
		{
			if (_targetAsset != null && _targetAsset.actions.Count > 0)
			{
				foreach (Action action in _targetAsset.actions)
				{
					PositionNode (action, true);
				}
			}
			return;
		}

		if (_target != null && _target.actions.Count > 0)
		{
			foreach (Action action in _target.actions)
			{
				PositionNode (action, false);
			}
		}
	}
	
	
	private void NodeWindow (int i)
	{
		if (actionsManager == null)
		{
			OnEnable ();
		}
		if (actionsManager == null)
		{
			return;
		}

		bool isAsset;
		Action _action;
		List<ActionParameter> parameters = null;

		if (_targetAsset != null)
		{
			_action = _targetAsset.actions[i];
			isAsset = _action.isAssetFile = true;
			if (_targetAsset.useParameters)
			{
				parameters = _targetAsset.parameters;
			}
		}
		else
		{
			_action = _target.actions[i];
			isAsset = _action.isAssetFile = false;
			if (_target.useParameters)
		    {
				parameters = _target.parameters;
			}
		}

		GUI.enabled = _action.isEnabled;

		if (!actionsManager.DoesActionExist (_action.GetType ().ToString ()))
		{
			EditorGUILayout.HelpBox ("This Action type has been disabled in the Actions Manager", MessageType.Warning);
		}
		else
		{
			int typeNumber = GetTypeNumber (i, isAsset);
			int categoryNumber = GetCategoryNumber (typeNumber);
			int subCategoryNumber = GetSubCategoryNumber (i, categoryNumber, isAsset);

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Action type:", GUILayout.Width (100));
			categoryNumber = EditorGUILayout.Popup (categoryNumber, actionsManager.GetActionCategories ());
			subCategoryNumber = EditorGUILayout.Popup (subCategoryNumber, actionsManager.GetActionSubCategories (categoryNumber));
			EditorGUILayout.EndVertical ();
			
			typeNumber = actionsManager.GetTypeNumber (categoryNumber, subCategoryNumber);

			EditorGUILayout.Space ();
			
			// Rebuild constructor if Subclass and type string do not match
			if (_action.GetType ().ToString () != actionsManager.GetActionName (typeNumber) && _action.GetType ().ToString () != ("AC." + actionsManager.GetActionName (typeNumber)))
			{
				Vector2 currentPosition = new Vector2 (_action.nodeRect.x, _action.nodeRect.y);

				// Store "After running data" to transfer over
				ActionEnd _end = new ActionEnd ();
				_end.resultAction = _action.endAction;
				_end.skipAction = _action.skipAction;
				_end.linkedAsset = _action.linkedAsset;
				_end.linkedCutscene = _action.linkedCutscene;

				Action _newAction = null;
				if (isAsset)
				{
					Action newAction = ActionListAssetEditor.RebuildAction (_action, typeNumber, _targetAsset, _end.resultAction, _end.skipAction, _end.linkedAsset, _end.linkedCutscene);
					_targetAsset.actions.Insert (i, newAction);
					_newAction = newAction;
				}
				else
				{
					_target.actions[i] = ActionListEditor.RebuildAction (_action, typeNumber, _end.resultAction, _end.skipAction, _end.linkedAsset, _end.linkedCutscene);
					_newAction = _action;
				}
				
				_newAction.nodeRect.x = currentPosition.x;
				_newAction.nodeRect.y = currentPosition.y;
			}
		
			_action.ShowGUI (parameters);
		}

		GUI.enabled = true;
		if (_action.endAction == ResultAction.Skip || _action.numSockets == 2 || _action is ActionCheckMultiple)
		{
			if (isAsset)
			{
				_action.SkipActionGUI (_targetAsset.actions, true);
			}
			else
			{
				_action.SkipActionGUI (_target.actions, true);
			}
		}
		
		if (i == 0)
		{
			_action.nodeRect.x = 50;
			_action.nodeRect.y = 50;
		}
		else if (deleteSkips == 0)
		{
			GUI.DragWindow ();
		}
	}
	
	
	private void LimitWindow (Action action)
	{
		if (action.nodeRect.x < 1)
		{
			action.nodeRect.x = 1;
		}
		
		if (action.nodeRect.y < 1)
		{
			action.nodeRect.y = 1;
		}
	}
	
	
	private void NodesGUI (bool isAsset)
	{
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().actionsManager)
		{
			actionsManager = AdvGame.GetReferences ().actionsManager;
		}
		
		if (isAsset)
		{
			if (_targetAsset.actions.Count > 0 && (_targetAsset.actions[0].nodeRect.x == 0 || _targetAsset.actions[0].nodeRect.y == 0))
			{
				PositionAllNodes (isAsset);
			}
		}
		else
		{
			if (_target.actions.Count > 0 && (_target.actions[0].nodeRect.x == 0 || _target.actions[0].nodeRect.y == 0))
			{
				PositionAllNodes (isAsset);
			}
		}
		
		bool loseConnection = false;
		Event e = Event.current;
		if (e.isMouse && actionChanging != null && e.type == EventType.MouseUp)
		{
			loseConnection = true;
		}

		if (isAsset)
		{
			numActions = _targetAsset.actions.Count;
			if (numActions < 1)
			{
				numActions = 1;
				AC.Action newAction = ActionListEditor.GetDefaultAction ();
				newAction.hideFlags = HideFlags.HideInHierarchy;
				_targetAsset.actions.Add (newAction);
				AssetDatabase.AddObjectToAsset (newAction, _targetAsset);
				AssetDatabase.SaveAssets ();
			}
			numActions = _targetAsset.actions.Count;
		}
		else
		{
			numActions = _target.actions.Count;
			if (numActions < 1)
			{
				numActions = 1;
				AC.Action newAction = ActionListEditor.GetDefaultAction ();
				_target.actions.Add (newAction);
			}
			numActions = _target.actions.Count;
		}
		
		EditorZoomArea.Begin (zoom, new Rect (0, 0, position.width / zoom, position.height / zoom));
		scrollPosition = GUI.BeginScrollView (new Rect (0, 0, position.width / zoom, position.height / zoom), scrollPosition, new Rect (0, 0, maxScroll.x, maxScroll.y), false, false);
		BeginWindows ();
		
		canMarquee = true;
		Vector2 newMaxScroll = Vector2.zero;
		for (int i=0; i<numActions; i++)
		{
			FixConnections (i, isAsset);

			Action _action;
			if (isAsset)
			{
				_action = _targetAsset.actions[i];
			}
			else
			{
				_action = _target.actions[i];
			}

			if (_action.nodeRect.x == 0 && _action.nodeRect.y == 0)
			{
				PositionNode (_action, isAsset);
			}
			
			Color tempColor = GUI.color;
			
			if (_action.isRunning && Application.isPlaying)
			{
				GUI.color = Color.cyan;
			}
			else if (_action.isMarked)
			{
				GUI.color = new Color (0.7f, 1f, 0.6f);
			}
			
			_action.nodeRect.height = 10;
			if (deleteSkips > 0)
			{
				deleteSkips --;
				Rect originalRect = _action.nodeRect;
				_action.nodeRect = GUILayout.Window (i, _action.nodeRect, NodeWindow, i + ": " + _action.title, GUILayout.Width (300));
				_action.nodeRect.x = originalRect.x;
				_action.nodeRect.y = originalRect.y;
			}
			else
			{
				_action.nodeRect = GUILayout.Window (i, _action.nodeRect, NodeWindow, i + ": " + _action.title, GUILayout.Width (300));
			}
			
			GUI.color = tempColor;
			
			if (_action.nodeRect.x + _action.nodeRect.width + 20 > newMaxScroll.x)
			{
				newMaxScroll.x = _action.nodeRect.x + _action.nodeRect.width + 200;
			}
			if (_action.nodeRect.height != 10)
			{
				if (_action.nodeRect.y + _action.nodeRect.height + 20 > newMaxScroll.y)
				{
					newMaxScroll.y = _action.nodeRect.y + _action.nodeRect.height + 200;
				}
			}
			
			LimitWindow (_action);
			DrawSockets (_action, isAsset);
			
			/*if (_action.endAction == ResultAction.Skip || _action.numSockets == 2)
			{
				if (isAsset)
				{
					_action.SkipActionGUI (_targetAsset.actions, false);
				}
				else
				{
					_action.SkipActionGUI (_target.actions, false);
				}
			}*/

			if (isAsset)
			{
				_targetAsset.actions = ActionListAssetEditor.ResizeList (_targetAsset, numActions);
			}
			else
			{
				_target.actions = ActionListEditor.ResizeList (_target.actions, numActions);
			}
			
			if (actionChanging != null && loseConnection && (new Rect (_action.nodeRect.x - 20, _action.nodeRect.y, 20, 20).Contains(e.mousePosition) || _action.nodeRect.Contains(e.mousePosition)))
			{
				Reconnect (actionChanging, _action, isAsset);
			}
			
			if (!isMarquee && _action.nodeRect.Contains (e.mousePosition))
			{
				canMarquee = false;
			}
			
		}
		
		if (loseConnection && actionChanging != null)
		{
			EndConnect (actionChanging, e.mousePosition, isAsset);
		}
		
		if (actionChanging != null)
		{
			AdvGame.DrawNodeCurve (actionChanging.nodeRect, e.mousePosition, Color.cyan, offsetChanging);
		}
		
		if (e.type == EventType.ContextClick && actionChanging == null && !isMarquee)
		{
			menuPosition = e.mousePosition;
			CheckEmptyMenu (e.mousePosition, isAsset);
		}
		
		EndWindows ();
		GUI.EndScrollView ();
		EditorZoomArea.End();
		
		if (newMaxScroll.y != 0)
		{
			maxScroll = newMaxScroll;
		}
	}
	
	
	private void UnmarkAll (bool isAsset)
	{
		if (isAsset)
		{
			if (_targetAsset && _targetAsset.actions.Count > 0)
			{
				foreach (Action action in _targetAsset.actions)
				{
					if (action)
					{
						action.isMarked = false;
					}
				}
			}
		}
		else
		{
			if (_target && _target.actions.Count > 0)
			{
				foreach (Action action in _target.actions)
				{
					if (action)
					{
						action.isMarked = false;
					}
				}
			}
		}
	}
	
	
	private Action InsertAction (int i, Vector2 position, bool isAsset)
	{
		List<Action> actionList = new List<Action>();
		if (isAsset)
		{
			actionList = _targetAsset.actions;
			Undo.RecordObject (_targetAsset, "Create action");
			ActionListAssetEditor.AddAction (actionsManager.GetDefaultAction (), i+1, _targetAsset);
		}
		else
		{
			actionList = _target.actions;
			ActionListEditor.ModifyAction (_target, _target.actions[i], "Insert after");
		}

		numActions ++;
		UnmarkAll (isAsset);
		
		actionList [i+1].nodeRect.x = position.x;
		actionList [i+1].nodeRect.y = position.y;
		actionList [i+1].endAction = ResultAction.Stop;
		actionList [i+1].isDisplayed = false;
		
		return actionList [i+1];
	}
	
	
	private void FixConnections (int i, bool isAsset)
	{
		List<Action> actionList = new List<Action>();
		if (isAsset)
		{
			actionList = _targetAsset.actions;
		}
		else
		{
			actionList = _target.actions;
		}

		if (actionList[i].numSockets == 0)
		{
			actionList[i].endAction = ResultAction.Stop;
		}
		
		else if (actionList[i] is ActionCheck)
		{
			ActionCheck tempAction = (ActionCheck) actionList[i];
			if (tempAction.resultActionTrue == ResultAction.Skip && !actionList.Contains (tempAction.skipActionTrueActual))
			{
				if (tempAction.skipActionTrue >= actionList.Count)
				{
					tempAction.resultActionTrue = ResultAction.Stop;
				}
			}
			if (tempAction.resultActionFail == ResultAction.Skip && !actionList.Contains (tempAction.skipActionFailActual))
			{
				if (tempAction.skipActionFail >= actionList.Count)
				{
					tempAction.resultActionFail = ResultAction.Stop;
				}
			}
		}
		else if (actionList[i] is ActionCheckMultiple)
		{
			ActionCheckMultiple tempAction = (ActionCheckMultiple) actionList[i];
			foreach (ActionEnd ending in tempAction.endings)
			{
				if (ending.resultAction == ResultAction.Skip && !actionList.Contains (ending.skipActionActual))
				{
					if (ending.skipAction >= actionList.Count)
					{
						ending.resultAction = ResultAction.Stop;
					}
				}
			}
		}
		else
		{
			if (actionList[i].endAction == ResultAction.Skip && !actionList.Contains (actionList[i].skipActionActual))
			{
				if (actionList[i].skipAction >= actionList.Count)
				{
					actionList[i].endAction = ResultAction.Stop;
				}
			}
		}
	}
	
	
	private void EndConnect (Action action1, Vector2 mousePosition, bool isAsset)
	{
		List<Action> actionList = new List<Action>();
		if (isAsset)
		{
			actionList = _targetAsset.actions;
		}
		else
		{
			actionList = _target.actions;
		}

		isMarquee = false;
		
		if (action1 is ActionCheck)
		{
			ActionCheck tempAction = (ActionCheck) action1;
			
			if (resultType)
			{
				if (actionList.IndexOf (action1) == actionList.Count - 1 && tempAction.resultActionTrue != ResultAction.Skip)
				{
					InsertAction (actionList.IndexOf (action1), mousePosition, isAsset);
					tempAction.resultActionTrue = ResultAction.Continue;
				}
				else if (tempAction.resultActionTrue == ResultAction.Stop)
				{
					tempAction.resultActionTrue = ResultAction.Skip;
					tempAction.skipActionTrueActual = InsertAction (actionList.Count-1, mousePosition, isAsset);
				}
				else
				{
					tempAction.resultActionTrue = ResultAction.Stop;
				}
			}
			else
			{
				if (actionList.IndexOf (action1) == actionList.Count - 1 && tempAction.resultActionFail != ResultAction.Skip)
				{
					InsertAction (actionList.IndexOf (action1), mousePosition, isAsset);
					tempAction.resultActionFail = ResultAction.Continue;
				}
				else if (tempAction.resultActionFail == ResultAction.Stop)
				{
					tempAction.resultActionFail = ResultAction.Skip;
					tempAction.skipActionFailActual = InsertAction (actionList.Count-1, mousePosition, isAsset);
				}
				else
				{
					tempAction.resultActionFail = ResultAction.Stop;
				}
			}
		}
		else if (action1 is ActionCheckMultiple)
		{
			ActionCheckMultiple tempAction = (ActionCheckMultiple) action1;
			ActionEnd ending = tempAction.endings [multipleResultType];
			
			if (actionList.IndexOf (action1) == actionList.Count - 1 && ending.resultAction != ResultAction.Skip)
			{
				InsertAction (actionList.IndexOf (action1), mousePosition, isAsset);
				ending.resultAction = ResultAction.Continue;
			}
			else if (ending.resultAction == ResultAction.Stop)
			{
				ending.resultAction = ResultAction.Skip;
				ending.skipActionActual = InsertAction (actionList.Count-1, mousePosition, isAsset);
			}
			else
			{
				ending.resultAction = ResultAction.Stop;
			}
		}
		else
		{
			if (actionList.IndexOf (action1) == actionList.Count - 1 && action1.endAction != ResultAction.Skip)
			{
				InsertAction (actionList.IndexOf (action1), mousePosition, isAsset);
				action1.endAction = ResultAction.Continue;
			}
			else if (action1.endAction == ResultAction.Stop)
			{
				// Remove bad "end" connection
				float x = mousePosition.x;
				foreach (AC.Action action in actionList)
				{
					if (action.nodeRect.x > x && !(action is ActionCheck) && !(action is ActionCheckMultiple) && action.endAction == ResultAction.Continue)
					{
						// Is this the "last" one?
						int i = actionList.IndexOf (action);
						if (actionList.Count == (i+1))
						{
							action.endAction = ResultAction.Stop;
						}
					}
				}

				action1.endAction = ResultAction.Skip;
				action1.skipActionActual = InsertAction (actionList.Count-1, mousePosition, isAsset);
			}
			else
			{
				action1.endAction = ResultAction.Stop;
			}
		}
		
		actionChanging = null;
		offsetChanging = 0;

		if (isAsset)
		{
			EditorUtility.SetDirty (_targetAsset);
		}
		else
		{
			EditorUtility.SetDirty (_target);
		}
	}


	private void Reconnect (Action action1, Action action2, bool isAsset)
	{
		isMarquee = false;
		
		if (action1 is ActionCheck)
		{
			ActionCheck actionCheck = (ActionCheck) action1;
			
			if (resultType)
			{
				actionCheck.resultActionTrue = ResultAction.Skip;
				if (action2 != null)
				{
					actionCheck.skipActionTrueActual = action2;
				}
			}
			else
			{
				actionCheck.resultActionFail = ResultAction.Skip;
				if (action2 != null)
				{
					actionCheck.skipActionFailActual = action2;
				}
			}
		}
		else if (action1 is ActionCheckMultiple)
		{
			ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple) action1;

			ActionEnd ending = actionCheckMultiple.endings [multipleResultType];

			ending.resultAction = ResultAction.Skip;
			if (action2 != null)
			{
				ending.skipActionActual = action2;
			}
		}
		else
		{
			action1.endAction = ResultAction.Skip;
			action1.skipActionActual = action2;
		}
		
		actionChanging = null;
		offsetChanging = 0;
		
		if (isAsset)
		{
			EditorUtility.SetDirty (_targetAsset);
		}
		else
		{
			EditorUtility.SetDirty (_target);
		}
	}
	
	
	private Rect SocketIn (Action action)
	{
		return new Rect (action.nodeRect.x - 20, action.nodeRect.y, 20, 20);
	}
	
	
	private void DrawSockets (Action action, bool isAsset)
	{
		List<Action> actionList = new List<Action>();
		if (isAsset)
		{
			actionList = _targetAsset.actions;
		}
		else
		{
			actionList = _target.actions;
		}

		int i = actionList.IndexOf (action);
		Event e = Event.current;
		
		if (SocketIn (action).Contains (e.mousePosition) && actionChanging)
		{
			GUI.Label (SocketIn (action), socketOut, "Label");
		}
		else
		{
			GUI.Label (SocketIn (action), socketIn, "Label");
		}
		
		if (action.numSockets == 0)
		{
			return;
		}
		
		int offset = 0;
		
		if (action is ActionCheck)
		{
			ActionCheck actionCheck = (ActionCheck) action;
			if (actionCheck.resultActionFail != ResultAction.RunCutscene)
			{
				Rect buttonRect = new Rect (action.nodeRect.x + action.nodeRect.width, action.nodeRect.y - 22 + action.nodeRect.height, 20, 20);
				
				if (e.isMouse && actionChanging == null && e.type == EventType.MouseDown && action.isEnabled && buttonRect.Contains(e.mousePosition))
				{
					offsetChanging = 10;
					resultType = false;
					actionChanging = action;
				}
				
				if (actionChanging == null && action.isEnabled && buttonRect.Contains (e.mousePosition))
				{
					GUI.Button (buttonRect, socketIn, "Label");
				}
				else
				{
					GUI.Button (buttonRect, socketOut, "Label");
				}
				
				if (actionCheck.resultActionFail == ResultAction.Skip)
				{
					offset = 17;
				}
			}
			if (actionCheck.resultActionTrue != ResultAction.RunCutscene)
			{
				Rect buttonRect = new Rect (action.nodeRect.x + action.nodeRect.width, action.nodeRect.y - 40 - offset + action.nodeRect.height, 20, 20);
				
				if (e.isMouse && actionChanging == null && e.type == EventType.MouseDown && action.isEnabled && buttonRect.Contains(e.mousePosition))
				{
					offsetChanging = 30 + offset;
					resultType = true;
					actionChanging = action;
				}
				
				if (actionChanging == null && action.isEnabled && buttonRect.Contains (e.mousePosition))
				{
					GUI.Button (buttonRect, socketIn, "Label");
				}
				else
				{
					GUI.Button (buttonRect, socketOut, "Label");
				}
			}
		}
		else if (action is ActionCheckMultiple)
		{
			ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple) action;

			foreach (ActionEnd ending in actionCheckMultiple.endings)
			{
				int j = actionCheckMultiple.endings.IndexOf (ending);

				if (ending.resultAction != ResultAction.RunCutscene)
				{
					Rect buttonRect = new Rect (action.nodeRect.x + action.nodeRect.width, action.nodeRect.y + (j * 43) - (actionCheckMultiple.endings.Count * 43) + action.nodeRect.height, 20, 20);
					
					if (e.isMouse && actionChanging == null && e.type == EventType.MouseDown && action.isEnabled && buttonRect.Contains(e.mousePosition))
					{
						offsetChanging = (actionCheckMultiple.endings.Count - j) * 43 - 13;
						multipleResultType = actionCheckMultiple.endings.IndexOf (ending);
						actionChanging = action;
					}
					
					if (actionChanging == null && action.isEnabled && buttonRect.Contains (e.mousePosition))
					{
						GUI.Button (buttonRect, socketIn, "Label");
					}
					else
					{
						GUI.Button (buttonRect, socketOut, "Label");
					}
				}
			}
		}
		else
		{
			if (action.endAction != ResultAction.RunCutscene)
			{
				Rect buttonRect = new Rect (action.nodeRect.x + action.nodeRect.width, action.nodeRect.y - 22 + action.nodeRect.height, 20, 20);
				
				if (e.isMouse && actionChanging == null && e.type == EventType.MouseDown && action.isEnabled && buttonRect.Contains(e.mousePosition))
				{
					offsetChanging = 10;
					actionChanging = action;
				}
				
				if (actionChanging == null && action.isEnabled && buttonRect.Contains (e.mousePosition))
				{
					GUI.Button (buttonRect, socketIn, "Label");
				}
				else
				{
					GUI.Button (buttonRect, socketOut, "Label");
				}
			}
		}
		
		action.DrawOutWires (actionList, i, offset);
	}
	
	
	private int GetTypeNumber (int i, bool isAsset)
	{
		List<Action> actionList = new List<Action>();
		if (isAsset)
		{
			actionList = _targetAsset.actions;
		}
		else
		{
			actionList = _target.actions;
		}

		int number = 0;
		ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
		if (actionsManager)
		{
			for (int j=0; j<actionsManager.GetActionsSize(); j++)
			{
				try
				{
					if (actionList[i].GetType ().ToString () == actionsManager.GetActionName (j) || actionList[i].GetType ().ToString () == ("AC." + actionsManager.GetActionName (j)))
					{
						number = j;
						break;
					}
				}
				
				catch
				{
					string defaultAction = actionsManager.GetDefaultAction ();
					Action newAction = (Action) CreateInstance (defaultAction);
					actionList[i] = newAction;

					if (isAsset)
					{
						AssetDatabase.AddObjectToAsset (newAction, _targetAsset);
					}
				}
			}
		}
		
		return number;
	}
	
	
	private int GetCategoryNumber (int i)
	{
		if (actionsManager)
		{
			return actionsManager.GetActionCategory (i);
		}
		
		return 0;
	}
	
	
	private int GetSubCategoryNumber (int i, int _categoryNumber, bool isAsset)
	{
		int number = 0;
		
		if (actionsManager)
		{
			if (isAsset)
			{
				return actionsManager.GetActionSubCategory (_targetAsset.actions[i].title, _categoryNumber);
			}
			else
			{
				return actionsManager.GetActionSubCategory (_target.actions[i].title, _categoryNumber);
			}
		}
		
		return number;
	}
	
	
	private bool AreAnyActionsMarked (bool isAsset)
	{
		List<Action> actionList = new List<Action>();
		if (isAsset)
		{
			actionList = _targetAsset.actions;
		}
		else
		{
			actionList = _target.actions;
		}

		foreach (Action action in actionList)
		{
			if (action.isMarked)
			{
				return true;
			}
		}
		
		return false;
	}
	
	
	private void CheckEmptyMenu (Vector2 mousePosition, bool isAsset)
	{
		List<Action> actionList = new List<Action>();
		if (isAsset)
		{
			actionList = _targetAsset.actions;
		}
		else
		{
			actionList = _target.actions;
		}

		if (actionList[0].nodeRect.Contains (mousePosition))
		{
			return;
		}
		
		CreateEmptyMenu (isAsset);
	}
	
	
	private void CreateEmptyMenu (bool isAsset)
	{
		GenericMenu menu = new GenericMenu ();
		menu.AddItem (new GUIContent ("Add new Action"), false, EmptyCallback, "Insert end");
		if (AdvGame.copiedActions != null && AdvGame.copiedActions.Count > 0)
		{
			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Paste"), false, EmptyCallback, "Paste");
		}
		
		menu.AddSeparator ("");
		menu.AddItem (new GUIContent ("Select all"), false, EmptyCallback, "Mark all");
		
		if (AreAnyActionsMarked (isAsset))
		{
			menu.AddItem (new GUIContent ("Deselect all"), false, EmptyCallback, "Unmark all");
			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Copy selected"), false, EmptyCallback, "Copy marked");
			menu.AddItem (new GUIContent ("Delete selected"), false, EmptyCallback, "Delete marked");
			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Enable selected"), false, EmptyCallback, "Enable marked");
			menu.AddItem (new GUIContent ("Disable selected"), false, EmptyCallback, "Disable marked");
		}
		
		menu.ShowAsContext ();
	}
	
	
	private void ActionCallback (object obj)
	{
		bool isAsset = false;
		List<Action> actionList = new List<Action>();
		if (_targetAsset != null)
		{
			isAsset = true;
			actionList = _targetAsset.actions;
		}
		else
		{
			actionList = _target.actions;
		}

		Action actionToAffect = null;
		
		foreach (Action action in actionList)
		{
			if (action.nodeRect.Contains (new Vector2 (menuPosition.x + 50, menuPosition.y)))
			{
				actionToAffect = action;
				break;
			}
		}
		
		if (actionToAffect == null)
		{
			return;
		}

		if (isAsset)
		{
			ActionListAssetEditor.ModifyAction (_targetAsset, actionToAffect, obj.ToString ());
			EditorUtility.SetDirty (_targetAsset);
		}
		else
		{
			ActionListEditor.ModifyAction (_target, actionToAffect, obj.ToString ());
			EditorUtility.SetDirty (_target);
		}
	}
	
	
	private void EmptyCallback (object obj)
	{
		bool isAsset = false;
		List<Action> actionList = new List<Action>();
		if (_targetAsset != null)
		{
			isAsset = true;
			actionList = _targetAsset.actions;
		}
		else
		{
			actionList = _target.actions;
		}

		if (obj.ToString () == "Insert end")
		{
			if (isAsset)
			{
				Action currentAction = actionList [actionList.Count-1];
				ActionListAssetEditor.ModifyAction (_targetAsset, currentAction, "Insert after");
			}
			else
			{
				ActionListEditor.ModifyAction (_target, null, obj.ToString ());
			}

			actionList[actionList.Count-1].nodeRect.x = menuPosition.x;
			actionList[actionList.Count-1].nodeRect.y = menuPosition.y;
			actionList[actionList.Count-1].isDisplayed = false;
		}
		else if (obj.ToString () == "Paste")
		{
			if (AdvGame.copiedActions.Count == 0)
			{
				return;
			}
			if (isAsset)
			{
				Undo.RecordObject (_targetAsset, "Paste actions");
			}
			else
			{
				Undo.RecordObject (_target, "Paste actions");
			}
			List<Action> pasteList = AdvGame.copiedActions;
			Vector2 firstPosition = new Vector2 (pasteList[0].nodeRect.x, pasteList[0].nodeRect.y);
			foreach (Action pasteAction in pasteList)
			{
				if (pasteList.IndexOf (pasteAction) == 0)
				{
					pasteAction.nodeRect.x = menuPosition.x;
					pasteAction.nodeRect.y = menuPosition.y;
				}
				else
				{
					pasteAction.nodeRect.x = menuPosition.x + (pasteAction.nodeRect.x - firstPosition.x);
					pasteAction.nodeRect.y = menuPosition.y + (pasteAction.nodeRect.y - firstPosition.y);
				}
				if (isAsset)
				{
					pasteAction.hideFlags = HideFlags.HideInHierarchy;
					AssetDatabase.AddObjectToAsset (pasteAction, _targetAsset);
				}
				pasteAction.isDisplayed = false;
				actionList.Add (pasteAction);
			}
			if (isAsset)
			{
				AssetDatabase.SaveAssets ();
			}
			AdvGame.DuplicateActionsBuffer ();
		}
		else if (obj.ToString () == "Mark all")
		{
			foreach (Action action in actionList)
			{
				action.isMarked = true;
			}
		}
		else if (obj.ToString () == "Unmark all")
		{
			foreach (Action action in _target.actions)
			{
				action.isMarked = false;
			}
		}
		else if (obj.ToString () == "Copy marked")
		{
			List<Action> copyList = new List<Action>();
			foreach (Action action in actionList)
			{
				if (action.isMarked)
				{
					action.isMarked = false;
					Action copyAction = Object.Instantiate (action) as Action;
					copyList.Add (copyAction);
				}
			}
			AdvGame.copiedActions = copyList;
		}
		else if (obj.ToString () == "Delete marked")
		{
			while (AreAnyActionsMarked (isAsset))
			{
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						if (isAsset)
						{
							ActionListAssetEditor.DeleteAction (action, _targetAsset);
						}
						else
						{
							actionList.Remove (action);
						}
						deleteSkips = actionList.Count;
						numActions --;
						DestroyImmediate (action);
						break;
					}
				}
			}
			if (actionList.Count == 0)
			{
				if (isAsset)
				{
					actionList.Add (ActionListEditor.GetDefaultAction ());
				}
			}
		}
		else if (obj.ToString () == "Enable marked")
		{
			foreach (Action action in actionList)
			{
				if (action.isMarked)
				{
					action.isEnabled = true;
				}
			}
		}
		else if (obj.ToString () == "Disable marked")
		{
			foreach (Action action in actionList)
			{
				if (action.isMarked)
				{
					action.isEnabled = false;
				}
			}
		}

		if (isAsset)
		{
			EditorUtility.SetDirty (_targetAsset);
		}
		else
		{
			EditorUtility.SetDirty (_target);
		}
	}
	
}