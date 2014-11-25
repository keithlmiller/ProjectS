/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionsManager.cs"
 * 
 *	This script handles the "Scene" tab of the main wizard.
 *	It is used to create the prefabs needed to run the game,
 *	as well as provide easy-access to game logic.
 * 
 */

using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using System;
using UnityEditor;
using System.Collections.Generic;
#endif

namespace AC
{

	[System.Serializable]
	public class SceneManager : ScriptableObject
	{
		
		#if UNITY_EDITOR

		public int selectedSceneObject;
		private string[] prefabTextArray;

		public int activeScenePrefab;
		private List<ScenePrefab> scenePrefabs;

		private DirectoryInfo dir;
		private FileInfo[] info;
		
		private string assetFolder = "Assets/AdventureCreator/Prefabs/";
			
		private string newFolderName = "";
		private string newPrefabName;
		private int index_name;
		private int index_dot;
		
		private GameObject gameEngine;
		
		private static GUILayoutOption
			buttonWidth = GUILayout.MaxWidth(120f);


		public void ShowGUI ()
		{
			GUILayout.Label ("Basic structure", EditorStyles.boldLabel);

			if (GUILayout.Button ("Organise room objects"))
			{
				InitialiseObjects ();
			}
			
			gameEngine = GameObject.FindWithTag (Tags.gameEngine);
			if (gameEngine && AdvGame.GetReferences ().settingsManager)
			{
				SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

				EditorGUILayout.BeginHorizontal ();
				newFolderName = EditorGUILayout.TextField (newFolderName);
					
				if (GUILayout.Button ("Create new folder", buttonWidth))
				{
					if (newFolderName != "")
					{
						GameObject newFolder = new GameObject();
						
						if (!newFolderName.StartsWith ("_"))
							newFolder.name = "_" + newFolderName;
						else
							newFolder.name = newFolderName;
							
						Undo.RegisterCreatedObjectUndo (newFolder, "Create folder " + newFolder.name);
						
						if (Selection.activeGameObject)
						{
							newFolder.transform.parent = Selection.activeGameObject.transform;
						}
						
						Selection.activeObject = newFolder;
					}
				}
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.Space ();
				
				if (gameEngine.GetComponent <SceneSettings>())
				{
					GUILayout.Label ("Scene settings", EditorStyles.boldLabel);
					gameEngine.GetComponent <SceneSettings>().navigationMethod = (AC_NavigationMethod) EditorGUILayout.EnumPopup ("Pathfinding method:", gameEngine.GetComponent <SceneSettings>().navigationMethod);
					gameEngine.GetComponent <NavigationManager>().ResetEngine ();
					if (gameEngine.GetComponent <NavigationManager>().navigationEngine != null)
					{
						gameEngine.GetComponent <NavigationManager>().navigationEngine.SceneSettingsGUI ();
					}

					if (settingsManager.IsUnity2D () && gameEngine.GetComponent <SceneSettings>().navigationMethod != AC_NavigationMethod.PolygonCollider)
					{
						EditorGUILayout.HelpBox ("This pathfinding method is not compatible with 'Unity 2D'.", MessageType.Warning);
					}

					EditorGUILayout.BeginHorizontal ();
						gameEngine.GetComponent <SceneSettings>().defaultPlayerStart = (PlayerStart) EditorGUILayout.ObjectField ("Default PlayerStart:", gameEngine.GetComponent <SceneSettings>().defaultPlayerStart, typeof (PlayerStart), true);
						if (gameEngine.GetComponent <SceneSettings>().defaultPlayerStart == null)
						{
							if (GUILayout.Button ("Auto-create", GUILayout.MaxWidth (90f)))
							{
								PlayerStart newPlayerStart = AddPrefab ("Navigation", "PlayerStart", true, false, true).GetComponent <PlayerStart>();
								newPlayerStart.gameObject.name = "Default PlayerStart";
								gameEngine.GetComponent <SceneSettings>().defaultPlayerStart = newPlayerStart;
							}
						}
					EditorGUILayout.EndHorizontal ();
					if (gameEngine.GetComponent <SceneSettings>().defaultPlayerStart)
					{
						EditorGUILayout.BeginHorizontal ();
							gameEngine.GetComponent <SceneSettings>().defaultPlayerStart.cameraOnStart = (_Camera) EditorGUILayout.ObjectField ("Default Camera:", gameEngine.GetComponent <SceneSettings>().defaultPlayerStart.cameraOnStart, typeof (_Camera), true);
							if (gameEngine.GetComponent <SceneSettings>().defaultPlayerStart.cameraOnStart == null)
							{
								if (GUILayout.Button ("Auto-create", GUILayout.MaxWidth (90f)))
								{
									if (settingsManager == null || settingsManager.cameraPerspective == CameraPerspective.ThreeD)
									{
										GameCamera newCamera = AddPrefab ("Camera", "GameCamera", true, false, true).GetComponent <GameCamera>();
										newCamera.gameObject.name = "NavCam 1";
										gameEngine.GetComponent <SceneSettings>().defaultPlayerStart.cameraOnStart = newCamera;
									}
									else if (settingsManager.cameraPerspective == CameraPerspective.TwoD)
									{
										GameCamera2D newCamera = AddPrefab ("Camera", "GameCamera2D", true, false, true).GetComponent <GameCamera2D>();
										newCamera.gameObject.name = "NavCam 1";
										gameEngine.GetComponent <SceneSettings>().defaultPlayerStart.cameraOnStart = newCamera;
									}
									else if (settingsManager.cameraPerspective == CameraPerspective.TwoPointFiveD)
									{
										GameCamera25D newCamera = AddPrefab ("Camera", "GameCamera2.5D", true, false, true).GetComponent <GameCamera25D>();
										newCamera.gameObject.name = "NavCam 1";
										gameEngine.GetComponent <SceneSettings>().defaultPlayerStart.cameraOnStart = newCamera;
									}
								}
							}
						EditorGUILayout.EndHorizontal ();
					}
					EditorGUILayout.BeginHorizontal ();
						gameEngine.GetComponent <SceneSettings>().sortingMap = (SortingMap) EditorGUILayout.ObjectField ("Default Sorting map:", gameEngine.GetComponent <SceneSettings>().sortingMap, typeof (SortingMap), true);
						if (gameEngine.GetComponent <SceneSettings>().sortingMap == null)
						{
							if (GUILayout.Button ("Auto-create", GUILayout.MaxWidth (90f)))
							{
								SortingMap newSortingMap = AddPrefab ("Navigation", "SortingMap", true, false, true).GetComponent <SortingMap>();
								newSortingMap.gameObject.name = "Default SortingMap";
								gameEngine.GetComponent <SceneSettings>().sortingMap = newSortingMap;
							}
						}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
						gameEngine.GetComponent <SceneSettings>().defaultSound = (Sound) EditorGUILayout.ObjectField ("Default Sound prefab:", gameEngine.GetComponent <SceneSettings>().defaultSound, typeof (Sound), true);
						if (gameEngine.GetComponent <SceneSettings>().defaultSound == null)
						{
							if (GUILayout.Button ("Auto-create", GUILayout.MaxWidth (90f)))
							{
								Sound newSound = AddPrefab ("Logic", "Sound", true, false, true).GetComponent <Sound>();
								newSound.gameObject.name = "Default Sound";
								gameEngine.GetComponent <SceneSettings>().defaultSound = newSound;
								newSound.playWhilePaused = true;
							}
						}
					EditorGUILayout.EndHorizontal ();

					GUILayout.Label ("Scene cutscenes", EditorStyles.boldLabel);
					EditorGUILayout.BeginHorizontal ();
						gameEngine.GetComponent <SceneSettings>().cutsceneOnStart = (Cutscene) EditorGUILayout.ObjectField ("On start:", gameEngine.GetComponent <SceneSettings>().cutsceneOnStart, typeof (Cutscene), true);
						if (gameEngine.GetComponent <SceneSettings>().cutsceneOnStart == null)
						{
							if (GUILayout.Button ("Auto-create", GUILayout.MaxWidth (90f)))
							{
								Cutscene newCutscene = AddPrefab ("Logic", "Cutscene", true, false, true).GetComponent <Cutscene>();
								newCutscene.gameObject.name = "OnStart";
								gameEngine.GetComponent <SceneSettings>().cutsceneOnStart = newCutscene;
							}
						}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
						gameEngine.GetComponent <SceneSettings>().cutsceneOnLoad = (Cutscene) EditorGUILayout.ObjectField ("On load:", gameEngine.GetComponent <SceneSettings>().cutsceneOnLoad, typeof (Cutscene), true);
						if (gameEngine.GetComponent <SceneSettings>().cutsceneOnLoad == null)
						{
							if (GUILayout.Button ("Auto-create", GUILayout.MaxWidth (90f)))
							{
								Cutscene newCutscene = AddPrefab ("Logic", "Cutscene", true, false, true).GetComponent <Cutscene>();
								newCutscene.gameObject.name = "OnLoad";
								gameEngine.GetComponent <SceneSettings>().cutsceneOnLoad = newCutscene;
							}
						}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
						gameEngine.GetComponent <SceneSettings>().cutsceneOnVarChange = (Cutscene) EditorGUILayout.ObjectField ("On variable change:", gameEngine.GetComponent <SceneSettings>().cutsceneOnVarChange, typeof (Cutscene), true);
						if (gameEngine.GetComponent <SceneSettings>().cutsceneOnVarChange == null)
						{
							if (GUILayout.Button ("Auto-create", GUILayout.MaxWidth (90f)))
							{
								Cutscene newCutscene = AddPrefab ("Logic", "Cutscene", true, false, true).GetComponent <Cutscene>();
								newCutscene.gameObject.name = "OnVarChange";
								gameEngine.GetComponent <SceneSettings>().cutsceneOnVarChange = newCutscene;
							}
						}
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.Space ();
				}
				
				GUILayout.Label ("Visibility", EditorStyles.boldLabel);

				GUILayout.BeginHorizontal ();
					GUILayout.Label ("Triggers", buttonWidth);
					if (GUILayout.Button ("On", EditorStyles.miniButtonLeft))
					{
						SetTriggerVisibility (true);
					}
					if (GUILayout.Button ("Off", EditorStyles.miniButtonRight))
					{
						SetTriggerVisibility (false);
					}
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
					GUILayout.Label ("Collision", buttonWidth);
					if (GUILayout.Button ("On", EditorStyles.miniButtonLeft))
					{
						SetCollisionVisiblity (true);
					}
					if (GUILayout.Button ("Off", EditorStyles.miniButtonRight))
					{
						SetCollisionVisiblity (false);
					}
				GUILayout.EndHorizontal ();
				
				GUILayout.BeginHorizontal ();
					GUILayout.Label ("Hotspots", buttonWidth);
					if (GUILayout.Button ("On", EditorStyles.miniButtonLeft))
					{
						SetHotspotVisibility (true);
					}
					if (GUILayout.Button ("Off", EditorStyles.miniButtonRight))
					{
						SetHotspotVisibility (false);
					}
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
					GUILayout.Label ("NavMesh", buttonWidth);
					if (GUILayout.Button ("On", EditorStyles.miniButtonLeft))
					{
						gameEngine.GetComponent <NavigationManager>().navigationEngine.SetVisibility (true);
					}
					if (GUILayout.Button ("Off", EditorStyles.miniButtonRight))
					{
						gameEngine.GetComponent <NavigationManager>().navigationEngine.SetVisibility (false);
					}
				GUILayout.EndHorizontal ();

				ListPrefabs ();

				if (GUI.changed)
				{
					EditorUtility.SetDirty (gameEngine.GetComponent <SceneSettings>());
					EditorUtility.SetDirty (gameEngine.GetComponent <PlayerMovement>());
					if (gameEngine.GetComponent <SceneSettings>().defaultPlayerStart)
					{
						EditorUtility.SetDirty (gameEngine.GetComponent <SceneSettings>().defaultPlayerStart);
					}
				}
			}
			else if (AdvGame.GetReferences ().settingsManager == null)
			{
				EditorGUILayout.HelpBox ("No Settings Manager defined - cannot display full Editor without it!", MessageType.Warning);
			}
		}
		
		
		private void PrefabButton (string subFolder, string prefabName)
		{
			if (GUILayout.Button (prefabName))
			{
				AddPrefab (subFolder, prefabName, true, true, true);
			}	
		}


