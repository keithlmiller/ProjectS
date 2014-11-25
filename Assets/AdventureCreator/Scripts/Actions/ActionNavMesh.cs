/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionNavMesh.cs"
 * 
 *	This action changes the active NavMesh.
 *	All NavMeshes must be on the same unique layer.
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
	public class ActionNavMesh : Action
	{

		public int constantID = 0;
		public int parameterID = -1;

		public NavigationMesh newNavMesh;
		public SortingMap sortingMap;
		public PlayerStart playerStart;
		public Cutscene cutscene;
		public SceneSetting sceneSetting = SceneSetting.DefaultNavMesh;

		public ChangeNavMeshMethod changeNavMeshMethod = ChangeNavMeshMethod.ChangeNavMesh;
		public InvAction holeAction;
		public PolygonCollider2D hole;


		public ActionNavMesh ()
		{
			this.isDisplayed = true;
			title = "Engine: Change scene setting";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			SceneSettings sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();

			if (sceneSetting == SceneSetting.DefaultNavMesh)
			{
				if (sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider && changeNavMeshMethod == ChangeNavMeshMethod.ChangeNumberOfHoles)
				{
					hole = AssignFile <PolygonCollider2D> (parameters, parameterID, constantID, hole);
				}
				else
				{
					newNavMesh = AssignFile <NavigationMesh> (parameters, parameterID, constantID, newNavMesh);
				}
			}
			else if (sceneSetting == SceneSetting.DefaultPlayerStart)
			{
				playerStart = AssignFile <PlayerStart> (parameters, parameterID, constantID, playerStart);
			}
			else if (sceneSetting == SceneSetting.SortingMap)
			{
				sortingMap = AssignFile <SortingMap> (parameters, parameterID, constantID, sortingMap);
			}
			else if (sceneSetting == SceneSetting.OnLoadCutscene || sceneSetting == SceneSetting.OnStartCutscene)
			{
				cutscene = AssignFile <Cutscene> (parameters, parameterID, constantID, cutscene);
			}
		}
		
		
		override public float Run ()
		{
			SceneSettings sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();

			if (sceneSetting == SceneSetting.DefaultNavMesh)
			{
				if (sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider && changeNavMeshMethod == ChangeNavMeshMethod.ChangeNumberOfHoles && hole != null)
				{
					NavigationMesh currentNavMesh = sceneSettings.navMesh;

					if (holeAction == InvAction.Add)
					{
						currentNavMesh.AddHole (hole);
					}
					else if (holeAction == InvAction.Remove)
					{
						currentNavMesh.RemoveHole (hole);
					}
				}
				else if (newNavMesh != null)
				{
					NavigationMesh oldNavMesh = sceneSettings.navMesh;
					oldNavMesh.TurnOff ();
					newNavMesh.TurnOn ();
					sceneSettings.navMesh = newNavMesh;

					if (newNavMesh.GetComponent <ConstantID>() == null)
					{
						Debug.LogWarning ("Warning: Changing to new NavMesh with no ConstantID - change will not be recognised by saved games.");
					}
				}
			}
			else if (sceneSetting == SceneSetting.DefaultPlayerStart && playerStart)
			{
				sceneSettings.defaultPlayerStart = playerStart;

				if (playerStart.GetComponent <ConstantID>() == null)
				{
					Debug.LogWarning ("Warning: Changing to new default PlayerStart with no ConstantID - change will not be recognised by saved games.");
				}
			}
			else if (sceneSetting == SceneSetting.SortingMap && sortingMap)
			{
				sceneSettings.sortingMap = sortingMap;

				// Reset all FollowSortingMap components
				FollowSortingMap[] followSortingMaps = FindObjectsOfType (typeof (FollowSortingMap)) as FollowSortingMap[];
				foreach (FollowSortingMap followSortingMap in followSortingMaps)
				{
					followSortingMap.UpdateSortingMap ();
				}

				if (sortingMap.GetComponent <ConstantID>() == null)
				{
					Debug.LogWarning ("Warning: Changing to new SortingMap with no ConstantID - change will not be recognised by saved games.");
				}
			}
			else if (sceneSetting == SceneSetting.OnLoadCutscene)
			{
				sceneSettings.cutsceneOnLoad = cutscene;

				if (cutscene.GetComponent <ConstantID>() == null)
				{
					Debug.LogWarning ("Warning: Changing to Cutscene On Load with no ConstantID - change will not be recognised by saved games.");
				}
			}
			else if (sceneSetting == SceneSetting.OnStartCutscene)
			{
				sceneSettings.cutsceneOnStart = cutscene;

				if (cutscene.GetComponent <ConstantID>() == null)
				{
					Debug.LogWarning ("Warning: Changing to Cutscene On Start with no ConstantID - change will not be recognised by saved games.");
				}
			}
			
			return 0f;
		}
		

		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			SceneSettings sceneSettings = null;
			if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>())
			{
				sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
			}

			sceneSetting = (SceneSetting) EditorGUILayout.EnumPopup ("Scene setting to change:", sceneSetting);

			if (sceneSettings == null)
			{
				return;
			}

			if (sceneSetting == SceneSetting.DefaultNavMesh)
			{
				if (sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider || sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider)
				{
					if (sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider)
					{
						changeNavMeshMethod = (ChangeNavMeshMethod) EditorGUILayout.EnumPopup ("Change NavMesh method:", changeNavMeshMethod);
					}

					if (sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider || changeNavMeshMethod == ChangeNavMeshMethod.ChangeNavMesh)
					{
						parameterID = Action.ChooseParameterGUI ("New NavMesh:", parameters, parameterID, ParameterType.GameObject);
						if (parameterID >= 0)
						{
							constantID = 0;
							newNavMesh = null;
						}
						else
						{
							newNavMesh = (NavigationMesh) EditorGUILayout.ObjectField ("New NavMesh:", newNavMesh, typeof (NavigationMesh), true);
							
							constantID = FieldToID <NavigationMesh> (newNavMesh, constantID);
							newNavMesh = IDToField <NavigationMesh> (newNavMesh, constantID, false);
						}
					}
					else if (changeNavMeshMethod == ChangeNavMeshMethod.ChangeNumberOfHoles)
					{
						holeAction = (InvAction) EditorGUILayout.EnumPopup ("Add or remove hole:", holeAction);
						string _label = "Hole to add:";
						if (holeAction == InvAction.Remove)
						{
							_label = "Hole to remove:";
						}

						parameterID = Action.ChooseParameterGUI (_label, parameters, parameterID, ParameterType.GameObject);
						if (parameterID >= 0)
						{
							constantID = 0;
							hole = null;
						}
						else
						{
							hole = (PolygonCollider2D) EditorGUILayout.ObjectField (_label, hole, typeof (PolygonCollider2D), true);
							
							constantID = FieldToID <PolygonCollider2D> (hole, constantID);
							hole = IDToField <PolygonCollider2D> (hole, constantID, false);
						}
					}
				}
				else
				{
					EditorGUILayout.HelpBox ("This action is not compatible with the Unity Navigation pathfinding method, as set in the Scene Manager.", MessageType.Warning);
				}
			}
			else if (sceneSetting == SceneSetting.DefaultPlayerStart)
			{
				parameterID = Action.ChooseParameterGUI ("New default PlayerStart:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					playerStart = null;
				}
				else
				{
					playerStart = (PlayerStart) EditorGUILayout.ObjectField ("New default PlayerStart:", playerStart, typeof (PlayerStart), true);
					
					constantID = FieldToID <PlayerStart> (playerStart, constantID);
					playerStart = IDToField <PlayerStart> (playerStart, constantID, false);
				}
			}
			else if (sceneSetting == SceneSetting.SortingMap)
			{
				parameterID = Action.ChooseParameterGUI ("New SortingMap:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					sortingMap = null;
				}
				else
				{
					sortingMap = (SortingMap) EditorGUILayout.ObjectField ("New SortingMap:", sortingMap, typeof (SortingMap), true);
					
					constantID = FieldToID <SortingMap> (sortingMap, constantID);
					sortingMap = IDToField <SortingMap> (sortingMap, constantID, false);
				}
			}
			else if (sceneSetting == SceneSetting.OnLoadCutscene)
			{
				parameterID = Action.ChooseParameterGUI ("New OnLoad cutscene:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					cutscene = null;
				}
				else
				{
					cutscene = (Cutscene) EditorGUILayout.ObjectField ("New OnLoad custscne:", cutscene, typeof (Cutscene), true);
					
					constantID = FieldToID <Cutscene> (cutscene, constantID);
					cutscene = IDToField <Cutscene> (cutscene, constantID, false);
				}
			}
			else if (sceneSetting == SceneSetting.OnStartCutscene)
			{
				parameterID = Action.ChooseParameterGUI ("New OnStart cutscene:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					cutscene = null;
				}
				else
				{
					cutscene = (Cutscene) EditorGUILayout.ObjectField ("New OnStart cutscene:", cutscene, typeof (Cutscene), true);
					
					constantID = FieldToID <Cutscene> (cutscene, constantID);
					cutscene = IDToField <Cutscene> (cutscene, constantID, false);
				}
			}
			
			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			labelAdd = " (" + sceneSetting.ToString () + ")";

			return labelAdd;
		}

		#endif
		
	}

}