		private void PrefabButton (string subFolder, string prefabName, Texture icon)
		{
			if (GUILayout.Button (icon))
			{
				AddPrefab (subFolder, prefabName, true, true, true);
			}	
		}

		
		public void InitialiseObjects ()
		{
			CreateFolder ("_Cameras");
			CreateFolder ("_Cutscenes");
			CreateFolder ("_DialogueOptions");
			CreateFolder ("_Interactions");
			CreateFolder ("_Lights");
			CreateFolder ("_Logic");
			CreateFolder ("_Moveables");
			CreateFolder ("_Navigation");
			CreateFolder ("_NPCs");
			CreateFolder ("_Sounds");
			CreateFolder ("_SetGeometry");
			
			// Create subfolders
			CreateSubFolder ("_Cameras", "_GameCameras");

			CreateSubFolder ("_Logic", "_ArrowPrompts");
			CreateSubFolder ("_Logic", "_Conversations");
			CreateSubFolder ("_Logic", "_Containers");
			CreateSubFolder ("_Logic", "_Hotspots");
			CreateSubFolder ("_Logic", "_Triggers");

			CreateSubFolder ("_Moveables", "_Tracks");
			
			CreateSubFolder ("_Navigation", "_CollisionCubes");
			CreateSubFolder ("_Navigation", "_CollisionCylinders");
			CreateSubFolder ("_Navigation", "_Markers");
			CreateSubFolder ("_Navigation", "_NavMeshSegments");
			CreateSubFolder ("_Navigation", "_NavMesh");
			CreateSubFolder ("_Navigation", "_Paths");
			CreateSubFolder ("_Navigation", "_PlayerStarts");
			CreateSubFolder ("_Navigation", "_SortingMaps");

			// Delete default main camera
			if (GameObject.FindWithTag (Tags.mainCamera))
			{
				GameObject mainCam = GameObject.FindWithTag (Tags.mainCamera);
				if (mainCam.GetComponent <MainCamera>() == null)
				{
					DestroyImmediate (mainCam);
				}
			}
			
			// Create main camera
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			AddPrefab ("Automatic", "MainCamera", false, false, false);
			PutInFolder (GameObject.FindWithTag (Tags.mainCamera), "_Cameras");
			if (settingsManager && settingsManager.IsUnity2D ())
			{
				GameObject.FindWithTag (Tags.mainCamera).GetComponent <Camera>().orthographic = true;
			}
			
			// Create Background Camera (if 2.5D)
			if (settingsManager && settingsManager.cameraPerspective == CameraPerspective.TwoPointFiveD)
			{
				CreateSubFolder ("_SetGeometry", "_BackgroundImages");
				AddPrefab ("Automatic", "BackgroundCamera", false, false, false);
				PutInFolder (GameObject.FindWithTag (Tags.backgroundCamera), "_Cameras");
			}

			// Create Game engine
			AddPrefab ("Automatic", "GameEngine", false, false, false);
			
			// Assign Player Start
			SceneSettings startSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
			if (startSettings && startSettings.defaultPlayerStart == null)
			{
				string playerStartPrefab = "PlayerStart";
				if (settingsManager.IsUnity2D ())
				{
					playerStartPrefab += "2D";
				}

				PlayerStart playerStart = AddPrefab ("Navigation", playerStartPrefab, true, false, true).GetComponent <PlayerStart>();
				startSettings.defaultPlayerStart = playerStart;
			}

			// Pathfinding method
			if (settingsManager.IsUnity2D ())
			{
				startSettings.navigationMethod = AC_NavigationMethod.PolygonCollider;
				startSettings.GetComponent <NavigationManager>().ResetEngine ();
			}
		}

		
		private void SetHotspotVisibility (bool isVisible)
		{
			Hotspot[] hotspots = FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			Undo.RecordObjects (hotspots, "Hotspot visibility");

			foreach (Hotspot hotspot in hotspots)
			{
				hotspot.showInEditor = isVisible;
				EditorUtility.SetDirty (hotspot);
			}
		}


		private void SetCollisionVisiblity (bool isVisible)
		{
			_Collision[] colls = FindObjectsOfType (typeof (_Collision)) as _Collision[];
			Undo.RecordObjects (colls, "Collision visibility");
			
			foreach (_Collision coll in colls)
			{
				coll.showInEditor = isVisible;
				EditorUtility.SetDirty (coll);
			}
		}

		
		private void SetTriggerVisibility (bool isVisible)
		{
			AC_Trigger[] triggers = FindObjectsOfType (typeof (AC_Trigger)) as AC_Trigger[];
			Undo.RecordObjects (triggers, "Trigger visibility");
			
			foreach (AC_Trigger trigger in triggers)
			{
				trigger.showInEditor = isVisible;
				EditorUtility.SetDirty (trigger);
			}
		}
		
		
		private void RenameObject (GameObject ob, string resourceName)
		{
			ob.name = AdvGame.GetName (resourceName);
		}
		

		public GameObject AddPrefab (string folderName, string prefabName, bool canCreateMultiple, bool selectAfter, bool putInFolder)
		{
			if (canCreateMultiple || !GameObject.Find (AdvGame.GetName (prefabName)))
			{
				string fileName = assetFolder + folderName + Path.DirectorySeparatorChar.ToString () + prefabName + ".prefab";

				GameObject newOb = (GameObject) PrefabUtility.InstantiatePrefab (AssetDatabase.LoadAssetAtPath (fileName, typeof (GameObject)));
				newOb.name = "Temp";

				if (folderName != "" && putInFolder)
				{
					if (!PutInFolder (newOb, "_" + prefabName + "s"))
					{
						string newName = "_" + prefabName;
						
						if (newName.Contains ("2D"))
						{
							newName = newName.Substring (0, newName.IndexOf ("2D"));
							
							if (!PutInFolder (newOb, newName + "s"))
							{
								PutInFolder (newOb, newName);
							}
							else
							{
								PutInFolder (newOb, newName);
							}
						}
						else if (newName.Contains ("2.5D"))
						{
							newName = newName.Substring (0, newName.IndexOf ("2.5D"));
							
							if (!PutInFolder (newOb, newName + "s"))
							{
								PutInFolder (newOb, newName);
							}
							else
							{
								PutInFolder (newOb, newName);
							}
						}
						else if (newName.Contains ("Animated"))
						{
							newName = newName.Substring (0, newName.IndexOf ("Animated"));
							
							if (!PutInFolder (newOb, newName + "s"))
							{
								PutInFolder (newOb, newName);
							}
							else
							{
								PutInFolder (newOb, newName);
							}
						}
						else if (newName.Contains ("ThirdPerson"))
						{
							newName = newName.Substring (0, newName.IndexOf ("ThirdPerson"));
							
							if (!PutInFolder (newOb, newName + "s"))
							{
								PutInFolder (newOb, newName);
							}
							else
							{
								PutInFolder (newOb, newName);
							}
						}
						else
						{
							PutInFolder (newOb, newName);
						}
					}
				}

				if (newOb.GetComponent <GameCamera2D>())
				{
					newOb.GetComponent <GameCamera2D>().SetCorrectRotation ();
				}
				
				RenameObject (newOb, prefabName);

				Undo.RegisterCreatedObjectUndo (newOb, "Created " + newOb.name);
				
				// Select the object
				if (selectAfter)
				{
					Selection.activeObject = newOb;
				}
				
				return newOb;
			}

			return null;
		}
		

		private bool PutInFolder (GameObject ob, string folderName)
		{
			if (ob && GameObject.Find (folderName))
			{
				if (GameObject.Find (folderName).transform.position == Vector3.zero && folderName.Contains ("_"))
				{
					ob.transform.parent = GameObject.Find (folderName).transform;

					return true;
				}
			}
			
			return false;
		}
		

		private void CreateFolder (string folderName)
		{
			if (!GameObject.Find (folderName))
			{
				GameObject newFolder = new GameObject();
				newFolder.name = folderName;
				Undo.RegisterCreatedObjectUndo (newFolder, "Created " + newFolder.name);
			}
		}
		
		
		private void CreateSubFolder (string baseFolderName, string subFolderName)
		{
			if (!GameObject.Find (subFolderName))
			{
				GameObject newFolder = new GameObject ();
				newFolder.name = subFolderName;
				Undo.RegisterCreatedObjectUndo (newFolder, "Created " + newFolder.name);

				if (newFolder != null && GameObject.Find (baseFolderName))
				{
					newFolder.transform.parent = GameObject.Find (baseFolderName).transform;
				}
				else
				{
					Debug.Log ("Folder " + baseFolderName + " does not exist!");
				}
			}
		}


		private ScenePrefab GetActiveScenePrefab ()
		{
			if (scenePrefabs == null)
			{
				DeclareScenePrefabs ();
			}

			if (scenePrefabs.Count < activeScenePrefab)
			{
				activeScenePrefab = 0;
			}

			return scenePrefabs[activeScenePrefab];
		}


		private void ListPrefabs ()
		{
			if (scenePrefabs == null || GUI.changed)
			{
				DeclareScenePrefabs ();
				GetPrefabsInScene ();
			}

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Scene prefabs", EditorStyles.boldLabel);

			EditorGUILayout.BeginVertical ("Button");

			GUILayout.BeginHorizontal ();
			GUIContent prefabHeader = new GUIContent ("  " + GetActiveScenePrefab ().subCategory, GetActiveScenePrefab ().icon);
			EditorGUILayout.LabelField (prefabHeader, EditorStyles.boldLabel, GUILayout.Height (40f));

			EditorGUILayout.HelpBox (GetActiveScenePrefab ().helpText, MessageType.Info);
			GUILayout.EndHorizontal ();

			EditorGUILayout.Space ();

			GUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("New prefab name:", GUILayout.Width (120f));
			newPrefabName = EditorGUILayout.TextField (newPrefabName);

			if (GUILayout.Button ("Add new", GUILayout.Width (60f)))
			{
				string fileName = assetFolder + GetActiveScenePrefab ().prefabPath + ".prefab";
				GameObject newOb = (GameObject) PrefabUtility.InstantiatePrefab (AssetDatabase.LoadAssetAtPath (fileName, typeof (GameObject)));
				if (newPrefabName != "")
				{
					newOb.name = newPrefabName;
					newPrefabName = "";
				}
				PutInFolder (newOb, GetActiveScenePrefab ().sceneFolder);
				Selection.activeGameObject = newOb;
				GetPrefabsInScene ();
			}
			GUILayout.EndHorizontal ();

			EditorGUILayout.Space ();

			if (GUI.changed || prefabTextArray == null)
			{
				GetPrefabsInScene ();
			}

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Existing " + GetActiveScenePrefab ().subCategory + " prefabs:");
			EditorGUILayout.BeginHorizontal ();
				selectedSceneObject = EditorGUILayout.Popup (selectedSceneObject, prefabTextArray);

				if (GUILayout.Button ("Select", EditorStyles.miniButtonLeft))
				{
					if (Type.GetType ("AC." + GetActiveScenePrefab ().componentName) != null)
					{
						MonoBehaviour[] objects = FindObjectsOfType (Type.GetType ("AC." + GetActiveScenePrefab ().componentName)) as MonoBehaviour [];
						if (objects != null && objects.Length > selectedSceneObject && objects[selectedSceneObject].gameObject != null)
						{
							Selection.activeGameObject = objects[selectedSceneObject].gameObject;
						}
					}
					else if (GetActiveScenePrefab ().componentName != "")
					{
						MonoBehaviour[] objects = FindObjectsOfType (Type.GetType (GetActiveScenePrefab ().componentName)) as MonoBehaviour [];
						if (objects != null && objects.Length > selectedSceneObject && objects[selectedSceneObject].gameObject != null)
						{
							Selection.activeGameObject = objects[selectedSceneObject].gameObject;
						}
					}
					
				}
				if (GUILayout.Button ("Refresh", EditorStyles.miniButtonRight))
				{
					GetPrefabsInScene ();
				}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();

			EditorGUILayout.Space ();
			ListAllPrefabs ("Camera");
			ListAllPrefabs ("Logic");
			ListAllPrefabs ("Moveable");
			ListAllPrefabs ("Navigation");
		}


		private void ListAllPrefabs (string _category)
		{
			GUISkin testSkin = (GUISkin) Resources.Load ("SceneManagerSkin");
			GUI.skin = testSkin;
			bool isEven = false;

			EditorGUILayout.LabelField (_category);

			EditorGUILayout.BeginHorizontal ();

			foreach (ScenePrefab prefab in scenePrefabs)
			{
				if (prefab.category == _category)
				{
					isEven = !isEven;

					if (prefab.icon)
					{
						if (GUILayout.Button (new GUIContent (" " + prefab.subCategory, prefab.icon)))
						{
							GUI.skin = null;
							ClickPrefabButton (prefab);
							GUI.skin = testSkin;
						}
					}
					else
					{
						if (GUILayout.Button (new GUIContent (" " + prefab.subCategory)))
						{
							GUI.skin = null;
							ClickPrefabButton (prefab);
							GUI.skin = testSkin;
						}
					}

					if (!isEven)
					{
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
					}
				}
			}

			EditorGUILayout.EndHorizontal ();

			GUI.skin = null;
		}


		private void ClickPrefabButton (ScenePrefab _prefab)
		{
			if (activeScenePrefab == scenePrefabs.IndexOf (_prefab))
			{
				// Clicked twice, add new
				string fileName = assetFolder + _prefab.prefabPath + ".prefab";
				GameObject newOb = (GameObject) PrefabUtility.InstantiatePrefab (AssetDatabase.LoadAssetAtPath (fileName, typeof (GameObject)));
				PutInFolder (newOb, _prefab.sceneFolder);
				EditorGUIUtility.PingObject (newOb);
			}

			activeScenePrefab = scenePrefabs.IndexOf (_prefab);
			GetPrefabsInScene ();
		}


		private void DeclareScenePrefabs ()
		{
			scenePrefabs = new List<ScenePrefab>();
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

			if (settingsManager == null || settingsManager.cameraPerspective == CameraPerspective.ThreeD)
			{
				scenePrefabs.Add (new ScenePrefab ("Camera", "GameCamera", "Camera/GameCamera", "_GameCameras", "The standard camera type for 3D games.", "GameCamera"));
				scenePrefabs.Add (new ScenePrefab ("Camera", "GameCamera Animated", "Camera/GameCameraAnimated", "_GameCameras", "Plays an Animation Clip when active, or syncs it with its target's position.", "GameCameraAnimated"));
				scenePrefabs.Add (new ScenePrefab ("Camera", "GameCamera Third-person", "Camera/GameCameraThirdPerson", "_GameCameras", "Rigidly follows its target, but can still be rotated.", "GameCameraThirdPerson"));
			}
			else
			{
				if (settingsManager.cameraPerspective == CameraPerspective.TwoD)
				{
					scenePrefabs.Add (new ScenePrefab ("Camera", "GameCamera 2D", "Camera/GameCamera2D", "_GameCameras", "The standard camera type for 2D games.", "GameCamera2D"));
				}
				else
				{
					scenePrefabs.Add (new ScenePrefab ("Camera", "GameCamera 2.5D", "Camera/GameCamera2.5D", "_GameCameras", "A stationary camera that can display images in the background.", "GameCamera25D"));
					scenePrefabs.Add (new ScenePrefab ("Camera", "Background Image", "SetGeometry/BackgroundImage", "SetGeometry", "A container for a 2.5D camera's background image.", "BackgroundImage"));
				}
			}

			scenePrefabs.Add (new ScenePrefab ("Logic", "Arrow prompt", "Logic/ArrowPrompt", "_ArrowPrompts", "An on-screen directional prompt for the player.", "ArrowPrompt"));
			scenePrefabs.Add (new ScenePrefab ("Logic", "Conversation", "Logic/Conversation", "_Conversations", "Stores a list of Dialogue Options, from which the player can choose.", "Conversation"));
			scenePrefabs.Add (new ScenePrefab ("Logic", "Container", "Logic/Container", "_Containers", "Can store a list of Inventory Items, for the player to retrieve and add to.", "Container"));
			scenePrefabs.Add (new ScenePrefab ("Logic", "Cutscene", "Logic/Cutscene", "_Cutscenes", "A sequence of Actions that can form a cinematic.", "Cutscene"));
			scenePrefabs.Add (new ScenePrefab ("Logic", "Dialogue Option", "Logic/DialogueOption", "_DialogueOptions", "An option available to the player when a Conversation is active.", "DialogueOption"));

			if (settingsManager != null && settingsManager.IsUnity2D ())
			{
				scenePrefabs.Add (new ScenePrefab ("Logic", "Hotspot 2D", "Logic/Hotspot2D", "_Hotspots", "A portion of the scene that can be interacted with.", "Hotspot"));
			}
			else
			{
				scenePrefabs.Add (new ScenePrefab ("Logic", "Hotspot", "Logic/Hotspot", "_Hotspots", "A portion of the scene that can be interacted with.", "Hotspot"));
			}

			scenePrefabs.Add (new ScenePrefab ("Logic", "Interaction", "Logic/Interaction", "_Interactions", "A sequence of Actions that run when a Hotspot is activated.", "Interaction"));
			scenePrefabs.Add (new ScenePrefab ("Logic", "Sound", "Logic/Sound", "_Sounds", "An audio source that syncs with AC's sound levels.", "Sound"));

			if (settingsManager != null && settingsManager.IsUnity2D ())
			{
				scenePrefabs.Add (new ScenePrefab ("Logic", "Trigger 2D", "Logic/Trigger2D", "_Triggers", "A portion of the scene that responds to objects entering it.", "AC_Trigger"));
			}
			else
			{
				scenePrefabs.Add (new ScenePrefab ("Logic", "Trigger", "Logic/Trigger", "_Triggers", "A portion of the scene that responds to objects entering it.", "AC_Trigger"));
			}

			scenePrefabs.Add (new ScenePrefab ("Moveable", "Draggable", "Moveable/Draggable", "_Moveables", "Can move along pre-defined Tracks, along planes, or be rotated about its centre.", "Moveable_Drag"));
			scenePrefabs.Add (new ScenePrefab ("Moveable", "PickUp", "Moveable/PickUp", "_Moveables", "Can be grabbed, rotated and thrown freely in 3D space.", "Moveable_PickUp"));
			scenePrefabs.Add (new ScenePrefab ("Moveable", "Straight Track", "Moveable/StraightTrack", "_Tracks", "Constrains a Drag object along a straight line, optionally adding rolling or screw effects.", "DragTrack_Straight"));
			scenePrefabs.Add (new ScenePrefab ("Moveable", "Curved Track", "Moveable/CurvedTrack", "_Tracks", "Constrains a Drag object along a circular line.", "DragTrack_Curved"));
			scenePrefabs.Add (new ScenePrefab ("Moveable", "Hinge Track", "Moveable/HingeTrack", "_Tracks", "Constrains a Drag object's position, only allowing it to rotate in a circular motion.", "DragTrack_Hinge"));

			scenePrefabs.Add (new ScenePrefab ("Navigation", "SortingMap", "Navigation/SortingMap", "_SortingMaps", "Defines how sprites are scaled and sorted relative to one another.", "SortingMap"));

			if (settingsManager != null && settingsManager.IsUnity2D ())
			{
				scenePrefabs.Add (new ScenePrefab ("Navigation", "Collision Cube 2D", "Navigation/CollisionCube2D", "_CollisionCubes", "Blocks Character movement, as well as cursor clicks if placed on the Default layer.", "_Collision"));
				scenePrefabs.Add (new ScenePrefab ("Navigation", "Marker 2D", "Navigation/Marker2D", "_Markers", "A point in the scene used by Characters and objects.", "Marker"));
			}
			else
			{
				scenePrefabs.Add (new ScenePrefab ("Navigation", "Collision Cube", "Navigation/CollisionCube", "_CollisionCubes", "Blocks Character movement, as well as cursor clicks if placed on the Default layer.", "_Collision"));
				scenePrefabs.Add (new ScenePrefab ("Navigation", "Collision Cylinder", "Navigation/CollisionCylinder", "_CollisionCylinders", "Blocks Character movement, as well as cursor clicks if placed on the Default layer.", "_Collision"));
				scenePrefabs.Add (new ScenePrefab ("Navigation", "Marker", "Navigation/Marker", "_Markers", "A point in the scene used by Characters and objects.", "Marker"));
			}

			if (GameObject.FindWithTag (Tags.gameEngine))
			{
				AC_NavigationMethod engine = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().navigationMethod;
				if (engine == AC_NavigationMethod.meshCollider)
				{
					scenePrefabs.Add (new ScenePrefab ("Navigation", "NavMesh", "Navigation/NavMesh", "_NavMesh", "A mesh that defines the area that Characters can move in.", "NavigationMesh"));
				}
				else if (engine == AC_NavigationMethod.PolygonCollider)
				{
					scenePrefabs.Add (new ScenePrefab ("Navigation", "NavMesh 2D", "Navigation/NavMesh2D", "_NavMesh", "A polygon that defines the area that Characters can move in.", "NavigationMesh"));
				}
				else if (engine == AC_NavigationMethod.UnityNavigation)
				{
					scenePrefabs.Add (new ScenePrefab ("Navigation", "NavMesh segment", "Navigation/NavMeshSegment", "_NavMeshSegments", "A plane that defines a portion of the area that Characters can move in.", "NavMeshSegment"));
				}
			}

			scenePrefabs.Add (new ScenePrefab ("Navigation", "Path", "Navigation/Path", "_Paths", "A sequence of points that describe a Character's movement.", "Paths"));
		
			if (settingsManager != null && settingsManager.IsUnity2D ())
			{
				scenePrefabs.Add (new ScenePrefab ("Navigation", "PlayerStart 2D", "Navigation/PlayerStart2D", "_PlayerStarts", "A point in the scene from which the Player begins.", "PlayerStart"));
			}
			else
			{
				scenePrefabs.Add (new ScenePrefab ("Navigation", "PlayerStart", "Navigation/PlayerStart", "_PlayerStarts", "A point in the scene from which the Player begins.", "PlayerStart"));
			}
		}


		public void GetPrefabsInScene ()
		{
			List<string> titles = new List<string>();
			MonoBehaviour[] objects;
			int i=1;

			if (Type.GetType ("AC." + GetActiveScenePrefab ().componentName) != null)
			{
				objects = FindObjectsOfType (Type.GetType ("AC." + GetActiveScenePrefab ().componentName)) as MonoBehaviour [];
				foreach (MonoBehaviour _object in objects)
				{
					titles.Add (i.ToString () + ": " + _object.gameObject.name);
					i++;
				}
			}
			else if (GetActiveScenePrefab ().componentName != "")
			{
				objects = FindObjectsOfType (Type.GetType (GetActiveScenePrefab ().componentName)) as MonoBehaviour [];
				foreach (MonoBehaviour _object in objects)
				{
					titles.Add (i.ToString () + ": " + _object.gameObject.name);
					i++;
				}
			}

			if (i == 1)
			{
				titles.Add ("(None found in scene)");
			}

			prefabTextArray = titles.ToArray ();
		}

		#endif

	}


	#if UNITY_EDITOR

	public struct ScenePrefab
	{

		public string category;
		public string subCategory;
		public string prefabPath;
		public string sceneFolder;
		public string helpText;
		public string componentName;
		public Texture2D icon;


		public ScenePrefab (string _category, string _subCategory, string _prefabPath, string _sceneFolder, string _helpText, string _componentName)
		{
			category = _category;
			subCategory = _subCategory;
			prefabPath = _prefabPath;
			sceneFolder = _sceneFolder;
			helpText = _helpText;
			componentName = _componentName;
			icon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/PrefabIcons/" + _componentName +  ".png", typeof (Texture2D));
		}

	}

	#endif

}